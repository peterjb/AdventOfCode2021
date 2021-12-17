using System.Text;

var hexbinmap = new Dictionary<char, char[]>()
{
    {'0', new char[4] {'0','0','0','0'} },
    {'1', new char[4] {'0','0','0','1'} },
    {'2', new char[4] {'0','0','1','0'} },
    {'3', new char[4] {'0','0','1','1'} },
    {'4', new char[4] {'0','1','0','0'} },
    {'5', new char[4] {'0','1','0','1'} },
    {'6', new char[4] {'0','1','1','0'} },
    {'7', new char[4] {'0','1','1','1'} },
    {'8', new char[4] {'1','0','0','0'} },
    {'9', new char[4] {'1','0','0','1'} },
    {'A', new char[4] {'1','0','1','0'} },
    {'B', new char[4] {'1','0','1','1'} },
    {'C', new char[4] {'1','1','0','0'} },
    {'D', new char[4] {'1','1','0','1'} },
    {'E', new char[4] {'1','1','1','0'} },
    {'F', new char[4] {'1','1','1','1'} }
};

var input = new string(File.ReadAllLines("input.txt")[0].SelectMany(c => hexbinmap[c]).ToArray());

using (var packetStream = new StringReader(input))
{
    var packet = readPacket(packetStream);
    Console.WriteLine($"Version sum: {packet.VersionSum}");
    Console.WriteLine($"Packet value: {packet.Value}");
}

BITSPacket readPacket(StringReader r)
{
    BITSPacket p = new BITSPacket();
    (p.Version, p.TypeId) = readPacketHeader(r);

    switch (p.TypeId)
    {
        case 4:
            p.literalValue = readLiteralValuePacketBody(r);
            break;
        default:
            p.Subpackets = readOperatorPacketBody(r);
            break;
    }
    return p;
}

(int version, int typeId) readPacketHeader(StringReader r)
{
    var version = new char[3];
    r.ReadBlock(version, 0, 3);

    var typeId = new char[3];
    r.ReadBlock(typeId, 0, 3);

    return (Convert.ToInt32(new String(version), 2), Convert.ToInt32(new String(typeId), 2));
}

long readLiteralValuePacketBody(StringReader r)
{
    var groupCount = 0;
    var prefix = '0';
    var group = new char[4];
    var value = new StringBuilder();

    do
    {
        prefix = (char)r.Read();
        r.ReadBlock(group, 0, 4);
        value.Append(group);
        groupCount++;
    } while (prefix == '1');

    return Convert.ToInt64(value.ToString(), 2);
}

IEnumerable<BITSPacket> readOperatorPacketBody(StringReader r)
{
    char lengthType = (char)r.Read();

    switch (lengthType)
    {
        case '0':
            {
                var lengthBits = new char[15];
                r.ReadBlock(lengthBits, 0, 15);
                var length = Convert.ToInt32(new String(lengthBits), 2);

                var subPacketsBits = new char[length];
                r.ReadBlock(subPacketsBits, 0, length);

                var subPacketsReader = new StringReader(new string(subPacketsBits));

                return readPackets(subPacketsReader);
            }
        case '1':
            {
                var lengthBits = new char[11];
                r.ReadBlock(lengthBits, 0, 11);
                var length = Convert.ToInt32(new String(lengthBits), 2);

                var packets = new List<BITSPacket>();
                for (var i = 0; i < length; i++)
                {
                    packets.Add(readPacket(r));
                }

                return packets;
            }
        default:
            throw new Exception("unrecognized length type when parsing operator packet");
    }
}

IEnumerable<BITSPacket> readPackets(StringReader r)
{
    var packets = new List<BITSPacket>();
    while(r.Peek() != -1)
    {
        packets.Add(readPacket(r));
    }
    return packets;
}

struct BITSPacket
{
    public int Version;
    public int TypeId;
    public long? literalValue;
    public IEnumerable<BITSPacket>? Subpackets;

    public long? Value
    {
        get
        {
            switch (TypeId)
            {
                case 0:
                    return Subpackets?.Sum(p => p.Value);
                case 1:
                    return Subpackets?.Aggregate(1L, (long? acc, BITSPacket p) => acc * p.Value);
                case 2:
                    return Subpackets?.MinBy(p => p.Value).Value;
                case 3:
                    return Subpackets?.MaxBy(p => p.Value).Value;
                case 4:
                    return literalValue;
                case 5:
                    return (Subpackets?.First().Value > Subpackets?.Last().Value ? 1L : 0L);
                case 6:
                    return (Subpackets?.First().Value < Subpackets?.Last().Value ? 1L : 0L);
                case 7:
                    return (Subpackets?.First().Value == Subpackets?.Last().Value ? 1L : 0L);
                default:
                    throw new Exception();
            }
        }
    }

    public int VersionSum
    {
        get
        {
            return Version + (Subpackets?.Sum(p => p.VersionSum) ?? 0);
        }
    }
}