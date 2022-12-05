var input = File.ReadAllLines("input.txt");
var separator = Array.FindIndex(input, string.IsNullOrEmpty);

var stacks = ParseStacks(input[..separator]);
var steps = input[(separator + 1)..].Select(Step.Parse).ToList();

foreach (var step in steps)
{
    var source = stacks[step.From - 1];
    var target = stacks[step.To - 1];

    for (int i = 0; i < step.Count; i++)
    {
        var crates = Pick(source, 1);
        Place(target, crates);
    }
}

var answer1 = new string(stacks.Select(s => s[^1]).ToArray());
Console.WriteLine($"Answer 1: {answer1}");

stacks = ParseStacks(input[..separator]);

foreach (var step in steps)
{
    var source = stacks[step.From - 1];
    var target = stacks[step.To - 1];

    var crates = Pick(source, step.Count);
    Place(target, crates);
}

var answer2 = new string(stacks.Select(s => s[^1]).ToArray());
Console.WriteLine($"Answer 2: {answer2}");

List<char> Pick(List<char> source, int count)
{
    var crates = source.GetRange(source.Count - count, count);
    source.RemoveRange(source.Count - count, count);

    return crates;
}

void Place(List<char> target, List<char> crates)
{
    target.AddRange(crates);
}

List<List<char>> ParseStacks(string[] input)
{
    var result = new List<List<char>>();
    var id = '1';

    while (true)
    {
        var column = input[^1].IndexOf(id);

        if (column < 0)
        {
            return result;
        }

        var stack = input[..^1]
            .Select(line => line[column])
            .Where(char.IsLetter)
            .Reverse()
            .ToList();

        result.Add(stack);
        id++;
    }
}

class Step
{
    public int Count { get; set; }
    public int From { get; set; }
    public int To { get; set; }

    public static Step Parse(string line)
    {
        var parts = line.Split(' ');
        
        return new()
        {
            Count = int.Parse(parts[1]),
            From = int.Parse(parts[3]),
            To = int.Parse(parts[5])
        };
    }
}
