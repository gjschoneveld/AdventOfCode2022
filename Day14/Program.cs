var input = File.ReadAllLines("input.txt");
var structures = input.Select(ParsePath).ToList();
var rock = RockLocations(structures);
var floor = rock.Max(p => p.y) + 2;
var entry = (x: 500, y: 0);

var answer1 = Simulate(false);
Console.WriteLine($"Answer 1: {answer1}");

var answer2 = Simulate(true);
Console.WriteLine($"Answer 2: {answer2}");

List<(int x, int y)> ParsePath(string line)
{
    return line.Split(" -> ").Select(ParsePoint).ToList();
}

(int x, int y) ParsePoint(string raw)
{
    var values = raw.Split(',').Select(int.Parse).ToList();

    return (values[0], values[1]); 
}

(int x, int y) Direction((int x, int y) start, (int x, int y) end)
{
    return (Math.Sign(end.x - start.x), Math.Sign(end.y - start.y));
}

HashSet<(int x, int y)> RockLocations(List<List<(int x, int y)>> paths)
{
    var rock = new HashSet<(int x, int y)>();

    foreach (var path in paths)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            var start = path[i];
            var end = path[i + 1];

            var position = start;
            var direction = Direction(start, end);

            while (position != end)
            {
                rock.Add(position);

                position = (position.x + direction.x, position.y + direction.y);
            }

            rock.Add(position);
        }
    }

    return rock;
}

(int x, int y)? DropSand(HashSet<(int x, int y)> rock, HashSet<(int x, int y)> sand, bool stop)
{
    var position = entry;

    var moved = true;

    while (moved)
    {
        moved = false;

        var nextPositions = new List<(int x, int y)>
        {
            (position.x, position.y + 1),
            (position.x - 1, position.y + 1),
            (position.x + 1, position.y + 1)
        };

        foreach (var next in nextPositions)
        {
            if (!rock.Contains(next) && !sand.Contains(next))
            {
                position = next;
                moved = true;

                break;
            }
        }

        if (position.y + 1 == floor)
        {
            return stop ? position : null;
        }
    }

    return position;
}

int Simulate(bool stop)
{
    var sand = new HashSet<(int x, int y)>();

    while (true)
    {
        var position = DropSand(rock, sand, stop);

        if (position == null)
        {
            break;
        }

        sand.Add(position.Value);

        if (position == entry)
        {
            break;
        }
    }

    return sand.Count;
}
