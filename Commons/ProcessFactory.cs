using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public static class ProcessFactory
    {

        public static readonly string DimensionProbeString = "-v error -select_streams v:0 -show_entries stream=width,height -of csv=s=x:p=0 \"{0}\"";
        public static readonly string AudioProbeString = "-i \"{0}\" -show_streams -select_streams a -loglevel error ";
        public static readonly string ProbeExecutable = "ffprobe.exe";

        public struct CommandData
        {
            public string text;
        };

        public static Process CreateProcess(string commandString, string inputFilePath, string outputFilePath,string outputDir)
        {
            Process process = new Process();
            ProcessStartInfo processStartInfo = BuildProcessStartInfo(commandString, inputFilePath, outputFilePath, outputDir);
            process.StartInfo = processStartInfo;
            return process;
        }

        private static string FormatCommandText(string commandText, string inputFilePath, string outputFilePath,string outputDir)
        {
            string formatedCommand = commandText.Replace("{in}", '"' + inputFilePath + '"');
            formatedCommand = formatedCommand.Replace("{out}", '"' + outputFilePath + '"');
            formatedCommand = formatedCommand.Replace("{outDIR}", outputDir);


            string resolution = Probe(DimensionProbeString, inputFilePath);
            string audio = Probe(AudioProbeString, inputFilePath);

            formatedCommand = formatedCommand.Contains("{resolution}") == true ? formatedCommand.Replace("{resolution}", resolution) : formatedCommand;
            formatedCommand = formatedCommand.Contains("{audio}") == true ? formatedCommand.Replace("{audio}", audio) : formatedCommand;

            return "/C " + formatedCommand;
        }

        public static string Probe(string probeTemplate, string probeParam)
        {
            Process process = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();

            psi.Arguments = string.Format(probeTemplate, probeParam);
            psi.FileName = ProbeExecutable;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;

            process.StartInfo = psi;

            process.Start();

            string probleResult = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            string toReturn = null;

            if (probeTemplate.Equals(AudioProbeString))
            {
                toReturn = probleResult.Equals("") ? "?" : "";
            }
            else if (probeTemplate.Equals(DimensionProbeString))
            {
                toReturn = probleResult.Trim();
            }
            return toReturn;
        }

        private static ProcessStartInfo BuildProcessStartInfo(string commandString, string inputFilePath, string outputFilePath,string outputDir)
        {
            ProcessStartInfo psi = new ProcessStartInfo();

            //Pornesc de la ideea ca comanda e buna => doar parsez si umplu golurile 

            psi.WindowStyle = ProcessWindowStyle.Hidden;

            if (!Directory.Exists(outputDir))
			   {
               Directory.CreateDirectory(outputDir);
            }

            psi.WorkingDirectory = outputDir;

            psi.Arguments = FormatCommandText(commandString, inputFilePath, outputFilePath, outputDir);

            psi.FileName = "cmd.exe";

            return psi;
        }

    }
}
