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
            Mat mat;
            var len = 1920 * (1080-2) * 3 / (2 * 2); //bit単位
            if(!rev)
            using (var f = File.OpenRead(filename))
            {
                byte[] buf = new byte[len/8];
                byte[] nbuf = new byte[len];
                List<Mat> frames = [];
                int bytesRead, x, y;
                VideoWriter writer = new(Path.GetDirectoryName(filename)+"\\"+Path.GetFileNameWithoutExtension(filename)+".mp4", FourCC.H264, 60, new Size(1920, 1080));
                if(!writer.IsOpened())
                {
                    Console.Error.WriteLine("Failed to open output file.");
                    return 1;
                }
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
                                int num = 1920*i-1; //+=1対策
                                for (int j = 0; j < 1920/2; j++)
                                {
                                    x = j * 2;
                                    num += 1;
                                    mat.At<Vec3b>(y, x) = mat.At<Vec3b>(y, x + 1) = mat.At<Vec3b>(y + 1, x) = mat.At<Vec3b>(y + 1, x + 1) = new Vec3b(nbuf[num], nbuf[num+1], nbuf[num+2]);
                                }
                            });
                        }
                        else
                        {
                            for(int num = 0; num < len/8; num++)
                            {
                                if(num <= bytesRead)
                                {
                                    string tmp = Convert.ToString(buf[num], 2).PadLeft(8, '0');
                                    for(int j = 0; j < 8; j++)
                                    {
                                        if(tmp[j] == '0') nbuf[num*8+j] = 0x00;
                                        else nbuf[num*8+j] = 0xff;
                                    }
                                }
                                else
                                {
                                    for(int j =  0; j < 8; j++) nbuf[num*8+j] = 0x00;
                                }
                            }
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
                        //Console.WriteLine("{0}, {1}", m.Max(list => list[0]).ToString(), m.Max(list => list[1]).ToString());
                        writer.Write(mat);
                        //Cv2.ImShow("test", mat);
                        //Cv2.WaitKey();
                    }
                    writer.Release();
                }
            else //デコード
            {
                mat = new Mat();
                var videofile = new VideoCapture(filename);
                if(!videofile.IsOpened())
                {
                    Console.Error.WriteLine("Failed to open video file.");
                    return 1;
                }
                int framenum = (int)videofile.Get(VideoCaptureProperties.FrameCount);
                var bytesRead = 0;
                if(File.Exists(Path.GetDirectoryName(filename)+"\\"+Path.GetFileNameWithoutExtension(filename))) File.Delete(Path.GetDirectoryName(filename)+"\\"+Path.GetFileNameWithoutExtension(filename));
                using (var stream = new FileStream(Path.GetDirectoryName(filename)+"\\"+Path.GetFileNameWithoutExtension(filename), FileMode.Append)) using (var writer = new BinaryWriter(stream)) for(int i = 0; i < framenum; i++)
                {
                    byte[] data = new byte[len];
                    while(!videofile.Read(mat)) Thread.Sleep(5);
                    { //tmpを封じ込める用
                        var tmp = "";
                        for(int w = 0; w < 1920; w+=2)
                        {
                            for(int j = 0; j < 3; j++) tmp += (mat.At<Vec3b>(1078, w)[j]+mat.At<Vec3b>(1078, w+1)[j]+mat.At<Vec3b>(1079, w)[j]+mat.At<Vec3b>(1079, w+1)[j])/4 > 255/2 ? "1" : "0";
                        }
                        //Console.WriteLine(tmp);
                        bytesRead = Convert.ToInt32(tmp, 2);
                        Console.WriteLine(bytesRead);
                    }
                    var range = bytesRead > 0 ? bytesRead*8 : len;
                    byte[] reslis = new byte[range/8];
                    char[] charlis = new char[range*10];
                        Parallel.For(0, (1080-2)/2, (i) =>
                        {
                            int y = i * 2;
                            int x;
                            int num = 1920/2*3*i-3;
                            for (int j = 0; j < 1920/2; j++)
                            {
                                x = j * 2;
                                num += 3;
                                for(int k = 0; k < 3; k++) charlis[num+k] = (mat.At<Vec3b>(y, x)[k] + mat.At<Vec3b>(y, x + 1)[k] + mat.At<Vec3b>(y + 1, x)[k] + mat.At<Vec3b>(y + 1, x + 1)[k])/4 > 127 ? '1' : '0';
                            }
                        });
                    /*
                    List<int> test = new List<int>();
                    List<int> test2 = new List<int>();
                    Parallel.For(0, range/2, (j) =>
                    {
                        int num = j*2;
                        int x = num/1920;
                        test.Add(x);
                        int y = num%1920;
                        test2.Add(y);
                        if(1079 <= x || 1919 <= y) Console.WriteLine($"{x}, {y}");
                        for(int k = 0; k < 3; k++) charlis[j*3+k] = (mat.At<Vec3b>(x, y)[k]+mat.At<Vec3b>(x, y+1)[k]+mat.At<Vec3b>(x+1, y)[k]+mat.At<Vec3b>(x+1, y+1)[k])/4 > 255/2 ? '1' : '0';
                    });
                    */
                    //Console.WriteLine(test.Max().ToString()+" "+test.Max().ToString());
                    Parallel.For(0, range/8, (i) => 
                    {
                        byte res = 0x00;
                        for(int k = i*8; k < (i+1)*8; k++) if(charlis[k] == '1') res |= (byte)(1 << (7-k));
                        reslis[i] = res;
                    });
                    writer.Write(reslis);
                    /*
                    for(int j = 0; j < range; i+=3)
                    {
                        tmpbi += (mat.At<Vec3b>(x, y)[0]+mat.At<Vec3b>(x, y+1)[0]+mat.At<Vec3b>(x+1, y)[0]+mat.At<Vec3b>(x+1, y)[0])/4 > 255/2 ? "1" : "0";
                        if(8 <= tmpbi.Length)
                        {
                            res = 0x00;
                            for(int k = 0; k < 8; k++)
                            {
                                if(tmpbi[k] == '1') res |= (byte)(1 << (7-k));
                            }
                            if(tmpbi.Length == 8) Array.Copy(tmpbi.ToArray(), 8, tmpbi.ToArray(), 0, tmpbi.Length-8);
                        }
                    }
                    */
                }
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