using System.Text.RegularExpressions;

/* Day 19
 * Got stuck on this one when my solution worked for sample, but not for the input. Made a dumb mistake
 * and wasn't inverting the rotation matrix when necessary, only the translation. This is also one 
 * where i end up second guessing what i'm doing because it seems too complex.
 */

var input = File.ReadAllLines("input.txt");

var scanners = new List<Scanner>();

for (var i = 0; i < input.Length; i++)
{
    var scanner = new Scanner();
    scanner.Id = int.Parse(Regex.Match(input[i], "\\d+").Value);
    while (true && ++i < input.Length)
    {
        if (string.IsNullOrEmpty(input[i]))
        {
            break;
        }
        var coords = input[i].Split(',');
        scanner.Beacons.Add((int.Parse(coords[0]), int.Parse(coords[1]), int.Parse(coords[2])));
    }
    scanners.Add(scanner);
}

//To match overlapping scanners, compute each beacons distance to each other beacon in the scanners beacon set
//Because scanners overlap by 12 or more beacons, and the distances between those beacons is independent of coordinate system
//we can check to see which beacons have intersecting distance lists
foreach (var scanner in scanners)
{
    foreach (var beacon in scanner.Beacons)
    {
        var distancesSquared = new List<int>();
        foreach (var b in scanner.Beacons)
        {
            if (b != beacon)
            {
                var dx = b.x - beacon.x;
                var dy = b.y - beacon.y;
                var dz = b.z - beacon.z;
                distancesSquared.Add(dx * dx + dy * dy + dz * dz);
            }
        }
        scanner.DistancesSquared.Add(beacon, distancesSquared);
    }
}

//just used for debugging purposes, a list of all the overlapping scanners and which beacons overlap
var overlaps = new Dictionary<(int i, int j), List<((int xi, int yi, int zi), (int xj, int yj, int zj))>>();

//when 2 scanners overlap, we compute a transform to map the coordinates in j to coordinates in i and store here
var transforms = new Dictionary<(int i, int j), transform>();

//compare each scanner with each other scanner, looking for overlaps
for (var i = 0; i < scanners.Count() - 1; i++)
{
    for (var j = i + 1; j < scanners.Count(); j++)
    {
        //we can tell if 2 beacons from different scanners are the same if we compare their distances to other beacons
        //if 2 beacons have 11+ distances to other beacons in ther scanner set that are the same, then the beacons are the same
        //and the 2 scanners overlap
        var os = scanners[i].DistancesSquared
            .Select(iScannerDistances2 => (beacon1: iScannerDistances2.Key, beacon2: scanners[j].DistancesSquared //select the beacon from i and any beacon from j
                .Where(jScannerDistances2 => jScannerDistances2.Value.Intersect(iScannerDistances2.Value).Count() >= 11) //where they have 11 or more distances in common
                .Select(jScannerDistances2 => jScannerDistances2.Key).FirstOrDefault()))
            .Where(overlap => overlap.beacon2 != (0, 0, 0)).ToList(); //get rid of the defaults, there has to be a better way to do this

        if (os.Count() > 0)
        {
            overlaps.Add((i, j), os);

            //to find the relative orientation, calculate a vector between the first and second beacons in each coordinate system
            //since the systems are only rotated in 90 degree on axis intervals and translated, the components of the vectors
            //should have equal magnitudes, just swizzled, so find out which components are which and construct a rotation matrix
            var ds1 = (dx: os[0].beacon2.x - os[1].beacon2.x, dy: os[0].beacon2.y - os[1].beacon2.y, dz: os[0].beacon2.z - os[1].beacon2.z);
            var ds2 = (dx: os[0].beacon1.x - os[1].beacon1.x, dy: os[0].beacon1.y - os[1].beacon1.y, dz: os[0].beacon1.z - os[1].beacon1.z);

            var roationMatrix = new int[3, 3];
            //for each axis, check which axis it ends up rotating to 

            //x axis
            if (Math.Abs(ds2.dx) == Math.Abs(ds1.dy))
            {
                var tval = ds2.dx / ds1.dy;
                roationMatrix[0, 1] = tval;
            }
            else if (Math.Abs(ds2.dx) == Math.Abs(ds1.dz))
            {
                var tval = ds2.dx / ds1.dz;
                roationMatrix[0, 2] = tval;
            }
            else
            {
                roationMatrix[0, 0] = ds2.dx / ds1.dx;
            }

            //y axis
            if (Math.Abs(ds2.dy) == Math.Abs(ds1.dx))
            {
                var tval = ds2.dy / ds1.dx;
                roationMatrix[1, 0] = tval;
            }
            else if (Math.Abs(ds2.dy) == Math.Abs(ds1.dz))
            {
                var tval = ds2.dy / ds1.dz;
                roationMatrix[1, 2] = tval;
            }
            else
            {
                roationMatrix[1, 1] = ds2.dy / ds1.dy;
            }

            //z axis
            if (Math.Abs(ds2.dz) == Math.Abs(ds1.dx))
            {
                var tval = ds2.dz / ds1.dx;
                roationMatrix[2, 0] = tval;
            }
            else if (Math.Abs(ds2.dz) == Math.Abs(ds1.dy))
            {
                var tval = ds2.dz / ds1.dy;
                roationMatrix[2, 1] = tval;
            }
            else
            {
                roationMatrix[2, 2] = ds2.dz / ds1.dz;
            }

            //to find how to translate the origin, we can now rotate a beacon in one coordinate system and find the vector from there to
            //the same beacon in the other coordinate system
            var rotatedBeacon2 = m.matmul(roationMatrix, os[0].beacon2);
            var translationVector = (os[0].beacon1.x - rotatedBeacon2.x, os[0].beacon1.y - rotatedBeacon2.y, os[0].beacon1.z - rotatedBeacon2.z);

            //store the transform as a rotation and translation
            transforms.Add((j, i), new transform() { rotation = roationMatrix, translation = translationVector });
        }
    }
}

