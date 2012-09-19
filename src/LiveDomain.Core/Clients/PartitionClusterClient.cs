using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Threading.Tasks;

namespace LiveDomain.Core
{
	public class PartitionClusterClient<M> : IEngine<M> where M :Model
	{
		public Dictionary<int, IEngine<M>> Clusters { get; internal set; }

		Dictionary<Type,object> _mergers = new Dictionary<Type,object>();
		Dictionary<Type, object> _dispatchers = new Dictionary<Type, object>();
		
		public PartitionClusterClient()
		{
			Clusters = new Dictionary<int, IEngine<M>>();
		}

		public void AddClusterNode(int id, IEngine<M> node)
		{
			Clusters.Add(id,node);
		}

		public void Register<T,R>(Func<T, int[]> dispatcher, Func<R[], R> responseMerger)
		{
			var key = typeof(T);
			_dispatchers[key] = dispatcher;
			_mergers[key] = responseMerger;
		}

		public void Register<T>(Func<T, int[]> dispatcher) where T : Command<M>
		{
			_dispatchers[typeof(T)] = dispatcher;
		}

		public Func<T, int[]> GetDispatcherFor<T>() 
		{
			var key = typeof(T);
			if (!_dispatchers.ContainsKey(key))
			{
				return (e) => Clusters.Keys.ToArray();// Select(c => c.Key).ToArray();
				//throw new InstanceNotFoundException("Dispatcher for type not found");
			}

			return (Func<T, int[]>)_dispatchers[key];
		}

		public Func<R[], R> GetMergerFor<T,R>()
		{
			var key = typeof (T);
			if(!_mergers.ContainsKey(key))
				throw new InstanceNotFoundException("Merger for type not found");

			return (Func<R[], R>) _mergers[key];
		}

		public IEngine<M>[] GetNodesFor<T>(T obj)
		{
			var dispatcher = GetDispatcherFor<T>();
			var nodes = dispatcher.Invoke(obj);
			return Clusters.Where(n => nodes.Contains(n.Key)).Select(n => n.Value).ToArray();
		}

		R MergeResults<T, R>(T obj, R[] results)
		{
			var merger = GetMergerFor<T, R>();
			return merger.Invoke(results);
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Implementation of IEngine<M>

		public T Execute<T>(Query<M, T> query)
		{
			var nodes = GetNodesFor(query);
			var queryResults = nodes.AsParallel().Select(n => n.Execute(query)).ToArray();
			var result = MergeResults(query, queryResults);
			return result;
		}

		public void Execute(Command<M> command)
		{
			Parallel.ForEach(GetNodesFor(command), node => node.Execute(command));
		}

		public T Execute<T>(CommandWithResult<M, T> command)
		{
			var nodes = GetNodesFor(command);
			var commandResults = nodes.AsParallel().Select(n => n.Execute(command)).ToArray();
			var result = MergeResults(command, commandResults);
			return result;
		}

		#endregion
	}
}