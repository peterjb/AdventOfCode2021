var input = File.ReadLines("input.txt").Select(
    x =>
    {
        var split = x.Split('|');
        //input is split into the 10 patterns that represent each digit (digitPatterns)
        //and the 4 outputs that needs to be translated (output)
        return new
        {
            digitPatterns = split[0].Split(' ', StringSplitOptions.RemoveEmptyEntries),
            output = split[1].Split(' ', StringSplitOptions.RemoveEmptyEntries)
        };
    }
);

//part 1 seems straight forward, just count the number of outputs that have length 2,3,4 or 7, which correspond to the digits 1,4,7 and 8
Console.WriteLine($"Count of simple outputs: {input.Sum(x => x.output.Count(y => y.Length == 2 || y.Length == 3 || y.Length == 4 || y.Length == 7))}");

/* Part 2 was a bit of a struggle for me. I imagine you can work it out in an SAT logic question style, but i got stuck in my head
 * the notion that i should try to scan through permutations of the wires until finding one that worked for all of the patterns.
 * the result is pretty torturous, but it works...
 *
 * Also, so far as I know, C# does not have a built in permutaion generation, so I've got an attempt at that below as well
 * 
 * The basic idea is to try each possible way the wires could be hooked up to the displays by permuting the string "abcdefg"
 * The position in the array corresponds to which part of the display it is hooked up to.
 *    0000
 *   1    2
 *   1    2
 *    3333
 *   4    5
 *   4    5
 *    6666
 * So the initial permutation would have 'a' mapped to the top light, 'b' mapped to the top left light, etc.
 *
 * Then check each pattern to see if it fits. E.G. if the pattern is 'fc', we need to check 'fc' and 'cf to see if 
 * either of those lights up 2 and 5, since we know that if the length of the pattern is 2, the digit is 1
 * 
 * This works for 1,4,7 and 8, which have unique lengths (8 we don't need to check because it will always match).
 * If the pattern has a lenght of 5, it could represent a 2,3 or 5. If it has length 6, it could represent 0,6 or 9.
 * For those, we have to check each permutation of the pattern against all three possibilites
 * 
 * So the algorithm is 
 *  for each line in the input 
 *    for each permutation of the wire map
 *      for each pattern
 *        for each permutation of the pattern
 *          -- Check if the pattern fits. If it does, make note of what digit it represents
 *          -- When a digit pattern fails to fit, move to the next permutation of the wire map, no need to check any more digit patterns
 *          
 */


//inputMap keeps track of which patterns correspond to which digits for each set of patterns
//inputMap[0] = the index into the patterns where the pattern for 0 lives
//inputMap[5] = the index into the patterns where the pattern for 5 lives
var inputMap = new int?[10];

//we have to keep a sum of the outputs
int sum = 0;

foreach (var note in input)
{
    //sorting the array makes a big difference in how long this takes
    //do the smaller digit patterns first. I believe this is because there are many fewer digit permutations
    //to check early on so we filter down the list of possible wire maps more quickly?
    //my intuition was the opposite :(
    //short to long takes a couple seconds, long to short takes a couple minutes
    Array.Sort(note.digitPatterns, (a, b) => a.Length - b.Length);

    //for every possible wire map, check the current input lines digit patterns against the map
    //if checkDigits returns true, we can stop because we have found the correct permutation
    foreach (var wirePermutation in "abcdefg".ToArray().Permutations())
    {
        if (checkDigits(wirePermutation, note.digitPatterns, inputMap))
            break;
    }

    //now that we have the correct permutation, determine the output number and add it to the running total
    int number = 0;

    //get the max tens place for constructing the number e.g. 1000, 100, 10, 1
    var placeValue = (int)Math.Pow(10, note.output.Length - 1);

    //for each digit in the output (each display has 4 digits)
    for (var i = 0; i < note.output.Length; i++)
    {
        //output[0] is left most digit
        var digitPattern = note.output[i];

        
        //search through the permutations of the output pattern for the input pattern
        //once we find which input pattern it matches, we can lookup what digit that represents in the input map
        foreach (var op in digitPattern.ToArray().Permutations().Select(x => new string(x)))
        {
            var index = Array.IndexOf(note.digitPatterns, op);
            if(index > -1)
            {
                number += Array.IndexOf(inputMap,index) * placeValue;
                break;
            }
        }

        placeValue /= 10;
    }

    sum += number;
}

