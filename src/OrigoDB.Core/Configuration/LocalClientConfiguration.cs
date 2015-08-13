namespace OrigoDB.Core
{
	public class LocalClientConfiguration : ClientConfiguration
	{
		private readonly EngineConfiguration _config;

		public LocalClientConfiguration(EngineConfiguration config)
		{
			_config = config;
			CreateWhenNotExists = true;
		}

		public bool CreateWhenNotExists { get; set; }

		public override IEngine<TModel> GetClient<TModel>()
		{
			if(_config.JournalPath == null)
			{
			    _config.JournalPath = _config.JournalPath ?? typeof (TModel).Name;
			}

            Engine engine;
		    if (!Config.Engines.TryGetEngine(_config.JournalPath, out engine))
		    {
                if (CreateWhenNotExists) engine = Engine.LoadOrCreate<TModel>(_config);
                else engine = Engine.Load<TModel>(_config);
		    }
			return new LocalEngineClient<TModel>((Engine<TModel>)engine);
		}
	}
}