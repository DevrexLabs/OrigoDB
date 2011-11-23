using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cms.Core
{
    [Serializable]
    public class HtmlMarkup : Markup
    {
        string _html;

        public override string RenderHtml()
        {
            return _html;
        }

        public HtmlMarkup(string html)
        {
            if (html == null) throw new ArgumentNullException("html");
            _html = html;
        }
    }
}