/* Part 1: Find the size of the beacon set
 *   Once we have all the transformations from one set to another,
 *   we can find tranformations from any coordinate system to scanner 0's coordinate system.
 *   Once we can do that, just transform all the beacons to scanner 0's coordinate system and count the unique positions
 * 
 */

//Construct the full set of beacons
var beaconSet = new HashSet<(int x, int y, int z)>();

//first, add scanner 0's beacons, as it will be our reference coordinate system
foreach (var b in scanners.First().Beacons)
{
    beaconSet.Add(b);
}

//for each other scanner, find a tranformation path between it and the reference scanner (0), then apply that transformation to each beacon
//and add it to the set. The hashset will ignore duplicates
//also, transform the origin for part 2
foreach (var s in scanners.Skip(1))
{
    var transformList = getTransform(s.Id);

    s.TransformedPosition = transformBeacon((0, 0, 0), transformList);

    foreach (var b in s.Beacons)
    {
        var transformedBeacon = transformBeacon(b, transformList);
        s.TransformedBeacons.Add(b, transformedBeacon);
        beaconSet.Add(transformedBeacon);
    }
}


/* Part 2
 *   Just go through all the scanners finding the maximum manhatten distance between any 2 scanners
 */
long maxDistance = long.MinValue;

for(var i = 0; i < scanners.Count(); i++)
{
    for(var j = i + 1; j < scanners.Count(); j++)
    {
        maxDistance = Math.Max(maxDistance, m.mdist(scanners[i].TransformedPosition, scanners[j].TransformedPosition));
    }
}

Console.WriteLine($"Part 1: {beaconSet.Count()} beacons.");
Console.WriteLine($"Part 2: {maxDistance} units between farthest apart scanners.");


/*
 *  This takes a list of transformations (and whether to invert them or not) and applies them in order to a beacon
 * 
 */
(int x, int y, int z) transformBeacon((int x, int y, int z) beacon, List<(transform transform, bool invert, (int i, int j))> transformList)
{
    (int x, int y, int z) transformedBeacon = default;

    transformedBeacon = beacon;
    foreach (var t in transformList)
    {
        var (translation, rotation) = (t.transform.translation, t.transform.rotation);
        if (t.invert)
        {
            //the inverse of the rotation is just the transpose
            translation = (-translation.x, -translation.y, -translation.z);
            rotation = m.transpose(rotation);
            transformedBeacon = m.vadd(translation, transformedBeacon);
            transformedBeacon = m.matmul(rotation, transformedBeacon);
        }
        else
        {
            transformedBeacon = m.matmul(rotation, transformedBeacon);
            transformedBeacon = m.vadd(translation, transformedBeacon);
        }
    }
    return transformedBeacon;
}

