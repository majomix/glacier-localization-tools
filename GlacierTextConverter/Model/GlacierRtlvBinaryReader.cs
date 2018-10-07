using System;
using System.IO;
using System.Text;

namespace GlacierTextConverter.Model
{
    public class GlacierRtlvBinaryReader : BinaryReader
    {
        public GlacierRtlvBinaryReader(FileStream fileStream, Encoding encoding)
            : base(fileStream, encoding)
        {
        }

        public RtlvTextFile ReadFile()
        {
            var file = new RtlvTextFile();
            var cypherStrategy = new CypherStrategyTEA();

            file.Header = ReadBytes(8);
            file.FileSize = this.ReadUInt32BE();
            file.StaticContext = ReadBytes(168);
            file.Identifier = ReadUInt32();
            file.StaticContext2 = ReadBytes(360);

            for (var i = 0; i < 12; i++)
            {
                var section = new RtvlLanguageSection();
                section.SectionLength = ReadInt16();
                section.Unknown = ReadInt16();
                section.Zeros = ReadInt32();
                section.StartingOffset = ReadInt32();
                section.Zeros2 = ReadInt32();
                file.Sections.Add(section);
            }

            for (var i = 0; i < 12; i++)
            {
                var length = ReadInt32();
                byte[] bytes = ReadBytes(length);

                if (length == 0)
                {
                    ReadInt32();
                }

                file.Sections[i].Lines.Add(cypherStrategy.Decypher(bytes));
            }

            return file;
        }
    }
}
