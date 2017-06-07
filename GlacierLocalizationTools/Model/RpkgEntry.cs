using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierLocalizationTools.Model
{
    public class RpkgEntry
    {
        public UInt64 Hash { get; set; }
        public UInt64 Offset { get; set; }
        public UInt32 Size { get; set; }
    }
}
