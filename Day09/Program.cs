﻿var input = File.ReadAllLines("input.txt");
var motions = input.Select(Parse).ToList();

var answer1 = ApplyMotions(motions, 2);
Console.WriteLine($"Answer 1: {answer1}");

var answer2 = ApplyMotions(motions, 10);
Console.WriteLine($"Answer 2: {answer2}");

(char direction, int count) Parse(string line)
{
    var parts = line.Split(' ');

    return (parts[0][0], int.Parse(parts[1]));
}

bool IsCloseEnough((int x, int y) head, (int x, int y) tail)
{
    return Math.Abs(head.x - tail.x) <= 1 && Math.Abs(head.y - tail.y) <= 1;
}

(int x, int y) MoveCloser((int x, int y) head, (int x, int y) tail)
{
    var dx = Math.Clamp(head.x - tail.x, -1, 1);
    var dy = Math.Clamp(head.y - tail.y, -1, 1);

    return (tail.x + dx, tail.y + dy);
}

int ApplyMotions(List<(char direction, int count)> motions, int knots)
{
    var positions = new (int x, int y)[knots];
    var visited = new HashSet<(int x, int y)>();

    foreach (var motion in motions)
    {
        for (int step = 0; step < motion.count; step++)
        {
            positions[0] = motion.direction switch
            {
                'U' => (positions[0].x, positions[0].y - 1),
                'D' => (positions[0].x, positions[0].y + 1),
                'L' => (positions[0].x - 1, positions[0].y),
                'R' => (positions[0].x + 1, positions[0].y),
                _ => throw new Exception()
            };

            for (int index = 1; index < positions.Length; index++)
            {
                if (!IsCloseEnough(positions[index - 1], positions[index]))
                {
                    positions[index] = MoveCloser(positions[index - 1], positions[index]);
                }
            }

            visited.Add(positions[^1]);
        }
    }

    return visited.Count;
}
