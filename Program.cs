using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//</ script >


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
            try
            {
                e.Delete(true);
                Console.WriteLine("deleted: " + e.FullName);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message + " # " + e.FullName);
            }
    }

    static List<DirectoryInfo> Collect()
    {

        var ass = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Assembly.Location;
        //var ass = @"Z:\O\Dokumente\Elder Scrolls Online\live\AddOns\DeleteRedundantLibs\Program.cs";
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent;
        Console.WriteLine("running in " + dir.FullName);


        var addons = dir.GetDirectories().Select(e => e.Name).ToList();

        var result = new List<DirectoryInfo>();
        foreach (var addon in dir.GetDirectories())
            foreach (var libFolder in addon.GetDirectories("libs"))
                foreach (var lib in libFolder.GetDirectories())
                    if (addons.Contains(lib.Name))
                        result.Add(lib);

        return result;
    }
}
