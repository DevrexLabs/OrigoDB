using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using LiveDomain.Core;
using System.Collections.Generic;
using System.Linq;

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

		#region ITransactionHandler<M> Members

		public T Execute<T>(Query<M, T> query)
		{
			return (T) SendAndRecieve(query);
		}

		public void Execute(Command<M> command)
		{
			SendAndRecieve(command);
		}

		public T Execute<T>(CommandWithResult<M, T> command)
		{
			return (T) SendAndRecieve(command);
		}

		#endregion

		object SendAndRecieve(object request)
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

					return message.Payload;
				}

				return response;
			}
		}
	}
}