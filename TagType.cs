using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Baka_Tsuki_Downloader
{
    static class TagType
    {
        public enum Type {h1, h2, h3, uncategorized};
        private static string h1 = "<h1>", h2 = "<h2>";
        public static Type getType(string tag)
        {
            if (Regex.IsMatch(tag, h1))
                return Type.h1;
            else if (tag.Equals(h2))
                return Type.h2;
            else
            return Type.uncategorized;
        }
    }
}
