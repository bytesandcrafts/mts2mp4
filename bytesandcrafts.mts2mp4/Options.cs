using CommandLine;

namespace bytesandcrafts.mts2mp4;

public class Options
{
    [Option('i', "input", Required = true, HelpText = "Input folder path.")]
    public string InputFolder { get; set; }

    [Option('o', "output", Required = true, HelpText = "Output folder path.")]
    public string OutputFolder { get; set; }
}