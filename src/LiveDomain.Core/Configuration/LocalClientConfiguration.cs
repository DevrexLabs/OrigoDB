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
			Engine<M> engine;
			if (CreateWhenNotExists) engine = Engine.LoadOrCreate<M>(_engineConfiguration);
			else engine = Engine.Load<M>(_engineConfiguration);

			// Todo register engine instance with "engines"
			return new LocalEngineClient<M>(engine);
		}
	}
}