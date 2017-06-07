using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierLocalizationTools.Model
{
    public class RpkgEditor
    {
        public RpkgFileStructure Archive { get; private set; }

        public void LoadRpgkFileStructure(RpkgBinaryReader reader)
        {
            Archive = reader.ReadHeader();
            
            for(int i = 0; i < Archive.NumberOfFiles; i++)
            {
                Archive.Entries.Add(reader.ReadEntry());
            }
            
        }
    }
}
