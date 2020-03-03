using System;
using System.Text;
using System.IO;
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
        public static string AsCloudFormationId(this string s)
        {
            return s.ReplaceCharsWith('_');
        }
        public static string AsWindowsFolder(this string s)
        {
            return s.ReplaceCharsWith('_', "\\/-.".ToArray());
        }
        public static string AsWindowsComputerName(this string s)
        {
            return s.ReplaceCharsWith('-');
        }
        public static string ReplaceCharsWith(this string s, char replaceWith, params char [] charsToKeep)
        {
            return s.Aggregate(new StringBuilder(), (a, c) => { a.Append(char.IsLetterOrDigit(c) || charsToKeep.Contains(c) ? c : replaceWith); return a; }).ToString();
        }
    }
}