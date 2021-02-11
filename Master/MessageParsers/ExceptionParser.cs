using Commons;
using Commons.MessageParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master.MessageParsers
{
    class ExceptionParser : IMessageParser
    {

        private static readonly string FailurePrompt = "The operation failed on the Slave Side, please read the ErrorLogs.txt files";

        public Message ParseMessage(Message message)
        {
            Logger.Log(message.MessageBody,prompt: FailurePrompt);
            return null;
        }
    }
}
