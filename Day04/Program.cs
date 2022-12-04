var input = File.ReadAllLines("input.txt");
var pairs = input.Select(Section.Parse).ToList();

var answer1 = pairs.Count(pair => pair[0].FullyContains(pair[1]) || pair[1].FullyContains(pair[0]));
Console.WriteLine($"Answer 1: {answer1}");

var answer2 = pairs.Count(pair => pair[0].SomeOverlap(pair[1]));
Console.WriteLine($"Answer 2: {answer2}");

class Section
{
    public int Start { get; set; }
    public int End { get; set; }

    public bool FullyContains(Section other)
    {
        return Start <= other.Start && End >= other.End;
    }

    public bool SomeOverlap(Section other)
    {
        return Start <= other.End && End >= other.Start;
    }

    public static List<Section> Parse(string line)
    {
        return line
            .Split(',', '-')
            .Select(int.Parse)
            .Chunk(2)
            .Select(v => new Section { Start = v[0], End = v[1] })
            .ToList();
    }
}
