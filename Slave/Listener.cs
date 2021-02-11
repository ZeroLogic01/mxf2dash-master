using Commons;
using Commons.MessageParsers;
using Slave.MessageParsers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Slave
{
    class Listener
    {
        private const string WaitingToConnect = "Waiting for a connection from the Master Application at the ip {0} on the port {1}";
 
        private const string ConnectionInitialisationException = "The communicator could not be initialised, please read the ErrorLogs.txt file";

        private bool mustWork = false;


        private GenericFactory<Message.Preamble, IMessageParser> MessageParserFactory;

        private void RegisterParsers()
        {
            MessageParserFactory = new GenericFactory<Message.Preamble, IMessageParser>();
            MessageParserFactory.DefaultValue = new InvalidParser();

            List<Tuple<Message.Preamble, IMessageParser>> parsers = new List<Tuple<Message.Preamble, IMessageParser>>(){
                     new Tuple<Message.Preamble,IMessageParser>(Message.Preamble.NEGOCIATION, new NegociationParser()),
                     new Tuple<Message.Preamble,IMessageParser>(Message.Preamble.BUSY, new BusyParser()),
                     new Tuple<Message.Preamble,IMessageParser>(Message.Preamble.NEW_FILE, new NewFileParser()),
                };

            parsers.ForEach(pair => MessageParserFactory.RegisterItem(pair.Item1, pair.Item2));

        }

        public Listener()
        {
            RegisterParsers();
        } 

        private void Listen(object state)
        {
            try
            {
                using (Socket listener = ListenerSocketFactory.Create(Settings.Instance.ListeningPort))
                {

                    IPEndPoint endPoint = (IPEndPoint)listener.LocalEndPoint;

                    Console.WriteLine(WaitingToConnect, endPoint.Address.ToString(), endPoint.Port);

                    while (mustWork)
                    {
                        try
                        {
                            listener.Listen(100);

                            Socket handler = listener.Accept();

                            IPEndPoint remoteEP = (IPEndPoint)handler.RemoteEndPoint;

                            Message msg = Communicator.Receive(handler);

                            IMessageParser messageParser = MessageParserFactory.GetItem(msg.MessagePreamble);

                            Message recv = messageParser.ParseMessage(msg);

                            if (recv != null)
                            {
                                Communicator.Send(recv,handler);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Log(e);
                        }

                    }
                }
            }
            catch(Exception e)
            {
                Logger.Log(e, prompt: ConnectionInitialisationException);
            }
        }

        public void Start()
        {
            mustWork = true;
            ThreadPool.QueueUserWorkItem(Listen);
        }

        public void Stop()
        {
            mustWork = false;
        }

    }
}
