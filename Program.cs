using System;
using System.IO;
using CommandLine;
using System.Linq;
using System.Text;
using TopDown.MassSpectrometry;
using TopDown.SpectralProcessing;
using TopDown.Thermo;

namespace RawAverager
{
    class Program
    {
        public class CommandLineOptions
        {
            [Option(shortName: 'p', longName: "path", Required = true, HelpText = "Path to folder with RAW files.")]
            public string Path { get; set; }

            [Option(shortName: 's', longName: "start", Required = true, HelpText = "Start scan to average.")]
            public int StartScan { get; set; }

            [Option(shortName: 'e', longName: "end", Required = true, HelpText = "End scan to average.")]
            public int EndScan { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                   .WithParsed<CommandLineOptions>(opts =>
                   {
                        foreach (var file in Directory.GetFiles(opts.Path, "*.RAW", SearchOption.TopDirectoryOnly))
                        {
                            string simpleFileName = Path.GetFileNameWithoutExtension(file);
                            Console.WriteLine(simpleFileName);

                            using (RawFileReader reader = new RawFileReader(file))
                            {
                                IAverager averager = reader.GetRawAverager();
                                System.Collections.Generic.IEnumerable<int> scanList = Enumerable.Range(opts.StartScan, opts.EndScan);
                                IProfileSpectrum avg = averager.AverageProfile(reader, scanList);

                                var masses = avg.GetMz();
                                var intensities = avg.GetIntensity();

                                var csv = new StringBuilder();

                                for (var i = 0; i < masses.Length; i++)
                                {
                                    var newLine = string.Format("{0},{1}", masses[i], intensities[i]);
                                    csv.AppendLine(newLine);
                                }

                                var filePath = Path.ChangeExtension(file, ".csv");
                                File.WriteAllText(filePath, csv.ToString());
                            }
                        }



                }); 
        }
    }


    

}
