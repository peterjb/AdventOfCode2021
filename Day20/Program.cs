using System.Text;

/* Compared to 18 and 19, i found this relatively straight-forward. Trick was accounting for the infinite size of the images.
 * 
 * Learned the technique of representing the grid data as a set of points from watching a youtube video from a previous day.
 * Can't remember which day, but this is the channel: https://www.youtube.com/channel/UCuWLIm0l4sDpEe28t41WITA 
 * 
 */

var file = "input";

var input = File.ReadAllLines($"{file}.txt");

var algorithm = input[0];

var inputFrame = new HashSet<(int r, int c)>();

for (var r = 2; r < input.Length; r++)
{
    for (var c = 0; c < input[r].Length; c++)
    {
        if (input[r][c] == '#')
        {
            inputFrame.Add((r - 2, c));
        }
    }
}

HashSet<(int r, int c)> Enhance(HashSet<(int r, int c)> inputFrame, string algorithm, int numEnhancements)
{
    //double buffer
    var frame1 = new HashSet<(int r, int c)>(inputFrame);
    var frame2 = new HashSet<(int r, int c)>();

    //the infinite boundary of the initial image is 'off'
    var boundary = '.';

    int xmin = 0;
    int xmax = 0;
    int ymin = 0;
    int ymax = 0;

    for (var i = 0; i < numEnhancements; i++)
    {
        //keep track of the boundary of the image as influence by the original data
        //increasing this boundary by 1 on all sides is the boundary of all the points we have to check for the next update
        xmin = frame1.Min(x => x.c);
        xmax = frame1.Max(x => x.c);
        ymin = frame1.Min(y => y.c);
        ymax = frame1.Max(y => y.c);

        //storage for the binary number to index the algorithm
        var aIndexBinary = new char[9];

        //loop through the grid of all points influenced by our initial image data (so plus 1 on each side to the min/max x/y
        for (var y = ymin - 1; y <= ymax + 1; y++)
        {
            for (var x = xmin - 1; x <= xmax + 1; x++)
            {
                //for each point in that grid, check it's neighbors to determine the bits for the algorithm index
                for (var r = 0; r < 3; r++)
                {
                    for (var c = 0; c < 3; c++)
                    {
                        var lr = y - 1 + r; //current row
                        var lc = x - 1 + c; //current col

                        //the tricky bit: If we are checking outside the boundary, use the current state of the infinite boundary
                        //it starts as off '.', but may update depending on the algorithm
                        if (lr > ymax || lr < ymin || lc > xmax || lc < xmin)
                        {
                            aIndexBinary[c + 3 * r] = boundary == '#' ? '1' : '0';
                        }
                        else if (frame1.Contains((lr, lc))) //if we are inside the boundary, check if that point is in the set (on)
                        {
                            aIndexBinary[c + 3 * r] = '1';
                        }
                        else
                        {
                            aIndexBinary[c + 3 * r] = '0';
                        }
                    }
                }
                //get the int value of the binary string
                var aIndex = Convert.ToInt32(new string(aIndexBinary), 2);

                //use it to index into the algorithm.
                if (algorithm[aIndex] == '#')
                {
                    frame2.Add((y, x));
                }
            }
        }

        //check the boundary conditions
        //if the current boundary is 'on', then it should be updated according to the last index in the algorithm
        //if it's 'off', then it should be updated according to the first index in the algorithm
        if (boundary == '#')
        {
            boundary = algorithm.Last();
        }
        else

        {
            boundary = algorithm.First();
        }

        //swap frames
        frame1 = frame2;
        frame2 = new HashSet<(int r, int c)>();
    }

    return frame1;
}

var part1 = Enhance(inputFrame, algorithm, 2);
Console.WriteLine(part1.Count());

var part2 = Enhance(inputFrame, algorithm, 50);
Console.WriteLine(part2.Count());

//For debugging
void printFrame(HashSet<(int r, int c)> frame, char boundary, int padding = 10)
{
    var xmin = frame.Min(x => x.c);
    var xmax = frame.Max(x => x.c);
    var ymin = frame.Min(y => y.c);
    var ymax = frame.Max(y => y.c);

    Console.ForegroundColor = ConsoleColor.Green;
    for (var r = ymin - padding; r <= ymax + padding; r++)
    {
        for(var c = xmin - padding; c <= xmax + padding; c++)
        {
            if(r == 0 && c == 0)
            {
                Console.BackgroundColor = ConsoleColor.Red;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
            }
            if(r < ymin || r > ymax || c < xmin || c > xmax)
            {
                Console.Write(boundary);
            }
            else if (frame.Contains((r, c)))
            {
                Console.Write('#');
            }
            else
            {
                Console.Write('.');
            }
        }
        Console.WriteLine();
    }
    Console.ForegroundColor = ConsoleColor.White;
}

void WriteFrame(HashSet<(int r, int c)> frame, string filename, char boundary, int padding)
{
    var xmin = frame.Min(x => x.c);
    var xmax = frame.Max(x => x.c);
    var ymin = frame.Min(y => y.c);
    var ymax = frame.Max(y => y.c);

    var sb = new StringBuilder();
    for (var r = ymin - padding; r <= ymax + padding; r++)
    {
        for (var c = xmin - padding; c <= xmax + padding; c++)
        {
            if (r < ymin || r > ymax || c < xmin || c > xmax)
            {
                sb.Append(boundary);
            }
            else if (frame.Contains((r, c)))
            {
                sb.Append('#');
            }
            else
            {
                sb.Append('.');
            }
        }
        sb.AppendLine();
    }
    File.WriteAllText(filename, sb.ToString());
}