using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeTracker.Core
{
    [Serializable]
    public class Client
    {
        public string Name { get; set; }
        public List<Project> Projects { get; set; }
    }
}
