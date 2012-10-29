using System;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using LiveDomain.Core.Utilities;

namespace LiveDomain.Core
{
	public class RemoteConnection : IDisposable
	{
		readonly string _host;
		readonly int _port;
		TcpClient _client;
		IFormatter _formatter = new BinaryFormatter();

		public NetworkStream Stream { get
		{
			EnsureConnected();
			return _client.GetStream();
		} }

		void EnsureConnected()
		{
			if(!_client.Client.IsConnected())
				_client = new TcpClient(_host,_port);
		}

		public RemoteConnection(string host,int port)
		{
			_host = host;
			_port = port;
			_client = new TcpClient(host,port);
		}

		public void Write(object graph)
		{
			_formatter.Serialize(Stream,graph);
		}

		public R Read<R>()
		{
			var response = _formatter.Deserialize(Stream);

			if(response is Heartbeat)
				return Read<R>();


			var message = response as NetworkMessage;
			if (message != null)
			{
				if (!message.Succeeded)
					throw message.Error;

				if (response is R)
					return (R)response;

				return (R)message.Payload;
			}

			return (R)response;
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			
		}

		#endregion
	}
}