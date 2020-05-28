using System;
using System.Collections.Generic;
using System.Data;
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

        var root = GetAddonPath();
        if (folders.ContainsKey(root))
        {
            Console.WriteLine("Found these libs in subfolders, which are not also stored in the addon root folder (" + root.FullName  + "), and therefore ignored:");
            foreach (var e in folders[root].OrderBy(e => e.Name))
                Console.WriteLine(e.FullName);
            Console.WriteLine();
        }
        folders.Remove(root);

        Console.WriteLine("Found these redundant libs:");
        foreach (var addon in folders.Keys)
            foreach (var e in folders[addon])
                Console.WriteLine(e.FullName);

        Console.Write("Delete them and add as dependency? (Y/N): ");
        var key = Console.ReadKey().KeyChar;
        var answer = Convert.ToString(key).ToUpper();
        if (!"Y".Equals(answer))
            return;

        Console.WriteLine();
        foreach (var e in folders)
        {
            Console.WriteLine();
            try
            {
                ModifyAddon(e.Key, e.Value);
            }
            catch (Exception ex)
            {
                Console.Write("Error cleaning folder: " + e.Key.FullName);
                Console.WriteLine(ex.Message);
            }
        }
    }

    public static void ModifyAddon(DirectoryInfo addon, List<DirectoryInfo> libs)
    {
        //var root = dir.Parent.Parent;
        var manifest = addon.Name + ".txt";
        var txt = addon.GetFiles(manifest);

        if (txt.Count() > 0)
        {
            var target = txt.First().FullName;
            var source = File.ReadAllLines(target).ToList();
            var alreadyLinked = new HashSet<string>();
            var s1 = "## DependsOn:"; var s2 = "## OptionalDependsOn:";
            int? idxDependsOn = null;

            foreach (var row in source)
                if (row.Contains(s1))
                {
                    idxDependsOn = source.IndexOf(row);
                    ParseLibsIntoList(row, s1, alreadyLinked);
                }
                else if (row.Contains(s2))
                    ParseLibsIntoList(row, s2, alreadyLinked);

            var neu = string.Empty;
            foreach (var dir in libs)
                if (!alreadyLinked.Contains(dir.Name.ToLower()))
                    neu += " " + dir.Name;

            if (!string.IsNullOrWhiteSpace(neu))
            {
                if (idxDependsOn == null)
                {
                    neu = s1 + neu;
                    source.Insert(0, neu);
                    idxDependsOn = 0;
                }
                else
                    source[(int)idxDependsOn] += neu;
                Console.WriteLine("modified line in " + target + " :");
                Console.WriteLine(source[(int)idxDependsOn]);

                //throw exception if no write access = abort
                File.WriteAllLines(target, source);
            }

        }
        else
        {
            Console.Write("Couldn't find addon manifest: " + manifest + Environment.NewLine + "Delete libs anyway (Y/N): ");
            var key = Console.ReadKey().KeyChar;
            var answer = Convert.ToString(key).ToUpper();
            if (!"Y".Equals(answer))
            {
                Console.WriteLine("User canceled cleanup of: " + addon.Name);
                return;
            }
        }

        foreach (var dir in libs)
        {
            dir.Delete(true);
            Console.WriteLine("deleted: " + dir.FullName);
        }
    }

    private static void ParseLibsIntoList(string row, string key, HashSet<string> alreadyLinked)
    {
        foreach (var e in row.Split(new[] { key, ">=", ",", " ", "  " }, StringSplitOptions.RemoveEmptyEntries))
            alreadyLinked.Add(e.ToLower());
    }

    static Dictionary<DirectoryInfo, List<DirectoryInfo>> Collect()
    {
        var root = GetAddonPath();
        Console.WriteLine("searching in " + root.FullName);

        var addons = root.GetDirectories().Select(e => e.Name.ToLower()).ToList();

        var result = new Dictionary<DirectoryInfo, List<DirectoryInfo>>();
        foreach (var addon in root.GetDirectories())
            foreach (var libFolderNames in new[] { "libs", "lib" })
                foreach (var libFolder in addon.GetDirectories(libFolderNames))
                    foreach (var lib in libFolder.GetDirectories())
                        if (addons.Contains(lib.Name.ToLower()))
                        {
                            if (result.ContainsKey(addon))
                                result[addon].Add(lib);
                            else
                                result[addon] = new List<DirectoryInfo> { lib };
                        }
                        else
                        {
                            if (result.ContainsKey(root))
                                result[root].Add(lib);
                            else
                                result[root] = new List<DirectoryInfo> { lib };
                        }

        return result;
    }


    static DirectoryInfo _GetAddonPath;
    public static DirectoryInfo GetAddonPath()
    {
        if (_GetAddonPath == null)
        {
            var addons = "AddOns".ToLower();
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            //var dir = new DirectoryInfo(@"C:\Users\Test\Documents\Elder Scrolls Online\live\AddOns\DeleteRedundantLibs");
            try
            {
                if (dir.Name.ToLower().Equals(addons))
                {
                    _GetAddonPath = dir;
                    return dir;
                }

                while (dir.Parent != null)
                {
                    dir = dir.Parent;
                    if (dir.Name.ToLower().Equals(addons))
                    {
                        _GetAddonPath = dir;
                        return dir;
                    }
                }
                throw new System.NotSupportedException("code cant reach here usually.");
            }
            catch
            {
                throw new Exception("working folder " + dir.FullName + " is not located somewhere in the AddOn folder (live/pts).");
            }
        }
        return _GetAddonPath;
    }
}
