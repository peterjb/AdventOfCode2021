//EX 1: (1968, 1063) 2091984
//EX 2: (1968, 1060092, 1063) 2086261056

//part 1
var position = File.ReadLines("input.txt").Aggregate((0, 0), (current, line) =>
{
    var command = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var amount = int.Parse(command[1]);
    switch(command[0])
    {
        case "forward":
            return (current.Item1 + amount, current.Item2);
        case "up":
            return (current.Item1, current.Item2 - amount);
        case "down":
            return (current.Item1, current.Item2 + amount);
        default:
            throw new ArgumentOutOfRangeException("unrecognized direction");
    }
 });

Console.WriteLine($"Final Position: {position}; Product: {position.Item1 * position.Item2}");

//part 2
var correctedPosition = File.ReadLines("input.txt").Aggregate((0, 0, 0), (current, line) =>
{
    var command = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var amount = int.Parse(command[1]);
    switch (command[0])
    {
        case "forward":
            return (current.Item1 + amount, current.Item2 + (current.Item3 * amount), current.Item3);
        case "up":
            return (current.Item1, current.Item2, current.Item3 - amount);
        case "down":
            return (current.Item1, current.Item2, current.Item3 + amount);
        default:
            throw new ArgumentOutOfRangeException("unrecognized direction");
    }
});

Console.WriteLine($"Final Position: {correctedPosition}; Product: {correctedPosition.Item1 * correctedPosition.Item2}");