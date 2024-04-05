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
            var rootCommand = new RootCommand("bin2imgs");
            rootCommand.Description = "Convert any file to image files";
            var filenamearg = new Argument<string>("file-name");
            rootCommand.Add(filenamearg);
            var reverseOption = new Option<string>(new[] { "--reverse", "-r" }, "Convert movie file to file");
            reverseOption.SetDefaultValue(false);
            reverseOption.AddAlias("-r");
            rootCommand.Add(reverseOption);
            string filename = "";
            string ofilename = "";
            bool rev = false;
            rootCommand.SetHandler((fileName, reverse) =>
            {
                //Console.WriteLine($"filename = {fileName}");
                filename = fileName;
                if(File.Exists(filename))
                {
                    Console.Error.WriteLine($"File not found: {filename}");
                    Environment.Exit(1);
                }
                if(rev = reverse != "")
                {
                    ofilename = reverse;
                    if(File.Exists(ofilename))
                    {
                        Console.Error.WriteLine($"File already exists: {ofilename}");
                        Console.Error.WriteLine("Do you want to delete it? (y/n)");
                        var lineread = Console.ReadLine();
                        if(lineread != null && string.Equals(lineread, "y"))
                        {
                            //File.Delete(ofilename);
                            //File.Create(ofilename);
                        }
                        else
                        {
                            Console.Error.WriteLine("Aborting...");
                            Console.Error.WriteLine("Press any key to exit...");
                            Console.ReadKey();
                            Environment.Exit(1);
                        }
                            return;
                        }
                }
            },
            filenamearg, reverseOption);
            rootCommand.InvokeAsync(args).Wait();
            if(!File.Exists(filename))
            {
                Console.Error.WriteLine($"File not found: {filename}");
                return 1;
            }
            Console.WriteLine(filename);
            if(rev)
            {

            }
            else
            {

            }
            return 0;
        }
    }
}