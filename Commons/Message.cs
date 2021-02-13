using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class Message
    {
        public readonly static int HeaderLength = 32;
        public readonly static int BodyLength = 2048;

        public readonly static int MessageBlockLength = HeaderLength + BodyLength;
        public readonly static char PaddingCharacter = '\0';

        private readonly static string Padding = new string(PaddingCharacter, BodyLength);
        private readonly static string MessageTemplate = "{0}{1}{2}{3}";

        private string Data;

        public enum Preamble
        {
            NEGOCIATION,
            EXCEPTION,
            SUCCESS,
            NEW_FILE,
            BUSY,
            TRUE,
            FALSE,
            /// <summary>
            /// Kill FFMPEG process
            /// </summary>
            KILL_PROCESS
        };

        public Message(string content, Preamble preamble)
        {
            string preambleString = preamble.ToString();
            string headerPadding = Padding.Substring(0, HeaderLength - preambleString.Length);
            string bodyPadding = Padding.Substring(0, BodyLength - content.Length);


            Data = string.Format(MessageTemplate, preambleString, headerPadding, content, bodyPadding);


        }

        public static Message Parse(byte[] receivedData)
        {
            string messageString = Encoding.ASCII.GetString(receivedData);

            if (messageString.Length != MessageBlockLength)
            {
                throw new ArgumentException("Received data length not equal to MessageBlockLength");
            }

            Preamble preamble = (Preamble)Enum.Parse(typeof(Preamble), messageString.Substring(0, messageString.IndexOf(PaddingCharacter)));
            string body = messageString.Substring(HeaderLength).TrimEnd(PaddingCharacter);

            return new Message(body, preamble);
        }

        public byte[] SendableData
        {
            get => Encoding.ASCII.GetBytes(Data);
        }
        public Preamble MessagePreamble
        {
            get => (Preamble)Enum.Parse(typeof(Preamble), Data.Substring(0, Data.IndexOf(PaddingCharacter)));
        }
        public string MessageBody
        {
            get => Data.Substring(HeaderLength).TrimEnd(PaddingCharacter);
        }
    }
}
