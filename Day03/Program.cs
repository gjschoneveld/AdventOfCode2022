var input = File.ReadAllLines("input.txt");

var common = input.Select(GetCompartments).SelectMany(FindCommon).ToList();
var priorities = common.Select(Priority).ToList();

var answer1 = priorities.Sum();
Console.WriteLine($"Answer 1: {answer1}");

common = input.Chunk(3).SelectMany(FindCommon).ToList();
priorities = common.Select(Priority).ToList();

var answer2 = priorities.Sum();
Console.WriteLine($"Answer 2: {answer2}");

string[] GetCompartments(string rucksack)
{
    var halfway = rucksack.Length / 2;

    return new[] { rucksack[..halfway], rucksack[halfway..] };
}

List<char> FindCommon(string[] lists)
{
    if (lists.Length == 1)
    {
        return lists[0].ToList();
    }

    var types = lists[0].Distinct().ToList();
    var intersect = types.Intersect(FindCommon(lists[1..])).ToList();

    return intersect;
}

int Priority(char c)
{
    if (char.IsLower(c))
    {
        return c - 'a' + 1;
    }

    return c - 'A' + 27;
}