Console.WriteLine($"Sum of outputs: {sum}");

//where all the heavy lifting happens. Try to fit each digit pattern into the wire map
//if we find them all return true, otherwise break out and return false at first mismatch
static bool checkDigits(char[] wireMap, string[] digitPatterns, int?[] inputMap)
{
    for (var i = 0; i < digitPatterns.Length; i++)
    {
        var d = digitPatterns[i];
        var match = false;
        int? digitMatch;
        foreach (var dp in d.ToArray().Permutations())
        {
            digitMatch = checkDigit(wireMap, dp);
            if(digitMatch.HasValue)
            {
                inputMap[digitMatch.Value] = i;
                match = true;
                break;
            }
        }
        if(!match)
        {
            return false;
        }
    }
    return true;
}

//Return the number a digit map represents if it fits in the current wire map, otherwise null
static int? checkDigit(char[] wireMap, char[] digitMap)
{
    switch (digitMap.Length)
    {
        case 2: //1
            return checkDigitHelper(wireMap, digitMap, 2, 5) ? 1 : null;
        case 3: //7
            return checkDigitHelper(wireMap, digitMap, 0, 2, 5) ? 7 : null;
        case 4: //4
            return checkDigitHelper(wireMap, digitMap, 1, 2, 3, 5) ? 4 : null;
        case 5: //2,3,5
            {
                if (checkDigitHelper(wireMap, digitMap, 0, 2, 3, 4, 6))
                    return 2;
                else if (checkDigitHelper(wireMap, digitMap, 0, 2, 3, 5, 6))
                    return 3;
                else if (checkDigitHelper(wireMap, digitMap, 0, 1, 3, 5, 6))
                    return 5;
                else
                    return null;
            }
        case 6: //0,6,9
            {
                if (checkDigitHelper(wireMap, digitMap, 0, 1, 2, 4, 5, 6))
                    return 0;
                else if (checkDigitHelper(wireMap, digitMap, 0, 1, 3, 4, 5, 6))
                    return 6;
                else if (checkDigitHelper(wireMap, digitMap, 0, 1, 2, 3, 5, 6))
                    return 9;
                else
                    return null;
            }
        case 7: //8
            return 8;
        default:
            throw new Exception("Shouldn't be here");
    }
}

//indexes are the segment indexes to check in the wire map for the specific digit we are looking to match
static bool checkDigitHelper(char[] wireMap, char[] digitMap, params int[] indexes)
{
    return Enumerable.Range(0, digitMap.Length).All(i => digitMap[i] == wireMap[indexes[i]]);

}

public static class Extensions
{

    public static IEnumerable<T[]> Permutations<T>(this T[] ts)
    {
        T[] result = new T[ts.Length];

        if (ts.Length == 1)
        {
            result[0] = ts[0];
            yield return result;
        }
        else
        {
            T[] ns = new T[ts.Length - 1];
            for (var i = 0; i < ts.Length; i++)
            {
                result[0] = ts[i];
                CopyAllBut(ts, ns, i);
                foreach (var s in ns.Permutations())
                {
                    Array.Copy(s, 0, result, 1, s.Count());
                    yield return result;
                }
            }
        }
    }

    static void CopyAllBut<T>(T[] source, T[] destination, int omit)
    {
        for (int i = 0; i < omit; i++)
        {
            destination[i] = source[i];
        }
        for (int i = omit; i < destination.Length; i++)
        {
            destination[i] = source[i + 1];
        }
    }

}

