using System;
using Trilogy.Game.Corruption.AddressDB;

namespace Trilogy.Game.Corruption.Patches
{
    internal class IDOLPatch
    {
        internal virtual void Apply(Misc.FileFormats.DOL dol, IAddressDB addresses) { }
    }
}
