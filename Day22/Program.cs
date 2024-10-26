using System.Text.RegularExpressions;
using Map = string[];
using Point = (int x, int y);

var input = File.ReadAllLines("input.txt");
var map = input[..^2];
var commands = ParseCommands(input[^1]);

var state = State.Parse(map);
state = ApplyCommands(map, commands, state);
var answer1 = state.Password();
Console.WriteLine($"Answer 1: {answer1}");

state = Cube.Parse(map);
state = ApplyCommands(map, commands, state);
var answer2 = state.Password();
Console.WriteLine($"Answer 2: {answer2}");

void Print(Map map, State state)
{
    for (int y = 0; y < map.Length; y++)
    {
        for (int x = 0; x < map[y].Length; x++)
        {
            if (state.Position == (x, y))
            {
                Console.Write(state.Direction switch
                { 
                    Direction.Right => '>',
                    Direction.Down => 'v',
                    Direction.Left => '<',
                    Direction.Up => '^',
                    _ => throw new()
                });

                continue;
            }

            Console.Write(map[y][x]);
        }

        Console.WriteLine();
    }

    Console.WriteLine();
}

State ApplyCommands(Map map, List<Command> commands, State state)
{
    //Print(map, state);

    foreach (var command in commands)
    {
        state = command.Apply(map, state);
        //Print(map, state);
    }

    return state;
}

List<Command> ParseCommands(string line)
{
    var match = Regex.Match(line, @"^(?<token>\d+|[LR])+$");
    var tokens = match.Groups["token"].Captures.Select(c => c.Value).ToList();
    return tokens.Select(Command.Parse).ToList();
}

enum Direction
{
    Right,
    Down,
    Left,
    Up,
}

abstract class Command
{
    public abstract State Apply(string[] map, State state);

    public static Command Parse(string x)
    {
        return x[0] switch
        {
            'L' or 'R' => new TurnCommand
            {
                Direction = x[0] == 'L' ? Direction.Left : Direction.Right
            },
            >= '0' and <= '9' => new MoveCommand
            {
                Steps = int.Parse(x)
            },
            _ => throw new()
        };
    }
}

class TurnCommand : Command
{
    public Direction Direction { get; set; }

    public override State Apply(string[] map, State state)
    {
        return state with { Direction = Turn(state, Direction).Direction };
    }

    public static Vector Turn(Vector vector, Direction direction)
    {
        return direction switch
        {
            Direction.Left => vector with { Direction = TurnLeft(vector.Direction) },
            Direction.Right => vector with { Direction = TurnRight(vector.Direction) },
            Direction.Down => vector with { Direction = TurnOpposite(vector.Direction) },
            _ => throw new()
        };
    }

    public static Direction TurnLeft(Direction direction)
    {
        return direction switch
        {
            Direction.Right => Direction.Up,
            Direction.Down => Direction.Right,
            Direction.Left => Direction.Down,
            Direction.Up => Direction.Left,
            _ => throw new()
        };
    }

    public static Direction TurnOpposite(Direction direction)
    {
        return TurnLeft(TurnLeft(direction));
    }

    public static Direction TurnRight(Direction direction)
    {
        return TurnLeft(TurnLeft(TurnLeft(direction)));
    }
}

class MoveCommand : Command
{
    public int Steps { get; set; }

    public override State Apply(string[] map, State state)
    {
        for (int step = 0; step < Steps; step++)
        {
            var next = state.Step(map, state);

            if (state.Value(map, next.Position) == '#')
            {
                break;
            }

            state = next;

            // Print(map, state);
        }

        return state;
    }
}

record Vector
{
    public Point Position { get; set; }
    public Direction Direction { get; set; }

    public int? Id { get; set; }

    public override string? ToString()
    {
        return Id != null ? $"{Id} {Direction}" : $"{Position} {Direction}";
    }
}

record State : Vector
{
    public int Value(Map map, Point position)
    {
        return map[position.y][position.x];
    }

    public bool IsValid(Map map, Point position)
    {
        return 0 <= position.y && position.y < map.Length &&
            0 <= position.x && position.x < map[position.y].Length &&
            map[position.y][position.x] != ' ';
    }

    public Point Delta(Direction direction)
    {
        return direction switch
        {
            Direction.Left => (-1, 0),
            Direction.Right => (1, 0),
            Direction.Up => (0, -1),
            Direction.Down => (0, 1),
            _ => throw new()
        };
    }

    public Point Add(Point position, Point delta)
    {
        return (position.x + delta.x, position.y + delta.y);
    }

    public virtual State Step(Map map, State state)
    {
        var delta = Delta(state.Direction);
        var next = Add(state.Position, delta);

        if (IsValid(map, next))
        {
            return state with { Position = next };
        }

        // not valid; step in reversed direction to find start
        delta = (-delta.x, -delta.y);
        next = state.Position;

        while (IsValid(map, next))
        {
            state = state with { Position = next };
            next = Add(next, delta);
        }

        return state;
    }

    public int Password()
    {
        return 1000 * (Position.y + 1) + 4 * (Position.x + 1) + (int)Direction;
    }

    public static State Parse(Map map)
    {
        return new()
        {
            Position = (map[0].IndexOf('.'), 0),
            Direction = Direction.Right
        };
    }
}

record Cube : State
{
    public int Size { get; set; }

