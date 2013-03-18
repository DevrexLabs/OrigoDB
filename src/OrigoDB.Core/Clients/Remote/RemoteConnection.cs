using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core
{
	public class RemoteConnection : IDisposable
	{
		string _host;
		int _port;
		TcpClient _client;
		Stream _clientStream;
		readonly IFormatter _formatter = new BinaryFormatter();

		public Stream Stream
		{
			get
			{
				EnsureConnected(); 
				return _clientStream;
			}
		}

		void EnsureConnected()
		{
			if (_client == null || !_client.Client.IsConnected())
			{
				_client = new TcpClient(_host, _port);
				_clientStream = _client.GetStream();
				Write(new ClientInfo());
			}
		}

		public RemoteConnection(string host, int port)
		{
			_host = host;
			_port = port;
		}

		void Write(object graph)
		{
			_formatter.Serialize(Stream, graph);
		}

		object Read()
		{
			var response = _formatter.Deserialize(Stream);

			if (response is Heartbeat)
				return Read(); // Read again since heartbeat can come in the middle of transactions.

			return response;
		}

		public R WriteRead<R>(object graph)
		{
			Write(graph);
			var response = Read();

			return ProcessTransaction<R>(graph, response);
		}


		R ProcessTransaction<R>(object request, object response)
		{
			var message = response as NetworkMessage;
			if (message != null)
			{
				if (message.Payload is RedirectMessage)
				{
					var redirect = message.Payload as RedirectMessage;
					_host = redirect.Host;
					_port = redirect.Port;
					_client.Close();
					_client = null;

					return WriteRead<R>(request);
				}
				if (message.Payload is TransitioningMessage)
				{
					var transition = message.Payload as TransitioningMessage;
					Thread.Sleep(transition.WaitTime);
					return WriteRead<R>(request);
				}
			}

			if (response is R)
				return (R)response;
			if (message != null && message.Payload is R)
				return (R)message.Payload;

			throw new InvalidDataException();
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			if (_client != null)
			{
				_client.Client.Close(0);
			}
		}

		#endregion
	}
}