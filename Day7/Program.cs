var input = File.ReadAllLines("input.txt")[0].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();

//part1
//i think the median is probably the cheapest? seems to work
int median = SortOfMedian(input); 
Console.WriteLine($"Part 1 Median - position: {input[median]}; cost: {input.Sum(x => Math.Abs(x - median))}");

//part 2
//struggling to come up with a formula, so brute force it
//sum of n integers is n*(n+1)/2
//assume it's in between min and max
//doing it this way because i want to know the position to move to for testing the forumla
var position = Enumerable.Range(input.Min(), input.Max()).MinBy(i =>
{
    return calcCost(input, i);
});

var cost = calcCost(input, position);

Console.WriteLine($"Part 2 Brute Force - position: {position}; cost: {cost}");

/* OK here we go for a formula...
 * want to minimize the sum of the cost function over the list of positions
 *
 * y(d) = sum(n * (n + 1) / 2) 
 *
 * n = abs(input[i] - x)
 * x is the position to move to
 * i = summation index
 * 
 * y(n) = sum( 1/2 * (n^2 + n) )
 * 
 * dy/dx = dy/dn*dn/dx = sum( 1/2 * (2*n + 1) ) * dn/dx
 *       = sum( n + 1/2 ) * dn/dx = (sum( n ) + N/2) * dn/dx
 *       = { sum( input[i] - x + 1/2 ) * -1 : i s.t. x > input[i], N1 = total indexes where this is true }
 *         + { sum( x - input[i] + 1/2 ) * 1  : i s.t. x < input[i], N2 = total indexes where this is true }
 *       = { N1(x-1/2) - sum( input[i] ) : i s.t. x > input[i] }
 *         + { N2(x+1/2) - sum( input[i] ) : i s.t. x < input[i] }
 *       = { (N1 + N2)x + (N2 - N1)/2 - sum( input[i] ) : for all i }
 *       = { Nx + (N2 - N1)/2 - sum( input[i]) }
 * set = 0 to find minimum
 *       x = sum( input[i] )/N + (N1 - N2)/2N
 * 
 * because (N1 - N2) < N, sum( input[i] )/N - 1/2 < x < sum( input[i] )/N + 1/2
 * 
 * I think? N1 and N2 depend on x, so i'm not exactly sure how to handle this
 * Because we want an whole number, I think we can just check maybe floor and ceil of sum( input[i] )/N ?
 * 
 * ... needs more thought, i'm sure there's something i'm missing, but the below code should work, probably :)
 * 
 */

var calc = input.Sum() / (double)input.Length;
var position1 = (int)Math.Ceiling(calc);
var position2 = (int)Math.Floor(calc);
var cost1 = calcCost(input, position1);
var cost2 = calcCost(input, position2);
bool cost1wins = cost1 < cost2;

Console.WriteLine($"Part 2 Formula - Ceil position: {position1}; cost: {cost1}{(cost1wins ?  "*" : "")}");
Console.WriteLine($"Part 2 Formula - Floor position: {position2}; cost: {cost2}{(!cost1wins ? "*" : "")}");

static int calcCost(IEnumerable<int> input, int position)
{
    return input.Sum(x =>
    {
        var d = Math.Abs(x - position);
        return d * (d + 1) / 2;
    });
}

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