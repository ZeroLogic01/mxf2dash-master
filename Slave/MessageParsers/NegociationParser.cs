using Commons;
using Commons.MessageParsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slave.MessageParsers
{
    public class NegociationParser : IMessageParser
    {
        private const string SuccessfulNegociation = "Negociation was succesful.";
        private const string UnsuccesfullNegociationSlave = "Negociation was unsuccesful due to {0}, please read the ErrorLogs.txt file.";
        private const string UnsuccesfullNegociationMaster = "Negociation was unsuccesful due to {0}, on slave {1}.";
        public Message ParseMessage(Message message)
        {
            string json = message.MessageBody;

            try
            {
                SharedSettings sharedSettings = JsonConvert.DeserializeObject<SharedSettings>(json);
                Settings.Instance.SharedSettings = sharedSettings;
                Console.WriteLine(SuccessfulNegociation);

                Message response = new Message(SuccessfulNegociation, Message.Preamble.SUCCESS);

                return response;

            }
            catch (Exception E)
            {
                string prompt = string.Format(UnsuccesfullNegociationSlave, "an exception");
                Logger.Log(E, prompt: prompt);


                string msgBody = string.Format(UnsuccesfullNegociationMaster, "an exception", ListenerSocketFactory.SelfIP.ToString());
                Message response = new Message(UnsuccesfullNegociationMaster, Message.Preamble.EXCEPTION);
                return response;

            }

            

        }
    }
}
