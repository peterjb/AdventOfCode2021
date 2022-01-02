/* 
 * Day 23 - This might've been the hardest one for me. It was the last one I finished.
 * It's kinda janky, but...
 *  Advent of Code 2021 complete!
 * 
 */

/* 
 * #############
 * #...........#
 * ###D#B#A#C###
 *   #D#C#B#A#
 *   #D#B#A#C#
 *   #C#A#D#B#
 *   #########
 */

var energyCosts = new Dictionary<char, long>()
{
    { 'A', 1 },
    { 'B', 10 },
    { 'C', 100 },
    { 'D', 1000 }
};

var entrances = new List<int> { 2, 4, 6, 8 };

var homeX = new Dictionary<char, int>()
{
    { 'A', 2 },
    { 'B', 4 },
    { 'C', 6 },
    { 'D', 8 }
};

Piece[] inputPieces = new Piece[]
{
    new Piece() { type = 'A', x = 4, y = 2 },
    new Piece() { type = 'A', x = 6, y = 1 },
    new Piece() { type = 'B', x = 4, y = 1 },
    new Piece() { type = 'B', x = 8, y = 2 },
    new Piece() { type = 'C', x = 2, y = 2 },
    new Piece() { type = 'C', x = 8, y = 1 },
    new Piece() { type = 'D', x = 2, y = 1 },
    new Piece() { type = 'D', x = 6, y = 2 },
};

Piece[] inputPieces2 = new Piece[]
{
    new Piece() { type = 'A', x = 4, y = 4 },
    new Piece() { type = 'A', x = 6, y = 1 },
    new Piece() { type = 'A', x = 8, y = 2 },
    new Piece() { type = 'A', x = 6, y = 3 },
    new Piece() { type = 'B', x = 4, y = 1 },
    new Piece() { type = 'B', x = 6, y = 2 },
    new Piece() { type = 'B', x = 4, y = 3 },
    new Piece() { type = 'B', x = 8, y = 4 },
    new Piece() { type = 'C', x = 8, y = 1 },
    new Piece() { type = 'C', x = 4, y = 2 },
    new Piece() { type = 'C', x = 8, y = 3 },
    new Piece() { type = 'C', x = 2, y = 4 },
    new Piece() { type = 'D', x = 2, y = 1 },
    new Piece() { type = 'D', x = 2, y = 2 },
    new Piece() { type = 'D', x = 2, y = 3 },
    new Piece() { type = 'D', x = 6, y = 4 },
};

Piece[] samplePieces = new Piece[]
{
    new Piece() { type = 'A', x = 2, y = 2 },
    new Piece() { type = 'A', x = 8, y = 2 },
    new Piece() { type = 'B', x = 2, y = 1 },
    new Piece() { type = 'B', x = 6, y = 1 },
    new Piece() { type = 'C', x = 4, y = 1 },
    new Piece() { type = 'C', x = 6, y = 2 },
    new Piece() { type = 'D', x = 4, y = 2 },
    new Piece() { type = 'D', x = 8, y = 1 },
};

Piece[] samplePieces2 = new Piece[]
{
    new Piece() { type = 'A', x = 2, y = 4 },
    new Piece() { type = 'A', x = 8, y = 4 },
    new Piece() { type = 'A', x = 8, y = 2 },
    new Piece() { type = 'A', x = 6, y = 3 },
    new Piece() { type = 'B', x = 2, y = 1 },
    new Piece() { type = 'B', x = 6, y = 1 },
    new Piece() { type = 'B', x = 6, y = 2 },
    new Piece() { type = 'B', x = 4, y = 3 },
    new Piece() { type = 'C', x = 4, y = 1 },
    new Piece() { type = 'C', x = 6, y = 4 },
    new Piece() { type = 'C', x = 4, y = 2 },
    new Piece() { type = 'C', x = 8, y = 3 },
    new Piece() { type = 'D', x = 4, y = 4 },
    new Piece() { type = 'D', x = 8, y = 1 },
    new Piece() { type = 'D', x = 2, y = 2 },
    new Piece() { type = 'D', x = 2, y = 3 },
};

