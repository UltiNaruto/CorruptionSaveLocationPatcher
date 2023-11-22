using Trilogy.Game.Corruption.AddressDB;

namespace Trilogy.Game.Corruption.Patches.DOL
{
    internal class SaveFileNamePatch : IDOLPatch
    {
        string SeedHash;

        public SaveFileNamePatch(string seedHash)
        {
            SeedHash = seedHash;
        }

        internal override void Apply(Misc.FileFormats.DOL dol, IAddressDB addresses)
        {
            /*var random = new Random(SeedHash.GetHashCode());
            var shortSeedHash = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8).Select(s => s[random.Next(s.Length)]).ToArray());*/
            // redirect to our save file path
            var load_save_redirect = 
                $$"""
                lis r3, 0x8000
                addi r3, r3, 0x2000
                stw r3, -0x5928(r2)
                b 0x{{addresses.GetAddress("GetSaveFileNameString", 0x4c):X8}}
                """.ReplaceLineEndings("\n").Split("\n");
            var load_save_redirect_size = load_save_redirect.Length * 4;
            if (load_save_redirect_size % 0x20 != 0)
                load_save_redirect_size += 0x20 - load_save_redirect_size % 0x20;

            if (!dol.IsInASection("text", 0x80001800))
                dol.AddSection("text", 0x80001800, load_save_redirect_size);

            dol.Patch(0x80001800, load_save_redirect);

            dol.Patch(addresses.GetAddress("GetSaveFileNameString", 0x48), $"b 0x80001800");

            // add our save file path
            if (!dol.IsInASection("data", 0x80002000))
                dol.AddSection("data", 0x80002000, 0x100);

            dol.Patch(0x80002000, $".ascii \"{SeedHash}_save.bin\"");
        }
    }
}
