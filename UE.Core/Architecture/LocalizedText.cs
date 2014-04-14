using System.Diagnostics;
using System.Xml.Serialization;

namespace UE.Core.Architecture
{
    [DebuggerDisplay("Culture={Culture},Text={Text}")]
    public class LocalizedText
    {
        [XmlAttribute]
        public string Culture { get; set; }

        [XmlAttribute]
        public string Text { get; set; }
    }
}
