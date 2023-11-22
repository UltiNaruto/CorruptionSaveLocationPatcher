using System.IO;
using System.Text;

namespace Trilogy.Game.Misc.Structs
{
    public class DiscInfo
    {
        DiscInfo() { }
        DiscInfo(string GameID, int Version) {
            this.GameID = GameID;
            this.Version = Version;
        }

        public readonly string GameID;
        public readonly int Version;
        public static DiscInfo Read(string path)
        {
            DiscInfo info = null;
            using(var reader = new BinaryReaderBE(File.OpenRead(path)))
            {
                info = new DiscInfo(
                    Encoding.ASCII.GetString(reader.ReadBytes(6)),
                    (int)reader.ReadUInt16()
                );
            }
            return info;
        }
    }
}
