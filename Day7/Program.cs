var input = File.ReadAllLines("input.txt")[0].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();

//part1
//i think the median is probably the cheapest? seems to work
int median = SortOfMedian(input);
Console.WriteLine(input.Sum(x => Math.Abs(x - median)));

//part 2
//struggling to come up with a formula, so brute force it
//sum of n integers is n*(n+1)/2
//assume it's in between min and max
var x = Enumerable.Range(input.Min(), input.Max()).Select(i => 
{
    return input.Sum(x =>
    {
        var d = Math.Abs(x - i);
        return d * (d + 1) / 2;
    });
}).Min();

Console.WriteLine(x);

//err... sort of
static int SortOfMedian(IList<int> input)
{
    var sorted = input.OrderBy(x => x).ToArray();
    var count = input.Count();

    if (count % 2 == 0)
    {
        return (sorted[count / 2] + sorted[count / 2 - 1]) / 2;
    }
    else
    {
        return sorted[count / 2];
    }
}