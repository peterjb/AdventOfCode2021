// Ex 1: 1713
// Ex 2: 1734

var input = File.ReadLines("input.txt")
    .Select(x => int.Parse(x));

//part 1 
var count1 = input.CountIncreases();

Console.WriteLine($"Increases: {count1}");

//part 2
//same as part1, except compare a rolling sum of values instead of the values themselves
var count2 = input.RollingSum(3)
    .Skip(2)
    .CountIncreases();

Console.WriteLine($"Rolling Sum Increases: {count2}");

public static class Extensions
{
    //use aggregate with tuple to keep track of previous value and count of increases
    //don't count the first one as an increase so compare initially to int.maxvalue
    public static int CountIncreases(this IEnumerable<int> ts)
    {
        return ts.Aggregate((previousValue: int.MaxValue, count: 0), ((int previousValue, int count) a, int value) =>
            (value, value > a.previousValue ? a.count + 1 : a.count)
        ).count;
    }

    //enumerate a rolling sum of `count` values
    //not sure if there's an equivalent using built in linq
    public static IEnumerable<int> RollingSum(this IEnumerable<int> ts, int count)
    {
        /* Keeps a record of intermediate sums
         * Not sure if it should automatically skip the first count - 1, right now it doesn't
         * Hasn't been tested outside of this exercise
         * 
         * Example with 3:
         * 
         *   List: 4,2,5,3
         *   Iter 1: 4,4,4 => 4 [4]
         *   Iter 2: 6,6,2 => 6 [4 + 2]
         *   Iter 3: 11,7,5 => 11 [4 + 2 + 5] (first full sum)
         *   Iter 4: 10,8,3 => 10 [2 + 5 + 3] (second full sum)
         * 
         */
        int[] vs = new int[count];
        int last = count - 1;
        foreach (var t in ts)
        {
            for (var i = 0; i < last; i++)
            {
                vs[i] = vs[i + 1] + t;
            }
            vs[last] = t;

            yield return vs[0];
        }
    }
}

