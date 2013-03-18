using System;
using OrigoDB.Core.Security;
using System.Collections.Generic;

namespace OrigoDB.Core
{
    /// <summary>
    /// Derive your model from this class, mark it serializable.
    /// </summary>
    [Serializable]
    public abstract class Model : MarshalByRefObject
    {

        private Dictionary<Type, Model> _children
            = new Dictionary<Type, Model>();

        protected IDictionary<Type, Model> Children
        {
            get
            {
                if (_children == null) _children = new Dictionary<Type, Model>();
                return _children;
            }
        }

        protected void AddChild(Model model)
        {
            _children.Add(model.GetType(), model);
        }

        /// <summary>
        /// Get the child model of type M, creating one if it doesn't exists
        /// </summary>
        public virtual M ChildFor<M>() where M : Model
        {
            //TODO: design review. use SnapshotRestored?
            try
            {
                return (M)_children[typeof(M)];
            }
            catch (NullReferenceException)
            {
                //Child modules where introduced with version 0.8.0
                if (_children == null) _children = new Dictionary<Type, Model>();
                else throw;
                return ChildFor<M>();
            }
            catch (KeyNotFoundException)
            {
                _children.Add(typeof(M), Activator.CreateInstance<M>());
                return ChildFor<M>();
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
