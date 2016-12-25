using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace sona
{
    class SearchEnc
    {
        public static string SearchEncoding(string url)
        {
            HtmlWeb web = new HtmlWeb();
            web.AutoDetectEncoding = false;
            HtmlAgilityPack.HtmlDocument document = web.Load(url);
            var headList = document.DocumentNode.SelectNodes("//meta");
            string str = "utf-8", s;
            foreach (var headLists in headList)
            {
                s = headLists.WriteTo();
                int strLI = s.LastIndexOf("charset");
                if (strLI != -1)
                {
                    string LI = s.Substring(strLI);
                    str = LI.Replace(" ", string.Empty);
                    str = str.Replace("charset=", string.Empty);
                    try
                    {
                        str = str.Replace("\">", string.Empty);
                    }
                    finally
                    {
                        str = str.Replace("\" />", string.Empty);
                    }
                }
            }
            return str;
        }

    }
}
