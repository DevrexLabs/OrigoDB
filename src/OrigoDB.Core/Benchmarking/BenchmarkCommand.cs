using System;

namespace OrigoDB.Core.Benchmarking
{
    [Serializable]
    public class BenchmarkCommand : Command<BenchmarkModel>
    {
        public readonly byte[] Payload;

        public BenchmarkCommand(int size)
        {
            Payload = new byte[size];
        }

        public override void Execute(BenchmarkModel model)
        {
            model.CommandsExecuted++;
            model.BytesWritten += Payload.Length;
        }
    }
}