using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace QBMigrationTool
{
    public class XmlUtils
    {
        public static XmlElement MakeSimpleElem(XmlDocument doc, string tagName, string tagVal)
        {
            XmlElement elem = doc.CreateElement(tagName);
            elem.InnerText = tagVal;
            return elem;
        }

        public static XmlDocument MakeRequestDocument()
        {
            XmlDocument inputXMLDoc = null;
            inputXMLDoc = new XmlDocument();
            inputXMLDoc.AppendChild(inputXMLDoc.CreateXmlDeclaration("1.0", null, null));
            inputXMLDoc.AppendChild(inputXMLDoc.CreateProcessingInstruction("qbxml", "version=\"12.0\""));
            return inputXMLDoc;
        }

        public static XmlElement MakeRequestParentElement(XmlDocument inputXMLDoc)
        {
            XmlElement qbXML;
            XmlElement qbXMLMsgsRq;

            qbXML = inputXMLDoc.CreateElement("QBXML");
            inputXMLDoc.AppendChild(qbXML);
            qbXMLMsgsRq = inputXMLDoc.CreateElement("QBXMLMsgsRq");
            qbXML.AppendChild(qbXMLMsgsRq);
            qbXMLMsgsRq.SetAttribute("onError", "stopOnError");

            return qbXMLMsgsRq;
        }

        public static string GetAdjustedDateAsQBString(string inputDate, int daysToAdd, bool dateOnly)
        {
            string adjustedDateString = "";
            DateTime adjustedDate;
            if (DateTime.TryParse(inputDate, out adjustedDate))
            {
                adjustedDate = adjustedDate.AddDays(daysToAdd);
                if (dateOnly)
                {
                    adjustedDateString = adjustedDate.ToString("yyyy-MM-dd");
                }
                else
                {
                    adjustedDateString = adjustedDate.ToString("yyyy-MM-ddTHH:mm:ssK");
                }
            }
            return adjustedDateString;
        } 
    }
}
