using System;

namespace Trilogy.Game.Corruption.AddressDB
{
    public abstract class IAddressDB
    {
        public abstract long GetFrontEndAddress(String symbol, Int32 offset = 0);
        public abstract long GetAddress(String symbol, Int32 offset = 0);
    }
}
