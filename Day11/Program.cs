namespace Day11;

abstract class Expression
{
    public abstract long Evaluate(long old);

    public static Expression ParseOperand(string text)
    {
        return text == "old" ? new Old() : new Constant { Value = long.Parse(text) };
    }

    public static Expression Parse(string line)
    {
        var parts = line.Split(' ');

        return new Binary
        {
            Left = ParseOperand(parts[0]),
            Right = ParseOperand(parts[2]),
            Operator = parts[1][0]
        };
    }
}

class Constant : Expression
{
    public required long Value { get; set; }

    public override long Evaluate(long old) => Value;

    public override string ToString() => Value.ToString();
}

class Old : Expression
{
    public override long Evaluate(long old) =>  old;

    public override string ToString() => "old";
}

class Binary : Expression
{
    public required Expression Left { get; set; }
    public required Expression Right { get; set; }
    public required char Operator { get; set; }

    public override long Evaluate(long old)
    {
        var left = Left.Evaluate(old);
        var right = Right.Evaluate(old);

        return Operator switch
        {
            '+' => left + right,
            '*' => left * right,
            _ => throw new Exception()
        };
    }

    public override string ToString() => $"{Left} {Operator} {Right}";
}

class Monkey
{
    public required int Id { get; set; }
    public required List<long> Items { get; set; }
    public required Expression Expression { get; set; }
    public required long Test { get; set; }
    public required int True { get; set; }
    public required int False { get; set; }

    public long Inspections { get; set; }

    public void InspectItem(List<Monkey> monkeys, long item, long? modulus)
    {
        var level = Expression.Evaluate(item);

        if (modulus == null)
        {
            // part 1
            level /= 3;
        }
        else
        {
            // part 2
            level %= (long)modulus;
        }

        var destination = level % Test == 0 ? True : False;
        monkeys[destination].Items.Add(level);

        Inspections++;
    }

    public void InspectItems(List<Monkey> monkeys, long? modulus)
    {
        foreach (var item in Items)
        {
            InspectItem(monkeys, item, modulus);
        }

        Items.Clear();
    }

    public override string ToString()
    {
        return $"""
            Monkey {Id}:
              Starting items: {string.Join(", ", Items)}
              Operation: new = {Expression}
              Test: divisible by {Test}
                If true: throw to monkey {True}
                If false: throw to monkey {False}
            """;
    }

    public static Monkey Parse(string[] input)
    {
        var parts = input.Select(line => line.Split(new[] { ' ', ':', ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
        var equals = input[2].IndexOf('=');

        return new()
        {
            Id = int.Parse(parts[0][1]),
            Items = parts[1][2..].Select(long.Parse).ToList(),
            Expression = Expression.Parse(input[2][(equals + 2)..]),
            Test = int.Parse(parts[3][3]),
            True = int.Parse(parts[4][5]),
            False = int.Parse(parts[5][5]),
        };
    }
}

class Program
{
    static List<string[]> Split(string[] input)
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

    static long Simulate(List<Monkey> monkeys, int rounds, long? modulus)
    {
        for (int r = 0; r < rounds; r++)
        {
            foreach (var monkey in monkeys)
            {
                monkey.InspectItems(monkeys, modulus);
            }
        }

        return monkeys
            .Select(m => m.Inspections)
            .OrderByDescending(m => m)
            .Take(2)
            .Aggregate((a, b) => a * b);
    }

    static void Main()
    {
        var input = File.ReadAllLines("input.txt");
        var sections = Split(input).ToList();
        var monkeys = sections.Select(Monkey.Parse).ToList();

        var answer1 = Simulate(monkeys, 20, null);
        Console.WriteLine($"Answer 1: {answer1}");

        monkeys = sections.Select(Monkey.Parse).ToList();
        var modulus = monkeys.Select(m => m.Test).Aggregate((a, b) => a * b);

        var answer2 = Simulate(monkeys, 10_000, modulus);
        Console.WriteLine($"Answer 2: {answer2}");
    }
}
