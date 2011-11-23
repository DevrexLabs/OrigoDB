using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cms.Core
{
    [Serializable]
    public abstract class Markup
    {
        public abstract string RenderHtml();
    }
}
