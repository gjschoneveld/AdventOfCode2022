var input = File.ReadAllLines("input.txt");
var cubes = input.Select(Parse).ToHashSet();

var answer1 = cubes.Sum(cube => Neighbours(cube).Count(nb => !cubes.Contains(nb.position)));
Console.WriteLine($"Answer 1: {answer1}");


var min = (x: cubes.Min(c => c.x - 1), y: cubes.Min(c => c.y - 1), z: cubes.Min(c => c.z - 1));
var max = (x: cubes.Max(c => c.x + 1), y: cubes.Max(c => c.y + 1), z: cubes.Max(c => c.z + 1));

var current = new HashSet<(int x, int y, int z)> { min };
var visited = new HashSet<(int x, int y, int z)>();
var reached = new HashSet<((int x, int y, int z) position, Side side)>();

while (current.Count > 0)
{
    var next = new HashSet<(int x, int y, int z)>();

    foreach (var water in current)
    {
        foreach (var neighbour in Neighbours(water))
        {
            if (cubes.Contains(neighbour.position))
            {
                // cube
                reached.Add(neighbour);
            }
            else if (!visited.Contains(neighbour.position) && WithinBounds(neighbour.position))
            {
                // water not visited yet
                next.Add(neighbour.position);
            }
        }
    }

    visited.UnionWith(current);
    current = next;
}

var answer2 = reached.Count;
Console.WriteLine($"Answer 2: {answer2}");

bool WithinBounds((int x, int y, int z) position)
{
    return min.x <= position.x && position.x <= max.x &&
        min.y <= position.y && position.y <= max.y &&
        min.z <= position.z && position.z <= max.z;
}

(int x, int y, int z) Parse(string line)
{
    var parts = line.Split(',').Select(int.Parse).ToList();

    return (x: parts[0], y: parts[1], z: parts[2]);
}

List<((int x, int y, int z) position, Side side)> Neighbours((int x, int y, int z) position)
{
    return new()
    {
        ((position.x + 1, position.y, position.z), Side.Right),
        ((position.x - 1, position.y, position.z), Side.Left),
        ((position.x, position.y + 1, position.z), Side.Top),
        ((position.x, position.y - 1, position.z), Side.Bottom),
        ((position.x, position.y, position.z + 1), Side.Front),
        ((position.x, position.y, position.z - 1), Side.Back)
    };
}

enum Side
{
    Right,
    Left,
    Top,
    Bottom,
    Front,
    Back
}
