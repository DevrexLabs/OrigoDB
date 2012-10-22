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

		#region IEngine<M> Members

		public T Execute<T>(Query<M, T> query)
		{
			return SendAndRecieve<T>(query);
		}

		public void Execute(Command<M> command)
		{
			SendAndRecieve<object>(command);
		}

		public T Execute<T>(CommandWithResult<M, T> command)
		{
			return SendAndRecieve<T>(command);
		}

		#endregion

		internal R SendMessage<R>(NetworkMessage message)
		{
			return SendAndRecieve<R>(message);
		}

		R SendAndRecieve<R>(object request)
		{
			using (var ctx = _requestContextFactory.GetContext())
			{
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(ctx.NetworkStream, request);
				object response = formatter.Deserialize(ctx.NetworkStream);

				var message = response as NetworkMessage;
				if (message != null)
				{
					if (!message.Succeeded)
						throw message.Error;

					return (R)message.Payload;
				}

				return (R)response;
			}
		}


        public T Execute<S, T>(Query<S, T> queryForSubModel) where S : Model
        {
            throw new NotImplementedException();
        }

        public void Execute<S>(Command<S> commandForSubModel) where S : Model
        {
            throw new NotImplementedException();
        }

        public T Execute<S, T>(CommandWithResult<S, T> commandForSubModel) where S : Model
        {
            throw new NotImplementedException();
        }
    }
}