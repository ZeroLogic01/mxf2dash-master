using System;
using System.Net;
using System.Net.Sockets;

namespace Commons
{
    public static class Communicator
    {

        private static readonly string ConnectingPrompt = "Connecting to {0} on port {1}";
        private static readonly string FailedConnectionPrompt = "Couldn`t connect and send message to {0}";
        private static readonly string SuccessfulConnectionPrompt = "Connection to {0} successfully. Sending data";
        private static readonly string MessageSuccessfulySentPrompt = "Message successfully sent to {0}";
        private static readonly string MessageUnsuccessfulySentPrompt = "Message unsuccessfully sent to {0}";
        private static readonly string IncompleteMessageReceivedPrompt = "Incomplete message received from {0}";
        private static readonly string MessageReceivedPrompt = "Message received from {0}";

        public static Message Receive(Socket socket)
        {
            IPEndPoint endPoint = (IPEndPoint)socket.RemoteEndPoint;
            IPAddress iPAddress = endPoint.Address;

            byte[] response = new byte[Message.MessageBlockLength];

            int bytesReceived = socket.Receive(response);

            if (bytesReceived != Message.MessageBlockLength)
            {
                string errMsg = string.Format(IncompleteMessageReceivedPrompt, iPAddress.ToString());
                throw new Exception(errMsg);
            }

            Message receivedMessage = Message.Parse(response);

            return receivedMessage;
        }

        public static Message Send(Message message, IPAddress address,int port,bool withResponse)
        {
            IPEndPoint endPoint = new IPEndPoint(address, port);

            string addressString = address.ToString();

            using (Socket socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                Console.WriteLine(ConnectingPrompt, address.ToString(), port);

                try
                {
                    socket.Connect(endPoint);
                }
                catch(Exception e)
                {
                    Logger.Log(e);
                }
                
                if (!socket.Connected)
                {
                    Console.WriteLine(FailedConnectionPrompt, addressString);
                    return null;
                }

                Console.WriteLine(SuccessfulConnectionPrompt, addressString);

                int bytesSent = socket.Send(message.SendableData);

                if (bytesSent != Message.MessageBlockLength)
                {
                    Console.WriteLine(MessageUnsuccessfulySentPrompt, addressString);
                    return null;
                }

                Console.WriteLine(MessageSuccessfulySentPrompt, addressString);

                if (withResponse)
                {
                    Message response = Receive(socket);
                    Console.WriteLine(MessageReceivedPrompt, addressString);
                    return response;
                }

                return null;
            }
        }
        public static Message Send(Message message, Socket socket)
        {
            socket.Send(message.SendableData);
            return null;
        }
    }
}
