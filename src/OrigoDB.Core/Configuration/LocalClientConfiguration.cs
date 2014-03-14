namespace OrigoDB.Core
{
	public class LocalClientConfiguration : ClientConfiguration
	{
		private EngineConfiguration _engineConfiguration;

		public LocalClientConfiguration(EngineConfiguration engineConfiguration)
		{
			_engineConfiguration = engineConfiguration;
			CreateWhenNotExists = true;
		}

		public bool CreateWhenNotExists { get; set; }

		public override IEngine<TModel> GetClient<TModel>()
		{

		    var location = _engineConfiguration.Location;
			if(!location.HasJournal)
		    {
		        location.SetLocationFromType<TModel>();
		    }

            Engine engine;
		    if (!Config.Engines.TryGetEngine(location.OfJournal, out engine))
		    {
                if (CreateWhenNotExists) engine = Engine.LoadOrCreate<TModel>(_engineConfiguration);
                else engine = Engine.Load<TModel>(_engineConfiguration);
		    }
			return new LocalEngineClient<TModel>((Engine<TModel>)engine);
		}
	}
}