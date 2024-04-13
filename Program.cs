using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Text.Encodings;
using Microsoft.VisualBasic;

namespace bin2imgs
{
    public class Program
    {
        public static int Main(string[] args)
        {
            //Console.InputEncoding = System.Text.Encoding.GetEncoding(932);
            //Console.OutputEncoding = System.Text.Encoding.GetEncoding(932);
            bool rev = false;
            string filename = "";
            if(args.Length < 1)
            {
                //Console.Error.WriteLine("Usage: bin2imgs <filename> [-r | --reverse]");
                //return 1;
                Console.WriteLine("Please enter the file path.");
                string? tmp = Console.ReadLine();
                if(!string.IsNullOrEmpty(tmp)) filename = tmp;
                else
                {
                    Console.Error.WriteLine("No filename specified.");
                    return 1;
                }
            }
            else
            {
                for(int i = 0; i < args.Length; i++)
                {
                    if(args[i] == "-r" || args[i] == "--reverse")
                    {
                        rev = true;
                    }
                    else if(filename == "")
                    {
                        filename = args[i];
                    }
                    else
                    {
                        Console.Error.WriteLine($"Unknown argument: {args[i]}");
                        return 1;
                    }
                }
            }
            filename = filename.Replace("\"", "");
            if(filename == null)
            {
                Console.Error.WriteLine("No filename specified.");
                return 1;
            }
            if(!File.Exists(filename))
            {
                Console.Error.WriteLine($"File not found: {Path.GetFullPath(filename)}");
                return 1;
            }
            var start = Curtime();
            //Console.WriteLine(filename);
            if(!rev) using (var f = File.OpenRead(filename))
            {
                Mat mat;
                byte[] buf = new byte[1920 * 1080 / 8 * 3 / (2 * 2)];
                ushort[] nbuf = new ushort[1920 * 1080 * 3 / (2 * 2)];
                //byte[] buf = new byte[(int)(1024)];
                int bytesRead, x, y;
                    while ((bytesRead = f.Read(buf, 0, buf.Length)) > 0)
                    {
                        //Console.WriteLine(Convert.ToBase64String(buf, 0, bytesRead));
                        mat = new Mat(1920, 1080, MatType.CV_8UC3, new Scalar(0, 0, 0));
                        List<List<long>> m = [];
                        if(bytesRead == buf.Length)
                        {
                            Parallel.For(0, 1920 * 1080 / 8 * 3 / (2 * 2), (i) =>
                            {
                                string tmp = Convert.ToString(buf[i], 2).PadLeft(8, '0');
                                //for (int j = 0; j < 8; j++) ushort.TryParse(tmp[j].ToString(), out ushort tmp2);
                                for (int j = 0; j < 8; j++)
                                {
                                    if(tmp[j] == '0') nbuf[i*8+j] = 0;
                                    else nbuf[i*8+j] = 1;
                                }
                                //Array.Copy(Convert.ToString(buf[i], 2).PadLeft(8, '0').Select(c => ushort.Parse(c.ToString())).ToArray(), 0, nbuf, i * 8, 8);
                            });
                            if(false)Parallel.For(0, mat.Height / 2, (i) =>
                            {
                                y = i * 2;
                                    char[] data = ['0', '0', '0'];
                                    if (y != mat.Height - 1)
                                {
                                    for (int j = 0; j < mat.Width / 2; j += 2)
                                    {
                                        x = j * 2;
                                        mat.At<Vec3b>(y, x) = new Vec3b(255, 255, 255);
                                        mat.At<Vec3b>(y, x + 1) = new Vec3b(255, 255, 255);
                                        mat.At<Vec3b>(y + 1, x) = new Vec3b(255, 255, 255);
                                        mat.At<Vec3b>(y + 1, x + 1) = new Vec3b(255, 255, 255);
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < mat.Width / 2; j += 2)
                                    {
                                        x = j * 2;
                                        mat.At<Vec3b>(y, x) = new Vec3b(255, 255, 255);
                                        mat.At<Vec3b>(y, x + 1) = new Vec3b(255, 255, 255);
                                        mat.At<Vec3b>(y - 1, x) = new Vec3b(255, 255, 255);
                                        mat.At<Vec3b>(y - 1, x + 1) = new Vec3b(255, 255, 255);
                                    }
                                }
                            });
                        }
                        //Console.WriteLine("{0}, {1}", m.Max(list => list[0]).ToString(), m.Max(list => list[1]).ToString());
                        //Cv2.ImShow("test", mat);
                        //Cv2.WaitKey();
                    }
                }
            else
            {

            }
            var sec = (Curtime()-start)/1000;
            Console.WriteLine($"Finish: {sec}s");
            double bps = new FileInfo(filename).Length/sec;
            string[] headlis = ["K", "M", "G", "P"];
            string head = "";
            int headnum = 0;
            while(1000 < bps)
            {
                bps/=1000;
                headnum++;
            }
            if(headnum != 0) head = headlis[headnum-1];
            var stbps = bps.ToString();
            Console.WriteLine($"{stbps}{head}B/s");
            return 0;
        }
        public static long Curtime() //手抜き用
        {
            return (long)DateTimeOffset.UtcNow.Subtract(DateTimeOffset.UnixEpoch).TotalMilliseconds;
        }
    }
}