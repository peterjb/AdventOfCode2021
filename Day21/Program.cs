var player1start = 8;
var player2start = 4;

DeterministicDie die = new DeterministicDie();

var part1 = runDeterministicGame();
Console.WriteLine($"Deterministic outcome: {part1}");

var rollSums = new List<int>();

for(var i = 1; i <= 3;i++)
{
    for(var j = 1; j <= 3; j++)
    {
        for (var k = 1; k <= 3; k++)
        {
            rollSums.Add(i + j + k);
        }
    }
}

var sumCounts = rollSums.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

var part2 = runDiracGame((player1start, player2start, 0, 0), 1);

Console.WriteLine($"Dirac Game, win count: {Math.Max(part2.player1Wins, part2.player2Wins)}");


//Part 1 is just running the game as described.
//DeterministicDie is just a helper class to track the die state
int runDeterministicGame()
{
    var player1pos = player1start;
    var player2pos = player2start;

    var player1score = 0;
    var player2score = 0;

    while (true)
    {
        player1pos = MovePlayerDeterministically(player1pos);
        player1score += player1pos;

        if (player1score >= 1000)
            break;

        player2pos = MovePlayerDeterministically(player2pos); 
        player2score += player2pos;

        if (player2score >= 1000)
            break;
    }

    //calculate the score as per instructions
    return Math.Min(player1score, player2score) * die.RollCount;
}

/* Part 2 - I'm guessing that naively trying to go through all the universes is not possible. The trick is to realize that while there are 27 ways to roll
 * the Dirac Die 3 times, we are only interested in the sum, and there are only 7 possible sums, 3 through 9 inclusive. This greatly reduces the amount of
 * work we have to do. Only takes a few seconds to run on my machine.
 * 
 * We can keep a tally of the number of universes we are dealing with at each step and keep multiplying on down.
 * E.G. if the sum of 3 rolls for player 1 is 4, there are 3 universes where that happens (1,1,2),(1,2,1),(2,1,1)
 *      if player 2 then has a roll total of 5, there are 6 universes where that happens (1,1,3),(1,2,2),etc...
 *      So there are 18 universes where player 1 rolls 4 and player 2 rolls 5, so pass that number into the next turn and multiply it in when calculating
 *      the number of universes
 */

(long player1Wins, long player2Wins) runDiracGame((int player1pos, int player2pos, int player1score, int player2score) state, long universeCount)
{
    //keep track of how many wins each player gets from this position on
    (long player1Wins, long player2Wins) winCounts = (0, 0);

    //sumCounts is calculated above, but is a dictionary with the keys being the possible sums of 3 dirac dice rolls and the values being how many times
    //(number of universes) that sum happens
    foreach(var s1 in sumCounts)
    {
        var newState = state;

        newState.player1pos = MovePlayerDiracly(newState.player1pos, s1.Key);
        newState.player1score += newState.player1pos;

        if (newState.player1score >= 21)
        {
            //if player 1 wins after this roll, add to the player 1 win count the number of universes that lead to this state
            //times the number of universes that result in this roll. Then move on to the next possible roll
            winCounts.player1Wins += s1.Value * universeCount;
        }
        else
        {
            //if player 1 doesn't win, then go through all the possible rolls for player 2
            foreach (var s2 in sumCounts)
            {
                newState.player2pos = MovePlayerDiracly(newState.player2pos, s2.Key);
                newState.player2score += newState.player2pos;

                if (newState.player2score >= 21)
                {
                    //same as for player 1, but need to take into account the number of universes for player 1's roll and player 2's roll
                    winCounts.player2Wins += universeCount * s2.Value * s1.Value;
                }
                else
                {
                    //if we don't have a winner, run the next round, passing in the new state and new universe count
                    var c = runDiracGame(newState, universeCount * s1.Value * s2.Value);
                    winCounts.player2Wins += c.player2Wins;
                    winCounts.player1Wins += c.player1Wins;
                }

                //reset the player2 info for next roll count
                newState.player2pos = state.player2pos;
                newState.player2score = state.player2score;
            }
        }
    }
    return winCounts;
}

int MovePlayerDeterministically(int position)
{
    var roll = 0;
    for (var i = 0; i < 3; i++)
    {
        roll += die.Roll();
    }
    return MovePlayerDiracly(position, roll);
}

int MovePlayerDiracly(int position, int roll)
{
    position += roll;
    position %= 10;
    if (position == 0)
        position = 10;
    return position;
}

public struct DeterministicDie
{
    public int RollCount { get; private set; }
    private int rollValue = 0;
    public int Roll()
    {
        RollCount++;
        rollValue++;
        if (rollValue > 100)
            rollValue = 1;
        return rollValue;
    }
}

