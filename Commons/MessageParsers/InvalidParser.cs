using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.MessageParsers
{
    public class InvalidParser : IMessageParser
    {
        public Message ParseMessage(Message message)
        {
            Console.WriteLine("Invalid preamble");
            return null;
        }
    }
}
