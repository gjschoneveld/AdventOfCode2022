using System.Linq;

var input = File.ReadAllLines("input.txt");

var answer1 = input.Select(GetCompartments).SelectMany(FindCommon).Sum(Priority);
Console.WriteLine($"Answer 1: {answer1}");

var answer2 = input.Chunk(3).SelectMany(FindCommon).Sum(Priority);
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

    return lists[0].Intersect(FindCommon(lists[1..])).ToList();
}

int Priority(char c)
{
    if (char.IsLower(c))
    {
        return c - 'a' + 1;
    }

    return c - 'A' + 27;
}
