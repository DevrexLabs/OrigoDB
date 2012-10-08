using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    public static class Config
    {
        private static Engines _engines = new Engines();

        public static Engines Engines
        {
            get { return _engines; }
        }
    }
}
