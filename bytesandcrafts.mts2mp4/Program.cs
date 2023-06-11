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
                var filesToProcess = new List<FilePair>();
                ProcessDirectory(o.InputFolder, o.OutputFolder, filesToProcess);

                for (var i = 0; i < filesToProcess.Count; i++)
                {
                    var filePair = filesToProcess[i];
                    Directory.CreateDirectory(Path.GetDirectoryName(filePair.OutputFile));
                    
                    Console.WriteLine($"{i} of {filesToProcess.Count} Processing file {filePair.InputFile} into {filePair.OutputFile}");
                    ProcessFile(filePair.InputFile, filePair.OutputFile);
                }
            });
    }

    public static void ProcessFile(string sourceFileName, string destinationFileName)
    {
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
    
    public static void ProcessDirectory(string source, string target, List<FilePair> filesToProcess)
    {
        // Collect files to process
        foreach (var fi in Directory.GetFiles(source))
        {
            if (Path.GetExtension(fi).ToLower() == ".mts")
            {
                var targetFileName = Path.Combine(target, Path.GetFileNameWithoutExtension(fi) + ".mp4");
                filesToProcess.Add(new FilePair()
                {
                    InputFile = fi,
                    OutputFile = targetFileName
                });
            }
        }

        // Process each subdirectory using recursion
        foreach (var sourceSubDir in Directory.GetDirectories(source))
        {
            var nextTargetSubDir = Path.Combine(target, Path.GetFileName(sourceSubDir));
            ProcessDirectory(sourceSubDir, nextTargetSubDir, filesToProcess);
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
