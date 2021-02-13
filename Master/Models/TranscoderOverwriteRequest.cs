using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Master.Models
{
    [XmlRoot("TranscoderOverWriteRequest")]
    public class TranscoderOverwriteRequest
    {
        [XmlElement("Filename", Order = 1)]
        public string FileName { get; set; }

        [XmlElement("TimeofRequest", Order = 2)]
        public string TimeOfRequest { get; set; }

        [XmlElement("Status", IsNullable = true, Order = 3)]
        public string Status { get; set; }

        [XmlElement("TimeofProcessed", IsNullable = true, Order = 4)]
        public string TimeofProcessed { get; set; }
    }
}
