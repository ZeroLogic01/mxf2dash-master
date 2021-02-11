using Commons;
using Commons.MessageParsers;
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

        private static readonly string SuccessMessageTemplate = "File {0} was converted succesfully";
        private static readonly string FailedMessageTemplate = "File {0} was not converted succesfully due to an exception. Please read the ErrorLog.txt file";

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

            string mainFile = Path.Combine(outDir,Path.GetFileNameWithoutExtension(filePath)) + ".MPD";


            Process p = ProcessFactory.CreateProcess(ss.GenericCommand, filePath, mainFile, outDir);

            WaitCallback cb = (object state) =>
            {
                try
                {
                    p.Start();
                    p.WaitForExit();
                    Console.WriteLine(SuccessMessageTemplate, filePath);
                }
                catch (Exception E)
                {
                    string prompt = string.Format(FailedMessageTemplate, filePath);
                    Logger.Log(E, prompt: prompt);
                }
                finally
                {
                    Settings.Instance.CurrentWork--;
                }
                
            };
            ThreadPool.QueueUserWorkItem(cb);

            return new Message("", Message.Preamble.TRUE);
        }
    }
}
