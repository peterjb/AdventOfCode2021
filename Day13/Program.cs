/* Day 13
 * This one is about reflection. Input is a list of points followed by a blank line
 * followed by a list of reflection axes.
 * 
 * This solution reads in the points and axes, constructs a boolean grid and marks the points as true.
 * For each axis, reflect the points below or to the right back above or to the left and then set the new 
 * size of the grid for the next fold. 
 * 
 * At the end a sequence of capital letters is left, which is the solution to part 2
 * 
 * The axes are specified by a position on an axis, e.g. x=5 means reflect about the vertical line x=5
 * So points on the right of x = 5 become points on the left of x=5 as if the grid was folded on that line.
 * So a point 7,2 would become 3,2, i.e. the reflected point should be the same distance away from the fold line
 * as the original point.
 * 
 */

var input = File.ReadAllLines("input.txt");

var points = new List<int[]>();
var folds = new List<(char axis, int position)>();

//Read in the data. I'm sure there's a cleaner way to do this...
var i = 0;
for(;i<input.Length;i++)
{
    if (string.IsNullOrEmpty(input[i]))
        break;
    points.Add(input[i].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToArray());
}
for(i++;i<input.Length;i++)
{
    var p = input[i].IndexOf('=');
    folds.Add((input[i][p - 1], int.Parse(input[i].Substring(p + 1))));
}

//Construct the grid 
//Find the width and height of the grid
var paperWidth = points.Max(p => p[0]) + 1;
var paperHeight = points.Max(p => p[1]) + 1;

var grid = new bool[paperHeight, paperWidth]; //default value is false

//Mark the dots from the input in the grid as true
foreach(var p in points)
{
    grid[p[1],p[0]] = true;
}

var foldCount = 0;
//For each fold, reflect the points on the (bottom/right) of the fold line to the (top/left)
foreach(var f in folds)
{
    if(f.axis == 'y')
    {
        //folding along y = f.position
        //start at the grid line below the fold line and go to the end
        for(var j = f.position + 1; j < paperHeight; j++)
        {
            for(var k = 0; k < paperWidth; k++)
            {
                //reflect the point. Since we are using booleans, we can just 'or' the reflected coordinate with the current
                //coordinate. If there was a point there already, it will still be there
                grid[2*f.position - j, k] |= grid[j, k];
            }
        }
        //The height of the paper is now the position of the fold
        paperHeight = f.position;
    }
    else //x
    {
        //folding along x = f.position
        //same thing as above, just different axis
        for (var j = 0; j < paperHeight; j++)
        {
            for (var k = f.position + 1; k < paperWidth; k++)
            {
                grid[j, 2 * f.position - k] |= grid[j, k];
            }
        }
        //The width of the paper is now the position of the fold
        paperWidth = f.position;
    }

    foldCount++;
    Console.WriteLine($"Fold {foldCount}: {dotCount()} dots.");
}

printGrid();

void printGrid()
{
    for (var j = 0; j < paperHeight; j++)
    {
        for (var k = 0; k < paperWidth; k++)
        {
            Console.Write($"{(grid[j, k] ? '#' : '.')} ");
        }
        Console.WriteLine();
    }
}

int dotCount()
{
    var count = 0;

    for (var j = 0; j < paperHeight; j++)
    {
        for (var k = 0; k < paperWidth; k++)
        {
            if (grid[j, k])
                count++;
        }
    }

    return count;
}


