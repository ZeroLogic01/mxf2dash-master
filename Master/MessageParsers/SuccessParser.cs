using Commons;
using Commons.MessageParsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.MessageParsers
{
    public class SuccessParser : IMessageParser
    {
        public Message ParseMessage(Message message)
        {
            Console.WriteLine(message.MessageBody);
            return null;
        }
    }
}
