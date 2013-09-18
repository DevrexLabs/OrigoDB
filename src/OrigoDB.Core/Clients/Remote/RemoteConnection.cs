using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using OrigoDB.Core.Clients;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core
{
	public class RemoteConnection : IDisposable
	{
		string _host;
		int _port;
		TcpClient _client;
		readonly IFormatter _formatter = new BinaryFormatter();

		internal bool IsConnected()
		{
			return (_client != null && _client.Client.IsConnected());
		}

		void EnsureConnected()
		{
			if (!IsConnected())
			{
				_client = new TcpClient(_host, _port);
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
			EnsureConnected();
			_formatter.Serialize(_client.GetStream(), graph);
		}

		object Read()
		{
			EnsureConnected();
			var response = _formatter.Deserialize(_client.GetStream());

			if (response is Heartbeat)
				return Read(); // Read again since heartbeat can come in the middle of transactions.

			return response;
		}

		public R WriteRead<R>(object graph)
		{
			Write(graph);
			var response = Read();

			// if (response is NetworkMessage && ((NetworkMessage) response).Payload is RedirectMessage)
			//     return response;

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
					throw new WrongNodeException(redirect.Host,redirect.Port);
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
			Disconnect();
		}

		#endregion

		internal void Disconnect()
		{
			if(IsConnected())
			{
				// Will fail cause tcp client is disposed.
				//Write(new GoodbyeMessage());
				_client.Client.Close(0);
				_client = null;
			}
		}
	}
}