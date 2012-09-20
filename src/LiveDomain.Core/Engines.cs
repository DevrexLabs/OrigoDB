using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LiveDomain.Core
{
	public static class Engines
	{
		static ConcurrentDictionary<string, Engine> _engines = new ConcurrentDictionary<string, Engine>();
		
		public static IEnumerable<Engine> All { get { return _engines.Select(engine => engine.Value); } }

		internal static void AddEngine<M>(string identifier, Engine<M> engine) where M : Model
		{
			var key = typeof(M).Name + identifier;
			if (_engines.ContainsKey(key))
				throw new NotSupportedException();
			_engines[key] = engine;
		}

		internal static Engine<M> GetEngine<M>(string identifier) where M : Model
		{
			var key = typeof(M).Name + identifier;
			if (!_engines.ContainsKey(key))
				throw new NotSupportedException();
			return (Engine<M>)_engines[key];
		}

		internal static bool HasEngine<M>(string identifier) where M : Model
		{
			var key = typeof(M).Name + identifier;
			return _engines.ContainsKey(key);
		}
	}
}