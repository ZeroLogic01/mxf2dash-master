using Commons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Master
{
    public class Slave
    {

        //private static readonly string FileNotFoundMessage = "File {0} was not found, warning issued and written to the log file";
        private static readonly string FileNamesMismatchMessage = "File {0} name received from the slave is not the same as file {1} sent, warning issued and written to the log file";

        private IPAddress _IP;
        public string IP { get => _IP.ToString(); set => _IP = IPAddress.Parse(value); }

        public List<FFMPEGProcess> FilesBeingProcessed = new List<FFMPEGProcess>();

        public bool IsBusy()
        {
            Message msg = new Message("", Message.Preamble.BUSY);

            Message recv = Communicator.Send(msg, _IP, Settings.Instance.SharedSettings.ConnectionPort, true);

            Message.Preamble preamble = recv.MessagePreamble;

            return preamble.Equals(Message.Preamble.TRUE);

        }

        public bool SendWork(string filename)
        {
            Message msg = new Message(filename, Message.Preamble.NEW_FILE);

            Message recv = Communicator.Send(msg, _IP, Settings.Instance.SharedSettings.ConnectionPort, true);

            Message.Preamble preamble = recv.MessagePreamble;

            var process = JsonConvert.DeserializeObject<FFMPEGProcess>(recv.MessageBody);

            if (process.FileName.Equals(filename))
            {
                // we don't want to maintain FilesBeingProcessed list with duplicate files
                var oldProcess = FilesBeingProcessed.FirstOrDefault(p => p.FileName.Equals(filename));

                if (oldProcess == null)
                {// if the file name doesn't exist, add the process to the list
                    FilesBeingProcessed.Add(process);
                }
                else
                {// just update the process id, update the 
                    oldProcess.ProcessId = process.ProcessId;
                }
            }
            else
            {
                string prompt = string.Format(FileNamesMismatchMessage, process.FileName, filename);
                Logger.Log(new Exception("File names mismatch occurred."), prompt: prompt);
            }



            return preamble.Equals(Message.Preamble.TRUE);

        }

        public bool KillProcess(FFMPEGProcess process)
        {
            var json = JsonConvert.SerializeObject(process);
            Message msg = new Message(json, Message.Preamble.KILL_PROCESS);

            Message recv = Communicator.Send(msg, _IP, Settings.Instance.SharedSettings.ConnectionPort, true);

            Message.Preamble preamble = recv.MessagePreamble;



            // To be done: On SUCCESS remove the entry from the FilesBeingProcessed
            if (preamble.Equals(Message.Preamble.SUCCESS))
            {
                FilesBeingProcessed.Remove(process);
                Console.WriteLine(recv.MessageBody);
            }
            else
            {
                Logger.Log(new Exception(recv.MessageBody));
                Console.WriteLine(recv.MessageBody);
            }


            return preamble.Equals(Message.Preamble.SUCCESS);
        }

    }
}
