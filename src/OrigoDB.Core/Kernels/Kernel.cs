using System;
using OrigoDB.Core.Logging;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core
{
    /// <summary>
    /// The kernel coordinates concurrent access to the
    /// model and executes commands and queries
    /// </summary>
    public abstract class Kernel
    {

        private static ILogger _log = LogProvider.Factory.GetLoggerForCallingType();

        protected Model _model;

        protected ISynchronizer _synchronizer;
        protected readonly ISerializer _serializer;

        /// <summary>
        /// Apply the command to the model and save it to the journal, 
        /// not necessarily in that order
        /// </summary>
        public abstract object ExecuteCommand(Command command);

        public virtual TResult ExecuteQuery<TModel, TResult>(Query<TModel, TResult> query) where TModel : Model
        {
            try
            {
                _synchronizer.EnterRead();
                object result = query.ExecuteStub(_model as TModel);
                EnsureSafeResults(ref result, query);
                return (TResult)result;
            }
            finally
            {
                _synchronizer.Exit();
            }
        }

        internal void SetSynchronizer(ISynchronizer synchronizer)
        {
            _synchronizer = synchronizer;
        }

        protected Kernel(EngineConfiguration config, Model model)
        {
            _serializer = config.CreateSerializer();
            _synchronizer = config.CreateSynchronizer();
            _model = model;
        }

        /// <summary>
        /// Make sure we don't return direct references to mutable objects within the model
        /// </summary>
        protected virtual void EnsureSafeResults(ref object result, IOperationWithResult operation)
        {
            if (result != null)
            {
                bool operationIsResponsible = operation != null && operation.ResultIsSafe;

                if (!operationIsResponsible && !result.IsImmutable())
                {
                    result = _serializer.Clone(result);
                }
            }
        }


        /// <summary>
        /// Provide synchronized read access to the model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readAction"></param>
        /// <returns></returns>
        public T Read<T>(Func<Model, T> readAction)
        {
            try
            {
                _synchronizer.EnterRead();
                return readAction.Invoke(_model);
            }
            finally
            {
                _synchronizer.Exit();
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
    }
}
