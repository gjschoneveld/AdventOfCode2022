var numbers = File.ReadAllLines("input.txt");

var answer = ToSnafu(numbers.Sum(ToDecimal));
Console.WriteLine($"Answer: {answer}");

long ToDecimal(string snafu)
{
    long result = 0;

    foreach (var digit in snafu)
    {
        result *= 5;
        result += digit switch
        {
            '2' => 2,
            '1' => 1,
            '0' => 0,
            '-' => -1,
            '=' => -2,
            _ => throw new()
        };
    }

    return result;
}

string ToSnafu(long number)
{
    var digits = new List<char>();

    while (number > 0)
    {
        var digit = number % 5;
        number /= 5;

        if (digit > 2)
        {
            digit -= 5;
            number++;
        }

        digits.Insert(0, digit switch
        {
            2 => '2',
            1 => '1',
            0 => '0',
            -1 => '-',
            -2 => '=',
            _ => throw new()
        });
    }

    return new string(digits.ToArray());
}
