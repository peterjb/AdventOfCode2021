//EX1: 5084
//EX2: 17882

//setup
//altered the input to make it slightly easier to read
var lines = File.ReadLines("input.txt").Select(x =>
{
    var points = x.Split(';');
    var p1 = points[0].Split(',');
    var p2 = points[1].Split(',');
    return new Line
    {
        P1 = (x: int.Parse(p1[0]), y: int.Parse(p1[1])),
        P2 = (x: int.Parse(p2[0]), y: int.Parse(p2[1]))
    };
});

Console.Write("Horizontal and Vertical Only ");
analyseLines(lines.Where(l => l.P1.x == l.P2.x || l.P1.y == l.P2.y));

Console.Write("All ");
analyseLines(lines);

static void analyseLines(IEnumerable<Line> lines)
{
    //find the width and height of the grid
    var width = lines.Max(l => Math.Max(l.P1.x, l.P2.x)) + 1;
    var height = lines.Max(l => Math.Max(l.P1.y, l.P2.y)) + 1;

    var grid = new int[height, width];

    foreach (var l in lines)
    {
        var startY = l.P1.y;
        var endY = l.P2.y;
        var startX = l.P1.x;
        var endX = l.P2.x;

        if (startX == endX)
        {
            if (startY > endY)
            {
                Swap(ref startY, ref endY);
            }
            drawHVLine(startX, startY, endX, endY, grid);
        }
        else if (startY == endY)
        {
            if (startX > endX)
            {
                Swap(ref startX, ref endX);
            }
            drawHVLine(startX, startY, endX, endY, grid);
        }
        else
        {
            if (startX > endX)
            {
                Swap(ref startX, ref endX);
                Swap(ref startY, ref endY);
            }
            var m = (endY - startY) / (endX - startX);
            Draw45Line(startX, startY, endX, endY, grid);
        }
    }

    if (height <= 20 && width <= 20)
    {
        printGrid(grid, height, width);
    }

    var c = 0;
    for (var i = 0; i < height; i++)
    {
        for (var j = 0; j < width; j++)
        {
            if (grid[i, j] > 1)
            {
                c++;
            }
        }
    }

    Console.WriteLine($"Overlapped Points: {c}");
}

static void printGrid(int[,] grid, int height, int width)
{
    for (var y = 0; y < height; y++)
    {
        for (var x = 0; x < width; x++)
        {
            if (grid[y, x] == 0)
            {
                Console.Write(". ");
            }
            else
            {
                Console.Write($"{grid[y, x]} ");
            }
        }
        Console.WriteLine();
    }
}

static void drawHVLine(int x1, int y1, int x2, int y2, int[,] grid)
{
    for (var y = y1; y <= y2; y++)
    {
        for (var x = x1; x <= x2; x++)
        {
            grid[y, x]++;
        }
    }
}

static void Draw45Line(int x1, int y1, int x2, int y2, int[,] grid)
{
    var inc = 1;
    if (y1 > y2)
    {
        inc = -1;
    }

    for (int x = x1, y = y1; x <= x2; x++, y += inc)
    {
        grid[y, x]++;
    }
}

static void Swap(ref int x, ref int y)
{
    var temp = y;
    y = x;
    x = temp;
}

struct Line
{
    public (int x, int y) P1;
    public (int x, int y) P2;
}

