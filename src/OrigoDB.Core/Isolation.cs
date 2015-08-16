using System;

namespace OrigoDB.Core
{
    [Flags]
    public enum Isolation
    {
        Unknown = 0,
        Input = 1,
        Output = 2,
        InputOutput = 3
    }
}
