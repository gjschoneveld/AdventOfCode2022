var input = File.ReadAllText("input.txt");

var answer1 = FindMarker(input, 4);
Console.WriteLine($"Answer 1: {answer1}");

var answer2 = FindMarker(input, 14);
Console.WriteLine($"Answer 2: {answer2}");

int FindMarker(string input, int size)
{
    for (int position = size; position < input.Length; position++)
    {
        var range = input.Substring(position - size, size);

        if (range.Distinct().Count() == size)
        {
            return position;
        }
    }

    return -1;
}
