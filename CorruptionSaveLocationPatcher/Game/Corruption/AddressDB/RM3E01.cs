using System;

namespace Trilogy.Game.Corruption.AddressDB
{
    public class RM3E01 : IAddressDB
    {
        public override long GetFrontEndAddress(string symbol, int offset = 0)
        {
            throw new NotImplementedException();
        }

        public override long GetAddress(String symbol, Int32 offset)
        {
            UInt32 address = 0;
            switch(symbol)
            {
                case "GetSaveFileNameString":
                    address = 0x801d6674;
                    break;
                case "g_Save_FileName":
                    address = 0x8057d898;
                    break;
                case "g_Banner_FileName":
                    address = 0x8057d8a4;
                    break;
                case "sda_register":
                    address = 0x806801c0;
                    break;
                case "toc_register":
                    address = 0x806869c0;
                    break;
                default:
                    break;
            }
            if(address == 0)
                throw new Exception($"[Wii Standalone NTSC-U] Couldn't find address for symbol {symbol}!");
            return address + offset;
        }
    }
}