Dictionary<State, long> stateCosts = new Dictionary<State, long>();
HashSet<State> states = new HashSet<State>();
long bestScore = long.MaxValue;

var pieces = inputPieces2;
int pieceCount = pieces.Length;
int roomDepth = pieceCount / 4;

System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

stopwatch.Start();
doMoves(pieces, 0);
stopwatch.Stop();

Console.WriteLine($"{bestScore} in {stopwatch.ElapsedMilliseconds}ms");

//A piece is home if it's x coord is it's home x coord and there are no pieces of a different type underneath it
bool isHome(Piece piece, Piece[] pieces)
{
    return piece.x == homeX[piece.type] && (
            !pieces.Any(p => p.x == piece.x && p.y > piece.y && p.type != piece.type)
        );
}

//A piece can go home if there are no pieces of a different type it's home, and it is not blocked from moving there
bool canGoHome(Piece piece, Piece[] pieces)
{
    if (pieces.Any(p => p.x == homeX[piece.type] && p.type != piece.type))
        return false;
    else
        return canMoveTo(piece, homeX[piece.type], 1, pieces); //dumb
}

bool canMoveTo(Piece piece, int x, int y, Piece[] pieces)
{
    //not blocked at home
    if (pieces.Any(p => p.x == piece.x && p.y < piece.y))
    {
        return false;
    }

    //not allowed to move to an entrance without going in
    if (entrances.Contains(x) && y == 0)
    {
        return false;
    }

    //check if there are any pieces between our x and the destination x (not including our piece)
    int x0 = piece.x;
    if (x0 > x)
    {
        var temp = x0;
        x0 = x;
        x = temp;
    }
    return !pieces.Any(p => p.x >= x0 && p.x <= x && p.y == 0 && p.x != piece.x);
}

//We are done when all pieces are at home
bool done(Piece[] pieces)
{
    return pieces.All(p => isHome(p, pieces));
}

bool isBlockedAtHome(Piece piece, Piece[] pieces)
{
    return pieces.Any(p => p.x == piece.x && p.y < piece.y);
}

//Actually move the piece, calculating the cost of doing so
long move(ref Piece piece, int x, int y)
{
    var cost = (Math.Abs(piece.x - x) + piece.y + y) * energyCosts[piece.type];
    piece.x = x; piece.y = y;
    return cost;
}

