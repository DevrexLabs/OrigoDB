using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using LiveDomain.Core;

namespace LiveDomain.Core
{
	public class RemoteEngineClient<M> : IEngine<M>, IDisposable where M : Model
	{
		readonly IRequestContextFactory _requestContextFactory;
		public RemoteEngineClient(IRequestContextFactory requestContext)
		{
			_requestContextFactory = requestContext;
		}

		#region IDisposable Members

		public void Dispose()
		{
			_requestContextFactory.Dispose();
		}

		#endregion

		internal R SendMessage<R>(NetworkMessage message)
		{
			return SendAndRecieve<R>(message);
		}

		internal NetworkMessage SendMessage(NetworkMessage message)
		{
			return SendAndRecieve<NetworkMessage>(message);
		}

		internal R SendAndRecieve<R>(object request)
		{
			if(!(request is NetworkMessage))
				request = new NetworkMessage() {Payload = request};

			using (var ctx = _requestContextFactory.GetContext())
			{
				ctx.Connection.Write(request);
				return ctx.Connection.Read<R>();
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