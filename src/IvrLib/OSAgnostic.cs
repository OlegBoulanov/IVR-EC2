using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace IvrLib
{
    public static class OSAgnostic
    {

        public static string Home
        {
            get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.GetEnvironmentVariable("USERPROFILE")  // not HOME, which may point to smth like U:\\
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Environment.GetEnvironmentVariable("HOME") 
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Environment.GetEnvironmentVariable("HOME") 
                : null;
        }
        public static string User
        {
            get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.GetEnvironmentVariable("USERNAME")
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Environment.GetEnvironmentVariable("USER")    // username, logname
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Environment.GetEnvironmentVariable("LOGNAME")
            : null;
        }
    }
}