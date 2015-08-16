using System;
using System.Runtime.Serialization;
using OrigoDB.Core.Logging;

namespace OrigoDB.Core
{
    /// <summary>
    /// The kernel coordinates concurrent access to the
    /// model and executes commands and queries
    /// </summary>
    public abstract class Kernel
    {

        private static ILogger _log = LogProvider.Factory.GetLoggerForCallingType();

        protected readonly IsolationSettings Isolation;

        private Model _model;
        protected ISynchronizer Synchronizer;
        protected readonly IFormatter ResultFormatter;


        public abstract object ExecuteCommand(Command command);

        public virtual TResult ExecuteQuery<TModel, TResult>(Query<TModel, TResult> query) where TModel : Model
        {
            try
            {
                Synchronizer.EnterRead();
                object result = query.ExecuteStub(_model as TModel);
                EnsureIsolation(ref result, query);
                return (TResult)result;
            }
            finally
            {
                Synchronizer.Exit();
            }
        }

        internal void SetSynchronizer(ISynchronizer synchronizer)
        {
            Synchronizer = synchronizer;
        }

        protected Kernel(EngineConfiguration config, Model model)
        {
            ResultFormatter = config.CreateFormatter(FormatterUsage.Results);
            Synchronizer = config.CreateSynchronizer();
            _model = model;
            Isolation = config.Isolation;
        }

        /// <summary>
        /// Make sure we don't return references to mutable objects within the model
        /// </summary>
        protected virtual void EnsureIsolation(ref object result, IOperationWithResult operation)
        {
            if (result != null)
            {
                var strategy = Isolation.ReturnValues;
                strategy.Apply(ref result, operation);
            }
        }

        /// <summary>
        /// Provide synchronized read access to the model
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="readAction"></param>
        /// <returns></returns>
        public TResult Read<TResult>(Func<Model, TResult> readAction)
        {
            try
            {
                Synchronizer.EnterRead();
                return readAction.Invoke(_model);
            }
            finally
            {
                Synchronizer.Exit();
            }
        }

        /// <summary>
        /// Provides synchronized read access to the model
        /// </summary>
        /// <param name="readAction"></param>
        public void Read(Action<Model> readAction)
        {
            Read((m) =>
            {
                readAction.Invoke(m);
                return 0;
            });
        }

        public Model Model
        {
            get
            {
                return _model;
            }
            protected set { _model = value; }
        }
    }
}
