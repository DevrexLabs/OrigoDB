using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrigoDB.Core
{
    //TODO: make disposable, provide base implementationtry/finally and Exit in dispose?
    public interface ISynchronizer
    {
        void EnterRead();
        void EnterUpgrade();
        void EnterWrite();
        void Exit();
    }
}
