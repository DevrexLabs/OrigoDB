using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Instrumentation;
using System.Reflection;
using System.Threading.Tasks;

namespace OrigoDB.Core
{
	public class PartitionClient<TModel> : ClusterClient<TModel> where TModel : Model
	{
		readonly Func<object, int[]> _allNodesDispatcher;
		Dictionary<string, Delegate> _dispatchers = new Dictionary<string, Delegate>();
		Dictionary<string, Delegate> _mergers = new Dictionary<string, Delegate>();

		public PartitionClient()
		{
			_allNodesDispatcher = _ => Enumerable.Range(0, Nodes.Count).ToArray();
		}

		public void SetDispatcherFor<T>(Func<T, int> dispatcher)
		{
			var key = typeof(T).Name;
			var func = new Func<T, int[]>(o => new[] { dispatcher.Invoke(o) });

			_dispatchers[key] = func;
		}

		public void SetDispatcherFor<T>(Func<T, int[]> dispatcher)
		{
			var key = typeof(T).Name;
			_dispatchers[key] = dispatcher;
		}

		public void SetMergerFor<T, R>(Func<R[], R> merger)
		{
			var key = typeof(T).Name;
			_mergers[key] = merger;
		}

		public void SetMergerFor<R>(Func<R[], R> merger)
		{
			var key = typeof(R).Name;
			_mergers[key] = merger;
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
			throw new ArgumentException("obj", "Merger for type not found");
		}

		private IEngine<TModel>[] GetNodesFor<T>(T obj)
		{
			var dispatcher = GetDispatcherFor(obj);
			var nodeIds = (int[])dispatcher.DynamicInvoke(obj);
			return nodeIds.Select(id => Nodes[id]).ToArray();
		}

		private R MergeResults<T, R>(T obj, R[] results)
		{
			var merger = GetMergerFor<T, R>(obj);
			return merger.Invoke(results);
		}


		
		public override TResult Execute<TResult>(Query<TModel, TResult> query)
		{

			var nodes = GetNodesFor(query);
			if(nodes.Length == 1) return nodes[0].Execute(query);

			var queryResults = nodes.AsParallel().Select(n => n.Execute(query)).ToArray();
			return MergeResults(query, queryResults);
		}

		public override void Execute(Command<TModel> command)
		{
			Parallel.ForEach(GetNodesFor(command), node => node.Execute(command));
		}

		public override TResult Execute<TResult>(Command<TModel, TResult> command)
		{
			var nodes = GetNodesFor(command);
			if (nodes.Length == 1) return nodes[0].Execute(command);
			var commandResults = nodes.AsParallel().Select(n => n.Execute(command)).ToArray();
			return MergeResults(command, commandResults);
		}

		public TResult Execute<TResult>(Query<TModel, TResult> query,int partition)
		{
			return Nodes[partition].Execute(query);
		}

		public void Execute(Command<TModel> command, int partition)
		{
			Nodes[partition].Execute(command);
		}

		public TResult Execute<TResult>(Command<TModel, TResult> command, int partition)
		{
			return Nodes[partition].Execute(command);
		}
	}
}