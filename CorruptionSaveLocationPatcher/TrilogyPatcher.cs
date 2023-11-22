using System;
using System.IO;
using Trilogy.Game.Misc.Structs;

namespace Trilogy
{
    public class TrilogyPatcher
    {
        public static void Patch(string inputPath, string seedHash)
        {
            var patches = new Game.Corruption.Patches.Patches();
            patches.Init(inputPath, seedHash);
            patches.Apply();

            Console.WriteLine("Done!");
        }

        static void Usage()
        {
            Console.Write(
                """
                Usage:
                    .\CorruptionSaveLocationPatch <extracted iso path> <seed hash>
                """
            );
        }

        static void Main(String[] args)
        {
            if (args.Length != 2)
            {
                Usage();
                return;
            }
            var inputPath = Path.GetFullPath(args[0]);
            var seedHash = args[1];
            if(!Directory.Exists(inputPath))
            {
                Console.WriteLine($"{inputPath} doesn't exist!");
                return;
            }
            Patch(inputPath, seedHash);
        }
    }
}