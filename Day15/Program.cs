/* Day 15 - Shortest path problem. Since i didn't remember how Dijkstra's algorithm worked and i'm not smart enough to rederive it,
 * this is based on the wikipedia pseudo code for it: https://en.wikipedia.org/wiki/Dijkstra's_algorithm
 * 
 * Good example of making sure to use the right data structures. My initial solution was sorting a dictionary by value(!) every iteration
 * which is fine for small graphs, but for part 2 was... slow.
 * 
 * .NETs priority queue doesn't seem to implement the decrease_priority function from the wikipedia article
 * so this uses an approximation of the priority queue algorith that uses a dictionary and a sortedset.
 * 
 */

//input is the original graph
var input = File.ReadLines("input.txt").Select(line => line.Select(x => (int)char.GetNumericValue(x)).ToArray()).ToArray();

//input2 is the expanded graph for part 2
var input2 = new int[input.Length * 5][];
for(var r = 0; r < input2.Length; r++)
{
    input2[r] = new int[input[0].Length * 5];
}

var h = input.Length;
var w = input[0].Length;

// construct the expanded grid according to the rules in the problem
// grid is repeated over a 5x5 grid. probably don't actually need to construct a 2nd grid
for(var rr = 0; rr < 5; rr++)
{
    for(var cc = 0; cc < 5; cc++)
    {
        for(var r = 0; r < h; r++)
        {
            for(var c = 0; c < w; c++)
            {
                //increase by 1 for each sub grid to the right and down from the top left we are
                //values are between 1 and 9 inclusive
                //(I was originally doing a modulus and an if statement here, but i saw someone elses solution 
                //with the subtraction :facepalm:
                var risk = input[r][c] + rr + cc;
                if (risk > 9)
                    risk -= 9;
                input2[r + rr * h][c + cc * w] = risk;
            }
        }
    }
}

//The approximation of a priority queue
//  distances stores the distance from the origin of every node in the graph as a dictionary
//      maping a node (point on the grid) to a distance
//  vertices is a sortedset of tuples. each tuple is (row, col, distance)
//      they are sorted by distance (to the origin)
//  if we need to update a vertex's priority (distance from origin) we can lookup the distance 
//  it's currently at and then use that to find the node in the vertices set, remove it, and then
//  re-add it with the updated distance
Dictionary<(int, int), int> distances;
SortedSet<(int, int, int)> vertices;

Console.WriteLine(shortestPath(input));
Console.WriteLine(shortestPath(input2));

//Implements Dijkstra's algorithm
//The cost of moving to an adjacent node in the graph is stored as the data in the graph array
int shortestPath(int[][] graph)
{
    //initialize the distances dictionary
    distances = new Dictionary<(int, int), int>();

    //get the width and height of the grid
    var h = graph.Length;
    var w = graph[0].Length;

    //the visited array just makes it so we don't have to search the vertices set to see if a node is still in the set
    var visited = new bool[h, w];

    //create the sorted set. the tricky part for me was the comparer. It needs to sort by distance, but also needs to compare = 
    //for insertion and removal, so first it looks at the distance. if there is no difference in the distance, in order to 
    //compare =, we compare the rows and the cols, but offset the cols difference. otherwise (1,0,5) = (0,1,5)
    //  no offset: 1 - 0 + 0 - 1 = 0
    //  offset: 1 - 0 + a*(0 - 1) = 1 - a
    //  pick 'a' big enough so the right term can never cancel the left
    //maybe it'd be easier/clearer to just do another compare?
    vertices = new SortedSet<(int, int, int)>(Comparer<(int, int, int)>.Create(((int, int, int) lhs, (int, int, int) rhs) =>
    {
        var diff = lhs.Item3 - rhs.Item3;
        diff = diff == 0 ? lhs.Item1 - rhs.Item1 + h * (lhs.Item2 - rhs.Item2) : diff; 
        return diff;
    }));
    
    //populate the vertices and distances data
    //initially all the vertices are at max distance from the origin as per the algorithm...
    for (var r = 0; r < h; r++)
    {
        for (var c = 0; c < w; c++)
        {
            vertices.Add((r, c, int.MaxValue));
            distances.Add((r, c), int.MaxValue);
        }
    }

    //... except for the origin point which is set as distance 0
    //using this system we have to remove the vertex and then re-add it to update it's distance
    vertices.Remove((0, 0, int.MaxValue));
    vertices.Add((0, 0, 0));

    //do the algorithm loop
    int i = 0, j = 0, d = 0;
    while (true)
    {
        //find the vertex that we haven't yet visited that is closest to the origin
        var current = vertices.First();
        i = current.Item1;
        j = current.Item2;
        d = current.Item3;

        //remove that vertex from the set of vertices
        vertices.Remove(current);

        //if it's the destination vertex, we are done
        if (i == h - 1 && j == w - 1)
            break;

        //mark that we've visted this vertex. we could just check to see if the vertex is in the set
        //but i was adapting the previous code and this should be faster anyway :p
        visited[i, j] = true;

        //check all the neighbors of this vertex to see of far away they are from the origin through the current vertex
        //more details by the helper function
        if (i > 0 && !visited[i - 1, j])
        {
            updateDistances(i - 1, j, d, graph);
        }
        if (j > 0 && !visited[i, j - 1])
        {
            updateDistances(i, j - 1, d, graph);
        }
        if (i < h - 1 && !visited[i + 1, j])
        {
            updateDistances(i + 1, j, d, graph);
        }
        if (j < w - 1 && !visited[i, j + 1])
        {
            updateDistances(i, j + 1, d, graph);
        }
    }

    //if we get here, we are done, so return the distance of the end point to the starting point
    return distances[(h - 1, w - 1)];
}

//if the cost of moving to this vertex (i,j) from the current vertex (represented by it's distance to the origin, d) is cheaper than any previous route to this vertex,
//update the distance to the origin from this vertex
void updateDistances(int i, int j, int d, int[][] graph)
{
    var cost = graph[i][j]; //cost of going to this vertex
    var oldD = distances[(i, j)]; //smallest distance to this vertex from the origin
    var newD = d + cost; //potential new smallest distance to this vertex going through the curent vertex (from the calling function)
    
    //if the new distance is smaller than the old distance, update the nodes distance to the origin
    if (newD < oldD)
    {
        distances[(i, j)] = newD; //update the distance
        vertices.Remove((i, j, oldD)); //remove the vertex from the set
        vertices.Add((i, j, newD)); //add it back with the new distance for sorting
    }
}