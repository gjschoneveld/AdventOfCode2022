var input = File.ReadAllLines("input.txt");
var sensors = input.Select(Sensor.Parse).ToList();

var minX = sensors.Min(s => s.Location.x - s.DistanceTo(s.NearestBeacon));
var maxX = sensors.Max(s => s.Location.x + s.DistanceTo(s.NearestBeacon));

var y = 2_000_000;
var empty = 0;

for (int x = minX; x <= maxX; x++)
{
    var position = (x, y);

    if (sensors.Any(s => s.IsEmpty(position)))
    {
        empty++;
    }
}

var answer1 = empty;
Console.WriteLine($"Answer 1: {answer1}");

var gap = FindGap(sensors);
var answer2 = gap.x * 4_000_000L + gap.y;
Console.WriteLine($"Answer 2: {answer2}");

(int x, int y) FindGap(List<Sensor> sensors)
{
    for (int y = 0; y <= 4_000_000; y++)
    {
        var x = FindGapX(sensors, y);

        if (x != null)
        {
            return ((int)x, y);
        }
    }

    throw new Exception();
}

int? FindGapX(List<Sensor> sensors, int y)
{
    var borders = sensors
        .Select(s => s.Borders(y))
        .Where(b => b != null)
        .Select(b => b!.Value)
        .OrderBy(b => b.left);

    var x = 0;

    foreach (var border in borders)
    {
        if (x > 4_000_000)
        {
            return null;
        }

        if (border.left > x)
        {
            return x;
        }

        x = Math.Max(x, border.right + 1);
    }

    return null;
}

class Sensor
{
    public (int x, int y) Location { get; set; }
    public (int x, int y) NearestBeacon { get; set; }

    public int DistanceTo((int x, int y) other)
    {
        return Math.Abs(Location.x - other.x) + Math.Abs(Location.y - other.y);
    }

    public bool IsEmpty((int x, int y) position)
    {
        return position != NearestBeacon && DistanceTo(position) <= DistanceTo(NearestBeacon);
    }

    public (int left, int right)? Borders(int y)
    {
        var verticalDistance = DistanceTo((Location.x, y));
        var horizontalDistance = DistanceTo(NearestBeacon) - verticalDistance;

        if (horizontalDistance < 0)
        {
            return null;
        }

        return (Location.x - horizontalDistance, Location.x + horizontalDistance);
    }

    public static Sensor Parse(string line)
    {
        var numbers = line
            .Split(new[] { '=', ',', ':' })
            .Where((_, index) => index % 2 == 1)
            .Select(int.Parse)
            .ToList();

        return new()
        {
            Location = (numbers[0], numbers[1]),
            NearestBeacon = (numbers[2], numbers[3]),
        };
    }
}
