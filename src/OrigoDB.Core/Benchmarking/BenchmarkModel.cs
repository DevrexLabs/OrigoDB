using System;

namespace OrigoDB.Core.Benchmarking
{
    [Serializable]
    public class BenchmarkModel : Model
    {
        public int CommandsExecuted;
        public long BytesWritten;
    }
}