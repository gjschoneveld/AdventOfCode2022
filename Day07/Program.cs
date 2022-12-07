var input = System.IO.File.ReadAllLines("input.txt");
var root = ConstructTree(input);

var sizes = new List<int>();
var total = GetFolderSize(root, sizes);

var answer1 = sizes.Where(s => s <= 100_000).Sum();
Console.WriteLine($"Answer 1: {answer1}");

var available = 70_000_000;
var needed = 30_000_000;

var required = available - needed;
var toFree = total - required;

var answer2 = sizes.Where(s => s >= toFree).Min();
Console.WriteLine($"Answer 2: {answer2}");

Folder ConstructTree(string[] input)
{
    var root = new Folder
    {
        Name = "/"
    };

    var current = root;

    foreach (var line in input)
    {
        if (line.StartsWith("$"))
        {
            // we have found a command
            var parts = line.Split(' ');

            if (parts[1] != "cd")
            {
                // we just ignore the ls
                continue;
            }

            current = parts[2] switch
            {
                "/" => root,
                ".." => current.Parent!,
                _ => (Folder)current.Content.First(n => n.Name == parts[2]),
            };

            continue;
        }

        // we have found a result of ls
        var node = Node.Parse(line);

        if (current.Content.Any(n => n.Name == node.Name))
        {
            // we already have this node (doesn't happen)
            continue;
        }

        // add node to the tree
        node.Parent = current;
        current.Content.Add(node);
    }

    return root;
}

int GetFolderSize(Folder folder, List<int> sizes)
{
    int size = 0;

    foreach (var child in folder.Content)
    {
        if (child is Folder inner)
        {
            size += GetFolderSize(inner, sizes); 
        }
        else if (child is File file)
        {
            size += file.Size;
        }
    }

    // as side effect we collect all folder sizes
    sizes.Add(size);

    return size;
}

abstract class Node
{
    public Folder? Parent { get; set; }
    public required string Name { get; set; }

    public static Node Parse(string line)
    {
        var parts = line.Split(' ');

        if (parts[0] == "dir")
        {
            return new Folder
            {
                Name= parts[1]
            };
        }

        return new File
        {
            Name = parts[1],
            Size = int.Parse(parts[0])
        };
    }
}

class File : Node
{
    public int Size { get; set; }
}

class Folder : Node
{
    public List<Node> Content { get; set; } = new();
}
