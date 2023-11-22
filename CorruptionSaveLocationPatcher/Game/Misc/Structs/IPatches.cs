using System;

namespace Trilogy.Game.Misc.Structs
{
    public abstract class IPatches
    {
        public abstract void Init(String inputPath, String seedHash);
        public abstract void Apply();
    }
}
