var input = File.ReadAllLines("input.txt");
Dictionary<string, Valve> valves = input.Select(Valve.Parse).ToDictionary(v => v.Name);

var cache = new Dictionary<State, int>();

var answer1 = MaxReleasedPressure("AA", 30, new());
Console.WriteLine($"Answer 1: {answer1}");

int MaxReleasedPressure(string name, int minutes, HashSet<string> open)
{
    if (minutes < 2)
    {
        return 0;
    }

    var state = new State
    {
        Name = name,
        Minutes = minutes,
        Open = open
    };

    if (cache.ContainsKey(state))
    {
        return cache[state]; 
    }

    var valve = valves[name];

    // move without opening
    var max = valve.Connections.Max(v => MaxReleasedPressure(v, minutes - 1, open));

    // open self and move
    if (valve.FlowRate > 0 && !open.Contains(name))
    {
        open.Add(name);
        var maxOpen = (minutes - 1) * valve.FlowRate + valve.Connections.Max(v => MaxReleasedPressure(v, minutes - 2, open));
        open.Remove(name);

        max = Math.Max(max, maxOpen);
    }

    cache[state] = max;

    return max;
}

class Valve
{
    public required string Name { get; set; }
    public required int FlowRate { get; set; }
    public required List<string> Connections { get; set; }

    public static Valve Parse(string line)
    {
        var parts = line.Split(new[] { ' ', '=', ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

        return new()
        {
            Name = parts[1],
            FlowRate = int.Parse(parts[5]),
            Connections = parts[10..].ToList()
        };
    }
}

class State : IEquatable<State>
{
    public required string Name { get; set; }
    public required int Minutes { get; set; }
    public required HashSet<string> Open { get; set; }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Name);
        hash.Add(Minutes);

        foreach (var open in Open.OrderBy(n => n))
        {
            hash.Add(open);
        }

        return hash.ToHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is State other && this == other;
    }

    public bool Equals(State? other)
    {
        if (other == null)
        {
            return false;
        }

        if (Name != other.Name || Minutes != other.Minutes)
        {
            return false;
        }

        return Open.OrderBy(n => n).SequenceEqual(other.Open.OrderBy(n => n));
    }
}
