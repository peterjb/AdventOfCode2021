// Ex 1: 1713
// Ex 2: 1734

//part 1 

//use aggregate with tuple to keep track of previous value and count of increases
//don't count the first one as an increase so compare initially to int.maxvalue
var count = File.ReadLines("input.txt")
    .Select(x => int.Parse(x))
    .Aggregate((int.MaxValue, 0), (tuple, value) =>
        (value, value > tuple.Item1 ? tuple.Item2 + 1 : tuple.Item2)
    ).Item2;

Console.WriteLine($"Increases: {count}");

//part 2

//same as part1, except compare a rolling sum of values instead of the values themselves
var count2 = File.ReadLines("input.txt")
    .Select(x => int.Parse(x))
    .RollingSum(3)
    .Skip(2)
    .Aggregate((int.MaxValue, 0), (tuple, value) =>
        (value, value > tuple.Item1 ? tuple.Item2 + 1 : tuple.Item2)
    ).Item2;

Console.WriteLine($"Rolling Sum Increases: {count2}");

public static class Extensions
{
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
         *   List: 4,2,5,3,1,6
         *   Iter 1: 4,4,4 => 4 [4]
         *   Iter 2: 6,6,2 => 6 [4 + 2]
         *   Iter 3: 11,7,5 => 11 [4 + 2 + 5] (first full sum)
         *   Iter 4: 10,8,3 => 10 [2 + 5 + 3] (second full sum)
         *   Iter 5: 9,4,1 => 9 [5 + 3 + 1] (third full sum)
         *   Iter 6: 10,7,6 => 10 [3 + 1 + 6] (fourth and final full sum)
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