//Recursively move the pieces around. For each piece, make all possible moves, exhaustively unless we know it will result in a worse answer
//Optimizations:
// 1) if we have already found a solution, and our score is already bigger than the solution score, just stop
// 2) if we have already been in this state at a lower score, just stop
//    I'm pretty sure the state thing is working as intended. It certainly broke when i messed up the state generation function 
//    (Ok i measured on the sample and it more than halved the time required having the state checks
void doMoves(Piece[] pieces, long score)
{
    //foreach state we are in, check if we can move each piece. If we can move a piece, check where that leaves us, and recurse if we aren't done
    for (var i = 0; i < pieces.Length; i++)
    {
        var piece = pieces[i];
        
        if (isHome(piece, pieces))
        {
            //if the piece is already home, leave it
            continue;
        }
        if (canGoHome(piece, pieces))
        {
            //if the piece can go home, move it and check if we are done

            //create a new copy of the pieces
            var newPieces = new Piece[pieceCount];
            Array.Copy(pieces, newPieces, pieceCount);

            var piecesAtHome = pieces.Where(p => p.x == homeX[piece.type]);
            var yPos = piecesAtHome.Count() > 0 ? piecesAtHome.MinBy(p => p.y).y - 1 : roomDepth;
            var nextScore = score + move(ref newPieces[i], homeX[piece.type], yPos);

            //if the score after moving is larger than our best completed score, forget it and move to the next piece
            if (nextScore > bestScore)
            {
                continue;
            }

            //if we are done, check if it's a better score than any previous scores and return
            if (done(newPieces))
            {
                //Console.WriteLine($"score: {nextScore}");
                //DrawPieces(pieces);
                bestScore = Math.Min(bestScore, nextScore);
                return;
            }
            else
            {
                //if we aren't done, get the new state and see if we've been here before
                //if not, or we have been here before but at a higher cost, keep going
                //i did measure and the state is working
                var s = getState(newPieces);
                if (!states.Contains(s))
                {
                    states.Add(s);
                    stateCosts.Add(s, nextScore);
                    doMoves(newPieces, nextScore);
                }
                else if (stateCosts[s] > nextScore)
                {
                    doMoves(newPieces, nextScore);
                }
            }
        }
        else if (piece.y > 0)
        {
            //otherwise, if the piece is still in a room (but not home) go through all possible moves for it recursing down if it doesn't result in a worse score
            for (var x = 0; x < 11; x++)
            {
                if (canMoveTo(piece, x, 0, pieces))
                {
                    var nextState = new Piece[pieceCount];
                    Array.Copy(pieces, nextState, pieceCount);
                    var nextScore = score + move(ref nextState[i], x, 0);
                    if (nextScore > bestScore)
                        continue;
                    else
                        doMoves(nextState, nextScore);
                }
            }
        }
    }
}

//I think this works? The idea is that it doesn't matter which A is where. In practice this doesn't seem to matter much because it is rare/maybe impossible? that 
//a state where all the A's are in the same position as a different state but they are permuted will happen?
//Having the state definitely makes a difference though. More than halved the time to find the solution for sample input.
//I've thought both too hard and not hard enough about this. This worked well enough to find the correct answer, but it is not vetted appropriately
State getState(Piece[] pieces)
{
    return new State()
    {
        A = pieces.Where(p => p.type == 'A').Aggregate(1, (acc, p) => acc * (2 + p.x)) + 100000 * pieces.Where(p => p.type == 'A').Sum(p => p.y),
        B = pieces.Where(p => p.type == 'B').Aggregate(1, (acc, p) => acc * (2 + p.x)) + 100000 * pieces.Where(p => p.type == 'B').Sum(p => p.y),
        C = pieces.Where(p => p.type == 'C').Aggregate(1, (acc, p) => acc * (2 + p.x)) + 100000 * pieces.Where(p => p.type == 'C').Sum(p => p.y),
        D = pieces.Where(p => p.type == 'D').Aggregate(1, (acc, p) => acc * (2 + p.x)) + 100000 * pieces.Where(p => p.type == 'D').Sum(p => p.y)
    };
}

void DrawPieces(Piece[] pieces)
{
    Console.WriteLine("#############");
    Console.Write("#");
    for (var i = 0; i < 11; i++)
    {
        phelp(pieces, i, 0);
    }
    Console.WriteLine('#');
    for (var i = 1; i <= roomDepth; i++)
    {
        if (i == 1)

            Console.Write("###");
        else
            Console.Write("  #");
        phelp(pieces, 2, i);
        Console.Write('#');
        phelp(pieces, 4, i);
        Console.Write('#');
        phelp(pieces, 6, i);
        Console.Write('#');
        phelp(pieces, 8, i);
        if (i == 1)
            Console.WriteLine("###");
        else
            Console.WriteLine('#');
    }
    Console.WriteLine("  #########");
    Console.WriteLine();
}

void phelp(Piece[] pieces, int x, int y)
{
    var p = pieces.FirstOrDefault(p => p.x == x && p.y == y);
    if (p.type == default(char))
    {
        Console.Write('.');
    }
    else
    {
        Console.Write(p.type);
    }
}

struct Piece
{
    public char type;
    public int x;
    public int y;
}

struct State
{
    public long A;
    public long B;
    public long C;
    public long D;
}
