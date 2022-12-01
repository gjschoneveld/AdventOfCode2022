List<List<string>> Split(string[] input)
{
    var result = new List<List<string>>();

    var start = 0;

    for (int i = 0; i < input.Length; i++)
    {
        if (input[i] == "")
        {
            result.Add(input[start..i].ToList());
            start = i + 1;
        }
    }

    result.Add(input[start..^0].ToList());

    return result;
}

var input = File.ReadAllLines("input.txt");
var elves = Split(input).Select(l => l.Select(int.Parse).Sum()).ToList();

var answer1 = elves.Max();
Console.WriteLine($"Answer 1: {answer1}");

var answer2 = elves.OrderByDescending(e => e).Take(3).Sum();
Console.WriteLine($"Answer 2: {answer2}");
