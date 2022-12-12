string[] input = File.ReadAllLines("input.txt");
List<List<char>> field = input.Select(line => line.ToList()).ToList();

var start = FindAndReplace('S', 'a');
var end = FindAndReplace('E', 'z');

var steps = 0;
var visited = new HashSet<(int x, int y)>();
var current = new HashSet<(int x, int y)> { start };

while(!current.Contains(end))
{
	current = current.SelectMany(ReachableNeighbours).Where(p => !visited.Contains(p)).ToHashSet();

    visited.UnionWith(current);
    steps++;
}

var answer1 = steps;
Console.WriteLine($"Answer 1: {answer1}");


steps = 0;
visited = new HashSet<(int x, int y)>();
current = new HashSet<(int x, int y)> { end };

while (!current.Any(p => GetValue(p) == 'a'))
{
    current = current.SelectMany(ReachableNeighboursReversed).Where(p => !visited.Contains(p)).ToHashSet();

    visited.UnionWith(current);
    steps++;
}

var answer2 = steps;
Console.WriteLine($"Answer 2: {answer2}");

(int x, int y) FindAndReplace(char original, char replacement)
{
	for (int y = 0; y < field.Count; y++)
	{
		for (int x = 0; x < field[y].Count; x++)
		{
			if (field[y][x] == original)
			{
                field[y][x] = replacement;

                return (x, y);
			}
		}
	}

	throw new Exception();
}

bool IsValid((int x, int y) position)
{
	return 0 <= position.y && position.y < field.Count &&
		0 <= position.x && position.x < field[position.y].Count;
}

char GetValue((int x, int y) position) => field[position.y][position.x];

List<(int x, int y)> Neighbours((int x, int y) position)
{
    return new List<(int x, int y)>
    {
        (position.x, position.y - 1),
        (position.x, position.y + 1),
        (position.x - 1, position.y),
        (position.x + 1, position.y)
    }.Where(IsValid).ToList();
}

List<(int x, int y)> ReachableNeighbours((int x, int y) position)
{
	var value = GetValue(position);

	return Neighbours(position).Where(nb => GetValue(nb) - value <= 1).ToList();
}

List<(int x, int y)> ReachableNeighboursReversed((int x, int y) position)
{
    var value = GetValue(position);

    return Neighbours(position).Where(nb => value - GetValue(nb) <= 1).ToList();
}
