using Commons;
using Commons.Commands;
using Commons.MessageParsers;
using Master.MessageParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Master.Commands
{
    class NegociateCommand : ICommand
    {
        private GenericFactory<Message.Preamble, IMessageParser> ParsersFactory = new GenericFactory<Message.Preamble, IMessageParser>();

        public NegociateCommand()
        {
            ParsersFactory.RegisterItem(Message.Preamble.EXCEPTION, new ExceptionParser());
            ParsersFactory.RegisterItem(Message.Preamble.SUCCESS, new SuccessParser());
        }

        private void Negociate(Slave slave)
        {
            string body = Settings.Instance.SharedSettings.Serialize();
            Message msg = new Message(body, Message.Preamble.NEGOCIATION);

            Message response = Communicator.Send(msg, IPAddress.Parse(slave.IP), Settings.Instance.SharedSettings.ConnectionPort,true);

            if (response != null)
            {
                IMessageParser messageParser = ParsersFactory.GetItem(response.MessagePreamble);
                messageParser.ParseMessage(response);
            }
            
        }

        public void Execute()
        {
            Settings.Instance.Slaves.ForEach(slave => Negociate(slave));
        }
    }
}
