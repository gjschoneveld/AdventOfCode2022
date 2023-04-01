using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

var input = File.ReadAllLines("input.txt");
var blueprints = input.Select(Blueprint.Parse).ToList();

var resources = new int[Blueprint.ELEMENT_COUNT];
var robots = new int[Blueprint.ELEMENT_COUNT];
robots[Blueprint.ELEMENT_ORE] = 1;

var cache = new Dictionary<(long, long), int>();
var answer1 = MaxGeode(blueprints[0], robots, resources, 24);
Console.WriteLine($"Answer 1: {answer1}");

(long, long) Hash(int[] robots, int[] resources, int minutes)
{
    var json = JsonSerializer.Serialize(new { robots, resources, minutes });

    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(json);
    byte[] hashBytes = MD5.HashData(inputBytes);

    return (BitConverter.ToInt64(hashBytes.AsSpan()[..8]), BitConverter.ToInt64(hashBytes.AsSpan()[8..]));
}

int MaxGeode(Blueprint blueprint, int[] robots, int[] resources, int minutes)
{
    if (minutes == 1)
    {
        return resources[Blueprint.ELEMENT_GEODE] + robots[Blueprint.ELEMENT_GEODE];
    }

    var hash = Hash(robots, resources, minutes);

    if (cache.ContainsKey(hash))
    {
        return cache[hash];
    }

    var buys = PossibleBuys(blueprint, resources);

    cache[hash] = buys.Max(b => MaxGeode(blueprint, Add(robots, b.robots), Add(b.leftover, robots), minutes - 1));

    return cache[hash];
}

List<(int[] robots, int[] leftover)> PossibleBuys(Blueprint blueprint, int[] resources)
{
    var buy = new int[Blueprint.ELEMENT_COUNT];

    var result = new List<(int[] robots, int[] leftover)>
    {
        (buy, resources)
    };

    while (true)
    {
        buy = (int[])buy.Clone();
        var toIncrement = 0;

        while (true)
        {
            buy[toIncrement]++;
            var leftover = Subtract(resources, CalculateCost(blueprint, buy));

            if (leftover.All(x => x >= 0))
            {
                result.Add((buy, leftover));
                break;
            }

            buy[toIncrement] = 0;
            toIncrement++;

            if (toIncrement >= buy.Length)
            {
                return result;
            }
        }
    }
}

int[] Add(int[] a, int[] b)
{
    var result = new int[Blueprint.ELEMENT_COUNT];

    for (int type = 0; type < a.Length; type++)
    {
        result[type] = a[type] + b[type];
    }

    return result;
}

int[] Subtract(int[] a, int[] b)
{
    var result = new int[Blueprint.ELEMENT_COUNT];

    for (int type = 0; type < a.Length; type++)
    {
        result[type] = a[type] - b[type];
    }

    return result;
}

int[] CalculateCost(Blueprint blueprint, int[] buy)
{
    var result = new int[Blueprint.ELEMENT_COUNT];

    for (int type = 0; type < buy.Length; type++)
    {
        var robot = blueprint.Robots[type];

        foreach (var costs in robot)
        {
            result[costs.type] += costs.count * buy[type];
        }
    }

    return result;
}

class Blueprint
{
    public const int ELEMENT_COUNT = 4;

    public const int ELEMENT_ORE = 0;
    public const int ELEMENT_CLAY = 1;
    public const int ELEMENT_OBSIDIAN = 2;
    public const int ELEMENT_GEODE = 3;

    public int Id { get; set; }

    public required List<List<(int type, int count)>> Robots { get; set; }

    public static Blueprint Parse(string line)
    {
        var values = Regex.Matches(line, @"\d+").OfType<Match>().Select(m => int.Parse(m.Value)).ToList();

        return new()
        { 
            Id = values[0],
            Robots = new()
            {
                new() { (ELEMENT_ORE, values[1]) },
                new() { (ELEMENT_ORE, values[2]) },
                new() { (ELEMENT_ORE, values[3]), (ELEMENT_CLAY, values[4]) },
                new() { (ELEMENT_ORE, values[5]), (ELEMENT_OBSIDIAN, values[6]) }
            }
        };
    }
}
