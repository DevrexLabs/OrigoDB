using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    /// <summary>
    /// An engine executes commands and queries
    /// </summary>
	public interface IEngine<M> where M : Model
	{
		T Execute<T>(Query<M, T> query);
		void Execute(Command<M> command);
		T Execute<T>(CommandWithResult<M, T> command);
	}

    /// <summary>
    /// A engine within the same process can also execute lambdas.
    /// </summary>
	public interface ILocalEngine<M> : IEngine<M> where M : Model
	{
		T Execute<T>(Func<M, T> query);
	}
}
