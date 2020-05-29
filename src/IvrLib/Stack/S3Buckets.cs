using System;
using System.Collections.Generic;
using System.Linq;

namespace IvrLib
{
    public class S3Buckets
    {
        public string Suffix { get; set; } = "suffix.not.set";
        public IEnumerable<string> ListBucket { get; set; }
        public IEnumerable<string> GetObject { get; set; }
        public IEnumerable<string> PutObject { get; set; }
        public IEnumerable<string> DeleteObject { get; set; }
 
    }
}