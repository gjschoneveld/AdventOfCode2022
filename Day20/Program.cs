var input = File.ReadAllLines("input.txt");
var items = CreateItems(input);

Mix(items);
var answer1 = GroveCoordinates(items);
Console.WriteLine($"Answer 1: {answer1}");

items = CreateItems(input, true);

for (int i = 0; i < 10; i++)
{
    Mix(items);
}

var answer2 = GroveCoordinates(items);
Console.WriteLine($"Answer 2: {answer2}");

long GroveCoordinates(List<Item> items)
{
    var item = items.First(item => item.Value == 0);
    var steps = 1000 % items.Count;

    var result = 0L;

    for (int i = 0; i < 3; i++)
    {
        for (int j = 0; j < steps; j++)
        {
            item = item!.Next;
        }

        result += item!.Value;
    }

    return result;
}

void Mix(List<Item> items)
{
    for (int i = 0; i < items.Count; i++)
    {
        var item = items[i];
        var next = item.Next;
        var previous = item.Previous;

        // remove item
        previous!.Next = next;
        next!.Previous = previous;

        // find new location
        var steps = item.Value;
        steps %= items.Count - 1;

        if (steps < 0)
        {
            steps += items.Count - 1;
        }

        for (int j = 0; j < steps; j++)
        {
            next = next!.Next;
        }

        // insert item
        previous = next!.Previous;
        previous!.Next = item;
        item.Previous = previous;
        item.Next = next;
        next.Previous = item;
    }
}

List<Item> CreateItems(string[] input, bool useKey = false)
{
    var items = input.Select(int.Parse).Select(v => new Item { Value = useKey ? 811589153L * v : v }).ToList();

    // create linked list
    for (int i = 0; i < items.Count; i++)
    {
        items[i].Previous = items[(i + items.Count - 1) % items.Count];
        items[i].Next = items[(i + 1) % items.Count];
    }

    return items;
}

class Item
{
    public Item? Previous { get; set; }
    public Item? Next { get; set; }
    public long Value { get; set; }
}
