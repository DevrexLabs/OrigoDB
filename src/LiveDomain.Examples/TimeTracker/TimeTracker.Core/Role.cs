using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeTracker.Core
{

    [Serializable]
    public class Role
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal DefaultHourlyRate { get; set; }
    }
}
