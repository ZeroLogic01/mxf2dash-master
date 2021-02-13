using Commons;
using Commons.MessageParsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Slave.MessageParsers
{
    class KillProcessParser : IMessageParser
    {
        private const string SuccessfulProcessKill = "FFMPEG process working on {0} was killed successfully due to overwrite request.";
        private const string FailedProcessKill = "FFMPEG process working on {0} failed to gets killed. After {1} tries & {2} seconds it gets timed-out.";

        public const string ProcessFailedToStopExceptionMessageTemplate = "Try number {0}: FFMPEG process (pid={1}) working on {2} failed to stop, warning issued and written to the log file.";

        public Message ParseMessage(Message message)
        {
            var process = JsonConvert.DeserializeObject<FFMPEGProcess>(message.MessageBody);

            var processToBeKilled = Process.GetProcessById(process.ProcessId);

            Message response;

            if (processToBeKilled != null || !processToBeKilled.HasExited)
            {
                AutoResetEvent autoReset = new AutoResetEvent(false);

                ThreadPool.QueueUserWorkItem(state => Kill_FFMPEG_Process(process, processToBeKilled, autoReset));

                // Blocks the current thread until the current instance receives a signal.
                // If it doesn't receive a signal it will timeout after KillWaitDuration seconds.
                if (autoReset.WaitOne(TimeSpan.FromSeconds(Settings.Instance.SharedSettings.KillWaitDuration)))
                {
                    // if a signal is received
                    // if processToBeKilled is null we can assume process already exited 
                    var prompt = string.Format(SuccessfulProcessKill, process.FileName);
                    response = new Message(prompt, Message.Preamble.SUCCESS);
                }
                else
                {
                    // when reset event gets timed out
                    var prompt = string.Format(FailedProcessKill, process.FileName, Settings.Instance.SharedSettings.KillTotalRetries,
                        Settings.Instance.SharedSettings.KillWaitDuration);

                    // also log it
                    Logger.Log(new Exception(prompt));

                    response = new Message(prompt, Message.Preamble.FALSE);
                }

            }
            else
            {
                // if processToBeKilled is null we can assume process already exited 
                var prompt = string.Format(SuccessfulProcessKill, process.FileName);
                response = new Message(prompt, Message.Preamble.SUCCESS);
            }

            Console.WriteLine(response.MessageBody);

            return response;
        }


        private void Kill_FFMPEG_Process(FFMPEGProcess process, Process processToBeKilled, AutoResetEvent autoReset)
        {
            int i = 0;
            do
            {
                try
                {
                    if (!processToBeKilled.HasExited)
                    {
                        processToBeKilled.Kill();

                        autoReset.Set();
                        break;
                    }
                }
                catch (Exception E)
                {
                    string msg = string.Format(ProcessFailedToStopExceptionMessageTemplate, i + 1, processToBeKilled.Id, process.FileName);
                    Logger.Log(E, prompt: msg);
                }
                i++;
            }
            while (i < Settings.Instance.SharedSettings.KillTotalRetries);

        }

    }
}
