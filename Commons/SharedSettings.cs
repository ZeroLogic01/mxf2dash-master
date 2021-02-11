using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class SharedSettings
    {
        private static readonly string FileNotFoundMessage = "File {0} was not found";
        private static readonly string WatchfolderNotFoundMessage = "Watchfolder at path {0} was not found";
        private static readonly string OutputfolderNotFoundMessage = "OutputFolder at path {0} was not found";
        private static readonly string PortNumberNegativeMessage = "Port number is negative";
        private static readonly string DelayNegativeMessage = "Delay is negative";

        public string Watchfolder { get; set; }
        public int Delay { get; set; }
        public int ConnectionPort { get; set; }
        public string GenericCommand { get; set; }
        public string OutputFolder { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static SharedSettings Create(string filename)
        {

            if (!File.Exists(filename))
            {
                string msg = string.Format(FileNotFoundMessage, filename);
                throw new FileNotFoundException(msg, filename);
                
            }

            string jsonContent = File.ReadAllText(filename);

            SharedSettings ss= JsonConvert.DeserializeObject<SharedSettings>(jsonContent);

            ss.Validate();

            return ss;
        } 

        public void Validate()
        {
            //Watchfolder does not exists
            if (!Directory.Exists(Watchfolder))
            {
                string message = string.Format(WatchfolderNotFoundMessage, Watchfolder);
                throw new DirectoryNotFoundException(message);
            }
            else if (!Directory.Exists(OutputFolder))
            {
                string message = string.Format(OutputfolderNotFoundMessage, OutputFolder);
                throw new DirectoryNotFoundException(message);
            } 
            else if (Delay < 0)
            {
                throw new ArgumentOutOfRangeException(PortNumberNegativeMessage);
            }
            else if (ConnectionPort < 0 || ConnectionPort > 65535)
            {
                throw new ArgumentOutOfRangeException(DelayNegativeMessage);
            }

        }

    }
}
