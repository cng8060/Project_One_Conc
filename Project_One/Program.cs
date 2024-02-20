using System.Collections.Immutable;
using System.Diagnostics;
using CommandLine;
using CommandLine.Text;

namespace Project_One
{
    public class ProgramOptions
    {
        [Option(shortName: 's', Group = "threadMode", HelpText = "Run in single threaded mode")]
        public bool SingleThread { get; set; }
        
        [Option(shortName: 'd', Group = "threadMode",
            HelpText = "Run in parallel mode (uses all available processors)")]
        public bool Parallel { get; set; }
        
        [Option(shortName: 'b', Group = "threadMode",
            HelpText = "Run in both parallel and single threaded mode. Runs parallel followed by sequential mode")]
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
        private static ImmutableList<string> _imgExts = ImmutableList.Create(
            ".jpeg", ".jpg", ".gif", ".png", ".bmp", ".tiff", ".svg", ".raw", ".webp", ".avif", ".heif"
            );
        
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ProgramOptions>(args).WithParsed(options =>
            {
                if (!Directory.Exists(options.Path))
                {
                    Console.WriteLine(options.Path + " isn't a directory, ending process...");
                    return;
                }

                if (options.SingleThread)
                {
                    DateTime startTime = DateTime.Now;
                    
                    var (countedFolders, countedFiles, totalSpaceAmount, countedImages,
                        imageSpaceAmount) = SequentialDiskUsage(options.Path);
                    
                    DateTime endTime = DateTime.Now;
                    Double elapsedSeconds = (endTime - startTime).TotalSeconds;
                    
                    Console.WriteLine(
                        "Sequential Calculated in: {0}s\n{1} folders, {2} files, {3} bytes\n{4} image files, {5} bytes",
                        elapsedSeconds, countedFolders.ToString("N0"), countedFiles.ToString("N0"),
                        totalSpaceAmount.ToString("N0"), countedImages.ToString("N0"),
                        imageSpaceAmount.ToString("N0"));
                }
                else if (options.Parallel)
                {
                    DateTime startTime = DateTime.Now;

                    var (countedFolders, countedFiles, totalSpaceAmount, countedImages,
                            imageSpaceAmount) = ParallelDiskUsage(options.Path);
                    
                    DateTime endTime = DateTime.Now;
                    Double elapsedSeconds = (endTime - startTime).TotalSeconds;
                    
                    Console.WriteLine(
                        "Parallel Calculated in: {0}s\n{1} folders, {2} files, {3} bytes\n{4} image files, {5} bytes",
                        elapsedSeconds, countedFolders.ToString("N0"), countedFiles.ToString("N0"),
                        totalSpaceAmount.ToString("N0"), countedImages.ToString("N0"),
                        imageSpaceAmount.ToString("N0"));
                }
                else
                {
                    Console.WriteLine("Not made yet :3");
                }
            });
        }

        private static (long, long, long, long, long) SequentialDiskUsage(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

            long countedFolders = 0;
            long countedFiles = 0;
            long totalSpaceAmount = 0;
            long countedImages = 0;
            long imageSpaceAmount = 0;

            foreach (string file in files)
            {
                FileInfo currFile = new FileInfo(file);
                
                countedFiles++;
                totalSpaceAmount += currFile.Length;

                if (_imgExts.Contains(currFile.Extension))
                {
                    countedImages++;
                    imageSpaceAmount += currFile.Length;
                }
            }

            foreach (string dir in dirs)
            {
                countedFolders++;

                var (cFo, cFi, tsa, ci, isa) = SequentialDiskUsage(dir);
                countedFiles += cFo;
                countedFiles += cFi;
                totalSpaceAmount += tsa;
                countedImages += ci;
                imageSpaceAmount += isa;
            }
            
            return (countedFolders, countedFiles, totalSpaceAmount, countedImages, imageSpaceAmount);
        }

        private static (long, long, long, long, long) ParallelDiskUsage(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

            long countedFolders = 0;
            long countedFiles = 0;
            long totalSpaceAmount = 0;
            long countedImages = 0;
            long imageSpaceAmount = 0;

            Parallel.ForEach(files, file =>
            {
                FileInfo currFile = new FileInfo(file);
                
                Interlocked.Increment(ref countedFiles);
                Interlocked.Add(ref totalSpaceAmount, currFile.Length);

                if (_imgExts.Contains(currFile.Extension))
                {
                    Interlocked.Increment(ref countedImages);
                    Interlocked.Add(ref imageSpaceAmount, currFile.Length);
                }
            });
            
            Parallel.ForEach(dirs, dir =>
            {
                Interlocked.Increment(ref countedFolders);
                
                var (cFo, cFi, tsa, ci, isa) = ParallelDiskUsage(dir);
                Interlocked.Add(ref countedFiles, cFo);
                Interlocked.Add(ref countedFiles, cFi);
                Interlocked.Add(ref totalSpaceAmount, tsa);
                Interlocked.Add(ref countedImages, ci);
                Interlocked.Add(ref imageSpaceAmount, isa);
            });
            
            return (countedFolders, countedFiles, totalSpaceAmount, countedImages, imageSpaceAmount);
        }
    }
}