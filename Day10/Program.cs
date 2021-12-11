var open  = "([{<";
var close = ")]}>";
var stack = new Stack<char>();

//Taking each part separately

//Part 1 - Corrupted Lines

//map the character to the score it receives
var corruptScores = new Dictionary<char, int>()
{
    {')', 3 },
    {']', 57 },
    {'}', 1197 },
    {'>', 25137 }
};

//Simple stack based solution. Read in each character of the line. If it is an opening character, push it onto the stack
//If is a closing character, pop the stack if it has anything on it and check to make sure the closing character
//matches the opening character that was on the stack. If it doesn't, or if the stack was empty, we have a corrupted line
//so calculate the score and return;
var syntaxErrorScore = File.ReadLines("input.txt").Sum(line =>
{
    stack.Clear();
    var index = 0;
    do
    {
        var c = line[index];
        if (open.Contains(c))
        {
            stack.Push(c);
        }
        else if (!stack.Any()) //stack is empty but we encountered a close character
        {
            return corruptScores[c];
        }
        else
        {
            var p = stack.Pop();
            if (open.IndexOf(p) != close.IndexOf(c))
            {
                return corruptScores[c];
            }
        }
        index++;
    } while (index < line.Length);
    return 0;
});

Console.WriteLine($"Syntax Error Score: {syntaxErrorScore}");

//Part 2 - Incomplete Lines
//Do the same thing as part 1, just ignore the corrupted lines (return score of 0)
//If we get to the end without any corruption, we can look at the stack to see what's left to close out the line
//and calculate the score. This time though, because of how scoring works, instead of summing the results, enumerate a list of scores
//and find the median (guaranteed odd number of scores)

//we can just look at what's in the stack when a line is done, which is all open characters, to calculate the score
var incompleteScores = new Dictionary<char, int>()
{
    {'(', 1 },
    {'[', 2 },
    {'{', 3 },
    {'<', 4 }
};

var autoCompleteScores = File.ReadLines("input.txt").Select(line =>
{
    stack.Clear();
    var index = 0;
    do
    {
        var c = line[index];
        if (open.Contains(c))
        {
            stack.Push(c);
        }
        else if (!stack.Any())
        {
            return 0; //corrupt
        }
        else
        {
            var p = stack.Pop();
            if (open.IndexOf(p) != close.IndexOf(c))
            {
                return 0; //corrupt
            }
        }
        index++;
    } while (index < line.Length);

    //calculate the score. The numbers get big, so use int64
    //elements are enumerated based on how they would come out if popped, so aggregate on the stack itself
    if (stack.Any())
    {
        return stack.Aggregate(0L, (a, c) => 5 * a + incompleteScores[c]);
    }
    else
    {
        return 0; //should never get here because all lines are either corrupt or incomplete
    }
}).Where(score => score > 0).OrderBy(score => score).ToArray();  
//scores of 0 were corrupted lines, so ignore. Need to stamp out the array to actually calculate the scores

Console.WriteLine($"Autocomplete Score: {autoCompleteScores[autoCompleteScores.Length / 2]}");

