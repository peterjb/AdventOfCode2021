//Problem is to construct a graph and traverse it according to certain rules
//counting the number of paths from the starting node to the ending node

//dictionary to store the nodes by name for constructing the graph
var nodeDictionary = new Dictionary<string, Cave>();

//construct the graph. each line in the input is an edge
//a node is represented by the cave class
//iterate through the edges, adding new nodes when they occur
//and adding each node to the other's connected caves list
foreach (var line in File.ReadLines("input.txt"))
{
    var edge = line.Split('-');
    Cave node1, node2;
    if (!nodeDictionary.ContainsKey(edge[0]))
    {
        node1 = new Cave(edge[0]);
        nodeDictionary.Add(edge[0], node1);
    }
    else
    {
        node1 = nodeDictionary[edge[0]];
    }

    if (!nodeDictionary.ContainsKey(edge[1]))
    {
        node2 = new Cave(edge[1]);
        nodeDictionary.Add(edge[1], node2);
    }
    else
    {
        node2 = nodeDictionary[edge[1]];
    }

    node1.ConnectedCaves.Add(node2);
    node2.ConnectedCaves.Add(node1);
}


var pathCount = 0;
Visit(nodeDictionary["start"]);
Console.WriteLine($"Part 1 - {pathCount} paths.");

pathCount = 0;
Visit2(nodeDictionary["start"], false);
Console.WriteLine($"Part 2 - {pathCount} paths.");

/* Part 1
 * Recurse through the graph, looking for the end node
 *  If you reach the end node, you've found a path so increase pathCount and return
 *  If you reach a small node that has already been visited, this is not a valid path so just return
 *  Otherwise you are still on a valid path, but not at the end, so
 *      increase the node's visitCount (prevent small nodes from being visited twice)
 *      visit each connected cave
 *      decrement the node's visitCount (when we leave, we will be trying a new path and might end up back here)
 */
void Visit(Cave node)
{
    if (node.Name == "end")
    {
        pathCount++;
        return;
    }
    else if (node.Small && node.VisitCount > 0)
    {
        return;
    }
    else
    {
        node.VisitCount++;
        foreach (var cave in node.ConnectedCaves)
        {
            Visit(cave);
        }
        node.VisitCount--;
        return;
    }
}

/* Part 2
 * Similar to part 1 with revised rules about small caves
 * This time, we are allowed to visit ONE small node TWICE per path
 * 2 changes from part 1 then:
 *  1) we need to modify our small node check
 *      a) is it the start node and we've already been there?
 *      b) have we already visited a small node twice and this node has already been visited?
 *  2) when we visit a node, we need to keep track if it's the second time we've visited a small node
 *     and then undo that after we've visited all of its connected caves
 *     EDIT: intellisense offered an interesting suggestion of factoring out the smallNodeVisitedTwice
 *     variable into a parameter of Visit2. I think it's a cleaner, more compact way to do what i was doing. 
 *     **Feelings of Existential Dread** :)
 */ 
void Visit2(Cave node, bool smallNodeVisitedTwice)
{
    if (node.Name == "end")
    {
        pathCount++;
        return;
    }
    else if (node.Small && ((node.Name == "start" && node.VisitCount > 0) || (smallNodeVisitedTwice && node.VisitCount > 0)))
    {
        return;
    }
    else
    {
        node.VisitCount++;
        if (node.Small && node.VisitCount == 2)
        {
            smallNodeVisitedTwice = true;
        }
        foreach (var cave in node.ConnectedCaves)
        {
            Visit2(cave, smallNodeVisitedTwice);
        }
        node.VisitCount--;
    }
}

class Cave
{
    public Cave(string name)
    {
        Name = name;
        Small = char.IsLower(name[0]);
    }
    public bool Small { get; private set; }
    public int VisitCount { get; set; } = 0;
    public string Name { get; private set; }
    public IList<Cave> ConnectedCaves { get; private set; } = new List<Cave>();
}