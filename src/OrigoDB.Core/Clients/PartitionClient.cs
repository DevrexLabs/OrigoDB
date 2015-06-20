using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrigoDB.Core
{


    /// <summary>
    /// ClusterClient that performs client side partitioning
    /// </summary>
	public class PartitionClient<TModel> : ClusterClient<TModel> where TModel : Model
	{
		readonly Func<object, int[]> _allNodesDispatcher;
        readonly Dictionary<string, Delegate> _dispatchers = new Dictionary<string, Delegate>();
        readonly Dictionary<string, Delegate> _mergers = new Dictionary<string, Delegate>();

		public PartitionClient()
		{
			_allNodesDispatcher = _ => Enumerable.Range(0, Nodes.Count).ToArray();
		}


        /// <summary>
        /// Associate a command or query type with a single node dispatcher.
        /// The dispatcher takes an instance of T and returns the id of the node to dispatch to
        /// </summary>
		public void SetDispatcherFor<T>(Func<T, int> dispatcher)
		{
			var key = typeof(T).Name;
			var func = new Func<T, int[]>(o => new[] { dispatcher.Invoke(o) });

			_dispatchers[key] = func;
		}


        /// <summary>
        /// Associate a command or query type with a multinode dispatcher.
        /// The dispatcher takes an instance of T and returns an array of ids to dispatch to.
        /// </summary>
		public void SetDispatcherFor<T>(Func<T, int[]> dispatcher)
		{
			var key = typeof(T).Name;
			_dispatchers[key] = dispatcher;
		}


        /// <summary>
        /// Associate a merger with a transaction type.
        /// A merger merges results from multiple nodes into a single result
        /// </summary>
        /// <typeparam name="TTransaction"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="merger"></param>
		public void SetMergerFor<TTransaction, TResult>(Func<TResult[], TResult> merger)
		{
			var key = typeof(TTransaction).Name;
			_mergers[key] = merger;
		}


        /// <summary>
        /// Associate a merger with a specific result type.
        /// </summary>
		public void SetMergerFor<TResult>(Func<TResult[], TResult> merger)
		{
			var key = typeof(TResult).Name;
			_mergers[key] = merger;
		}


		/// <summary>
		/// Dispatch the query to the appropriate nodes and merge the results with a merger
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="query"></param>
		/// <returns>The merged result</returns>
		public override TResult Execute<TResult>(Query<TModel, TResult> query)
		{
			var nodes = GetNodesFor(query);
			if(nodes.Length == 1) return nodes[0].Execute(query);

			var queryResults = nodes.AsParallel().Select(n => n.Execute(query)).ToArray();
			return MergeResults(query, queryResults);
		}

        /// <summary>
        /// Dispatch a command to the appropriate nodes
        /// </summary>
        /// <param name="command">the command to execute</param>
		public override void Execute(Command<TModel> command)
		{
			Parallel.ForEach(GetNodesFor(command), node => node.Execute(command));
		}


        /// <summary>
        /// Dispatch a command and merge the results
        /// </summary>
		public override TResult Execute<TResult>(Command<TModel, TResult> command)
		{
			var nodes = GetNodesFor(command);
			if (nodes.Length == 1) return nodes[0].Execute(command);
			var commandResults = nodes.AsParallel().Select(n => n.Execute(command)).ToArray();
			return MergeResults(command, commandResults);
		}

        public override object Execute(Command command)
        {
            throw new NotImplementedException();
        }

        public override object Execute(Query query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Execute a query on a specific node
        /// </summary>
        public TResult Execute<TResult>(Query<TModel, TResult> query, int nodeIndex)
		{
			return Nodes[nodeIndex].Execute(query);
		}

        /// <summary>
        /// Execute on a specific node
        /// </summary>
        public void Execute(Command<TModel> command, int nodeIndex)
		{
			Nodes[nodeIndex].Execute(command);
		}

        /// <summary>
        /// Execute on a specific node
        /// </summary>
		public TResult Execute<TResult>(Command<TModel, TResult> command, int nodeIndex)
		{
			return Nodes[nodeIndex].Execute(command);
		}

        private Delegate GetDispatcherFor<T>(T obj)
        {
            var key = obj.GetType().Name;
            if (!_dispatchers.ContainsKey(key) || _dispatchers[key] == null)
            {
                return _allNodesDispatcher;
            }

            return _dispatchers[key];
        }

        private Func<R[], R> GetMergerFor<T, R>(T obj)
        {
            var key = obj.GetType().Name;
            if (_mergers.ContainsKey(key))
            {
                return (Func<R[], R>)_mergers[key];
            }

            key = typeof(R).Name;
            if (_mergers.ContainsKey(key))
            {
                return (Func<R[], R>)_mergers[key];
            }
            throw new ArgumentException("Merger for type not found", "obj");
        }

        private IEngine<TModel>[] GetNodesFor<T>(T obj)
        {
            var dispatcher = GetDispatcherFor(obj);
            var nodeIds = (int[])dispatcher.DynamicInvoke(obj);
            return nodeIds.Select(id => Nodes[id]).ToArray();
        }

        protected virtual R MergeResults<T, R>(T obj, R[] results)
        {
            var merger = GetMergerFor<T, R>(obj);
            return merger.Invoke(results);
        }
	}
}