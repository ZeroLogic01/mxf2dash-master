using Commons;
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

        private static readonly string FileNotFoundMessage = "File {0} was not found, warning issued and written to the log file";

        private IPAddress _IP;
        public string IP { get => _IP.ToString(); set => _IP = IPAddress.Parse(value); }

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

            return preamble.Equals(Message.Preamble.TRUE);

        }

    }
}
