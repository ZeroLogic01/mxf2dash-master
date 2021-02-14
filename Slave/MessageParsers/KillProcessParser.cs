using Commons;
using Commons.MessageParsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
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

                WaitCallback cb = (object state) =>
                {
                    int i = 0;
                    do
                    {
                        try
                        {
                            if (!processToBeKilled.HasExited)
                            {
                                //    processToBeKilled.Kill();

                                KillProcessAndChildren(processToBeKilled.Id);

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

                };

                ThreadPool.QueueUserWorkItem(cb);

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


        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                //Console.WriteLine(proc.Id);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
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
