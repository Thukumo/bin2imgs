using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace bin2imgs
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var rootCommand = new RootCommand("Convert any file to image files");
            rootCommand.Description = "bin2imgs";
            var filenamearg = new Argument<string>("file-name");
            rootCommand.Add(filenamearg);
            var reverseOption = new Option<bool>(new[] { "--reverse", "-r" }, "Convert movie file to file");
            reverseOption.SetDefaultValue(false);
            reverseOption.AddAlias("-r");
            rootCommand.Add(reverseOption);
            string filename = "";
            bool rev = false;
            rootCommand.SetHandler((fileName, reverse) =>
            {
                //Console.WriteLine($"filename = {fileName}");
                //Console.WriteLine($"--reverse = {reverse}");
                filename = fileName;
                rev = reverse;
            },
            filenamearg, reverseOption);
            rootCommand.InvokeAsync(args).Wait();
            if(!File.Exists(filename))
            {
                Console.Error.WriteLine($"File not found: {filename}");
                return(1);
            }
            Console.WriteLine();
            return 0;
        }
    }
}