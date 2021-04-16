using System;
using System.IO;

namespace GlacierTextConverter.Model
{
    public class GlacierRtlvBinaryReader : BinaryReader
    {
        public GlacierRtlvBinaryReader(Stream stream, HitmanVersion version)
            : base(stream)
        {
        }

        public RtlvTextFile ReadFile()
        {
            var file = new RtlvTextFile();
            var cypherStrategy = new CypherStrategyTEA();

            BaseStream.Seek(24, SeekOrigin.Begin);

            var staticContext1Size = 0;
            var check = ReadInt32();
            if (check == 136)
            {
                staticContext1Size = 168;
            }
            else if (check == 120)
            {
                staticContext1Size = 140;
            }
            else
            {
                throw new Exception();
            }

            BaseStream.Seek(88, SeekOrigin.Begin);
            var startOfLanguageSections = ReadInt32() + 12;
            BaseStream.Seek(0, SeekOrigin.Begin);

            file.Header = ReadBytes(8);
            file.FileSize = this.ReadUInt32BE();
            file.StaticContext = ReadBytes(staticContext1Size);
            file.Identifier = ReadUInt32();
            file.StaticContext2 = ReadBytes(startOfLanguageSections - (int)BaseStream.Position);
            var numberOfLanguages = ReadUInt32();

            for (var i = 0; i < numberOfLanguages; i++)
            {
                var section = new RtvlLanguageSection();
                section.SectionLength = ReadInt16();
                section.Unknown = ReadInt16();
                section.Zeros = ReadInt32();
                section.StartingOffset = ReadInt32();
                section.Zeros2 = ReadInt32();
                file.Sections.Add(section);
            }

            for (var i = 0; i < numberOfLanguages; i++)
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
