using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core.Journaling;

namespace OrigoDB.Core.Storage
{
    /// <summary>
    /// Knows how to rebuild a model from snapshot and/or journal storage
    /// </summary>
    public class ModelLoader
    {

        readonly ICommandStore _commandStore;
        readonly ISnapshotStore _snapshotStore;


        public ModelLoader(EngineConfiguration config, ICommandStore commandStore = null, ISnapshotStore snapshotStore = null)
        {
            _commandStore = commandStore ?? config.CreateCommandStore();
            _snapshotStore = snapshotStore ?? config.CreateSnapshotStore();
        }

        public Model LoadModel(Type modelType = null)
        {
            Model model = null;

            //Try to load from the most recent snapshot
            var snapshotInfo = _snapshotStore.Snapshots.LastOrDefault();
            if (snapshotInfo != null)
            {
                model = _snapshotStore.LoadSnapshot(snapshotInfo);
                model.SnapshotRestored();
            }

            //If no snapshot present see if the first journal entry is of type ModelCreated
            if (model == null)
            {
                var firstJournalEntry = _commandStore.GetJournalEntries()
                    .Take(1)
                    .OfType<JournalEntry<ModelCreated>>()
                    .SingleOrDefault();

                if (firstJournalEntry != null) modelType = firstJournalEntry.Item.Type;
                if (modelType == null) throw new InvalidOperationException("No model type present");
                model = (Model)Activator.CreateInstance(modelType);
            }

            
            var ctx = Execution.Begin();
            //Replay commands
            foreach (var commandEntry in _commandStore.CommandEntriesFrom(model.Revision + 1))
            {
                ctx.Now = commandEntry.Created;
                commandEntry.Item.Redo(ref model);
                ctx.Events.Clear();
                model.Revision++;
            }
            model.JournalRestored();
            return model;
        }
    }
}