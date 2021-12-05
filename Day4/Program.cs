//EX 1: 74320
//EX 2: 17884

var boardSize = 5;
var input = File.ReadAllLines("input.txt");

List<BingoBoard> boards = new List<BingoBoard>();

//read in pulls
var pulls = input[0].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();

//read in boards
for (var i = 2; i < input.Length; i += 6)
{
    boards.Add(new BingoBoard(input.Skip(i).Take(boardSize).ToArray(), boardSize));
}

//do pulls
for (var j = 0; j < pulls.Length; j++)
{
    foreach (var board in boards.Where(b => !b.Finished))
    {
        board.DoPull(j, pulls[j]);
    }
}

//part 1
var first = boards.MinBy(x => x.WinningPull);
Console.WriteLine($"Winner: board {boards.IndexOf(first) + 1} with score {first.Score} after {first.WinningPull} pulls.");

//part 2
var last = boards.MaxBy(x => x.WinningPull);
Console.WriteLine($"Loser: board {boards.IndexOf(last) + 1} with score {last.Score} after {last.WinningPull} pulls.");

class BingoBoard
{
    public BingoBoard(string[] values, int boardSize)
    {
        boardValues = new int[boardSize][];
        rowState = new int[boardSize];
        colState = new int[boardSize];

        for (var i = 0; i < boardSize; i++)
        {
            boardValues[i] = values[i].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
            rowState[i] = boardValues[i].Sum();
            sumOfBoardValues += rowState[i];
        }

        for (var j = 0; j < boardSize; j++)
        {
            colState[j] = boardValues.Sum(x => x[j]);
        }
    }
    public bool DoPull(int pullNumber, int value)
    {
        for (var i = 0; i < boardValues.Length; i++)
        {
            for (var j = 0; j < boardValues[i].Length; j++)
            {
                if (boardValues[i][j] == value)
                {
                    rowState[i] -= value;
                    colState[j] -= value;
                    sumOfBoardValues -= value;
                    if (rowState[i] == 0 || colState[j] == 0)
                    {
                        Finished = true;
                        Score = value * sumOfBoardValues;
                        WinningPull = pullNumber;
                        return true;
                    }
                    return false;
                }
            }
        }
        return false;
    }
    public bool Finished { get; private set; } = false;
    public int Score { get; private set; } = 0;
    public int WinningPull { get; private set; } = 0;

    readonly int[][] boardValues;
    readonly int[] rowState;
    readonly int[] colState;
    int sumOfBoardValues;
}
