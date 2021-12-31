var numbers = File.ReadLines("input.txt")
    .Select(l => SnailFishNumber.Parse(new StringReader(l))).ToArray();

//Part 1 is the hard part this time, but ultimately we are just adding up numbers:
var result = numbers.Aggregate((a, b) => a + b);
Console.WriteLine($"Result : {result} : Magnitude : {result.Magnitude}" );

//Part 2, just do all the pairwise sums, both directions, and find the max
long max = long.MinValue;

for(var i = 0; i < numbers.Length; i++)
{
    for(var j = 0; j < numbers.Length; j++)
    {
        if(i != j)
        {
            max = Math.Max(max, (numbers[i] + numbers[j]).Magnitude);
        }
    }
}

Console.WriteLine($"Max sum magnitude: {max}.");

/*
 * A Snail Fish number is just a tree of numbers
 *   Adding 2 numbers is simple (lhs + rhs) -> (lhs, rhs), but the numbers have to be reduced
 *     Reduction is by exploding numbers nested more than 4 deep, and splitting numbers > 9 into a pair
 *        Exploding is the tricky part because we need to track the explosion back through the left and right
 *        sides of the tree as necessary
 *   
 *   I got stuck because i thought the order of operations was the first reduction operation that applied left to right
 *   But that was a misread, it's 
 *      1) do every explosion, one at a time, left to right, until no more explosions. 
 *      2) Then check for a split left to right. 
 *      3) If we didn't split, we are done. Otherwise go back to 1.
 * 
 */
class SnailFishNumber
{
    public SnailFishNumber(object left, object right)
    {
        Left = left;   
        Right = right;
    }

    public readonly object Left;
    public readonly object Right;

    private long? magnitude = null;
    public long Magnitude {
        get
        {
            if (magnitude == null)
            {
                if(Left is int l)
                {
                    magnitude = 3 * l;
                }
                else
                {
                    magnitude = 3 * (Left as SnailFishNumber).Magnitude;
                }
                if (Right is int r)
                {
                    magnitude += 2 * r;
                }
                else
                {
                    magnitude += 2 * (Right as SnailFishNumber).Magnitude;
                }
            } 
            return magnitude.Value;
        }
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append('[');
        sb.Append(Left.ToString());
        sb.Append(',');
        sb.Append(Right.ToString());
        sb.Append(']');
        return sb.ToString();
    }

    public static SnailFishNumber operator +(SnailFishNumber lhs, SnailFishNumber rhs)
    {
        var result = new SnailFishNumber(lhs, rhs);

        while (true)
        {
            var r = Explode(result, 0);
            while (r.exploded)
            {
                r = Explode(r.result, 0);
            }
            var s = Split(r.result); 

            result = s.result as SnailFishNumber;

            if (!s.split)
                break;
        }

        return result;
    }

