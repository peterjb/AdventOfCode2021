/* WORK IN PROGRESS, DOES NOT WORK */
/* WORK IN PROGRESS, DOES NOT WORK */
/* WORK IN PROGRESS, DOES NOT WORK */

using System.Text.RegularExpressions;

var input = File.ReadAllLines("sample.txt");

var scanners = new List<Scanner>();


for(var i = 0; i < input.Length; i++)
{
    var scanner = new Scanner();
    scanner.Id = int.Parse(Regex.Match(input[i], "\\d+").Value);
    while(true && ++i < input.Length)
    { 
        if(string.IsNullOrEmpty(input[i]))
        {
            break;
        }
        var coords = input[i].Split(',');
        scanner.Beacons.Add((int.Parse(coords[0]), int.Parse(coords[1]), int.Parse(coords[2])));
    }
    scanners.Add(scanner);
}

foreach(var scanner in scanners)
{
    foreach(var beacon in scanner.Beacons)
    {
        var d2s = new List<int>();
        foreach(var b in scanner.Beacons)
        {
            if(b != beacon)
            {
                var dx = b.x - beacon.x; 
                var dy = b.y - beacon.y;
                var dz = b.z - beacon.z;
                d2s.Add(dx * dx + dy * dy + dz * dz);
            }
        }
        scanner.Distances2.Add(beacon, d2s);
    }
}

var overlaps = new Dictionary<(int i, int j), List<((int xi, int yi, int zi), (int xj, int yj, int zj))>>();
var transforms = new Dictionary<(int i, int j), int[,]>();
for(var i = 0; i < scanners.Count() - 1;i++)
{
    for (var j = i + 1; j < scanners.Count(); j++)
    {
        var os = scanners[i].Distances2
            .Select(id2 => (id2.Key, scanners[j].Distances2
                .Where(jd2 => jd2.Value.Intersect(id2.Value).Count() >= 11)
                .Select(jd22 => jd22.Key).FirstOrDefault()))
            .Where(overlap => overlap.Item2 != (0, 0, 0)).ToList();
        if(os.Count() > 0)
        {
            overlaps.Add((i, j), os);
            (int dx, int dy, int dz) ds1 = default, ds2 = default;
            for(var mz = 0; mz < os.Count()-1; mz++)
            {
                var t = new int[3, 4];
                ds1 = (os[mz].Item2.x - os[mz + 1].Item2.x, os[mz].Item2.y - os[mz + 1].Item2.y, os[mz].Item2.z - os[mz + 1].Item2.z);
                ds2 = (os[mz].Key.x - os[mz + 1].Key.x, os[mz].Key.y - os[mz + 1].Key.y, os[mz].Key.z - os[mz + 1].Key.z);
                //if(transforms.Any(t => t.Key.i == j))
                //{
                //    var temp = ds1;
                //    ds1 = ds2;
                //    ds2 = temp;
                //}
                //if (ds1.dx != ds1.dy && ds1.dy != ds1.dz && ds1.dz != ds1.dx)
                //break;

                //x transforms
                if (Math.Abs(ds2.dx) == Math.Abs(ds1.dy))
                {
                    var tval = ds2.dx / ds1.dy;
                    t[0, 1] = tval;
                }
                else if (Math.Abs(ds2.dx) == Math.Abs(ds1.dz))
                {
                    var tval = ds2.dx / ds1.dz;
                    t[0, 2] = tval;
                }
                else
                {
                    t[0, 0] = ds2.dx / ds1.dx;
                }

                //y transforms
                if (Math.Abs(ds2.dy) == Math.Abs(ds1.dx))
                {
                    var tval = ds2.dy / ds1.dx;
                    t[1, 0] = tval;
                }
                else if (Math.Abs(ds2.dy) == Math.Abs(ds1.dz))
                {
                    var tval = ds2.dy / ds1.dz;
                    t[1, 2] = tval;
                }
                else
                {
                    t[1, 1] = ds2.dy / ds1.dy;
                }

                //z transforms
                if (Math.Abs(ds2.dz) == Math.Abs(ds1.dx))
                {
                    var tval = ds2.dz / ds1.dx;
                    t[2, 0] = tval;
                }
                else if (Math.Abs(ds2.dz) == Math.Abs(ds1.dy))
                {
                    var tval = ds2.dz / ds1.dy;
                    t[2, 1] = tval;
                }
                else
                {
                    t[2, 2] = ds2.dz / ds1.dz;
                }


                var t1 = m.matmul(t, os[mz].Item2);
                t[0, 3] = os[mz].Key.x - t1.x;
                t[1, 3] = os[mz].Key.y - t1.y;
                t[2, 3] = os[mz].Key.z - t1.z;

                if (transforms.Any(t => t.Key.i == j))
                {
                    //transforms.Add((i, j), itz);
                }
                else
                {
                    //transforms.Add((j, i), t);
                }
            }

            
        }
    }
}

var beaconSet = new HashSet<(int x, int y, int z)>();

foreach(var b in scanners.First().Beacons)
{
    beaconSet.Add(b);
}

foreach (var s in scanners.Skip(1))
{
    var currentScanner = 0;
    var nextScanner = currentScanner;
    int[,] t;
    List<int> visited = new List<int>();
    (int x, int y, int z) transformedBeacon = default;
    foreach (var b in s.Beacons)
    {
        currentScanner = s.Id;
        nextScanner = currentScanner;
        visited.Clear();
        transformedBeacon = b;
        do
        {
            currentScanner = nextScanner;
            var transform = transforms.Where(t => t.Key.i == currentScanner && !visited.Contains(t.Key.j)).FirstOrDefault();
            t = transform.Value;
            nextScanner = transform.Key.j;
            visited.Add(transform.Key.i);
            transformedBeacon = m.matmul(t, transformedBeacon);
            if(currentScanner == s.Id)
            {
                s.TransformedBeacons.Add(transformedBeacon);
            }
        } while (nextScanner != 0);
        beaconSet.Add(transformedBeacon);
    }
}

var x = m.matmul(transforms[(1, 0)], overlaps.First().Value[0].Item2);
var xy = m.matmul(transforms[(1, 0)], overlaps.Skip(1).First().Value[0].Item2);

var xxy = m.matmul(transforms[(4, 1)], overlaps.Skip(2).First().Value[0].Item2);
var zzz = 0;


public static class m
{
    public static (int x, int y, int z) matmul(int[,] t, (int x, int y, int z) p)
    {
        return (t[0, 0] * p.x + t[0, 1] * p.y + t[0, 2] * p.z + t[0, 3],
                t[1, 0] * p.x + t[1, 1] * p.y + t[1, 2] * p.z + t[1, 3],
                t[2, 0] * p.x + t[2, 1] * p.y + t[2, 2] * p.z + t[2, 3]);
    }
}

class Scanner
{
    public int Id;
    public HashSet<(int x, int y, int z)> Beacons = new HashSet<(int x, int y, int z)>();
    public HashSet<(int x, int y, int z)> TransformedBeacons = new HashSet<(int x, int y, int z)>();
    public Dictionary<(int x, int y, int z), List<int>> Distances2 = new Dictionary<(int x, int y, int z), List<int>>();
}
