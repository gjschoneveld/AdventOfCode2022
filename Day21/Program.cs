var input = File.ReadAllLines("input.txt");
var monkeys = input.Select(Monkey.Parse).ToDictionary(m => m.Name);

var answer1 = monkeys["root"].GetValuePart1(monkeys);
Console.WriteLine($"Answer 1: {answer1}");

var answer2 = FindSelf(monkeys);
Console.WriteLine($"Answer 2: {answer2}");

long FindSelf(Dictionary<string, Monkey> monkeys)
{
    long max = 1;

    while (monkeys["root"].GetValuePart2(monkeys, -max) == monkeys["root"].GetValuePart2(monkeys, max))
    {
        max *= 10;
    }

    var left = -max;
    var right = max;

    var ascending = monkeys["root"].GetValuePart2(monkeys, left) < 0;

    while (true)
    {
        var middle = left + (right - left) / 2;

        var comparison = monkeys["root"].GetValuePart2(monkeys, middle);

        if (comparison == 0)
        {
            return middle;
        }

        if (!ascending)
        {
            comparison = -comparison;
        }

        if (comparison < 0)
        {
            left = middle + 1;
        }
        else
        {
            right = middle - 1;
        }
    }
}

abstract class Monkey
{
    public required string Name { get; set; }

    public abstract decimal GetValuePart1(Dictionary<string, Monkey> monkeys);
    public abstract decimal GetValuePart2(Dictionary<string, Monkey> monkeys, decimal self);

    public static Monkey Parse(string x)
    {
        var parts = x.Split((char[])[':', ' '], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 2)
        {
            return new ValueMonkey
            {
                Name = parts[0],
                Value = decimal.Parse(parts[1])
            };
        }

        return new MathMonkey
        {
            Name = parts[0],
            Left = parts[1],
            Right = parts[3],
            Operation = parts[2][0]
        };
    }
}

class ValueMonkey : Monkey
{
    public required decimal Value { get; set; }

    public override decimal GetValuePart1(Dictionary<string, Monkey> monkeys)
    {
        return Value;
    }

    public override decimal GetValuePart2(Dictionary<string, Monkey> monkeys, decimal self)
    {
        if (Name == "humn")
        {
            return self;
        }

        return Value;
    }
}

class MathMonkey : Monkey
{
    public required string Left { get; set; }
    public required string Right { get; set; }
    public required char Operation { get; set; }

    public override decimal GetValuePart1(Dictionary<string, Monkey> monkeys)
    {
        var left = monkeys[Left].GetValuePart1(monkeys);
        var right = monkeys[Right].GetValuePart1(monkeys);

        return Operation switch
        {
            '+' => left + right,
            '-' => left - right,
            '*' => left * right,
            '/' => left / right,
            _ => throw new()
        };
    }

    public override decimal GetValuePart2(Dictionary<string, Monkey> monkeys, decimal self)
    {
        var left = monkeys[Left].GetValuePart2(monkeys, self);
        var right = monkeys[Right].GetValuePart2(monkeys, self);

        if (Name == "root")
        {
            return Math.Sign(left - right);
        }

        return Operation switch
        {
            '+' => left + right,
            '-' => left - right,
            '*' => left * right,
            '/' => left / right,
            _ => throw new()
        };
    }
}
