var numbers = File.ReadAllLines("input.txt");

var answer = ToSnafu(numbers.Sum(ToDecimal));
Console.WriteLine($"Answer: {answer}");

// alternative to see if possible
var alternative = numbers.Aggregate(SnafuAdd);
Console.WriteLine($"Answer: {alternative}");

long ToDecimalDigit(char digit)
{
    return digit switch
    {
        '2' => 2,
        '1' => 1,
        '0' => 0,
        '-' => -1,
        '=' => -2,
        _ => throw new()
    };
}

char ToSnafuDigit(long digit)
{
    return digit switch
    {
        2 => '2',
        1 => '1',
        0 => '0',
        -1 => '-',
        -2 => '=',
        _ => throw new()
    };
}

long ToDecimal(string snafu)
{
    long result = 0;

    foreach (var digit in snafu)
    {
        result *= 5;
        result += ToDecimalDigit(digit);
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

        digits.Insert(0, ToSnafuDigit(digit));
    }

    return new string(digits.ToArray());
}

string SnafuAdd(string a, string b)
{
    var digits = new List<char>();

    int carry = 0;

    for (int i = 0; i < Math.Max(a.Length, b.Length); i++)
    {
        var digitA = i < a.Length ? ToDecimalDigit(a[^(i + 1)]) : 0;
        var digitB = i < b.Length ? ToDecimalDigit(b[^(i + 1)]) : 0;

        var sum = digitA + digitB + carry;

        (sum, carry) = sum switch
        {
            > 2 => (sum - 5, 1),
            < -2 => (sum + 5, -1),
            _ => (sum, 0)
        };

        digits.Insert(0, ToSnafuDigit(sum));
    }

    if (carry != 0)
    {
        digits.Insert(0, ToSnafuDigit(carry));
    }

    return new string(digits.ToArray());
}
