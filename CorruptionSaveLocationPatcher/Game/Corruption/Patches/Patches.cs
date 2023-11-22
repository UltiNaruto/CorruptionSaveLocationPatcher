using DiscInfo = Trilogy.Game.Misc.Structs.DiscInfo;
using GameVersion = Trilogy.Game.Corruption.Structs.Version;

using System;
using System.IO;
using System.Reflection;
using Trilogy.Game.Corruption.AddressDB;

namespace Trilogy.Game.Corruption.Patches
{
    public class Patches : Misc.Structs.IPatches
    {
        String CurrentDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public String BasePath { get; private set; }
        public String GameDolPath { get; private set; }
        public String[] FrontEndDolPath { get; private set; }
        public String SeedHash { get; private set; }
        GameVersion Version = GameVersion.NONE;
        IAddressDB Addresses = null;

        #region Getters
        public GameVersion GetVersion()
        {
            return Version;
        }
        #endregion

        public override void Init(String inputPath, String seedHash)
        {
            DiscInfo discInfo = DiscInfo.Read(Path.Combine(inputPath, "sys", "boot.bin"));
            // Check which version we have
            switch (discInfo.GameID)
            {
                case "RM3E01":
                    switch (discInfo.Version)
                    {
                        case 0:
                            Version = GameVersion.WII_STANDALONE_NTSC_U;
                            Addresses = new RM3E01();
                            break;
                        default:
                            throw new Exception("Unknown Wii Standalone NTSC-U revision (Rev " + discInfo.Version + ") supplied!");
                    }
                    BasePath = Path.Combine(inputPath, "files");
                    GameDolPath = Path.Combine(inputPath, "sys", "main.dol");
                    FrontEndDolPath = new String[] {
                        Path.Combine(inputPath, "sys", "main.dol")
                    };
                    break;
                case "RM3P01":
                    Reset();
                    throw new Exception("Wii Standalone PAL is not supported!");
                case "RM3J01":
                    Reset();
                    throw new Exception("Wii Standalone NTSC-J is not supported!");
                case "R3ME01":
                    Reset();
                    throw new Exception("Wii Trilogy NTSC-U is not supported!");
                case "R3MP01":
                    Reset();
                    throw new Exception("Wii Trilogy NTSC-U is not supported!");
                default:
                    Reset();
                    throw new Exception("Unknown game supplied!");
            }
            SeedHash = seedHash;
        }

        void PatchDOL()
        {
            Misc.FileFormats.DOL gameDOL = new Misc.FileFormats.DOL();
            gameDOL.import(File.OpenRead(GameDolPath));
            // game DOL patches
            foreach (var patch in new IDOLPatch[]
            {
                new DOL.SaveFileNamePatch(SeedHash),
            })
            {
                patch.Apply(gameDOL, Addresses);
            }
            gameDOL.export(File.Open(GameDolPath, FileMode.Truncate, FileAccess.Write));
        }

        public override void Apply()
        {
            // Apply dol patches
            Console.WriteLine("Applying MP3 DOL patches...");
            PatchDOL();
            // Reset globals
            Reset();
        }

        void Reset()
        {
            Version = GameVersion.NONE;
            Addresses = null;
            BasePath = String.Empty;
        }
    }
}