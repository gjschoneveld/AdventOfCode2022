using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

var input = File.ReadAllLines("input.txt");
var blueprints = input.Select(Blueprint.Parse).ToList();

var resources = CreateElementCollection();
var robots = CreateElementCollection();
robots[Element.Ore] = 1;

var cache = new Dictionary<(long, long, long, long), int>();
var answer1 = MaxGeode(blueprints[0], robots, resources, 24);
Console.WriteLine($"Answer 1: {answer1}");

Dictionary<Element, int> CreateElementCollection()
{
    return Enum.GetValues(typeof(Element)).Cast<Element>().ToDictionary(e => e, e => 0);
}

(long, long, long, long) Hash(Dictionary<Element, int> robots, Dictionary<Element, int> resources, int minutes)
{
    var json = JsonSerializer.Serialize(new { robots, resources, minutes });

    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(json);
    byte[] hashBytes = SHA256.HashData(inputBytes);

    return (BitConverter.ToInt64(hashBytes[..8]), BitConverter.ToInt64(hashBytes[8..16]), BitConverter.ToInt64(hashBytes[16..24]), BitConverter.ToInt64(hashBytes[24..]));
}

int MaxGeode(Blueprint blueprint, Dictionary<Element, int> robots, Dictionary<Element, int> resources, int minutes)
{
    var hash = Hash(robots, resources, minutes);

    if (cache.ContainsKey(hash))
    {
        return cache[hash];
    }

    if (minutes == 1)
    {
        return resources[Element.Geode] + robots[Element.Geode];
    }

    var buys = PossibleBuys(blueprint, resources);

    cache[hash] = buys.Max(b => MaxGeode(blueprint, Add(robots, b.robots), Add(b.leftover, robots), minutes - 1));

    return cache[hash];
}

List<(Dictionary<Element, int> robots, Dictionary<Element, int> leftover)> PossibleBuys(Blueprint blueprint, Dictionary<Element, int> resources)
{
    var buy = CreateElementCollection();

    var result = new List<(Dictionary<Element, int> robots, Dictionary<Element, int> leftover)>
    {
        (buy, resources)
    };

    while (true)
    {
        buy = buy.ToDictionary(x => x.Key, x => x.Value);
        var toIncrement = buy.Keys.First();

        while (true)
        {
            buy[toIncrement]++;
            var leftover = Subtract(resources, CalculateCost(blueprint, buy));

            if (leftover.All(x => x.Value >= 0))
            {
                result.Add((buy, leftover));
                break;
            }

            buy[toIncrement] = 0;
            toIncrement++;

            if (!buy.ContainsKey(toIncrement))
            {
                return result;
            }
        }
    }
}

Dictionary<Element, int> Add(Dictionary<Element, int> a, Dictionary<Element, int> b)
{
    var result = new Dictionary<Element, int>();

    foreach (var element in a.Keys)
    {
        result[element] = a[element] + b[element];
    }

    return result;
}

Dictionary<Element, int> Subtract(Dictionary<Element, int> a, Dictionary<Element, int> b)
{
    var result = new Dictionary<Element, int>();

    foreach (var element in a.Keys)
    {
        result[element] = a[element] - b[element];
    }

    return result;
}

Dictionary<Element, int> CalculateCost(Blueprint blueprint, Dictionary<Element, int> buy)
{
    var result = CreateElementCollection();

    foreach ((var type, var count) in buy)
    {
        var robot = blueprint.Robots[type];

        foreach (var costs in robot)
        {
            result[costs.type] += costs.count * count;
        }
    }

    return result;
}

class Blueprint
{
    public int Id { get; set; }

    public required Dictionary<Element, List<(Element type, int count)>> Robots { get; set; }

    public static Blueprint Parse(string line)
    {
        var values = Regex.Matches(line, @"\d+").OfType<Match>().Select(m => int.Parse(m.Value)).ToList();

        return new()
        { 
            Id = values[0],
            Robots = new()
            {
                [Element.Ore] = new() { (Element.Ore, values[1]) },
                [Element.Clay] = new() { (Element.Ore, values[2]) },
                [Element.Obsidian] = new() { (Element.Ore, values[3]), (Element.Clay, values[4]) },
                [Element.Geode] = new() { (Element.Ore, values[5]), (Element.Obsidian, values[6]) }
            }
        };
    }
}

enum Element
{
    Ore,
    Clay,
    Obsidian,
    Geode
}
