using CommandLine;
using CommandLine.Text;
using System.IO;

namespace Project_One
{
    public class ProgramOptions
    {
        [Option(shortName: 's', Group = "threadMode", HelpText = "Run in single threaded mode")]
        public bool SingleThread { get; set; }
        
        [Option(shortName: 'd', Group = "threadMode", HelpText = "Run in parallel mode (uses all available processors)")]
        public bool Parallel { get; set; }
        
        [Option(shortName: 'b', Group = "threadMode", HelpText = "Run in both parallel and single threaded mode. Runs parallel followed by sequential mode")]
        public bool Both { get; set; }
        
        [Value(index: 0, MetaName = "Path", Required = true, HelpText = "Starting directory to parse through")]
        public string? Path { get; set; }

        [Usage(ApplicationAlias = "du")]
        public static IEnumerable<Example> Examples
        {
            get 
            {
                return new List<Example>()
                {
                    new Example("Summarize disk usage of the set of FILES, recursively for directories",
                        new ProgramOptions { Both = true, Path = "C:/Windows"})
                };
            }
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ProgramOptions>(args).WithParsed(options =>
            {
                if (!Directory.Exists(options.Path))
                {
                    Console.WriteLine(options.Path + " isn't a directory, ending process...");
                    return;
                }
                
                Console.WriteLine("Made it past :3");
            });
        }

        /*static string SequentialDiskUsage(string path)
        {
            //
        }

        static string ParallelDiskUsage(string path)
        {
            //
        }*/
    }
}