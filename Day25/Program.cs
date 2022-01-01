/* Day 25
 * 
 * Part 1 only, because i still need to finish Day 23 :(
 * 
 * 
 */

var input = File.ReadAllLines("input.txt").Select(x => x.ToArray()).ToArray();

var h = input.Length;
var w = input[0].Length;

var input2 = new char[h][];
for (var i = 0; i < h; i++)
{
    input2[i] = new char[w];
}

clear(input2);

char[][] temp;

//print(input);
int steps = 0;
var moved = false;
do
{
    moved = false;
    //move east
    for(var r = 0; r < h; r++)
    {
        for(var c = 0; c < w; c++)
        {
            if(input[r][c] == '>')
            {
                var nc = c == w-1 ? 0 : c + 1;

                if(input[r][nc] == '.')
                {
                    input2[r][nc] = '>';
                    c++;
                    moved = true;
                }
                else
                {
                    input2[r][c] = '>';
                }
            }
            else
            {
                input2[r][c] = input[r][c];
            }
        }
    }

    temp = input;
    input = input2;
    input2 = temp;
    clear(input2);

    //move south
    for (var c = 0; c < w; c++)
    {
        for (var r = 0; r < h; r++)
        {
            var nr = r == h - 1 ? 0 : r + 1;
            if (input[r][c] == 'v')
            {
                if (input[nr][c] == '.')
                {
                    input2[nr][c] = 'v';
                    r++;
                    moved = true;
                }
                else
                {
                    input2[r][c] = 'v';
                }
            }
            else
            {
                input2[r][c] = input[r][c];
            }
        }
    }

    steps++;

    temp = input;
    input = input2;
    input2 = temp;
    clear(input2);

     //print(input);
    //Console.ReadKey();
} while (moved);

Console.WriteLine(steps);

void print(char[][] board)
{
    for(var r = 0; r < h; r++)
    {
        for(var c = 0; c < w; c++)
        {
            Console.Write(board[r][c]);
        }
        Console.WriteLine();
    }
    Console.WriteLine();
}

void clear(char[][] board)
{
    for (var r = 0; r < h; r++)
    {
        for (var c = 0; c < w; c++)
        {
            board[r][c] = '.';
        }
    }
}