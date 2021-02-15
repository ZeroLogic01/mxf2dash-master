using Commons;
using Master.Models;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Master
{
    public static class OverwriteRequestFileHelper
    {
        private const string FileNotFoundMessage = "XML File {0} was not found, warning issued and written to the log file";
        private const string NullObjectMessage = "Cannot save null {0} object in {1} file";
        private const string DirectoryCreationFailedMessage = "Cannot create {0}, please read the ErrorLogs.txt file";

        #region Fields

        private static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
        private static readonly XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

        #endregion

        public static TranscoderOverwriteRequest Read(string xmlFile)
        {
            if (!File.Exists(xmlFile))
            {
                string prompt = string.Format(FileNotFoundMessage, xmlFile);
                Logger.Log(new FileNotFoundException($"Couldn't find {xmlFile}."), prompt: prompt);
                return null;
            }

            // Creates an instance of the XmlSerializer class;
            // specifies the type of object to be de-serialized.

            /*
             * Why XmlSerializer is throwing file not found exceptions
             * https://stackoverflow.com/questions/1127431/xmlserializer-giving-filenotfoundexception-at-constructor
             */
            XmlSerializer serializer = new XmlSerializer(typeof(TranscoderOverwriteRequest));
            // If the XML document has been altered with unknown
            // nodes or attributes, handles them with the
            // UnknownNode and UnknownAttribute events.
            serializer.UnknownNode += new
            XmlNodeEventHandler(Serializer_UnknownNode);
            serializer.UnknownAttribute += new
            XmlAttributeEventHandler(Serializer_UnknownAttribute);

            try
            {
                TranscoderOverwriteRequest overwriteRequest;
                // A FileStream is needed to read the XML document.
                using (FileStream fs = new FileStream(xmlFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // Uses the De-serialize method to restore the object's state
                    // with data from the XML document. */
                    overwriteRequest = (TranscoderOverwriteRequest)serializer.Deserialize(fs);
                    fs?.Close();
                }
                return overwriteRequest;
            }
            catch (Exception E)
            {
                Logger.Log(E, prompt: E.Message);
                return null;
            }

        }

        public static bool Write(string filename, TranscoderOverwriteRequest overWriteRequest)
        {
            if (overWriteRequest == null)
            {
                string prompt = string.Format(NullObjectMessage, nameof(TranscoderOverwriteRequest), Path.GetDirectoryName(filename));
                Logger.Log(new FileNotFoundException(prompt), prompt: prompt);
                return false;
            }

            string directoryName = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directoryName))
            {
                try
                {
                    Directory.CreateDirectory(directoryName);
                }
                catch (Exception E)
                {
                    string prompt = string.Format(DirectoryCreationFailedMessage, directoryName);
                    Logger.Log(E, prompt: prompt);
                    return false;
                }

            }

            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var writer = XmlWriter.Create(fs, WriterSettings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TranscoderOverwriteRequest));
                    serializer.Serialize(writer, overWriteRequest, Namespaces);
                    writer?.Close();
                    fs?.Close();
                }
            }
            catch (Exception E)
            {
                Logger.Log(E, prompt: E.Message);
                return false;
            }

            return true;
        }

        private static void Serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine("TranscoderOverwriteRequest- Unknown Node Detected: " + e.Name + "\t" + e.Text);
        }

        private static void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            System.Xml.XmlAttribute attr = e.Attr;
            Console.WriteLine("TranscoderOverwriteRequest- Unknown attribute " +
            attr.Name + "='" + attr.Value + "'");
        }
    }
}
