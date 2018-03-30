using System;
using System.IO;

namespace SampleLanguage
{
    class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 1)
            {
                var len = int.Parse(Console.ReadLine());
                var read = 0;
                var data = new char[len];

                while (read < len)
                    read += Console.In.Read(data, read, len - read);

                var result = TransformSampleSubst(new string(data));
                Console.Write(result);
                return 0;
            }

            if (args.Length == 3)
            {
                var data = File.ReadAllText(args[1]);
                var result = TransformSample(data);
                File.WriteAllText(args[2], result);
                return 0;
            }

            Console.Error.WriteLine("Incorrect number of arguments.");
            return 1;
        }

        public static string TransformSample(string input)
        {
            var p = new SampleParser();
            var t = new SampleTransform();
            var data = p.Parse(input);
            return t.Transform(data);
        }

        public static string TransformSampleSubst(string input)
        {
            var p = new SubstSampleParser();
            var t = new SubstSampleTransform();
            var data = p.Parse(input);
            return t.Transform(data);
        }
    }
}
