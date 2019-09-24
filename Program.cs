using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public class Program
{
    public static void Main()
    {
        try
        {
            Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            Console.Write(System.Environment.NewLine + "Press Key to exit ...");
            Console.ReadKey();
        }
    }

    static void Run()
    {
        var folders = Collect();

        if (!folders.Any())
        {
            Console.Write("Didn't found redundant libs.");
            return;
        }

        Console.WriteLine("Found these redundant libs:");
        foreach (var e in folders)
            Console.WriteLine(e.FullName);

        Console.Write("Delete (Y/N): ");
        var key = Console.ReadKey().KeyChar;
        var answer = Convert.ToString(key).ToUpper();
        if (!"Y".Equals(answer))
            return;

        Console.WriteLine();
        foreach (var e in folders)
        {
            Console.WriteLine();
            ModifyAddon(e);
        }
    }

    public static void ModifyAddon(DirectoryInfo dir)
    {
        var root = dir.Parent.Parent;
        var txt = root.GetFiles(root.Name + ".txt");

        if (txt.Count() > 0)
        {
            var target = txt.First().FullName;
            var source = File.ReadAllLines(target).ToList();
            var alreadyLinked = new HashSet<string>();
            var s1 = "## DependsOn:"; var s2 = "## OptionalDependsOn:";
            int? idxOptionalDependsOn = null;

            foreach (var row in source)
            {
                if (!row.Contains(s1) && !row.Contains(s2))
                    continue;
                if (row.Contains(s2))
                    idxOptionalDependsOn = source.IndexOf(row);
                foreach (var e3 in row.Split(new[] { s1, s2, " " }, StringSplitOptions.RemoveEmptyEntries))
                    alreadyLinked.Add(e3);
            }
            if (!alreadyLinked.Contains(dir.Name))
            {
                if (idxOptionalDependsOn == null)
                {
                    source.Insert(0, s2);
                    idxOptionalDependsOn = 0;
                }

                source[(int)idxOptionalDependsOn] += " " + dir.Name;
                Console.WriteLine("modifying file: " + target);
                Console.WriteLine(source[(int)idxOptionalDependsOn]);
                File.WriteAllLines(target, source);
            }

        }
        try
        {
            dir.Delete(true);
            Console.WriteLine("deleted: " + dir.FullName);
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message + " # " + dir.FullName);
        }
    }

    static List<DirectoryInfo> Collect()
    {
        var dir = GetAddonPath();
        Console.WriteLine("searching in " + dir.FullName);

        var addons = dir.GetDirectories().Select(e => e.Name).ToList();

        var result = new List<DirectoryInfo>();
        foreach (var addon in dir.GetDirectories())
            foreach (var libFolder in addon.GetDirectories("libs"))
                foreach (var lib in libFolder.GetDirectories())
                    if (addons.Contains(lib.Name))
                        result.Add(lib);

        return result;
    }
    public static DirectoryInfo GetAddonPath()
    {
        var addons = "AddOns".ToLower();
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        //var dir = new DirectoryInfo(@"C:\Users\Test\Documents\Elder Scrolls Online\live\AddOns\DeleteRedundantLibs");
        try
        {
            if (dir.Name.ToLower().Equals(addons))
                return dir;

            while (dir.Parent != null)
            {
                dir = dir.Parent;
                if (dir.Name.ToLower().Equals(addons))
                    return dir;
            }
            throw new System.NotSupportedException("code cant reach here usually.");
        }
        catch
        {
            throw new Exception("working folder " + dir.FullName + " is not located somewhere in the AddOn folder (live/pts).");
        }
    }
}
