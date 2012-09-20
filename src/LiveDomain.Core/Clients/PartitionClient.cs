using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Instrumentation;
using System.Reflection;
using System.Threading.Tasks;

namespace LiveDomain.Core
{
	public class PartitionClient<M> : IEngine<M> where M : Model
	{
		readonly Func<object, int[]> _defaultPartitionDispatcher;
		Dictionary<string, MulticastDelegate> _dispatchers = new Dictionary<string, MulticastDelegate>();
		Dictionary<string, object> _mergers = new Dictionary<string, object>();
		Dictionary<int, IEngine<M>> _partitions;

		public PartitionClient()
		{
			_partitions = new Dictionary<int, IEngine<M>>();
			_defaultPartitionDispatcher = _ => _partitions.Keys.ToArray();
		}

		/// <summary>
		/// Set/Get Partition
		/// </summary>
		/// <param name="index">Partition ID</param>
		/// <returns>Engine/Client for partition</returns>
		public IEngine<M> this[int index]
		{
			get { return _partitions.ContainsKey(index) ? _partitions[index] : null; }
			set { _partitions[index] = value; }
		}

		public void SetPartition(int id, IEngine<M> client)
		{
			this[id] = client;
		}

		public IEngine<M> GetPartition(int id)
		{
			return this[id];
		}

		public void Register<T, R>(Func<T, int[]> dispatcher, Func<R[], R> responseMerger)
		{
			var key = typeof (T).Name;
			_dispatchers[key] = dispatcher;
			_mergers[key] = responseMerger;
		}

		public void Register<T>(Func<T, int[]> dispatcher) where T : Command<M>
		{
			_dispatchers[typeof (T).Name] = dispatcher;
		}

		public MulticastDelegate GetDispatcherFor<T>(T obj)
		{
			var key = obj.GetType().Name;
			if (!_dispatchers.ContainsKey(key))
			{
				return _defaultPartitionDispatcher;
				//or if we want to be hard ~> throw new InstanceNotFoundException("Dispatcher for type not found");
			}

			return _dispatchers[key];
		}

		public Func<R[], R> GetMergerFor<T, R>(T obj)
		{
			var key = obj.GetType().Name;
			if (!_mergers.ContainsKey(key))
				throw new InstanceNotFoundException("Merger for type not found");

			return (Func<R[], R>) _mergers[key];
		}

		public IEngine<M>[] GetNodesFor<T>(T obj)
		{
			var dispatcher = GetDispatcherFor(obj);
			var nodes = (int[]) dispatcher.DynamicInvoke(obj);
			return _partitions.Where(p => nodes.Contains(p.Key)).Select(n => n.Value).ToArray();
		}

		R MergeResults<T, R>(T obj, R[] results)
		{
			var merger = GetMergerFor<T, R>(obj);
			return merger.Invoke(results);
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
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