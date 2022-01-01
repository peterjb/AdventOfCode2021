/* so this is a mess - this is a brute forceish "solution"
 * when just runnning down from 999... i noticed that z was also going down for certain ranges
 * so the guess was we might be able to hone in on solutions by going through each digit position
 * and trying 1-9 and setting which made z the lowest, then moving to the next digit.
 * 
 * Letting that run (code below) spits out solutions. At first i was going left to right, but that
 * was only generating solutions that start with 5. Going right to left generates more solutions.
 * 
 * I did not get lucky and somehow find the max this way. 
 * 
 * But i did notice some obvious patterns. Solutions that start with 9 end in 5.
 * 8's end in 4. 5's end in 1. 7's end in 3. The 2nd digit is always 1 and the middle digit is always 9
 * 
 * So i ran the model numbers i found through the numberDist function to get the output below.
 * This seemed to indicate we could reduce the search space because only certain numbers were showing up
 * 
 * I decided to just try taking the max digit in each position and see if it was a solution:
 *     91699394894995 and it was the max
 * Did the same with the smallest numbers:
 *     51147191161261 and it was the min
 *  
 * Feels a bit lucky...
 * 
 * digit 0
 * 7: 1
 * 5: 4
 * 8: 18
 * 9: 36
 * 
 * digit 1
 * 1: 59
 * 
 * digit 2
 * 6: 7
 * 3: 10
 * 5: 10
 * 1: 10
 * 2: 10
 * 4: 12
 * 
 * digit 3
 * 9: 7
 * 6: 10
 * 8: 10
 * 4: 10
 * 5: 10
 * 7: 12
 * 
 * digit 4
 * 8: 19
 * 7: 20
 * 9: 20
 * 
 * digit 5
 * 2: 19
 * 1: 20
 * 3: 20
 * 
 * digit 6
 * 9: 59
 * 
 * digit 7
 * 4: 7
 * 2: 15
 * 3: 16
 * 1: 21
 * 
 * digit 8
 * 2: 1
 * 1: 1
 * 3: 4
 * 4: 6
 * 7: 6
 * 6: 9
 * 8: 12
 * 5: 20
 * 
 * digit 9
 * 7: 7
 * 8: 10
 * 6: 11
 * 9: 31
 * 
 * digit 10
 * 2: 7
 * 3: 10
 * 1: 11
 * 4: 31
 * 
 * digit 11
 * 3: 1
 * 2: 1
 * 4: 4
 * 5: 6
 * 8: 6
 * 7: 9
 * 9: 12
 * 6: 20
 * 
 * digit 12
 * 9: 7
 * 7: 15
 * 8: 16
 * 6: 21
 * 
 * digit 13
 * 3: 1
 * 1: 4
 * 4: 18
 * 5: 36
 */

var alu = new ALU();

alu.Instructions = File.ReadAllLines("input.txt").ToList();

var bestRun = long.MaxValue;
var bestest = long.MaxValue;
char[] ip = generateInput();
char best = ' ';
SortedSet<long> zeroes = new SortedSet<long>();

while (true)
{
    //starting from the left seemed to home in on the smaller model numbers, was always finding 5s
    //thats probably a clue, but i'm not sure what to do with that info
    //set this to i >= x for tyring specific starting digits
    for(var i = 13; i >= 0; i--)
    {
        //keep track of the lowest z for each possible number in this position
        bestRun = long.MaxValue;
        for(var j = 0; j < 9; j++)
        {
            ip[i] = (char)((int)'9' - j);

            Array.Copy(ip, alu.Input, 14);
            alu.Run();
            if(alu.z < bestRun)
            {
                bestRun = alu.z;
                best = ip[i];
            }

            if (alu.z == 0)
            {
                if (zeroes.Add(long.Parse(new string(ip))))
                {
                    Console.WriteLine(new string(ip));
                }
                break;

            }
            alu.Reset();
        }
        //set the digit to be whatever number gave us the lowest z and move to the next digit
        ip[i] = best;

        alu.Reset();
    }
    //if our best run this time through is better than our overall best, keep trying to find a lower z
    if (bestRun < bestest)
        bestest = bestRun;
    //otherwise, try a new random input
    else
    {
        bestest = long.MaxValue;
        ip = generateInput();
    }
    alu.Reset();
}

