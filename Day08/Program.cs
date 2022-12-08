var input = File.ReadAllLines("input.txt");
var field = input.Select(line => line.Select(tree => tree - '0').ToList()).ToList();

(int x, int y) topleft = (0, 0);
(int x, int y) bottomright = (field[0].Count - 1, field.Count - 1);

(int x, int y) north = (0, -1);
(int x, int y) south = (0, 1);
(int x, int y) east = (1, 0);
(int x, int y) west = (-1, 0);

var visible = new HashSet<(int x, int y)>();
FindVisibleTrees(field, visible, topleft, east, south);
FindVisibleTrees(field, visible, topleft, south, east);
FindVisibleTrees(field, visible, bottomright, west, north);
FindVisibleTrees(field, visible, bottomright, north, west);

var answer1 = visible.Count;
Console.WriteLine($"Answer 1: {answer1}");

var answer2 = AvailablePositions(field).Max(p => ScenicScore(field, p));
Console.WriteLine($"Answer 2: {answer2}");

bool IsValidPosition(List<List<int>> field, (int x, int y) position)
{
    return 0 <= position.y && position.y < field.Count &&
        0 <= position.x && position.x < field[position.y].Count;
}

int GetValue(List<List<int>> field, (int x, int y) position)
{
    return field[position.y][position.x];
}

void FindVisibleTrees(List<List<int>> field, HashSet<(int x, int y)> visible, (int x, int y) start, (int x, int y) walkDirection, (int x, int y) sightDirection)
{
    var position = start;

    while (IsValidPosition(field, position))
    {
        var sight = position;
        var highest = -1;

        while (IsValidPosition(field, sight))
        {
            var value = GetValue(field, sight);

            if (value > highest)
            {
                highest = value;
                visible.Add(sight);
            }

            sight = (sight.x + sightDirection.x, sight.y + sightDirection.y);
        }

        position = (position.x + walkDirection.x, position.y + walkDirection.y);
    }
}

int ViewingDistance(List<List<int>> field, (int x, int y) position, (int x, int y) direction)
{
    var distance = 0;
    var self = GetValue(field, position);

    position = (position.x + direction.x, position.y + direction.y);
    
    while (IsValidPosition(field, position))
    {
        distance++;

        var value = GetValue(field, position);

        if (value >= self)
        {
            break;
        }

        position = (position.x + direction.x, position.y + direction.y);
    }

    return distance;
}

int ScenicScore(List<List<int>> field, (int x, int y) position)
{
    return ViewingDistance(field, position, north) *
        ViewingDistance(field, position, south) *
        ViewingDistance(field, position, east) *
        ViewingDistance(field, position, west);
}

IEnumerable<(int x, int y)> AvailablePositions(List<List<int>> field)
{
    for (int y = 0; y < field.Count; y++)
    {
        for (int x = 0; x < field[y].Count; x++)
        {
            yield return (x, y);
        }
    }
}
