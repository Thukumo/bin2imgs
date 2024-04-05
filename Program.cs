using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Threading.Tasks;

namespace bin2imgs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rootCommand = new RootCommand("Convert any file to image files");
            rootCommand.Description = "bin2imgs";
            rootCommand.AddArgument(new Argument<string>("filename"));
            rootCommand.AddOption(new Option<bool>(new[] { "--reverse", "-r" }, "Convert movie file to file"));
            /*
            rootCommand.Handler = CommandHandler.Create<string, bool>((filename, rev) =>
            {
                reverse = rev;
                if (!reverse && !File.Exists(filename))
                {
                    Console.WriteLine("file not found");
                    return;
                }
            });
            */
            rootCommand.InvokeAsync(args).Wait();
            //Console.WriteLine();
        }
    }
}