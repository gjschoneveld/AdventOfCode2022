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

var input = File.ReadAllLines("input.txt");
var elves = Split(input).Select(l => l.Sum(int.Parse)).ToList();

var answer1 = elves.Max();
Console.WriteLine($"Answer 1: {answer1}");

var answer2 = elves.OrderByDescending(e => e).Take(3).Sum();
Console.WriteLine($"Answer 2: {answer2}");
