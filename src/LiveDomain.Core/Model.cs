using System;
using LiveDomain.Core.Security;
using System.Collections.Generic;

namespace LiveDomain.Core
{
    /// <summary>
    /// Derive your model from this class, mark it serializable.
    /// </summary>
    [Serializable]
    public abstract class Model : MarshalByRefObject
    {

        protected Dictionary<Type, Model> _childModels 
            = new Dictionary<Type, Model>();

        protected void AddChildModel(Model model)
        {
            _childModels.Add(model.GetType(), model);
        }

        public T ChildFor<T>() where T : Model
        {
            try
            {
                return (T)_childModels[typeof (T)];
            }
            catch (Exception)
            {
                throw new ArgumentException("No such child model", typeof(T).ToString());
            }
        }

        /// <summary>
        /// SnapshotRestored is called after the most recent snaphot has been loaded 
        /// but before any commands are restored.
        /// </summary>
        protected internal virtual void SnapshotRestored() { }
        
        /// <summary>
        /// This method is called after the model has been restored from 
        /// persistent storage and before the engine is available for transactions.
        /// </summary>
        protected internal virtual void JournalRestored() { }

    }
}