    public Vector? Left { get; set; }
    public Vector? Right { get; set; }
    public Vector? Up { get; set; }
    public Vector? Down { get; set; }
    public Vector? Front { get; set; }
    public Vector? Back { get; set; }

    public Vector? Turn(Vector? vector, Direction direction)
    {
        if (vector == null)
        {
            return null;
        }

        return TurnCommand.Turn(vector, direction);
    }

    public Cube MoveLeft()
    {
        return this with
        {
            Left = Front,
            Right = Back,
            Up = Turn(Up, Direction.Right),
            Down = Turn(Down, Direction.Left),
            Front = Right,
            Back = Left
        };
    }

    public Cube MoveRight()
    {
        return MoveLeft().MoveLeft().MoveLeft();
    }

    public Cube MoveUp()
    {
        return this with
        {
            Left = Turn(Left, Direction.Left),
            Right = Turn(Right, Direction.Right),
            Up = Front,
            Down = Back,
            Front = Turn(Down, Direction.Down),
            Back = Turn(Up, Direction.Down)
        };
    }

    public Cube MoveDown()
    {
        return MoveUp().MoveUp().MoveUp();
    }

    public Cube RotateLeft()
    {
        return this with
        {
            Direction = TurnCommand.TurnLeft(Direction),
            Left = Turn(Up, Direction.Left),
            Right = Turn(Down, Direction.Right),
            Up = Turn(Right, Direction.Left),
            Down = Turn(Left, Direction.Right),
            Front = Turn(Front, Direction.Left),
            Back = Turn(Back, Direction.Right)
        };
    }

    public static List<Vector> GetNeighbours(Map map, int size, Point position)
    {
        List<Vector> candidates = [
            new Vector { Direction = Direction.Left, Position = (position.x - 1, position.y) },
            new Vector { Direction = Direction.Right, Position = (position.x + 1, position.y) },
            new Vector { Direction = Direction.Up, Position = (position.x, position.y - 1) },
            new Vector { Direction = Direction.Down, Position = (position.x, position.y + 1) },
        ];

        return candidates.Where(s =>
                0 <= s.Position.y &&
                s.Position.y * size < map.Length &&
                0 <= s.Position.x &&
                s.Position.x * size < map[s.Position.y * size].Length &&
                map[s.Position.y * size][s.Position.x * size] != ' ')
            .ToList();
    }

    public static Cube FillFaces(Cube cube, Map map, Point position)
    {
        cube.Front = new()
        {
            Position = position,
            Direction = Direction.Up
        };

        var neighbours = GetNeighbours(map, cube.Size, position);

        foreach (var neighbour in neighbours)
        {
            switch (neighbour.Direction)
            {
                case Direction.Left:
                    cube = cube.MoveRight();
                    break;
                case Direction.Right:
                    cube = cube.MoveLeft();
                    break;
                case Direction.Up:
                    cube = cube.MoveDown();
                    break;
                case Direction.Down:
                    cube = cube.MoveUp();
                    break;
            }

            if (cube.Front == null)
            {
                cube = FillFaces(cube, map, neighbour.Position);
            }

            switch (neighbour.Direction)
            {
                case Direction.Left:
                    cube = cube.MoveLeft();
                    break;
                case Direction.Right:
                    cube = cube.MoveRight();
                    break;
                case Direction.Up:
                    cube = cube.MoveUp();
                    break;
                case Direction.Down:
                    cube = cube.MoveDown();
                    break;
            }
        }

        return cube;
    }

    public Point GetFace(Point position)
    {
        return (position.x / Size, position.y / Size);
    }

    public override State Step(Map map, State state)
    {
        var delta = Delta(state.Direction);
        var next = Add(state.Position, delta);

        if (IsValid(map, next) && GetFace(state.Position) == GetFace(next))
        {
            return state with { Position = next };
        }

        // not valid; walk around the edge of the cube
        var cube = (Cube)state;
        Point local = (state.Position.x % Size, state.Position.y % Size);

        switch (state.Direction)
        {
            case Direction.Left:
                cube = cube.MoveRight();
                local = (Size - 1, local.y);
                break;
            case Direction.Right:
                cube = cube.MoveLeft();
                local = (0, local.y);
                break;
            case Direction.Up:
                cube = cube.MoveDown();
                local = (local.x, Size - 1);
                break;
            case Direction.Down:
                cube = cube.MoveUp();
                local = (local.x, 0);
                break;
        }

        while (cube.Front!.Direction != Direction.Up)
        {
            cube = cube.RotateLeft();
            local = (local.y, Size - local.x - 1);
        }

        return cube with { Position = (cube.Front!.Position.x * Size + local.x, cube.Front!.Position.y * Size + local.y) };
    }

    public static new Cube Parse(Map map)
    {
        var size = (int)Math.Sqrt(map.SelectMany(r => r.Where(v => v != ' ')).Count() / 6);

        var x = 0;
        var y = 0;

        while (map[y][x * size] == ' ')
        {
            x++;
        }

        var cube = FillFaces(new Cube { Size = size }, map, (x, y)) with { Position = (map[0].IndexOf('.'), 0) };

        // give faces an id to simplify debugging
        List<Vector> faces = [cube.Front, cube.Back, cube.Left, cube.Right, cube.Up, cube.Down];
        faces = [.. faces.OrderBy(v => v.Position.y).ThenBy(v => v.Position.x)];

        for (int i = 0; i < faces.Count; i++)
        {
            faces[i].Id = i + 1;
        }

        return cube;
    }
}
