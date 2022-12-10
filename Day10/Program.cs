namespace Day10;

class Noop : Instruction
{
    public override int Cycles => 1;

    public override void Execute(CPU cpu)
    {

    }
}

class AddX : Instruction
{
    public override int Cycles => 2;
    public int Argument { get; set; }

    public override void Execute(CPU cpu)
    {
        cpu.Register += Argument;
    }
}

abstract class Instruction
{
    public abstract int Cycles { get; }

    public abstract void Execute(CPU cpu);

    public static Instruction Parse(string line)
    {
        var parts = line.Split(' ');

        switch (parts[0])
        {
            case "noop":
                return new Noop();
            case "addx":
                return new AddX
                {
                    Argument = int.Parse(parts[1])
                };
            default:
                throw new Exception();
        }
    }
}

class CPU
{
    public int ProgramCounter { get; set; }
    public int Register { get; set; } = 1;
    public required List<Instruction> Program { get; set; }
    public int Cycle { get; set; } = 1;

    public void Run(Action<int, int> callback)
    {
        while (0 <= ProgramCounter && ProgramCounter < Program.Count)
        {
            var instruction = Program[ProgramCounter];

            for (int i = 0; i < instruction.Cycles; i++)
            {
                callback(Cycle, Register);
                Cycle++;
            }

            instruction.Execute(this);
            ProgramCounter++;
        }
    }
}

class Program
{
    static void Main()
    {
        var input = File.ReadAllLines("input.txt");
        var program = input.Select(Instruction.Parse).ToList();

        var signalStrengths = new List<int>();

        var cpu = new CPU { Program = program };
        cpu.Run((cycle, register) => {
            if ((cycle + 20) % 40 == 0)
            {
                signalStrengths.Add(cycle * register);
            }
        });

        var answer1 = signalStrengths.Sum();
        Console.WriteLine($"Answer 1: {answer1}");


        Console.WriteLine($"Answer 2:");

        var width = 40;

        cpu = new CPU { Program = program };
        cpu.Run((cycle, register) => {
            var position = (cycle - 1) % width;

            if (Math.Abs(register - position) <= 1)
            {
                Console.Write('█');
            }
            else
            {
                Console.Write(' ');
            }

            if (cycle % width == 0)
            {
                Console.WriteLine();
            }
        });
    }
}
