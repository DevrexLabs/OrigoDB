using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core
{

    /// <summary>
    /// An engine executes commands and queries
    /// </summary>
	public interface IEngine<M> where M : Model
	{
        T Execute<S, T>(Query<S, T> query) where S : Model;
        void Execute<S>(Command<S> command) where S : Model;
        T Execute<S, T>(CommandWithResult<S, T> command) where S : Model;
	}
}
