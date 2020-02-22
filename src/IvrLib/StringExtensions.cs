using System;
using System.Collections.Generic;
using System.Linq;

namespace IvrLib
{
    public static class StringExtensions
    {
        public static IEnumerable<string> Csv(this string s)
        {
            return s.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
        }
    }
}