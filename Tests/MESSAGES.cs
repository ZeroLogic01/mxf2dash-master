using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Commons;

namespace Tests
{
    /// <summary>
    /// Summary description for Communication
    /// </summary>
    [TestClass]
    public class MESSAGES
    {
        [TestMethod]
        public void TEST_MESSAGE_LENGTH_CORRECTNESS()
        {

            string messageContent = "tests";
            
            Message msg = new Message(messageContent, Message.Preamble.NEGOCIATION);

            int expected = Message.MessageBlockLength;

            Assert.AreEqual(expected, msg.SendableData.Length);

        }

        [TestMethod]
        public void TEST_MESSAGE_PREAMBLE_CORRECTNESS()
        {
            string messageContent = "tests";

            var preambles = Enum.GetValues(typeof(Message.Preamble));

            foreach (Message.Preamble preamble in preambles)
            {
                Message message = new Message(messageContent, preamble);
                Assert.AreEqual(preamble, message.MessagePreamble);
            }

        }

        [TestMethod]
        public void TEST_MESSAGE_BODY_CORRECTNESS()
        {
            var preambles = Enum.GetValues(typeof(Message.Preamble));

            foreach (Message.Preamble preamble in preambles)
            {
                for (int i = 0; i <= Message.BodyLength; i++)
                {
                    string messageContent = new string('f', i);
                    Message message = new Message(messageContent, preamble);

                    Assert.AreEqual(messageContent, message.MessageBody);

                }
                
               
            }

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TEST_MESSAGE_BODY_OVERFLOW()
        {
            Message.Preamble preamble = Message.Preamble.EXCEPTION;

            string body = new string('r', Message.BodyLength + 10);

            new Message(body,preamble);

        }
    }
}
