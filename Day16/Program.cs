using System.Security.Cryptography;
using System.Text.Json;

var input = File.ReadAllLines("input.txt");
Dictionary<string, Valve> valves = input.Select(Valve.Parse).ToDictionary(v => v.Name);

(string name, int steps, bool open) start = ("AA", 0, false);
RemoveUselessValves();

var cache = new Dictionary<long, int>();

var answer1 = MaxReleasedPressurePart1(start, 30, new());
Console.WriteLine($"Answer 1: {answer1}");

cache.Clear();

var answer2 = MaxReleasedPressurePart2(new() { start, start }, 26, new());
Console.WriteLine($"Answer 2: {answer2}");

int MaxReleasedPressurePart1((string name, int steps, bool open) current, int minutes, HashSet<string> open)
{
    if (minutes < 1)
    {
        return 0;
    }

    var hash = Hash(new() { current }, minutes, open);

    if (cache.TryGetValue(hash, out int value))
    {
        return value;
    }

    // move
    var moves = Moves(current);
    var max = moves.Max(m => MaxReleasedPressurePart1(m, minutes - 1, open));

    // open
    var mayOpen = current.steps == 0 && valves[current.name].FlowRate > 0 && !open.Contains(current.name);

    if (mayOpen)
    {
        open.Add(current.name);
        var maxOpen = MaxReleasedPressurePart1((current.name, 0, true), minutes - 1, open);
        open.Remove(current.name);

        max = Math.Max(max, maxOpen);
    }

    if (current.open)
    {
        max += valves[current.name].FlowRate * minutes;
    }

    cache[hash] = max;

    return max;
}

int MaxReleasedPressurePart2(List<(string name, int steps, bool open)> current, int minutes, HashSet<string> open)
{
    if (minutes < 1)
    {
        return 0;
    }

    var hash = Hash(current, minutes, open);

    if (cache.TryGetValue(hash, out int value))
    {
        return value;
    }

    var moves = current.Select(Moves).ToList();
    var mayOpen = current.Select(c => c.steps == 0 && valves[c.name].FlowRate > 0 && !open.Contains(c.name)).ToList();

    var max = 0;

    // move both
    foreach (var m0 in moves[0])
    {
        var maxMove = moves[1].Max(m => MaxReleasedPressurePart2(new() { m0, m }, minutes - 1, open));
        max = Math.Max(max, maxMove);
    }

    // open both
    if (mayOpen[0] && mayOpen[1] && current[0].name != current[1].name)
    {
        open.Add(current[0].name);
        open.Add(current[1].name);
        var maxOpen = MaxReleasedPressurePart2(current.Select(c => (c.name, 0, true)).ToList(), minutes - 1, open);
        open.Remove(current[0].name);
        open.Remove(current[1].name);

        max = Math.Max(max, maxOpen);
    }

    // open first, move second
    if (mayOpen[0])
    {
        open.Add(current[0].name);
        var maxOpenMove = moves[1].Max(m => MaxReleasedPressurePart2(new() { (current[0].name, 0, true), m }, minutes - 1, open));
        open.Remove(current[0].name);

        max = Math.Max(max, maxOpenMove);
    }

    // move first, open second
    if (mayOpen[1])
    {
        open.Add(current[1].name);
        var maxMoveOpen = moves[0].Max(m => MaxReleasedPressurePart2(new() { m, (current[1].name, 0, true) }, minutes - 1, open));
        open.Remove(current[1].name);

        max = Math.Max(max, maxMoveOpen);
    }

    for (int i = 0; i < current.Count; i++)
    {
        if (current[i].open)
        {
            max += valves[current[i].name].FlowRate * minutes;
        }
    }

    cache[hash] = max;

    return max;
}

List<(string name, int steps, bool open)> Moves((string name, int steps, bool open) current)
{
    if (current.steps > 0)
    {
        return new()
        {
            (current.name, current.steps - 1, false)
        };
    }

    return valves[current.name].Connections.Select(c => (c.name, c.distance - 1, false)).ToList();
}

long Hash(List<(string name, int steps, bool open)> current, int minutes, HashSet<string> open)
{
    var data = new
    {
        Current = current.OrderBy(c => c.name).ThenBy(c => c.steps).ThenBy(c => c.open).Select(c => new
        {
            Name = c.name,
            Steps = c.steps,
            Open = c.open
        }).ToList(),
        Minutes = minutes,
        Open = open.OrderBy(n => n).ToList()
    };

    var json = JsonSerializer.Serialize(data);

    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(json);
    byte[] hashBytes = MD5.HashData(inputBytes);

    var combined = new byte[8];

    var index = 0;

    foreach (var b in hashBytes)
    {
        combined[index] ^= b;
        index = (index + 1) % combined.Length;
    }

    return BitConverter.ToInt64(combined);
}

void RemoveUselessValves()
{
    var empty = valves.Where(kv => kv.Value.FlowRate == 0 && kv.Key != start.name).ToList();

    foreach ((var name, var valve) in empty)
    {
        foreach (var c1 in valve.Connections)
        {
            var source = valves[c1.name];

            source.Connections.RemoveAll(c => c.name == name);

            foreach (var c2 in valve.Connections)
            {
                var target = valves[c2.name];

                if (source == target)
                {
                    continue;
                }

                var distance = c1.distance + c2.distance;

                var currentConnection = source.Connections.FirstOrDefault(c => c.name == target.Name);

                if (currentConnection == default || currentConnection.distance > distance)
                {
                    source.Connections.Remove(currentConnection);
                    source.Connections.Add((target.Name, distance));
                }
            }
        }

        valves.Remove(name);
    }
}

class Valve
{
    public required string Name { get; set; }
    public required int FlowRate { get; set; }
    public required List<(string name, int distance)> Connections { get; set; }

    public static Valve Parse(string line)
    {
        var parts = line.Split(new[] { ' ', '=', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

        return new()
        {
            Name = parts[1],
            FlowRate = int.Parse(parts[5]),
            Connections = parts[10..].Select(n => (n, 1)).ToList()
        };
    }
}
