using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using OrigoDB.Core;

namespace OrigoDB.Core
{
	public class RemoteEngineClient<M> : IEngine<M>, IDisposable where M : Model
	{
		public string Host { get; set; }
		public int Port { get; set; }
		readonly IRequestContextFactory _requestContextFactory;
		//public RemoteEngineClient(IRequestContextFactory requestContext)
		//{
		//    _requestContextFactory = requestContext;
		//}

		public RemoteEngineClient(IRequestContextFactory requestContext, string host, int port)
		{
			Host = host;
			Port = port;
			_requestContextFactory = requestContext;
		}

		#region IDisposable Members

		public void Dispose()
		{
			_requestContextFactory.Dispose();
		}

		#endregion

		internal object SendAndRecieve(object request)
		{
			return SendAndRecieve<object>(request);
		}

		internal R SendAndRecieve<R>(object request)
		{
			if(!(request is NetworkMessage))
				request = new NetworkMessage {Payload = request};

			using (var ctx = _requestContextFactory.GetContext())
			{
				return ctx.Connection.WriteRead<R>(request);
			}
		}


        public T Execute<S, T>(Query<S, T> query) where S : Model
        {
			return SendAndRecieve<T>(query);
        }

        public void Execute<S>(Command<S> command) where S : Model
        {
			SendAndRecieve<object>(command);
        }

        public T Execute<S, T>(CommandWithResult<S, T> command) where S : Model
        {
			return SendAndRecieve<T>(command);
        }
    }
}