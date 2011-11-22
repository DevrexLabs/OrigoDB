using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    public interface ILockStrategy
    {
        void EnterRead();
        void EnterUpgrade();
        void EnterWrite();
        void Exit();
    }
}
