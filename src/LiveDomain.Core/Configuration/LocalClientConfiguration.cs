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
			if(string.IsNullOrEmpty(_engineConfiguration.Location))
				_engineConfiguration.Location = typeof (M).Name;

			Engine engine;
			if (!Config.Engines.TryGetEngine(_engineConfiguration.Location, out engine))
			{
				if (CreateWhenNotExists) engine = Engine.LoadOrCreate<M>(_engineConfiguration);
				else engine = Engine.Load<M>(_engineConfiguration);
			}
			return new LocalEngineClient<M>((Engine<M>)engine);
		}
	}
}