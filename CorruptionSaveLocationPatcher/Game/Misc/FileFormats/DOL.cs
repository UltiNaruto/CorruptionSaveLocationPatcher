using ppcasm_cs;
using System;
using System.IO;
using System.Linq;

namespace Trilogy.Game.Misc.FileFormats
{
    internal class DOL : Structs.BinaryStruct
    {
        internal static readonly UInt32 MemoryBaseAddress = 0x80000000;

        Int32[] TextSectionOffsets = null;
        Int32[] DataSectionOffsets = null;
        UInt32[] TextSectionAddresses = null;
        UInt32[] DataSectionAddresses = null;
        Int32[] TextSectionSizes = null;
        Int32[] DataSectionSizes = null;

        UInt32 BSSAddress;
        Int32 BSSSize;
        UInt32 EntryPointAddress;

        // Pad 32 bytes
        MemoryStream[] TextSections = null;
        MemoryStream[] DataSections = null;

        public UInt32[] TextSectionCursors = null;
        public UInt32[] DataSectionCursors = null;

        public DOL()
        {
            TextSectionOffsets = Enumerable.Repeat(0, 7).ToArray();
            DataSectionOffsets = Enumerable.Repeat(0, 11).ToArray();
            TextSectionAddresses = Enumerable.Repeat<UInt32>(0, 7).ToArray();
            DataSectionAddresses = Enumerable.Repeat<UInt32>(0, 11).ToArray();
            TextSectionSizes = Enumerable.Repeat(0, 7).ToArray();
            DataSectionSizes = Enumerable.Repeat(0, 11).ToArray();
            TextSections = new MemoryStream[7]
            {
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream()
            };
            DataSections = new MemoryStream[11]
            {
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream(),
                new MemoryStream()
            };
            TextSectionCursors = Enumerable.Repeat<UInt32>(0, 7).ToArray();
            DataSectionCursors = Enumerable.Repeat<UInt32>(0, 11).ToArray();
        }

        public bool IsInASection(String type, UInt32 address)
        {
            int i;

            if (type == "text")
            {
                for (i = 0; i < 7; i++)
                    if (TextSectionAddresses[i] >= address && TextSectionAddresses[i] + TextSectionSizes[i] <= address)
                        return true;
            }
            else
            {
                for (i = 0; i < 11; i++)
                    if (DataSectionAddresses[i] >= address && DataSectionAddresses[i] + DataSectionSizes[i] <= address)
                        return true;
            }
            return false;
        }

        public void AddSection(String type, UInt32 address, Int32 size)
        {
            int i, new_section_idx, len = type == "text" ? 7 : 11;
            for (new_section_idx = 0; new_section_idx < len; new_section_idx++)
            {
                if (len == 7 && TextSectionOffsets[new_section_idx] == 0)
                    break;
                if (len == 11 && DataSectionOffsets[new_section_idx] == 0)
                    break;
            }
            if (new_section_idx == len)
                throw new Exception($"No more space to add a {type} section!");

            var offset = 0;
            for (i = 0; i < 7; i++)
                if (TextSectionOffsets[i] + TextSectionSizes[i] > offset)
                    offset = TextSectionOffsets[i] + TextSectionSizes[i];
            for (i = 0; i < 11; i++)
                if (DataSectionOffsets[i] + DataSectionSizes[i] > offset)
                    offset = DataSectionOffsets[i] + DataSectionSizes[i];

            if (type == "text")
            {
                TextSectionOffsets[new_section_idx] = offset;
                TextSectionAddresses[new_section_idx] = address;
                TextSectionSizes[new_section_idx] = size;
                TextSections[new_section_idx].Position = 0;
                TextSections[new_section_idx].Write(Enumerable.Repeat<byte>(0, size).ToArray());
                TextSections[new_section_idx].Position = 0;
            }
            else if (type == "data")
            {
                DataSectionOffsets[new_section_idx] = offset;
                DataSectionAddresses[new_section_idx] = address;
                DataSectionSizes[new_section_idx] = size;
                DataSections[new_section_idx].Position = 0;
                DataSections[new_section_idx].Write(Enumerable.Repeat<byte>(0, size).ToArray());
                DataSections[new_section_idx].Position = 0;
            }
            else
                throw new Exception($"Section of type {type} is invalid!");
        }

        public void Patch(long address, params String[] asm)
        {
            Int32 sectionIdx = -1;
            Int32 fileAddress = GetSectionOffset((UInt32)address, ref sectionIdx);
            if (fileAddress == -1)
                throw new Exception($"Address not found in the DOL file (address => 0x{address:X8})");
            if (sectionIdx < 7)
                TextSections[sectionIdx].Position = fileAddress;
            else
                DataSections[sectionIdx - 7].Position = fileAddress;

            foreach (PPC_Instruction instruction in PPCASM.Parse((UInt32)address, asm))
            {
                if (sectionIdx < 7)
                    TextSections[sectionIdx].Write(instruction.Value, 0, instruction.Value.Length);
                else
                    DataSections[sectionIdx - 7].Write(instruction.Value, 0, instruction.Value.Length);
            }

            if (sectionIdx < 7)
                TextSections[sectionIdx].Position = 0;
            else
                DataSections[sectionIdx - 7].Position = 0;
        }

