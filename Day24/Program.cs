using Map = string[];
using Point = (int x, int y);

var map = File.ReadAllLines("input.txt");
var start = (x: map[0].IndexOf('.'), y: 0);
var end = (x: map[^1].IndexOf('.'), y: map.Length - 1);
var blizzards = new List<Blizzard>();

for (int y = 1; y < map.Length - 1; y++)
{
    for (int x = 1; x < map[y].Length - 1; x++)
    {
        if (map[y][x] == '.')
        {
            continue;
        }

        var direction = map[y][x] switch
        {
            '<' => Direction.Left,
            '>' => Direction.Right,
            '^' => Direction.Up,
            'v' => Direction.Down,
            _ => throw new() 
        };

        blizzards.Add(new()
        {
            Direction = direction,
            Position = (x, y)
        });
    }
}

var steps = 0;
var positions = new HashSet<Point>();

Walk(start, end);

var answer1 = steps;
Console.WriteLine($"Answer 1: {answer1}");

Walk(end, start);
Walk(start, end);

var answer2 = steps;
Console.WriteLine($"Answer 2: {answer2}");

void Print()
{
    var blizzardsByLocation = blizzards.GroupBy(b => b.Position).ToDictionary(g => g.Key);

    Console.WriteLine(steps);

    for (int x = 0; x < map[0].Length; x++)
    {
        if ((x, 0) == start)
        {
            if (positions.Contains(start))
            {
                Console.Write('E');
            }
            else
            {
                Console.Write('.');
            }
        }
        else
        {
            Console.Write('#');
        }
    }

    Console.WriteLine();

    for (int y = 1; y < map.Length - 1; y++)
    {
        Console.Write('#');

        for (int x = 1; x < map[y].Length - 1; x++)
        {
            if (positions.Contains((x, y)))
            {
                Console.Write('E');
            }
            else if (!blizzardsByLocation.ContainsKey((x, y)))
            {
                Console.Write('.');
            }
            else if (blizzardsByLocation[(x, y)].Count() > 1)
            {
                // clip to 9 so it fits in the cell
                Console.Write(Math.Min(9, blizzardsByLocation[(x, y)].Count()));
            }
            else
            {
                Console.Write(blizzardsByLocation[(x, y)].First().Direction switch
                {
                    Direction.Right => '>',
                    Direction.Down => 'v',
                    Direction.Left => '<',
                    Direction.Up => '^',
                    _ => throw new()
                });
            }
        }

        Console.WriteLine('#');
    }

    for (int x = 0; x < map[^1].Length; x++)
    {
        if ((x, map.Length - 1) == end)
        {
            if (positions.Contains(end))
            {
                Console.Write('E');
            }
            else
            {
                Console.Write('.');
            }
        }
        else
        {
            Console.Write('#');
        }
    }

    Console.WriteLine();
    Console.WriteLine();
}

void Walk(Point start, Point end)
{
    positions = [start];

    //Print();

    while (!positions.Contains(end))
    {
        blizzards = blizzards.Select(b => b.Step(map)).ToList();
        var occupied = blizzards.Select(b => b.Position).ToHashSet();

        var newPositions = new HashSet<Point>();

        foreach (var (x, y) in positions)
        {
            List<Point> candidates = [
                (x, y - 1),
                (x - 1, y),
                (x, y),
                (x + 1, y),
                (x, y + 1),
            ];

            var valid = candidates.Where(p => Blizzard.IsValid(map, p) && !occupied.Contains(p));
            newPositions.UnionWith(valid);
        }

        positions = newPositions;
        steps++;

        //Print();
    }
}

enum Direction
{
    Right,
    Down,
    Left,
    Up,
}

record Blizzard
{
    public Direction Direction { get; set; }
    public Point Position { get; set; }

    public static bool IsValid(Map map, Point position)
    {
        return 0 <= position.y && position.y < map.Length &&
            0 <= position.x && position.x < map[position.y].Length &&
            map[position.y][position.x] != '#';
    }

    public static int Value(Map map, Point position)
    {
        return map[position.y][position.x];
    }

    public static Point Delta(Direction direction)
    {
        return direction switch
        {
            Direction.Left => (-1, 0),
            Direction.Right => (1, 0),
            Direction.Up => (0, -1),
            Direction.Down => (0, 1),
            _ => throw new()
        };
    }

    public static Point Add(Point position, Point delta)
    {
        return (position.x + delta.x, position.y + delta.y);
    }

    public Blizzard Step(Map map)
    {
        var delta = Delta(Direction);
        var next = Add(Position, delta);

        if (IsValid(map, next))
        {
            return this with { Position = next };
        }

        // not valid; step in reversed direction to find start
        delta = (-delta.x, -delta.y);
        next = Position;
        var result = this;

        while (IsValid(map, next))
        {
            result = result with { Position = next };
            next = Add(next, delta);
        }

        return result;
    }
}
