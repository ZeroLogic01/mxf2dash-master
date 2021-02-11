using Commons;
using Commons.MessageParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slave.MessageParsers
{
    class BusyParser : IMessageParser
    {
        public Message ParseMessage(Message message)
        {
            if (Settings.Instance.CurrentWork >= Settings.Instance.WorkPower)
            {
                return new Message("", Message.Preamble.TRUE);
            }
            else
            {
                return new Message("", Message.Preamble.FALSE);
            }
        }
    }
}
