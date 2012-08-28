using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveDomain.Core
{
    //TODO: make disposable, provide base implementationtry/finally and Exit in dispose?
    public interface ILockStrategy
    {
        void EnterRead();
        void EnterUpgrade();
        void EnterWrite();
        void Exit();
    }
}
