using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
	public interface ITransactionHandler<M> where M : Model
	{
		T Execute<T>(Query<M, T> query);
		void Execute(Command<M> command);
		T Execute<T>(CommandWithResult<M, T> command);
	}

	public interface ILocalTransactionHandler<M> : ITransactionHandler<M> where M : Model
	{
		T Execute<T>(Func<M, T> query);
	}
}
