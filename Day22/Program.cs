using System.Text.RegularExpressions;

var input = File.ReadAllLines("input.txt");

part1();
part2();

//Part 1 just brute force runs through each line of input, considering only the range -50 to 50, setting or unsetting cubes
//(adding or removing them from a set)
void part1()
{
    var cubes = new HashSet<(int x, int y, int z)>();
    foreach (var line in input)
    {
        var inst = line.Split(' ');
        var coords = Regex.Replace(inst[1], ".=", "").Split(',');

        var x = coords[0].Split('.', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
        var y = coords[1].Split('.', StringSplitOptions.RemoveEmptyEntries).Select(y => int.Parse(y)).ToArray();
        var z = coords[2].Split('.', StringSplitOptions.RemoveEmptyEntries).Select(z => int.Parse(z)).ToArray();

        for (var i = Math.Max(-50, x[0]); i <= Math.Min(50, x[1]); i++)
        {
            for (var j = Math.Max(-50, y[0]); j <= Math.Min(50, y[1]); j++)
            {
                for (var k = Math.Max(-50, z[0]); k <= Math.Min(50, z[1]); k++)
                {
                    if (cubes.Contains((i, j, k)))
                    {
                        if (inst[0] == "off")
                            cubes.Remove((i, j, k));
                    }
                    else if (inst[0] == "on")
                    {
                        cubes.Add((i, j, k));
                    }
                }
            }
        }
    }
    Console.WriteLine($"Part 1: {cubes.Count()} cubes on.");
}


/*
 * Part 2 involves using the entire space of cuboids, so the brute force method won't work (too many cuboids)
 * The idea is to look at it kind of like a CSG (Constructive Solid Geometry) problem. So start with the first rule as the initial volume of cuboids turned on.
 * If the next rule is to turn on another volume of cuboids, then perform a union of the rule cuboid and the first cuboid we've already turned on. 
 * If the rule is to turn off a volume cuboids, extract that rule cuboid out of the existing cuboid. The union and extract operations return the resulting volume
 * as a list of the resulting cuboids. Then apply the next rule to that list, returning an updated volume. Continue until no more rules.
 * 
 * This is efficient (maybe not how i did it, but in general) because we are keeping track of volumes of cuboids, not individual cubes.
 * 
 * It turns out union and extract can be done in the same steps because in both cases we can remove any cuboids that are contained in the rule.
 * In the union case we then just add the rule to the list of cuboids at the end
 * 
 */
void part2()
{
    //get all the rules
    var rules = input.Select(l => Cuboid.getFromLine(l)).ToArray();

    //initialize the volume of cuboids with the first rule (assuming it is an "on" rule because an "off" rule would be pointless)
    IEnumerable<Cuboid> cuboids = new List<Cuboid>() { rules.First() }; //first rule should be a cuboid

    //for every rule, update the cuboid list by performing a union (or extraction) of the rule on the list
    foreach (var rule in rules.Skip(1))
    {
        cuboids = Union(rule, cuboids);
    }

    //once we have our final volume, spit out the total size
    Console.WriteLine($"Part 2: {cuboids.Aggregate(0L, (s, c) => s + c.Size)} cubes on.");
}

/*
 * For each cuboid in the current list of cuboids:
 *  Does the rule contain the cuboid? If so we can remove it from our list
 *    If the rule is "on", then it will be included within the rule
 *    If the rule is "off", then it will be turned off and should not be included
 *  Does the rule not contain the cuboid? If so, add it to our list
 *    Whether the rule is "on" or "off" this cuboid remains in our volume of "on" cuboids
 *  Otherwise, they overlap
 *    Brute force: look at the cuboid that cointains both the rule and the current cuboid, split by the 
 *    boundaries of the 2 into 27 sub-cuboids
 *      For each of those sub cuboids, if that sub cuboid is only in the cuboid we are looking at and not the rule, add it to the list
 *      Similarly to above, if the sub cuboid is in the rule, we exclude it
 *      There will be some sub-cuboids that are not in either, ignore them
 *      
 *      2d picture - plane broken up into 9 areas
 *      
 *        x1      x2  x3 x4
 *     y1 - - - - c c c -    This area (x2-x3,y1-y2) is kept because it's not in the rule
 *        - - - - c c c -
 *     y2 r r r r c c c r   *These c's are part of r, so they should be ignored
 *        r r r r c c c r
 *     y3 r r r r c c c r
 *        r r r r r r r r
 *     y4 r r r r r r r r
 *     
 * Once we are done with the cuboids, if it is an "on" rule, add it to the list because we excluded and cuboids that were contained in the rule.
 * This way, an "on" and "off" rule are basically the samee operation. I think this also reduces the amount of cuboids we produce.
 * Then we are done this round.
 */
IEnumerable<Cuboid> Union(Cuboid rule, IEnumerable<Cuboid> cuboids)
{
    List<Cuboid> result = new List<Cuboid>();
    foreach (var c in cuboids)
    {
        switch (rule.Contains(c))
        {
            case 1:
                //cuboid is in the rule, so ignore
                continue;
            case -1:
                //cuboid is not in the rule, so add to list
                result.Add(c);
                break;
            case 0:
                //cuboid is partially in the rule, so break up
                //xs is the sorted list of all the x-coordinates of the two volumes, etc.
                //this will generate at most a set of 27 sub-cuboids to check
                var xs = rule.X.Union(c.X).OrderBy(c => c).ToArray();
                var ys = rule.Y.Union(c.Y).OrderBy(c => c).ToArray();
                var zs = rule.Z.Union(c.Z).OrderBy(c => c).ToArray();

                for (var i = 0; i < xs.Length - 1; i++)
                {
                    for (var j = 0; j < ys.Length - 1; j++)
                    {
                        for (var k = 0; k < zs.Length - 1; k++)
                        {
                            //I initially had some math here because i was struggling to get contains to work properly, but adjusting the intervals made it unecessary
                            if (c.Contains(xs[i], ys[j], zs[k]) && !rule.Contains(xs[i], ys[j], zs[k]))
                                //I struggled here, because the intervals are intially closed. Changing them to be closed-open fixed my issues.
                                result.Add(new Cuboid()
                                {
                                    X = new long[] { xs[i], xs[i + 1] },
                                    Y = new long[] { ys[j], ys[j + 1] },
                                    Z = new long[] { zs[k], zs[k + 1] },
                                    State = "on"
                                });
                        }
                    }
                }
                break;
        }
    }
    if (rule.State == "on")
        result.Add(rule);
    return result;
}

/* 
 *  This is to help work with the cuboids
 *  I couldn't figure out how to just work with the coordinates, so the model
 *  adds one to the range of each coordinate, so a cuboids x range is [x[0],x[1])
 *  when considering if a point is inside the cuboid 
 */

class Cuboid
{
    public string State { get; set; } = "bad";
    public long[] X { get; set; }
    public long[] Y { get; set; }
    public long[] Z { get; set; }

    private long? size = null;
    public long Size
    {
        get
        {
            if (size == null)
            {
                size = (X[1] - X[0]) * (Y[1] - Y[0]) * (Z[1] - Z[0]);
            }
            return size.Value;
        }
    }

    public bool Contains(double x, double y, double z)
    {
        return x >= X[0] && x < X[1] && y >= Y[0] && y < Y[1] && z >= Z[0] && z < Z[1];
    }

    //Returns 1 if c is inside (or equal to) this cuboid
    //       -1 if c is outside this cuboid
    //        0 if they otherwise overlap
    public int Contains(Cuboid c)
    {
        if (c.X[0] >= X[0] && c.X[1] <= X[1] && c.Y[0] >= Y[0] && c.Y[1] <= Y[1] && c.Z[0] >= Z[0] && c.Z[1] <= Z[1])
        {
            return 1;
        }
        else if ((c.X[0] < X[0] && c.X[1] < X[0]) || (c.X[0] > X[1] && c.X[1] > X[1]) ||
                 (c.Y[0] < Y[0] && c.Y[1] < Y[0]) || (c.Y[0] > Y[1] && c.Y[1] > Y[1]) ||
                 (c.Z[0] < Z[0] && c.Z[1] < Z[0]) || (c.Z[0] > Z[1] && c.Z[1] > Z[1]))
        {
            return -1;
        }
        return 0;
    }

    public static Cuboid getFromLine(string line)
    {
        var inst = line.Split(' ');
        var coords = Regex.Replace(inst[1], ".=", "").Split(',');

        var r = new Cuboid()
        {
            X = coords[0].Split('.', StringSplitOptions.RemoveEmptyEntries).Select(x => long.Parse(x)).ToArray(),
            Y = coords[1].Split('.', StringSplitOptions.RemoveEmptyEntries).Select(y => long.Parse(y)).ToArray(),
            Z = coords[2].Split('.', StringSplitOptions.RemoveEmptyEntries).Select(z => long.Parse(z)).ToArray(),
            State = inst[0]
        };

        //here's where we fudge the input to make the end coordinates exclusive
        r.X[^1] += 1;
        r.Y[^1] += 1;
        r.Z[^1] += 1;

        return r;
    }
}