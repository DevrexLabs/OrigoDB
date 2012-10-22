using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LiveDomain.Core
{
    public abstract class ClusterClient<M> : IEngine<M> where M : Model
    {
        List<IEngine<M>> _nodes = new List<IEngine<M>>();

        public List<IEngine<M>> Nodes
        {
            get { return _nodes; }
        }

        public abstract T Execute<S, T>(Query<S, T> queryForSubModel) where S : Model;
        public abstract void Execute<T>(Command<T> command) where T : Model;
        public abstract T Execute<S, T>(CommandWithResult<S, T> command) where S : Model;
    }
}