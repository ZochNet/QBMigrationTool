using rototrack_data_access;
using rototrack_model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace QBMigrationTool
{
    public class SiteDAL
    {
        public static XmlDocument BuildUpdateRq(WorkOrder wo)
        {
            RotoTrackDb db = new RotoTrackDb();
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            
            string listID = wo.QBListId;
            Site site = db.Sites.Find(wo.SiteId);

            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Site Name", site.SiteName);
            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Site Unit Number", site.UnitNumber);
            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Site County", site.County);
            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Site City/State", site.CityState);
            doc = BuildDataExtModOrDelRq(doc, parent, listID, "Site POAFE", site.POAFENumber);

            return doc;
        }

        private static XmlDocument BuildDataExtModOrDelRq(XmlDocument doc, XmlElement parent, string customerListID, string DataExtName, string DataExtValue)
        {
            string reqType = "DataExtMod";
            if ((DataExtValue == null) || (DataExtValue == ""))
            {
                reqType = "DataExtDel";
            }

            XmlElement DataExtModRq = doc.CreateElement(reqType + "Rq");
            parent.AppendChild(DataExtModRq);
            DataExtModRq.SetAttribute("requestID", "1");

            XmlElement DataExtMod = doc.CreateElement(reqType);
            DataExtModRq.AppendChild(DataExtMod);
            DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "OwnerID", "0"));
            DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "DataExtName", DataExtName));
            DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListDataExtType", "Customer"));

            XmlElement ListObjRef = doc.CreateElement("ListObjRef");
            DataExtMod.AppendChild(ListObjRef);
            ListObjRef.AppendChild(XmlUtils.MakeSimpleElem(doc, "ListID", customerListID));

            if (reqType == "DataExtMod") DataExtMod.AppendChild(XmlUtils.MakeSimpleElem(doc, "DataExtValue", DataExtValue));

            return doc;
        }

    }
}
