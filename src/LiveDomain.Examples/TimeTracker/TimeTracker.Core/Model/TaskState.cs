using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeTracker.Core
{
    [Serializable]
    public enum TaskState
    {
        NotStarted,
        Active,
        Completed,
        Failed

    }
}
