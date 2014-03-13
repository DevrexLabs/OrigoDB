using System;

namespace OrigoDB.Core
{
	public class RemoteEngineClient<TModel> : IEngine<TModel>, IDisposable where TModel : Model
	{
		public string Host { get; set; }
		public int Port { get; set; }
		readonly IRequestContextFactory _requestContextFactory;


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

		internal TResponse SendAndRecieve<TResponse>(object request)
		{
			if(!(request is NetworkMessage))
				request = new NetworkMessage {Payload = request};

			using (var ctx = _requestContextFactory.GetContext())
			{
				return ctx.Connection.WriteRead<TResponse>(request);
			}
		}


        public TResult Execute<TResult>(Query<TModel, TResult> query)
        {
			return SendAndRecieve<TResult>(query);
        }

        public void Execute(Command<TModel> command)
        {
			SendAndRecieve<object>(command);
        }

        public TResult Execute<TResult>(Command<TModel, TResult> command)
        {
			return SendAndRecieve<TResult>(command);
        }
    }
}