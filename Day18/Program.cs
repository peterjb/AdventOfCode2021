/* WORK IN PROGRESS, DOES NOT WORK */
/* WORK IN PROGRESS, DOES NOT WORK */
/* WORK IN PROGRESS, DOES NOT WORK */

var input = File.ReadLines("sample.txt");

var numbers = new List<SnailFishNumber>();

foreach(var line in input)
{
    numbers.Add(ParseSnailFishNumber(new StringReader(line)) as SnailFishNumber);
}

SnailFishNumber ParseSnailFishNumber(StringReader reader)
{
    var c = reader.Read(); // '['
    var n = reader.Peek();
    object left, right;
    if (n == '[')
    {
        left = ParseSnailFishNumber(reader);
    }
    else
    {
        left = (int)char.GetNumericValue((char)reader.Read());
    }
    reader.Read(); // ','
    n = reader.Peek();
    if (n == '[')
    {
        right = ParseSnailFishNumber(reader);
    }
    else
    {
        right = (int)char.GetNumericValue((char)reader.Read());
    }
    reader.Read(); // ']'
    return new SnailFishNumber(left, right);   
}

Console.WriteLine($"{numbers[0]}: {numbers[0].Magnitude}");

Console.Write($"{numbers[0] + numbers[1]}");
var x = 0;


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
        var r = Reduce(result, 0);
        while (r.reduced)
        {
            r = Reduce(r.result, 0);
        }
        return r.result as SnailFishNumber;
    }
    
    private static (object result, bool reduced, int? leftExplosion, int? rightExplosion) Reduce(object number, int depth)
    {
        if(depth == 4 && number is SnailFishNumber sn)
        {
            return (0, true, sn.Left as int?, sn.Right as int?);
        }
        else if(number is int regularNumber)
        {
            if(regularNumber > 9)
            {
                return (new SnailFishNumber((int)Math.Floor(regularNumber / 2.0), (int)Math.Ceiling(regularNumber / 2.0)), true, null, null);
            }
            else
            {
                return (regularNumber, false, null, null);
            }
        }
        else
        {
            var snailFishNumber = number as SnailFishNumber;
            var l = Reduce(snailFishNumber.Left, depth + 1);
            if(l.reduced)
            {
                if(l.rightExplosion != null)
                {
                    if(snailFishNumber.Right is int sfir)
                    {
                        return (new SnailFishNumber(l.result, sfir + l.rightExplosion.Value), true, l.leftExplosion, null);
                    }
                    else
                    {
                        return (new SnailFishNumber(l.result, ExplodeDown(l.rightExplosion.Value, snailFishNumber.Right as SnailFishNumber)), true, l.leftExplosion, null);
                    }
                }
                else
                {
                    return (new SnailFishNumber(l.result, snailFishNumber.Right), true, l.leftExplosion, null);
                }
            }
            else
            {
                var r = Reduce(snailFishNumber.Right, depth + 1);
                if(r.reduced)
                {
                    if (r.leftExplosion != null)
                    {
                        if (l.result is int sfil)
                        {
                            l.result = (int)l.result + r.leftExplosion.Value;
                        }
                    }
                }
                return (new SnailFishNumber(l.result, r.result), r.reduced, null, r.rightExplosion);
            }
        }
    }

    
    private static SnailFishNumber ExplodeDown(int right, SnailFishNumber rest)
    {
        if(rest.Left is int leftRegularNumber)
        {
            return new SnailFishNumber(right + leftRegularNumber, rest.Right);
        }
        else
        {
            return ExplodeDown(right, rest.Left as SnailFishNumber);
        }
    }
}