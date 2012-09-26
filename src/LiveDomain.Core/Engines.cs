using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LiveDomain.Core
{
	public class Engines
	{
		static Dictionary<string, Engine> _engines = new Dictionary<string, Engine>();

		public IEnumerable<Engine> All { get { return _engines.Select(kv => kv.Value); } }

		public void AddEngine(string identifier, Engine engine)
		{
			lock (_engines)
			{
				if (_engines.ContainsKey(identifier))
					throw new NotSupportedException();
				_engines[identifier] = engine;
			}
		}

		public Engine<M> GetEngine<M>(string identifier) where M : Model
		{
			lock (_engines)
			{
				if (!_engines.ContainsKey(identifier))
					throw new NotSupportedException();
				return (Engine<M>)_engines[identifier];
			}
		}

		public bool HasEngine(string identifier)
		{
			lock (_engines)
			{
				return _engines.ContainsKey(identifier);
			}
		}

		internal bool TryGetEngine(string identifier,out Engine engine)
		{
			return _engines.TryGetValue(identifier, out engine);
		}

		public void CloseAll()
		{
			lock (_engines)
			{
				foreach (var engine in _engines.Select(k => k.Value))
				{
					engine.Close();
				}
				_engines.Clear();
			}
		}

		internal void Close(Engine engine)
		{
			lock (_engines)
			{
				engine.Close();
				var keys = _engines.Where(kv => kv.Value == engine);
				foreach (var kv in keys)
				{
					_engines.Remove(kv.Key);
				}
			}
		}
	}
}