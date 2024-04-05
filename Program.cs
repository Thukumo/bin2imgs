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
            var rootCommand = new RootCommand
            {
            new Option<string>(
                    "-f",
                    description: "Specify input file name(filename)"),
                new Option<bool>(
                    "-r",
                    getDefaultValue: () => false,
                description: "Do mov2bin.(reverse)")
            };
            rootCommand.Description = "bin2imgs";
            bool reverse = false;
            rootCommand.Handler = CommandHandler.Create<string, bool>((filename, rev) =>
            {
                reverse = rev;
                if (!reverse && !File.Exists(filename))
                {
                    Console.WriteLine("file not found");
                    return;
                }
            });
            rootCommand.InvokeAsync(args).Wait();
        }
    }
}