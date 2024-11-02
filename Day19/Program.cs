using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

var input = File.ReadAllLines("input.txt");
var blueprints = input.Select(Blueprint.Parse).ToList();

var expected = File.ReadAllLines("expected.txt")
    .Select(line => line.Split(","))
    .Select(parts => parts.Select(int.Parse).ToArray())
    .Select(parts => (robots: parts[..4], ResourceScope:parts[4..]))
    .ToList();

var totalQualityLevel = 0;

foreach (var blueprint in blueprints)
{
    Console.WriteLine($"Blueprint {blueprint.Id}");

    var resources = new int[Blueprint.ELEMENT_COUNT];
    var robots = new int[Blueprint.ELEMENT_COUNT];
    robots[Blueprint.ELEMENT_ORE] = 1;

    var states = new List<(int[] robots, int[] resources)> { (robots, resources) };

    for (int minute = 1; minute <= 24; minute++)
    {
        Console.WriteLine($"Minute {minute}");

        // remove inferior states
        Console.Write($"before {states.Count}, ");

        // found somewhere that you can do this optimization; no proof
        var geodeRobots = states.Max(s => s.robots[Blueprint.ELEMENT_GEODE]);
        states = states.Where(s => s.robots[Blueprint.ELEMENT_GEODE] == geodeRobots).ToList();

        if (geodeRobots == 0)
        {
            var obsidianRobots = states.Max(s => s.robots[Blueprint.ELEMENT_OBSIDIAN]);
            states = states.Where(s => s.robots[Blueprint.ELEMENT_OBSIDIAN] == obsidianRobots).ToList();
        }

        // the states are sorted so better states are always at lower indices
        var index = 0;

        while (index < states.Count)
        {
            states = states.Where(s => states[index] == s || !Logic.IsBetter(states[index], s)).ToList();
            index++;
        }

        Console.WriteLine($"after {states.Count}");

        states = states
            .SelectMany(s => Logic.Next(blueprint, s))
            .Distinct(new StateEqualityComparer())
            .OrderDescending(new StateComparer(blueprint))
            .ToList();
    }


    var geodes = states.Max(s => s.resources[Blueprint.ELEMENT_GEODE]);
    totalQualityLevel += geodes * blueprint.Id;

    Console.WriteLine($"{geodes} geodes");

    var best = states.Where(s => s.resources[Blueprint.ELEMENT_GEODE] == geodes).ToList();
    Console.WriteLine(best.Any(s => Logic.IsSame(s, expected[^1])));
}

var answer1 = totalQualityLevel;
Console.WriteLine($"Answer 1: {answer1}");

class Logic
{
    public static bool IsSame((int[] robots, int[] resources) stateA, (int[] robots, int[] resources) stateB)
    {
        for (int type = 0; type < stateA.robots.Length; type++)
        {
            if (stateA.robots[type] != stateB.robots[type])
            {
                return false;
            }

            if (stateA.resources[type] != stateB.resources[type])
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsBetterV2(Blueprint blueprint, (int[] robots, int[] resources) stateA, (int[] robots, int[] resources) stateB)
    {
        var valueA = Add(stateA.resources, CalculateCost(blueprint, stateA.robots));
        var valueB = Add(stateB.resources, CalculateCost(blueprint, stateB.robots));

        var equal = true;

        for (int type = 0; type < valueA.Length; type++)
        {
            if (valueA[type] < valueB[type])
            {
                return false;
            }

            if (valueA[type] > valueB[type])
            {
                equal = false;
            }
        }

        return !equal;
    }

    public static bool IsBetter((int[] robots, int[] resources) stateA, (int[] robots, int[] resources) stateB)
    {
        for (int type = 0; type < stateA.robots.Length; type++)
        {
            if (stateA.robots[type] < stateB.robots[type])
            {
                return false;
            }

            if (stateA.resources[type] < stateB.resources[type])
            {
                return false;
            }
        }

        return true;
    }

    public static bool HasTooManyResources(int[] resources, int[] minNeeded)
    {
        for (int type = 0; type < resources.Length; type++)
        {
            if (resources[type] < minNeeded[type])
            {
                return false;
            }
        }

        return true;
    }

    public static int[] MinNeededToBuyAnyRobot(Blueprint blueprint)
    {
        var result = new int[Blueprint.ELEMENT_COUNT];

        for (int type = 0; type < result.Length; type++)
        {
            var robot = blueprint.Robots[type];

            foreach (var costs in robot)
            {
                result[costs.type] = Math.Max(result[costs.type], costs.count);
            }
        }

        return result;
    }

    public static List<(int[] robots, int[] resources)> Next(Blueprint blueprint, (int[] robots, int[] resources) state)
    {
        var buys = PossibleBuys(blueprint, state.resources);

        return buys.Select(b => (Add(state.robots, b.robots), Add(b.leftover, state.robots))).ToList();
    }

    public static List<(int[] robots, int[] leftover)> PossibleBuys(Blueprint blueprint, int[] resources)
    {
        var minNeeded = MinNeededToBuyAnyRobot(blueprint);

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

                if (leftover.All(x => x >= 0) && !HasTooManyResources(leftover, minNeeded))
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

    public static int[] Add(int[] a, int[] b)
    {
        var result = new int[Blueprint.ELEMENT_COUNT];

        for (int type = 0; type < a.Length; type++)
        {
            result[type] = a[type] + b[type];
        }

        return result;
    }

    public static int[] Subtract(int[] a, int[] b)
    {
        var result = new int[Blueprint.ELEMENT_COUNT];

        for (int type = 0; type < a.Length; type++)
        {
            result[type] = a[type] - b[type];
        }

        return result;
    }

    public static int[] CalculateCost(Blueprint blueprint, int[] buy)
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

class StateEqualityComparer : IEqualityComparer<(int[] robots, int[] resources)>
{
    public bool Equals((int[] robots, int[] resources) x, (int[] robots, int[] resources) y)
    {
        for (int type = 0; type < x.robots.Length; type++)
        {
            if (x.robots[type] != y.robots[type])
            {
                return false;
            }

            if (x.resources[type] != y.resources[type])
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode([DisallowNull] (int[] robots, int[] resources) obj)
    {
        var hashCode = new HashCode();

        for (int type = 0; type < obj.robots.Length; type++)
        {
            hashCode.Add(obj.robots[type]);
            hashCode.Add(obj.resources[type]);
        }

        return hashCode.ToHashCode();
    }
}

class StateComparer(Blueprint blueprint) : IComparer<(int[] robots, int[] resources)>
{
    public int Compare((int[] robots, int[] resources) x, (int[] robots, int[] resources) y)
    {
        var valueX = Logic.Add(x.resources, Logic.CalculateCost(blueprint, x.robots)).Sum();
        var valueY = Logic.Add(y.resources, Logic.CalculateCost(blueprint, y.robots)).Sum();

        if (valueX != valueY)
        {
            return valueX - valueY;
        }

        for (int type = 0; type < x.robots.Length; type++)
        {
            if (x.robots[type] != y.robots[type])
            {
                return x.robots[type] - y.robots[type];
            }

            if (x.resources[type] != y.resources[type])
            {
                return x.resources[type] - y.resources[type];
            }
        }

        return 0;
    }
}
