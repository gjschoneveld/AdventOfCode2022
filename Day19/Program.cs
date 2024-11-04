using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

var input = File.ReadAllLines("input.txt");
var blueprints = input.Select(Blueprint.Parse).ToList();

var totalQualityLevel = 0;

foreach (var blueprint in blueprints)
{
    var geodes = Logic.FindMaxGeodes(blueprint, 24);
    totalQualityLevel += geodes * blueprint.Id;
}

var answer1 = totalQualityLevel;
Console.WriteLine($"Answer 1: {answer1}");

// part 2 takes 10 minutes
var product = 1;

foreach (var blueprint in blueprints.Take(3))
{
    var geodes = Logic.FindMaxGeodes(blueprint, 32);
    product *= geodes;
}

var answer2 = product;
Console.WriteLine($"Answer 2: {answer2}");

class Logic
{
    public static int FindMaxGeodes(Blueprint blueprint, int minutes)
    {
        //Console.WriteLine($"Blueprint {blueprint.Id}");

        var resources = new int[Blueprint.ELEMENT_COUNT];
        var robots = new int[Blueprint.ELEMENT_COUNT];
        robots[Blueprint.ELEMENT_ORE] = 1;

        var states = new List<(int[] robots, int[] resources)> { (robots, resources) };

        for (int minute = 1; minute <= minutes; minute++)
        {
            //Console.WriteLine($"Minute {minute}");

            // remove inferior states

            // the states are sorted so better states are always at lower indices
            var index = 0;

            while (index < states.Count)
            {
                states = states.Where(s => states[index] == s || !Logic.IsBetter(blueprint, states[index], s)).ToList();
                index++;
            }

            states = states
                .SelectMany(s => Logic.Next(blueprint, s))
                .Distinct(new StateEqualityComparer())
                .OrderDescending(new StateComparer())
                .ToList();
        }

        var geodes = states.Max(s => s.resources[Blueprint.ELEMENT_GEODE]);

        //Console.WriteLine($"{geodes} geodes");

        return geodes;
    }

    public static bool IsBetter(Blueprint blueprint, (int[] robots, int[] resources) stateA, (int[] robots, int[] resources) stateB)
    {
        if (blueprint.Robots[Blueprint.ELEMENT_GEODE].All(x => stateA.robots[x.type] >= x.count))
        {
            if (stateA.robots[Blueprint.ELEMENT_GEODE] < stateB.robots[Blueprint.ELEMENT_GEODE])
            {
                return false;
            }

            if (stateA.resources[Blueprint.ELEMENT_GEODE] < stateB.resources[Blueprint.ELEMENT_GEODE])
            {
                return false;
            }

            return true;
        }

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
        var buys = PossibleBuys(blueprint, state);

        return buys.Select(b => (Add(state.robots, b.robots), Add(b.leftover, state.robots))).ToList();
    }

    public static List<(int[] robots, int[] leftover)> PossibleBuys(Blueprint blueprint, (int[] robots, int[] resources) state)
    {
        var result = new List<(int[] robots, int[] leftover)>();

        var minNeeded = MinNeededToBuyAnyRobot(blueprint);

        if (!HasTooManyResources(state.resources, minNeeded))
        {
            result.Add((new int[Blueprint.ELEMENT_COUNT], state.resources));
        };

        for (int type = 0; type < Blueprint.ELEMENT_COUNT; type++)
        {
            if (type != Blueprint.ELEMENT_GEODE && state.robots[type] >= minNeeded[type])
            {
                // we don't need more robots of this type
                continue;
            }

            var buy = new int[Blueprint.ELEMENT_COUNT];
            buy[type] = 1;

            var leftover = Subtract(state.resources, CalculateCost(blueprint, buy));

            if (leftover.All(x => x >= 0))
            {
                result.Add((buy, leftover));
            }
        }

        return result;
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

class StateComparer() : IComparer<(int[] robots, int[] resources)>
{
    public int Compare((int[] robots, int[] resources) x, (int[] robots, int[] resources) y)
    {
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
