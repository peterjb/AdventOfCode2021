//EX 1: 3923414
//EX 2: 5852595

//setup
var input = File.ReadLines("input.txt").ToArray();

var bitCount = input[0].Length;
var bitSums = new int[bitCount];

//count how many 1s are in each position
foreach(var s in input)
{
    for(var i = 0; i < bitCount; i++)
    {
        if(s[i] == '1')
        {
            bitSums[i]++;
        }
    }
}

//part 1

//problem doesn't appear to specify what to do if equal, but does not occur in data
//so this simple compare works fine. If it's greater or equal, then I think 
//midPoint = input.Length - (input.Length / 2) would work for odd counts
int gamma = 0, epsilon = 0;
var mask = 1 << (bitCount - 1);
int midPoint = input.Length / 2;
for (var j = 0; j < bitCount; j++)
{
    //construct the numbers
    if(bitSums[j] > midPoint)
    {
        gamma |= mask >> j;
    }
    else
    {
        epsilon |= mask >> j;
    }
}

Console.WriteLine($"Epsilon: {epsilon}; Gamma: {gamma}; Power: {epsilon*gamma}");

//part 2

//just keeping it simple and doing o2 and co2 separately
string[] o2matches = input, co2matches = input;

for(var k = 0; k < bitCount; k++)
{
    //for each bit position, starting to left, count the 1 bits in the list of remaining candidates
    var onesCount = o2matches.Sum(x => x[k] == '1' ? 1 : 0);
    //determine if we are looking for a 0 or 1
    char o2Matchbit =  onesCount >= o2matches.Length - onesCount ? '1' : '0';
    //create a new list of just the values that match the bit at that position
    o2matches = o2matches.Where(x => x[k] == o2Matchbit).ToArray();
    //if we are down to 1 candidate we can stop early
    if(o2matches.Length == 1)
    {
        break;
    }
}

for (var k = 0; k < bitCount; k++)
{
    //same as for o2, just different match criteria
    var onesCount = co2matches.Sum(x => x[k] == '1' ? 1 : 0);
    char co2Matchbit = onesCount < co2matches.Length - onesCount ? '1' : '0';
    co2matches = co2matches.Where(x => x[k] == co2Matchbit).ToArray();
    if (co2matches.Length == 1)
    {
        break;
    }
}

var o2 = Convert.ToInt32(o2matches[0], 2);
var co2 = Convert.ToInt32(co2matches[0], 2);

Console.WriteLine($"O2: {o2}; CO2:{co2}; Life Support Rating:{o2*co2}");