using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WpfApp1.Models
{
    [XmlRoot("SerialPortSettings")]
    public class SerialPortSettings
    {
        [XmlElement("PortName")]
        public string PortName { get; set; }

        [XmlElement("BaudRate")]
        public int BaudRate { get; set; }

        [XmlElement("DataBits")]
        public int DataBits { get; set; }

        [XmlElement("StopBits")]
        public StopBits StopBits { get; set; }

        [XmlElement("Parity")]
        public Parity Parity { get; set; }
    }
}