void numberDist()
{
    var nums = File.ReadAllLines("test.txt").Select(x => x.ToArray());

    var groups = new List<IGrouping<char, char[]>>[14];

    for (var i = 0; i < 14; i++)
    {
        Console.WriteLine($"digit {i}");
        var t = nums.GroupBy(x => x[i]).ToList();
        groups[i] = t;
        foreach (var g in t.OrderBy(z => z.Count()))
        {
            Console.WriteLine($"{g.Key}: {g.Count()}");
        }
        Console.WriteLine();
    }
}

char[] generateInput()
{
    var input = new char[14];
    //this was when i was testing for specific starting numbers
    input[0] = (char)Random.Shared.Next('1', '9' + 1); ;
    for(var i = 1; i < 14; i++)
    {
        input[i] = (char)Random.Shared.Next('1', '9' + 1);
    }
    return input;
}

class ALU
{
    public long w { get => variableMap['w']; }
    public long x { get => variableMap['x']; }
    public long y { get => variableMap['y']; }
    public long z { get => variableMap['z']; }

    private Dictionary<char, long> variableMap = new Dictionary<char, long>()
    {
        { 'w', 0 },
        { 'x', 0 },
        { 'y', 0 },
        { 'z', 0 }
    };

    public List<string> Instructions = new List<string>();

    private int inputIndex = 0;
    public char[] Input = new char[14];

    public void Run()
    {
        foreach(var i in Instructions)
        {
            var parts = i.Split(' ');
            if(parts.Length == 2)
            {
                inp(parts[1][0]);
            }
            else
            {
                long? v = null;
                char? b = null;
                if(variableMap.ContainsKey(parts[2][0]))
                {
                    b = parts[2][0];
                }
                else
                {
                    v = long.Parse(parts[2]);
                }
                switch (parts[0])
                {
                    case "add":
                        add(parts[1][0], b, v);
                        break;
                    case "mul":
                        mul(parts[1][0], b, v);
                        break;
                    case "div":
                        div(parts[1][0], b, v);
                        break;
                    case "mod":
                        mod(parts[1][0], b, v);
                        break;
                    case "eql":
                        eql(parts[1][0], b, v);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }

    public void Reset()
    {
        inputIndex = 0;
        variableMap['w'] = 0;
        variableMap['x'] = 0;
        variableMap['y'] = 0;
        variableMap['z'] = 0;
    }

    public void inp(char a)
    {
        variableMap[a] = (long)char.GetNumericValue(Input[inputIndex]);
        inputIndex++;
    }

    public void add(char a, char? b, long? v)
    {
        variableMap[a] = variableMap[a] + (b.HasValue ? variableMap[b.Value] : v.Value);
    }

    public void mul(char a, char? b, long? v)
    {
        variableMap[a] = variableMap[a] * (b.HasValue ? variableMap[b.Value] : v.Value);
    }
    public void div(char a, char? b, long? v)
    {
        variableMap[a] = variableMap[a] / (b.HasValue ? variableMap[b.Value] : v.Value);
    }

    public void mod(char a, char? b, long? v)
    {
        variableMap[a] = variableMap[a] % (b.HasValue ? variableMap[b.Value] : v.Value);
    }

    public void eql(char a, char? b, long? v)
    {
        variableMap[a] = variableMap[a] == (b.HasValue ? variableMap[b.Value] : v.Value) ? 1 : 0;
    }

    public override string ToString()
    {
        return $"w:{w}, x:{x}, y:{y}, z:{z}";
    }
}