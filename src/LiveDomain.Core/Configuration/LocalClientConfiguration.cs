namespace LiveDomain.Core
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

		public override IEngine<M> GetClient<M>()
		{

		    var location = _engineConfiguration.Location;
			if(!location.HasJournal)
		    {
		        location.SetLocationFromType<M>();
		    }

			Engine engine;
			if (CreateWhenNotExists) engine = Engine.LoadOrCreate<M>(_engineConfiguration);
			else engine = Engine.Load<M>(_engineConfiguration);
			return new LocalEngineClient<M>((Engine<M>)engine);
		}
	}
}