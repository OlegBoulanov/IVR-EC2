using System;
using System.IO;

namespace IvrLib
{
    public static class PathExtensions
    {
        public static string Home()
        {
            var home = Environment.GetEnvironmentVariable("HOME");
            if(string.IsNullOrWhiteSpace(home)) home = $"{Environment.GetEnvironmentVariable("HOMEDRIVE")}{Path.DirectorySeparatorChar}{Environment.GetEnvironmentVariable("HOMEPATH")}";
            return home;
        }
    }
}