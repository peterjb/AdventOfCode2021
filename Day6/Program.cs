//EX1: 391888
//EX2: 1754597645339

var days = 256;
int childCycle = 8; //max index, so 9 days
int adultCycle = 6; //index, so 7 days

//read in all the fish
var fish = File.ReadAllLines("input.txt")[0].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => byte.Parse(x)).ToList();

//get the initial counts of fish by day in cycle
var groups = fish.GroupBy(x => x).Select(y => (cycle: y.Key, count: y.Count())).ToArray();

//keeps track of the amount of fish in each day of the lanternfish cycle
//fish at index 0 are at timer 0, etc.
var counts = new Int64[childCycle + 1];

//set the initial number of fish at each cycle day from the input
foreach(var g in groups)
{
    counts[g.cycle] = g.count;
}

//each day, update the number of fish at each day of the cycle
//fish at cycle 5 are now at cycle 4, etc.
//fish at cycle 0 reset to cycle 6, and an equal number of new fish are put at cycle 8 (the children of those fish at 0)
for(var i = 0; i < days; i++)
{
    var temp = counts[0];
    //shift the array to the left
    Array.Copy(counts, 1, counts, 0, counts.Length - 1);
    
    counts[adultCycle] += temp; //fish at cycle 0 go to cyle 6
    counts[childCycle] = temp; //this is where we add new fish.
}

Console.WriteLine($"{counts.Sum()} Fish after {days} days");

//not_smart();

//this uses the algorithm outlined in the task description which obviously
//doesn't work very well for large numbers of days e.g. 256
//not very smart, keeping it for posterity i guess.
static void not_smart()
{
    var days = 80;
    var fish = File.ReadAllLines("input.txt")[0].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => byte.Parse(x)).ToList();
    var taskCount = 8;
    var batchSize = fish.Count() / taskCount;
    
    Task<Int64>[] tasks = new Task<Int64>[taskCount];
    List<byte>[] lists = new List<byte>[taskCount];
    
    int i = 0;
    for(; i < taskCount-1; i++)
    {
        int j = i; //for lamda closure
        lists[i] = fish.Skip(i*batchSize).Take(batchSize).ToList();
        tasks[i] = Task.Run(() => simulateFish(lists[j], days));
    }
    lists[i] = fish.Skip(i * batchSize).ToList();
    tasks[i] = Task.Run(() => simulateFish(lists[i], days));
    
    Task.WaitAll(tasks);
    Console.WriteLine($"{tasks.Sum(t => t.Result)} Fish after {days} days");
}

//the algorithm from the task description
static Int64 simulateFish(List<byte> fish, int days)
{
    for (int i = 0; i < days; i++)
    {
        var c = fish.Count();
        for (int j = 0; j < c; j++)
        {
            if (fish[j] == 0)
            {
                fish[j] = 6;
                fish.Add(8);
            }
            else
            {
                fish[j]--;
            }
        }
    }

    return fish.Count();
}
