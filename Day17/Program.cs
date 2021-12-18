var targetXCoords = new int[2] { 138, 184 };
var targetYCoords = new int[2] { -125, -71 };

//sample
//var x = new int[2] { 20, 30 };
//var y = new int[2] { -10, -5 };

//Part 1
//This is not a general solution:
//
//Assuming the trench is always below, the argument is that when the probe comes back down, because the problem is
//symetric in y, if we shoot the probe up, the velocity in the y direction when it reaches y = 0 again will be the
//same as at the start. On the next step, it's velocity will by vy0 + 1. The maximum velocity it can have for the
//next step and still be in the target area is the position of the bottom of the target area,
//so vy0 is 125 - 1 = 124 for my input.
//
//the height it will reach if it starts at vy0 = 124 is just summing the numbers from 1 to 124 or 1/2*124*125 = 7750
//this only works if the x distance is not too far away, which i did check, but will use in the answer for part 2
var vy0max = Math.Abs(targetYCoords[0]) - 1;
var vy0min = -vy0max - 1;

Console.WriteLine($"Max height: {(vy0max*vy0max + vy0max)/2}");

//Part 2
//
//Goal is to first find the min and max x and y velocities that will put the x and y 
//coordinates in the target area. Then find all x and y velocities in between the min and max and what time range
//they are in the target area for. Then we can go through and find where those two lists have solutions that overlap.

//assuming the trench is always to the right and down from (0,0)

//The minimum initial x velocity will be the smallest n for which the x[0] <= sum(n,1) <= x[1]
//the x velocity slows by 1 every step, and the distance increases by v every step
//so the distance is just the sum vx0 + (vx0 - 1) + ... + 1. So the min vx0 is the positive solution to
//vx0^2 + vx0 - 2x[0] = 0; Not sure if there's much point in doing this, can just search 0 to max;
var vx0min = (int)Math.Ceiling(.5 * (Math.Sqrt(1 + 8 * targetXCoords[0]) - 1));

//the maximum initial x velocity will be the distance to the far edge of the target area, so 184
//(any faster and it will never be in the target area)
var vx0max = targetXCoords[1];

//Store all the initial x velocities that result in the probe having an x coord within the x bounds of the target area
//and the time range the probe is in that boundary. Do this by stepping through the launch until the x velocity goes to 0
//or the probe goes past the target area
var vxSolutions = new List<(int x, int t0, int t1)>();
for (var vx0 = vx0min; vx0 <= vx0max; vx0++)
{
    List<int> times = new List<int>(); ;
    var xDistance = 0;
    for (var deltaX = vx0; deltaX > 0; deltaX--)
    {
        xDistance += deltaX;
        if (xDistance >= targetXCoords[0] && xDistance <= targetXCoords[1])
        {
            times.Add((deltaX == 1) ? int.MaxValue : vx0 - deltaX + 1);
        }
        if(xDistance > targetXCoords[1])
        {
            break;
        }
    }
    if (times.Count > 0)
    {
        vxSolutions.Add((vx0, times.First(), times.Last()));
    }
}

//Do the same for the initial y velocities
//Slightly different than x because of how the y velocity doesn't stop changing
var vySolutions = new List<(int y, int t0, int t1)>();
for (var vy0 = vy0min; vy0 <= vy0max; vy0++)
{
    List<int> times = new List<int>();
    var yDistance = 0;
    var vy = vy0;
    var t = 1;
    while(yDistance > targetYCoords[0]) //[0] is the bottom of the target area. The probe will be heading down
    {
        yDistance += vy;
        if (yDistance >= targetYCoords[0] && yDistance <= targetYCoords[1])
        {
            times.Add(t);
        }
        vy--;
        t++;
    }
    if (times.Count > 0)
    {
        vySolutions.Add((vy0, times.First(), times.Last()));
    }
}

var solutions = new List<(int, int)>();

//check each pair of x velocity and y velocity to see if their times in the target area overlap.
foreach(var xsoln in vxSolutions)
{
    foreach(var ysoln in vySolutions)
    {
        //if the time ranges overlap, we have a firing solution
        if(!(ysoln.t1 < xsoln.t0 || ysoln.t0 > xsoln.t1))
        {
            solutions.Add((xsoln.x, ysoln.y));
        }
    }
}

Console.WriteLine($"Number of solutions: {solutions.Count()}");
