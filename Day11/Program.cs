var input = File.ReadLines("input.txt").Select(line => line.Select(c => (energy: (int)char.GetNumericValue(c), flashed: false)).ToArray()).ToArray();

var stepFlashes = 0;
var totalFlashes = 0;
var flashes = 0;
var steps = 0;

// Follow the algorithm as described by the problem. Each iteration of the outer while loop is one step
// Each step
//   1) increase the energy of the octopi by 1
//   2) see which ones flash, which causes neighbors energy to increase by 1, and keep going until there are no more flashes
//   3) reset the energy level to 0 for any that flashed
//
// Part 2 is where the problem lies. This solution goes through the grid calling the flash function on each octopus.
// The flash function then recursively checks neighbors to see if they should then flash and so on.
while(true)
{
    stepFlashes = 0;
    flashes = 0;

    //increase energy
    input.ForEach2D((i, j, octopus) => input[i][j].energy++);
    
    input.ForEach2D((i,j,octopus) =>
    {
        flashes += Flash(i, j, input, 0);
    });

    totalFlashes += flashes;
    stepFlashes += flashes;

    input.ForEach2D((i, j, octopus) => {
        input[i][j].flashed = false;
        if (input[i][j].energy > 9)
            input[i][j].energy = 0;
    });

    steps++;

    //part 1 - how many flashes after 100 steps
    if(steps == 100)
        Console.WriteLine($"Flashes after 100 steps: {totalFlashes}");

    //part 2 - how many steps until all of the octopi flash in the same step
    //there are 100 octopi, so check if there were 100 flashes in the step
    if (stepFlashes == 100)
    {
        Console.WriteLine($"Steps until all flash in one step {steps}");
        break;
    }
}

//Recursively check neighbors if the current octopus flashed
//incomingEnergy parameter is to differentiate between the top level calls and the recursive calls
//top level calls are not due to a flash, so we don't increase the energy first
//but recursive calls are due to a flash.
static int Flash(int i, int j, (int energy, bool flashed)[][] input, int incomingEnergy = 1)
{
    int h = input.Length;
    int w = input[i].Length;

    var flashes = 0;

    
    input[i][j].energy += incomingEnergy;

    if (input[i][j].energy > 9 && !input[i][j].flashed)
    {
        flashes++;
        input[i][j].flashed = true; 

        if (i > 0)
            flashes += Flash(i - 1, j, input);
        if (j > 0)
            flashes += Flash(i, j - 1, input);
        if (i > 0 && j > 0)
            flashes += Flash(i - 1, j - 1, input);
        if (i < h - 1)
            flashes += Flash(i + 1, j, input);
        if (j < w - 1)
            flashes += Flash(i, j + 1, input);
        if (i < h - 1 && j < w - 1)
            flashes += Flash(i + 1, j + 1, input);
        if (i > 0 && j < w - 1)
            flashes += Flash(i - 1, j + 1, input);
        if (i < h - 1 && j > 0)
            flashes += Flash(i + 1, j - 1, input);
    }

    return flashes;
        
}

static class Extensions
{
    public static void ForEach2D<T>(this T[][] ts, Action<int, int, T> action)
    {
        for (var i = 0; i < ts.Length; i++)
        {
            for (var j = 0; j < ts[i].Length; j++)
            {
                action(i,j,ts[i][j]);
            }
        }
    }
}