    /* Here was the tricky part for me, checking for explosions
     * 
     *   Traverse the tree, building the reduced snail fish number.
     *      If we reach depth 4 and we aren't at a leaf, we need to explode the node.
     *      Exploding is making the current node a 0, and returning the left and right parts to be 
     *      added to the first number to the left and right of where we are at in the tree
     *      I think we are guaranteed based on the definition of these numbers and the reduction algorithm that there will never be 
     *      a non-leaf node deeper than this, meaning left and right will always be leaves (regular numbers)
     *      
     *      Once we explode, we are done checking this round, but we need to deal with the remainders:
     *          If we were exploding a left branch, pass the right remainder down the right branch, and then return the left remainder back up the tree
     *              ExplodeRight function - Go down the branch looking for the *Left*most number and add the remainder to it
     *          If we were exploding a right branch, pass the left remainder down the left branch, and then return the right remainder back up the tree
     *              ExplodeLeft function - Go down the branch looking for the *Right*most number and add the remainder to it
     *           
     */
    private static (object result, bool exploded, int? leftExplosion, int? rightExplosion) Explode(object number, int depth)
    {
        if (depth == 4 && number is SnailFishNumber sn)
        {
            return (0, true, sn.Left as int?, sn.Right as int?);
        }
        else if (number is int regularNumber)
        {
            return (regularNumber, false, null, null);
        }
        else
        {
            var snailFishNumber = number as SnailFishNumber;
            var l = Explode(snailFishNumber.Left, depth + 1);
            if (l.exploded)
            {
                if (l.rightExplosion != null)
                {
                    if (snailFishNumber.Right is int rnum)
                    {
                        return (new SnailFishNumber(l.result, rnum + l.rightExplosion.Value), true, l.leftExplosion, null);
                    }
                    else
                    {
                        return (new SnailFishNumber(l.result, ExplodeDownRight(l.rightExplosion.Value, snailFishNumber.Right as SnailFishNumber)), true, l.leftExplosion, null);
                    }
                }
                else
                {
                    return (new SnailFishNumber(l.result, snailFishNumber.Right), true, l.leftExplosion, null);
                }
            }
            else
            {
                var r = Explode(snailFishNumber.Right, depth + 1);
                if (r.exploded)
                {
                    if (r.leftExplosion != null)
                    {
                        if (snailFishNumber.Left is int lnum)
                        {
                            return (new SnailFishNumber(lnum + r.leftExplosion.Value, r.result), true, null, r.rightExplosion);
                        }
                        else
                        {
                            return (new SnailFishNumber(ExplodeDownLeft(r.leftExplosion.Value, snailFishNumber.Left as SnailFishNumber), r.result), true, null, r.rightExplosion);
                        }
                    }
                    else
                    {
                        return (new SnailFishNumber(snailFishNumber.Left, r.result), true, null, r.rightExplosion);
                    }
                }
                return (snailFishNumber, false, null, null);
            }
        }
    }

    private static SnailFishNumber ExplodeDownRight(int right, SnailFishNumber rest)
    {
        if (rest.Left is int leftRegularNumber)
        {
            return new SnailFishNumber(right + leftRegularNumber, rest.Right);
        }
        else
        {
            return new SnailFishNumber(ExplodeDownRight(right, rest.Left as SnailFishNumber), rest.Right);
        }
    }

    private static SnailFishNumber ExplodeDownLeft(int left, SnailFishNumber rest)
    {
        if (rest.Right is int rightRegularNumber)
        {
            return new SnailFishNumber(rest.Left, left + rightRegularNumber);
        }
        else
        {
            return new SnailFishNumber(rest.Left, ExplodeDownLeft(left, rest.Right as SnailFishNumber));
        }
    }

    /* split is simpler, just traverse the tree looking for the first number that is > 9 and split it
     * according to the rules. We just have to stop splitting after the first split.
     */
    private static (object result, bool split) Split(object number)
    {
        if(number is int regularNumber)
        {
            if(regularNumber > 9)
            {
                return (new SnailFishNumber((int)Math.Floor(regularNumber / 2.0), (int)Math.Ceiling(regularNumber / 2.0)), true);
            }
            else
            {
                return (regularNumber, false);
            }
        }
        else
        {
            var s = number as SnailFishNumber;
            var l = Split(s.Left);
            if (l.split)
                return (new SnailFishNumber(l.result, s.Right), true);
            else
            {
                var r = Split(s.Right);
                if (r.split)
                    return (new SnailFishNumber(s.Left, r.result), true);


                return (number, false);
            }
        }
    } 

    public static SnailFishNumber Parse(StringReader reader)
    {
        reader.Read(); // '['
        var n = reader.Peek();
        object left, right;
        if (n == '[')
        {
            left = Parse(reader);
        }
        else
        {
            left = (int)char.GetNumericValue((char)reader.Read());
        }
        reader.Read(); // ','
        n = reader.Peek();
        if (n == '[')
        {
            right = Parse(reader);
        }
        else
        {
            right = (int)char.GetNumericValue((char)reader.Read());
        }
        reader.Read(); // ']'
        return new SnailFishNumber(left, right);
    }
}