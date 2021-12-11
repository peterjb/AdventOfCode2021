using System.Text;

var input = File.ReadLines("input.txt").Select(x => x.Select(y => (int)char.GetNumericValue(y)).ToArray()).ToArray();


//assume rectangle
int w = input[0].Length;
int h = input.Length;

//Part 1, just run through the grid checking if the point is the lowest of its non-diagonal neighbors
int sum = 0;

for(var i = 0; i < h; i++)
{
    for(var j = 0; j < w; j++)
    {
        if(isLowpoint(i,j,input))
        {
            sum += input[i][j] + 1;
        }
    }
}

Console.WriteLine($"Sum of risk levels of low points: {sum}");
//Part 2
//Not sure best way to go about this. This code goes through all the points in the grid looking for a connected low point.
//It keeps track as it goes so it doesn't have to re-find a lowpoint. This works because as the problem is stated,
//we can snake around looking for a lowpoint or a point for which we already know its basin. If we end up at a lowpoint
//that hasn't been reached before, we add a new basin to the map. We also keep count as we go because we need to find
//the product of the three largest basins

//There seem to be some assumptions being made here, like the same number won't repeat itself vertically or horizontally
//unless it's a 9

var basinMap = new int[h, w]; //keeps track of which basin every grid point is in, -1 for the ridges (height 9)
var basinSizes = new Dictionary<int, int>(); //keeps track of the number of grid points in a basin
var basinCount = 0; //counter that gives us the id for the current basin

for (int i = 0; i < h; i++)
{
    for (int j = 0; j < w; j++)
    {
        //for each point, get the basin id. If it's not -1, keep count of how many are in each basin
        var basinId = findBasin(i, j, input, basinMap, ref basinCount);
        if (basinId != -1)
        {
            if (basinSizes.ContainsKey(basinId))
                basinSizes[basinId]++;
            else
                basinSizes.Add(basinId, 1);
        }
    }
}

//Find the product of the 3 biggest basins
Console.WriteLine($"Product of sizes of 3 biggest basins: {basinSizes.OrderByDescending(x => x.Value).Take(3).Aggregate(1, (acc, kvp) => acc * kvp.Value)}");

//Simple function to check if a point on the grid is a low point as defined by the problem.
static bool isLowpoint(int i, int j, int[][] input)
{
    var h = input[i][j];
    if (i != 0 && h > input[i - 1][j])
        return false;
    if (j != 0 && h > input[i][j - 1])
        return false;
    if (i != input.Length - 1 && h > input[i + 1][j])
        return false;
    if (j != input[i].Length - 1 && h > input[i][j + 1])
        return false;

    return true;
}

//Basically for the given point, find the lowpoint it's connected to, or a nearby point
//it's connected to that we already know what lowpoint it's connect to:
//   For a given point, if we haven't visited this point before:
//     If it's a ridge, return -1
//     If it's a lowpoint, create a new basinId and add it to the basin map at this point
//     Otherwise start looking around for a point lower than this and run findBasin on it
//       because we know that it will be in the same basin as the current point.
//       (if it's not a lowpoint and not a ridge, then it must have a neighbor lower than it)
//   If we have visited here before, just return its basin id
static int findBasin(int i, int j, int[][] input, int[,] basinMap, ref int basinId)
{
    var h = input[i][j];
    
    if (basinMap[i, j] == 0) //if we haven't been here before
    {
        if (h == 9) //if this is a ridge, just return
            return -1;
        else if (isLowpoint(i,j,input))
        {
            basinMap[i, j] = ++basinId; //if it's 
        }
        else
        {
            if (i != 0 && h > input[i - 1][j])
                basinMap[i,j] = findBasin(i - 1,j, input, basinMap, ref basinId);
            else if (j != 0 && h > input[i][j - 1])
                basinMap[i, j] = findBasin(i, j - 1, input, basinMap, ref basinId);
            else if (i != input.Length - 1 && h > input[i + 1][j])
                basinMap[i, j] = findBasin(i + 1, j, input, basinMap, ref basinId);
            else if (j != input[i].Length - 1 && h > input[i][j + 1])
                basinMap[i, j] = findBasin(i, j + 1, input, basinMap, ref basinId);
        }
    }

    return basinMap[i, j];
}