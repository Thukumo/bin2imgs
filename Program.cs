using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Text.Encodings;
using System.Globalization;

namespace bin2imgs
{
    public class Program
    {
        public static int Main(string[] args)
        {
            //Console.InputEncoding = System.Text.Encoding.GetEncoding(932);
            //Console.OutputEncoding = System.Text.Encoding.GetEncoding(932);
            bool rev = false;
            bool max = false;
            bool output_video = false;
            string filename = "";
            if(args.Length < 1)
            {
                //Console.Error.WriteLine("Usage: bin2imgs <filename> [-r | --reverse]");
                //return 1;
                Console.WriteLine("Please enter the file path.");
                string? tmp = Console.ReadLine();
                if(!string.IsNullOrEmpty(tmp)) filename = tmp.Replace("\"", "");
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
                    else if(args[i] == "-m" || args[i] == "--max")
                    {
                        max = true;
                    }
                    else if(args[i] == "-h" || args[i] == "--help")
                    {
                        Console.WriteLine("Usage: bin2imgs <filename> [-r | --reverse] [-m | --max]");
                        return 0;
                    }
                    else if(args[i] == "-v" || args[i] == "--video")
                    {
                        output_video = true;
                    }
                    else if(filename == "" && File.Exists(args[i].Replace("\"", "")))
                    {
                        filename = args[i].Replace("\"", "");;
                    }
                    else
                    {
                        Console.Error.WriteLine($"Unknown argument: {args[i]}");
                        return 1;
                    }
                }
            }
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
                var len = 1920 * (1080-2) * 3 / (2 * 2);
                byte[] buf = new byte[len/8];
                byte[] nbuf = new byte[len];
                List<Mat> frames = [];
                int bytesRead, x, y;
                    while ((bytesRead = f.Read(buf, 0, buf.Length)) > 0)
                    {
                        mat = new Mat(1080, 1920, MatType.CV_8UC3, new Scalar(0, 0, 0));
                        if(bytesRead == buf.Length)
                        {
                            //Console.WriteLine(Environment.ProcessorCount*(cpulate/100.0));
                            Parallel.For(0, len /(8*3), new ParallelOptions(){MaxDegreeOfParallelism = max ? Environment.ProcessorCount : ((Environment.ProcessorCount-2) > 0 ? Environment.ProcessorCount-2 : 1)}, (i) =>
                            //Parallel.For(0, len /(8*3), (i) =>
                            {   //終了条件最後の/3は12bitsずつ処理するため
                                //↑多分このループ処理の中で(1920*(1080-2)) * 3のジャグ配列(2次元)にしたかったんだろうけどめんどいから放置
                                int num = i*3;
                                string[] tmp = ["", "", ""];
                                for(int j = 0; j < 3; j++) tmp[j] = Convert.ToString(buf[num+j], 2).PadLeft(8, '0');
                                //for (int j = 0; j < 8; j++) ushort.TryParse(tmp[j].ToString(), out ushort tmp2);
                                for(int j = 0; j < 3; j++) for(int k = 0; k < 8; k++)
                                {
                                    if(tmp[j][k] == '0') nbuf[(num+j)*8+k] = 0x00;
                                    else nbuf[(num+j)*8+k] = 0xff;
                                }
                                //Array.Copy(Convert.ToString(buf[i], 2).PadLeft(8, '0').Select(c => ushort.Parse(c.ToString())).ToArray(), 0, nbuf, i * 8, 8);
                            });
                            Parallel.For(0, (1080-2)/2, (i) =>
                            {
                                y = i * 2;
                                ushort[] data = [0, 0, 0];
                                int num;
                                Vec3b v;
                                for (int j = 0; j < 1920/2; j++)
                                {
                                    x = j * 2;
                                    num = mat.Width*i+j;
                                    v = new Vec3b(nbuf[num], nbuf[num+1], nbuf[num+2]);
                                    //Console.WriteLine(y.ToString()+" "+x.ToString());
                                    mat.At<Vec3b>(y, x) = v;
                                    mat.At<Vec3b>(y, x + 1) = v;
                                    mat.At<Vec3b>(y + 1, x) = v;
                                    mat.At<Vec3b>(y + 1, x + 1) = v;
                                }
                            });

                        }
                        else
                        {

                        }
                        string binary = Convert.ToString(bytesRead, 2).PadLeft(1920*3/2, '0');
                        Parallel.For(0, 1920/2, new ParallelOptions(){MaxDegreeOfParallelism = max ? Environment.ProcessorCount : ((Environment.ProcessorCount-2) > 0 ? Environment.ProcessorCount-2 : 1)}, (i) =>
                        {
                            int n = i*2;
                            byte[] tmp = new byte[3];
                            for(int j = 0; j < 3; j++)
                            {
                                if(binary[n+j] == '0') tmp[j] = 0x00;
                                else tmp[j] = 0xff;
                            }
                            mat.At<Vec3b>(1078, n) = new Vec3b(tmp[0], tmp[1], tmp[2]);
                            mat.At<Vec3b>(1078, n+1) = new Vec3b(tmp[0], tmp[1], tmp[2]);
                            mat.At<Vec3b>(1079, n) = new Vec3b(tmp[0], tmp[1], tmp[2]);
                            mat.At<Vec3b>(1079, n+1) = new Vec3b(tmp[0], tmp[1], tmp[2]);
                        });
                        if(output_video) frames.Add(mat.Clone());
                        //Console.WriteLine("{0}, {1}", m.Max(list => list[0]).ToString(), m.Max(list => list[1]).ToString());
                        if(!output_video) Cv2.ImWrite("hoge.png", mat, new ImageEncodingParam(ImwriteFlags.JpegQuality, 100));
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