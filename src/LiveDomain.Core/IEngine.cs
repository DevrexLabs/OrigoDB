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
        //T Execute<T>(Query<M, T> query);
        //void Execute(Command<M> command);
        //T Execute<T>(CommandWithResult<M, T> command);

        T Execute<S, T>(Query<S, T> queryForSubModel) where S : Model;
        void Execute<S>(Command<S> commandForSubModel) where S : Model;
        T Execute<S, T>(CommandWithResult<S, T> commandForSubModel) where S : Model;


	}

    /// <summary>
    /// A engine within the same process can also execute lambdas.
    /// </summary>
	public interface ILocalEngine<M> : IEngine<M> where M : Model
	{
		T Execute<T>(Func<M, T> query);
	}
}
