using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{

    public class NullLockingStrategy : ILockStrategy
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
