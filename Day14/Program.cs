//Day 14 - I struggled here, again, exercising the recursion muscle. Also memoization to the recue
using System.Text;

var input = File.OpenText("input.txt");

//read in the template
var template = input.ReadLine();
input.ReadLine();

//put the instructions into a dictionary mapping a pair of 2 elements to 1 element to insert in the chain
var instructions = new Dictionary<(char lhs, char rhs), char>();

while(!input.EndOfStream)
{
    var instruction = input.ReadLine();
    instructions[(instruction[0], instruction[1])] = instruction.Last();
}

/* Part 1
 * Follows the algorithm outlined in the problem to construct the chain and then perform the calculation
 * This only works well for smal numbers of steps, e.g. 10
 */

substitution(template);

void substitution(string template)
{
    var polymer = template;
    var sb = new StringBuilder();
    for (var i = 0; i < 10; i++)
    {
        for (var j = 0; j < polymer.Length - 1; j++)
        {
            var pair = polymer.Substring(j, 2);

            sb.Append(pair[0]);
            if (instructions.ContainsKey((polymer[j], polymer[j + 1])))
            {
                sb.Append(instructions[(polymer[j], polymer[j + 1])]);
            }
        }
        sb.Append(polymer.Last());
        polymer = sb.ToString();
        sb.Clear();
    }

    var polymerInfo = polymer.GroupBy(x => x).OrderBy(g => g.Count());
    Console.WriteLine(polymerInfo.Last().Count() - polymerInfo.First().Count());
}

/* Part 2
 * 
 * Too many steps to construct the polymer string, so build up the chain depth first, counting the inserted elements
 * instead of actually building the string
 * 
 * Doing it this way solves the problem of not being able to construct a chain of trillions of elements, however
 * the initial straight recursive way was going to take at least hours to complete.
 * 
 * So use memoization to speed things up. Each element of the memo represents the numbers of each element created from a pair of elements
 * at a certain step number. Recording these results and using them in future steps greatly speeds up the task.
 *  
 */

var stepCount = 40;
var memo = new Dictionary<(char, char, int), Dictionary<char, long>>();

counting(template);

/*
 * Each pair of elements is a self contained problem. Starting with the template, call step on each pair.
 */
void counting(string template)
{
    //this is a dictionary to hold the counts of each element in the chain, starting with the template
    var elementCounts = new Dictionary<char, long>();
    foreach (var c in template)
    {
        elementCounts.AddOrInc(c);
    }

    //merge the results from each pair in the template
    for (var i = 0; i < template.Length - 1; i++)
    {
        elementCounts.Merge(
            step(template[i], template[i + 1], 0)
        );
    }
    
    Console.WriteLine(elementCounts.Max(x => x.Value) - elementCounts.Min(x => x.Value));
}

/*
 * if we've already looked at a pair at this depth, just return it from the memo
 * otherwise, produce the inserted element, count it, and recurse down, running step on the lhs and the insert
 * and the insert and the rhs, mergin the results together and storing it in the memo.
 * 
 * pretty simple when i look at it now, but i struggled to reason it out for a while.
 */
Dictionary<char, long> step(char lhs, char rhs, int d)
{
    if (memo.ContainsKey((lhs, rhs, d)))
    {
        return memo[(lhs, rhs, d)];
    }
    else
    {
        var result = new Dictionary<char, long>();

        var c = instructions[(lhs, rhs)];
        result.AddOrInc(c);

        if (d < stepCount - 1)
        {
            result.Merge(step(lhs, c, d + 1));
            result.Merge(step(c, rhs, d + 1));
        }

        memo.Add((lhs, rhs, d), result);
        return result;
    }
}

//Dictionary helpers
static class Extensions
{
    public static void Merge(this Dictionary<char, long> lhs, Dictionary<char, long> rhs)
    {
        foreach (var pair in rhs)
        {
            if(lhs.ContainsKey(pair.Key))
            {
                lhs[pair.Key] += pair.Value;
            }
            else
            {
                lhs.Add(pair.Key, pair.Value);
            }
        }
    }

    public static void AddOrInc(this Dictionary<char, long> lhs, char c)
    {
        if(lhs.ContainsKey(c))
        {
            lhs[c]++;
        }
        else
        {
            lhs.Add(c, 1);
        }
    }
}