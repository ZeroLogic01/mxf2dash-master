using Commons;
using Commons.MessageParsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Slave.MessageParsers
{
    class NewFileParser : IMessageParser
    {
        private const string SuccessMessageTemplate = "File {0} was converted successfully";
        private const string FailedMessageTemplate = "File {0} was not converted successfully due to an exception. Please read the ErrorLog.txt file";
        private const string FailedMessageOnOverwriteRequestTemplate = "File {0} conversion failed unexpectedly. Please read the ErrorLog.txt file for more info.";

        public Message ParseMessage(Message message)
        {
            if (Settings.Instance.CurrentWork >= Settings.Instance.WorkPower)
            {
                Message resp = new Message("", Message.Preamble.FALSE);
                return resp;
            }

            Settings.Instance.CurrentWork++;

            string filePath = message.MessageBody;
            var ss = Settings.Instance.SharedSettings;

            string outDir = Path.Combine(ss.OutputFolder, Path.GetFileNameWithoutExtension(filePath));

            string mainFile = Path.Combine(outDir, Path.GetFileNameWithoutExtension(filePath)) + ".MPD";



            ManualResetEvent manualReset = new ManualResetEvent(false);
            Process p = ProcessFactory.CreateProcess(ss.GenericCommand, filePath, mainFile, outDir);

            WaitCallback cb = (object state) =>
            {
                try
                {
                    p.Start();

                    manualReset.Set();

                    p.WaitForExit();
                    if (p.ExitCode == 0)
                    {
                        Console.WriteLine(SuccessMessageTemplate, filePath);
                    }
                    //else
                    //{
                    //    string prompt = string.Format(FailedMessageOnOverwriteRequestTemplate, filePath);
                    //    Logger.Log(new Exception($"Process exited with {p.ExitCode} exit code"), prompt: prompt);
                    //}
                }
                catch (Exception E)
                {
                    string prompt = string.Format(FailedMessageTemplate, filePath);
                    Logger.Log(E, prompt: prompt);
                }
                finally
                {
                    Settings.Instance.CurrentWork--;
                    manualReset.Set();
                }

            };
            ThreadPool.QueueUserWorkItem(cb);

            manualReset.WaitOne();

            string json = JsonConvert.SerializeObject(new FFMPEGProcess() { FileName = filePath, ProcessId = p.Id });

            return new Message(json, Message.Preamble.TRUE);
        }
    }
}
