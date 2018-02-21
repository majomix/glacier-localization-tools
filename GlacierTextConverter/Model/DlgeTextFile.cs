using System;
namespace GlacierTextConverter.Model
{
    public class DlgeTextFile
    {
        public string Name { get; set; }
        public DlgeStructure Structure { get; set; }
    }

    public class DlgeStructure
    {
        public UInt32 Identifier { get; set; }
        public Int32 Category { get; set; }
        public string[] Dialogues { get; set; }
    }
}
