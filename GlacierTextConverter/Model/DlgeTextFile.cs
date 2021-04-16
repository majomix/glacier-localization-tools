using System;
using System.Collections.Generic;

namespace GlacierTextConverter.Model
{
    public class DlgeTextFile
    {
        public string Name { get; set; }
        public List<DlgeStructure> Structures { get; set; }
        public byte[] Extra { get; set; }
        public string Category { get; set; }

        public DlgeTextFile()
        {
            Structures = new List<DlgeStructure>();
        }
    }

    public class DlgeStructure
    {
        public UInt32 Identifier { get; set; }
        public Int32 Category { get; set; }
        public string[] Dialogues { get; set; }
        public bool MetaDataNegative { get; set; }
        public int ChainId { get; set; }
    }
}