        Int32 GetSectionOffset(UInt32 memAddress, ref Int32 sectionIdx)
        {
            int i;
            if (memAddress < MemoryBaseAddress || memAddress > MemoryBaseAddress + 0x01FFFFFF)
                throw new Exception($"0x{memAddress:X8} is out of range in the MEM1 region");
            sectionIdx = 0;
            for (i = 0; i < 7; i++)
            {
                if (TextSectionOffsets[i] != 0)
                {
                    if (memAddress >= TextSectionAddresses[i] &&
                        memAddress < TextSectionAddresses[i] + TextSectionSizes[i])
                    {
                        return (Int32)((long)memAddress - (long)TextSectionAddresses[i]);
                    }
                }
                sectionIdx++;
            }
            for (i = 0; i < 11; i++)
            {
                if (DataSectionOffsets[i] != 0)
                {
                    if (memAddress >= DataSectionAddresses[i] &&
                        memAddress < DataSectionAddresses[i] + DataSectionSizes[i])
                    {
                        return (Int32)((long)memAddress - (long)DataSectionAddresses[i]);
                    }
                }
                sectionIdx++;
            }
            return -1;
        }

        public override int StructSize
        {
            get
            {
                int len = 256;
                foreach(var textSection in TextSections)
                {
                    len += (int)textSection.Length;
                }
                foreach (var dataSection in DataSections)
                {
                    len += (int)dataSection.Length;
                }
                return len;
            }
        }

        public override void import(Stream stream)
        {
            var i = 0;

            using (var reader = new BinaryReaderBE(stream))
            {
                // Read header
                for (i = 0; i < 7; i++)
                {
                    TextSectionOffsets[i] = reader.ReadInt32();
                }
                for (i = 0; i < 11; i++)
                {
                    DataSectionOffsets[i] = reader.ReadInt32();
                }
                for (i = 0; i < 7; i++)
                {
                    TextSectionAddresses[i] = reader.ReadUInt32();
                }
                for (i = 0; i < 11; i++)
                {
                    DataSectionAddresses[i] = reader.ReadUInt32();
                }
                for (i = 0; i < 7; i++)
                {
                    TextSectionSizes[i] = reader.ReadInt32();
                    TextSectionCursors[i] = (UInt32)TextSectionSizes[i];
                }
                for (i = 0; i < 11; i++)
                {
                    DataSectionSizes[i] = reader.ReadInt32();
                    DataSectionCursors[i] = (UInt32)DataSectionSizes[i];
                }

                BSSAddress = reader.ReadUInt32();
                BSSSize = reader.ReadInt32();
                EntryPointAddress = reader.ReadUInt32();

                // Read all text sections
                for (i = 0; i < 7; i++)
                {
                    if (TextSectionOffsets[i] != 0)
                    {
                        reader.BaseStream.Position = TextSectionOffsets[i];
                        TextSections[i].Write(reader.ReadBytes((int)TextSectionSizes[i]));
                        TextSections[i].Position = 0;
                    }
                }
                // Read all data sections
                for (i = 0; i < 11; i++)
                {
                    if (DataSectionOffsets[i] != 0)
                    {
                        reader.BaseStream.Position = DataSectionOffsets[i];
                        DataSections[i].Write(reader.ReadBytes((int)DataSectionSizes[i]));
                        DataSections[i].Position = 0;
                    }
                }
            }
        }

        public override void export(Stream stream)
        {
            var i = 0;

            using (var writer = new BinaryWriterBE(stream))
            {
                // Write Header
                for (i = 0; i < 7; i++)
                {
                    writer.Write(TextSectionOffsets[i]);
                }
                for (i = 0; i < 11; i++)
                {
                    writer.Write(DataSectionOffsets[i]);
                }
                for (i = 0; i < 7; i++)
                {
                    writer.Write(TextSectionAddresses[i]);
                }
                for (i = 0; i < 11; i++)
                {
                    writer.Write(DataSectionAddresses[i]);
                }
                for (i = 0; i < 7; i++)
                {
                    writer.Write(TextSectionSizes[i]);
                }
                for (i = 0; i < 11; i++)
                {
                    writer.Write(DataSectionSizes[i]);
                }

                writer.Write(BSSAddress);
                writer.Write(BSSSize);
                writer.Write(EntryPointAddress);
                // Write all text sections
                for (i = 0; i < 7; i++)
                {
                    if (TextSections[i].Length != 0)
                    {
                        writer.BaseStream.Position = TextSectionOffsets[i];
                        TextSections[i].Position = 0;
                        writer.Write(TextSections[i].GetBuffer(), 0, TextSectionSizes[i]);
                        TextSections[i].Position = 0;
                    }
                }
                // Write all data sections
                for (i = 0; i < 11; i++)
                {
                    if (DataSections[i].Length != 0)
                    {
                        writer.BaseStream.Position = DataSectionOffsets[i];
                        DataSections[i].Position = 0;
                        writer.Write(DataSections[i].GetBuffer(), 0, DataSectionSizes[i]);
                        DataSections[i].Position = 0;
                    }
                }
            }
        }
    }
}
