var input = File.ReadAllLines("input.txt");
var pairs = Split(input);
var trees = pairs.Select((p, i) => (index: i + 1, nodes: p.Select(ParseLine).ToList())).ToList();

var right = trees.Where(p => InRightOrder(p.nodes[0], p.nodes[1]) == true).ToList();

var answer1 = right.Sum(p => p.index);
Console.WriteLine($"Answer 1: {answer1}");


var dividers = new List<Node> { ParseLine("[[2]]"), ParseLine("[[6]]") };
var all = trees.SelectMany(p => p.nodes).Concat(dividers).ToList();

all.Sort((a, b) => ToComparisonResult(InRightOrder(a, b)));

var answer2 = dividers.Select(d => all.IndexOf(d) + 1).Aggregate((a, b) => a * b);
Console.WriteLine($"Answer 2: {answer2}");

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

Node ParseLine(string line)
{
    var tokens = Tokenize(line);

    var index = 0;

    return Parse(tokens, ref index);
}

List<Token> Tokenize(string line)
{
    var result = new List<Token>();

    int index = 0;

    while (index < line.Length)
    {
        if (!char.IsDigit(line[index]))
        {
            result.Add(line[index] switch
            {
                '[' => new Start(),
                ']' => new End(),
                ',' => new Separator(),
                _ => throw new Exception()
            });

            index++;

            continue;
        }

        var digits = new List<char>();

        while (index < line.Length && char.IsDigit(line[index]))
        {
            digits.Add(line[index]);
            index++;
        }

        result.Add(new Number { Value = int.Parse(string.Join("", digits)) });
    }

    return result;
}

Node Parse(List<Token> tokens, ref int index)
{
    if (tokens[index] is Number number)
    {
        index++;

        return number;
    }

    // consume [
    index++;

    var children = new List<Node>();
    var end = false;

    while (!end)
    {
        if (tokens[index] is Number || tokens[index] is Start)
        {
            var node = Parse(tokens, ref index);
            children.Add(node);
        }

        // consume , or ]
        end = tokens[index] is End;
        index++;
    }

    return new Collection { Children = children };
}

bool? InRightOrder(Node left, Node right)
{
    if (left is Number leftInt && right is Number rightInt)
    {
        if (leftInt.Value < rightInt.Value)
        {
            return true;
        }
        else if (leftInt.Value > rightInt.Value)
        {
            return false;
        }
        else
        {
            return null;
        }
    }

    if (left is Collection leftList && right is Collection rightList)
    {
        for (int i = 0; i < Math.Min(leftList.Children.Count, rightList.Children.Count); i++)
        {
            var innerResult = InRightOrder(leftList.Children[i], rightList.Children[i]);

            if (innerResult != null)
            {
                return innerResult;
            }
        }

        if (leftList.Children.Count < rightList.Children.Count)
        {
            return true;
        }
        else if (leftList.Children.Count > rightList.Children.Count)
        {
            return false;
        }
        else
        {
            return null;
        }
    }

    if (left is Number)
    {
        return InRightOrder(new Collection { Children = new() { left } }, right);
    }

    return InRightOrder(left, new Collection { Children = new() { right } });
}

int ToComparisonResult(bool? value)
{
    if (value == null)
    {
        return 0;
    }

    if (value == true)
    {
        return -1;
    }

    return 1;
}

interface Token { }
interface Node { }

class Start : Token { }

class End : Token { }

class Separator : Token { }

class Number : Token, Node
{
    public int Value { get; set; }
}

class Collection : Node
{
    public List<Node> Children { get; set; } = new();
}
