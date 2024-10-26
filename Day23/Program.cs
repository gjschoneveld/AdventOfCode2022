using Point = (int x, int y);

var input = File.ReadAllLines("input.txt");
var elves = new HashSet<Point>();

for (int y = 0; y < input.Length; y++)
{
    for (int x = 0; x < input[y].Length; x++)
    {
        if (input[y][x] == '#')
        {
            elves.Add((x, y));
        }
    }
}

int step;

for (step = 1; step <= 10; step++)
{
    Round(step);
}

var minX = elves.Min(p => p.x);
var maxX = elves.Max(p => p.x);
var minY = elves.Min(p => p.y);
var maxY = elves.Max(p => p.y);

var answer1 = (maxX - minX + 1) * (maxY - minY + 1) - elves.Count;
Console.WriteLine($"Answer 1: {answer1}");

while (Round(step))
{
    step++;
}

var answer2 = step;
Console.WriteLine($"Answer 2: {answer2}");

bool Round(int step)
{
    var changed = false;

    var moves = elves
        .Select(p => GetProposedPosition(p, step))
        .Where(p => p != null)
        .Select(p => p!.Value)
        .GroupBy(p => p.Proposed)
        .Where(g => g.Count() == 1)
        .Select(g => g.First())
        .ToList();

    foreach (var (position, newPosition) in moves)
    {
        elves.Remove(position);
        elves.Add(newPosition);
        changed = true;
    }

    return changed;
}

(Point Current, Point Proposed)? GetProposedPosition(Point position, int step)
{
    Dictionary<string, Point> neighbours = new()
    {
        ["NW"] = (position.x - 1, position.y - 1),
        ["N"] = (position.x, position.y - 1),
        ["NE"] = (position.x + 1, position.y - 1),
        ["W"] = (position.x - 1, position.y),
        ["E"] = (position.x + 1, position.y),
        ["SW"] = (position.x - 1, position.y + 1),
        ["S"] = (position.x, position.y + 1),
        ["SE"] = (position.x + 1, position.y + 1),
    };

    List<List<string>> rules = [
        ["N", "NE", "NW"],
        ["S", "SE", "SW"],
        ["W", "NW", "SW"],
        ["E", "NE", "SE"]
    ];

    if (neighbours.All(kv => !elves.Contains(kv.Value)))
    {
        return null;
    }

    for (int i = 0; i < rules.Count; i++)
    {
        var index = (i + step - 1) % rules.Count;
        
        if (rules[index].Select(d => neighbours[d]).All(p => !elves.Contains(p)))
        {
            return (position, neighbours[rules[index][0]]);
        }
    }

    return null;
}
