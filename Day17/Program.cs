using System.Security.Cryptography;
using System.Text.Json;

var input = File.ReadAllText("input.txt");

var shapeText = File.ReadAllLines("shapes.txt");
var shapes = Split(shapeText).Select(Shape.Parse).ToList();

var minX = 0;
var maxX = 6;
var minY = 0;

var answer1 = Simulate(new(), 2022);
Console.WriteLine($"Answer 1: {answer1}");


(var first, var second) = FindPeriod();

var periodCount = second.Count - first.Count;
var periodHeight = second.Height - first.Height;

var periods = (1_000_000_000_000L - first.Count) / periodCount;
var remainder = (1_000_000_000_000L - first.Count) % periodCount;

var end = Simulate(new(), first.Count + (int)remainder) - first.Height;

var answer2 = first.Height + periods * periodHeight + end;
Console.WriteLine($"Answer 2: {answer2}");

(State first, State second) FindPeriod()
{
    var history = new Dictionary<(long, long, long, long), State>();

    var state = new State();

    while (true)
    {
        Simulate(state, shapes.Count);

        var snapshot = state.Snapshot();
        var hash = snapshot.Hash();

        if (history.ContainsKey(hash))
        {
            return (history[hash], snapshot);
        }

        history[hash] = snapshot;
    }
}

long Simulate(State state, int count)
{
    for (int i = 0; i < count; i++)
    {
        var position = (x: 2, y: (state.Rocks.Count > 0 ? state.Rocks.Max(r => r.y) : -1) + 4);

        var shape = shapes[state.ShapeIndex];
        state.ShapeIndex = (state.ShapeIndex + 1) % shapes.Count;

        while (true)
        {
            // push
            var next = (x: position.x + (input[state.JetIndex] == '<' ? -1 : 1), position.y);
            state.JetIndex = (state.JetIndex + 1) % input.Length;

            if (IsAllowed(state.Rocks, shape, next))
            {
                position = next;
            }

            // fall
            next = (position.x, y: position.y - 1);

            if (IsAllowed(state.Rocks, shape, next))
            {
                position = next;
            }
            else
            {
                state.Count++;
                state.Rocks.UnionWith(shape.PiecesAtPosition(position));

                // if there is a full line remove everything below it because that is unreachable
                foreach (var y in shape.PiecesAtPosition(position).Select(p => p.y).Distinct())
                {
                    var all = true;

                    for (int x = minX; x <= maxX; x++)
                    {
                        all &= state.Rocks.Contains((x, y));
                    }

                    if (all)
                    {
                        state.Rocks.RemoveWhere(p => p.y < y);
                    }
                }

                break;
            }
        }
    }

    return state.Height;
}

bool IsAllowed(HashSet<(int x, int y)> rocks, Shape shape, (int x, int y) position)
{
    var pieces = shape.PiecesAtPosition(position);

    if (pieces.Min(p => p.x) < minX)
    {
        // too much to the left
        return false;
    }

    if (pieces.Max(p => p.x) > maxX)
    {
        // too much to the right
        return false;
    }

    if (pieces.Min(p => p.y) < minY)
    {
        // too much to the bottom
        return false;
    }

    if (pieces.Any(rocks.Contains))
    {
        // overlap with existing rocks
        return false;
    }

    return true;
}

List<string[]> Split(string[] input)
{
    var separatorIndices = input
        .Select((value, index) => (value, index))
        .Where(x => x.value == "")
        .Select(x => x.index)
        .ToList();

    var startIndices = separatorIndices.Select(i => i + 1).Prepend(0).ToList();
    var endIndices = separatorIndices.Append(input.Length).ToList();

    return startIndices.Zip(endIndices, (s, e) => input[s..e]).ToList();
}

class Shape
{
    public required List<(int x, int y)> Pieces { get; set; }

    public List<(int x, int y)> PiecesAtPosition((int x, int y) position)
    {
        return Pieces.Select(p => (x: p.x + position.x, y: p.y + position.y)).ToList();
    }

    public static Shape Parse(string[] lines)
    {
        var pieces = new List<(int x, int y)>();

        for (int y = 0; y < lines.Length; y++)
        {
            for (int x = 0; x < lines[^(y + 1)].Length; x++)
            {
                if (lines[^(y + 1)][x] == '#')
                {
                    pieces.Add((x, y));
                }
            }
        }

        return new()
        {
            Pieces = pieces
        };
    }
}

class State
{
    public HashSet<(int x, int y)> Rocks { get; set; } = new();

    public int ShapeIndex { get; set; }
    public int JetIndex { get; set; }

    public int Count { get; set; }
    public long Height => Rocks.Max(r => r.y) + 1;

    public (long, long, long, long) Hash()
    {
        var offset = Rocks.Min(r => r.y);

        var rockList = Rocks.Select(r => new { X = r.x, Y = r.y - offset }).OrderBy(r => r.Y).ThenBy(r => r.X).ToList();

        var json = JsonSerializer.Serialize(new { JetIndex, ShapeIndex, Rocks = rockList });

        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(json);
        byte[] hashBytes = SHA256.HashData(inputBytes);

        return (BitConverter.ToInt64(hashBytes[..8]), BitConverter.ToInt64(hashBytes[8..16]), BitConverter.ToInt64(hashBytes[16..24]), BitConverter.ToInt64(hashBytes[24..]));
    }

    public State Snapshot()
    {
        // example input never has a full line so it keeps growing; we take last 400 because for real input it is less and so we will take the full state there
        var depth = 400;

        var maxY = Rocks.Max(r => r.y);

        return new()
        {
            ShapeIndex = ShapeIndex,
            JetIndex = JetIndex,
            Count = Count,
            Rocks = Rocks.Where(r => maxY - r.y < depth).ToHashSet()
        };
    }
}