/* this finds the list of transforms that will take us from scannerId's coordinate system to scanner 0's coordinate system
 * recursively looks through the list of transforms until it gets to 0. Nothing fancy, not trying to find the shortest path
 * this feels off, but it's working
 */
List<(transform transform, bool invert, (int i, int j))> getTransform(int scannerId)
{
    Stack<(transform, bool, (int i, int j))> st = new Stack<(transform, bool, (int i, int j))>();
    List<int> visited = new List<int>() { scannerId };

    thelper(scannerId, st, visited);

    return st.ToList();
}

bool thelper(int scannerId, Stack<(transform transform, bool invert, (int i, int j))> stack, List<int> visited)
{
    //first see if there's anyway to get to 0 from scannerId
    //we need to keep track of the direction of the transform
    if (transforms.ContainsKey((scannerId, 0)))
    {
        stack.Push((transforms[(scannerId, 0)], false, (scannerId, 0)));
        return true;
    }
    else if (transforms.ContainsKey((0, scannerId)))
    {
        stack.Push((transforms[(scannerId, 0)], true, (0, scannerId)));
        return true;
    }
    else
    {
        //if we don't have a path to 0 from here, search through all the transforms that either start at scannerId, or end at scannerId (which we will invert)
        //that don't go/start somewhere we've already been

        //first check starting at scannerId
        var candidates = transforms.Where(t => t.Key.i == scannerId && !visited.Contains(t.Key.j));
        foreach (var candidate in candidates)
        {
            visited.Add(candidate.Key.i);
            var found = thelper(candidate.Key.j, stack, visited);
            if (found)
            {
                stack.Push((candidate.Value, false, candidate.Key));
                return true;
            }
        }

        //If we are still here check ending at scannerId
        candidates = transforms.Where(t => t.Key.j == scannerId && !visited.Contains(t.Key.i));
        foreach (var candidate in candidates)
        {
            visited.Add(candidate.Key.j);
            var found = thelper(candidate.Key.i, stack, visited);
            if (found)
            {
                stack.Push((candidate.Value, true, candidate.Key));
                return true;
            }
        }

        //If we are still here we've hit a dead end
        return false;
    }
}

class Scanner
{
    public int Id;
    public (int x, int y, int z) TransformedPosition = default;
    public HashSet<(int x, int y, int z)> Beacons = new HashSet<(int x, int y, int z)>();
    public Dictionary<(int x, int y, int z), (int x, int y, int z)> TransformedBeacons = new Dictionary<(int x, int y, int z), (int x, int y, int z)>();
    public Dictionary<(int x, int y, int z), List<int>> DistancesSquared = new Dictionary<(int x, int y, int z), List<int>>();
}

//utilites
class transform
{
    public (int x, int y, int z) translation;
    public int[,] rotation;
}

public static class m
{
    public static (int x, int y, int z) vadd((int x, int y, int z) lhs, (int x, int y, int z) rhs)
    {
        return (lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
    }

    public static (int x, int y, int z) vsub((int x, int y, int z) lhs, (int x, int y, int z) rhs)
    {
        return (lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
    }

    public static long mdist((int x, int y, int z) lhs, (int x, int y, int z) rhs)
    {
        var v = m.vsub(lhs, rhs);
        return Math.Abs(v.x) + Math.Abs(v.y) + Math.Abs(v.z);
    }

    public static (int x, int y, int z) matmul(int[,] t, (int x, int y, int z) p)
    {
        return (t[0, 0] * p.x + t[0, 1] * p.y + t[0, 2] * p.z,
                t[1, 0] * p.x + t[1, 1] * p.y + t[1, 2] * p.z,
                t[2, 0] * p.x + t[2, 1] * p.y + t[2, 2] * p.z);
    }

    public static int[,] transpose(int[,] m)
    {
        var r = new int[3, 3];
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                r[i, j] = m[j, i];
            }
        }
        return r;
    }
}
