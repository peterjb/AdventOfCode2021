/* 
 * #############
 * #...........#
 * ###D#B#A#C###
 *   #C#A#D#B#
 *   #########
 */


var energyCosts = new Dictionary<char, int>()
{
    { 'A', 1 },
    { 'B', 10 },
    { 'C', 100 },
    { 'D', 1000 }
};

var entrances = new List<int> { 2, 4, 6, 8 };

var state = (('D', 2, 1), ('C', 2, 2), ('B', 4, 1), ('A', 4, 2), ('A', 6, 1), ('D', 6, 2), ('C', 8, 1), ('B', 8, 2));

