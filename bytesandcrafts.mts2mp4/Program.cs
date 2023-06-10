using System.Diagnostics;
using CommandLine;

namespace bytesandcrafts.mts2mp4;

public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                ProcessDirectory(new DirectoryInfo(o.InputFolder), new DirectoryInfo(o.OutputFolder));
            });
    }

    public static void ProcessFile(string sourceFileName, string destinationFileName)
    {
        Console.WriteLine($"Processing file {sourceFileName} into {destinationFileName}");
        if (File.Exists(destinationFileName))
            File.Delete(destinationFileName);
        
        var convertResult = Convert(sourceFileName, destinationFileName);
        if (convertResult != 0)
            Console.WriteLine($"Failed to convert {sourceFileName};");
        else
        {
            var timestampResult = ChangeTimestamp(sourceFileName, destinationFileName);
            if (timestampResult != 0)
                Console.WriteLine($"Failed to change timestamp for {sourceFileName};");
        }
    }
    
    public static void ProcessDirectory(DirectoryInfo source, DirectoryInfo target)
    {
        // Check if the target directory exists, if not, create it
        if (!Directory.Exists(target.FullName))
        {
            Directory.CreateDirectory(target.FullName);
        }

        // Process each file
        foreach (var fi in source.GetFiles())
        {
            var targetFileName = Path.Combine(target.FullName, fi.Name);
            ProcessFile(fi.FullName, targetFileName);
        }

        // Process each subdirectory using recursion
        foreach (DirectoryInfo sourceSubDir in source.GetDirectories())
        {
            var nextTargetSubDir = target.CreateSubdirectory(sourceSubDir.Name);
            ProcessDirectory(sourceSubDir, nextTargetSubDir);
        }
    }

    private static int ChangeTimestamp(string sourceFileName, string destinationFileName)
    {
        var startInfo = new ProcessStartInfo();
        startInfo.FileName = "exiftool";
        startInfo.Arguments = $@"-TagsFromFile ""{sourceFileName}"" -FileModifyDate ""{destinationFileName}""";
        
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;
        
        var process = Process.Start(startInfo);
        process.WaitForExit();
        
        return process.ExitCode;
    }

    private static int Convert(string sourceFileName, string destinationFileName)
    {
        var startInfo = new ProcessStartInfo();
        startInfo.FileName = "ffmpeg";
        startInfo.Arguments =
            $@"-i ""{sourceFileName}"" -c:v libx265 -preset fast -crf 28 -tag:v hvc1 -c:a eac3 -b:a 224k ""{destinationFileName}""";
        
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;
        
        var process = Process.Start(startInfo);
        process.WaitForExit();
        
        return process.ExitCode;
    }
}
