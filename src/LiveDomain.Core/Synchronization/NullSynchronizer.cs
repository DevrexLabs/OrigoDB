using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    public class NullSynchronizer : ISynchronizer
    {

        public void EnterRead()
        {
            
        }

        public void EnterUpgrade()
        {
            
        }

        public void EnterWrite()
        {
            
        }

        public void Exit() { }

    }
}
