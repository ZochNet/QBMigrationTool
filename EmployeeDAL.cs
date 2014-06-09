using rototrack_data_access;
using rototrack_model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using zochnet_utils;

namespace QBMigrationTool
{
    public class EmployeeDAL
    {
        public static XmlDocument BuildQueryRequest(string activeStatus, string fromModifiedDate, string toModifiedDate, string ownerID)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("EmployeeQueryRq");
            parent.AppendChild(queryElement);

            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ActiveStatus", activeStatus));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));            
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "OwnerID", ownerID));

            return doc;
        }

        public static void HandleResponse(string response)
        {
            WalkEmployeeQueryRs(response);
        }

        private static void WalkEmployeeQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList EmployeeQueryRsList = responseXmlDoc.GetElementsByTagName("EmployeeQueryRs");
            if (EmployeeQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = EmployeeQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList EmployeeRetList = responseNode.SelectNodes("//EmployeeRet");//XPath Query
                    for (int i = 0; i < EmployeeRetList.Count; i++)
                    {
                        XmlNode EmployeeRet = EmployeeRetList.Item(i);
                        WalkEmployeeRet(EmployeeRet);
                    }
                }
            }
        }

        private static void WalkEmployeeRet(XmlNode EmployeeRet)
        {
            if (EmployeeRet == null) return;

            Employee emp = null;
            RotoTrackDb db = new RotoTrackDb();

            string ListID = EmployeeRet.SelectSingleNode("./ListID").InnerText;
            if (db.Employees.Any(f => f.QBListId == ListID))
            {
                emp = db.Employees.First(f => f.QBListId == ListID);
            }
            else
            {
                emp = new Employee();
                db.Employees.Add(emp);
            }
            emp.QBListId = ListID;

            string Name = EmployeeRet.SelectSingleNode("./Name").InnerText;
            emp.Name = Name;

            if (EmployeeRet.SelectSingleNode("./IsActive") != null)
            {
                string IsActive = EmployeeRet.SelectSingleNode("./IsActive").InnerText;
                emp.IsActive = (IsActive == "true") ? true : false;
            }
            if (EmployeeRet.SelectSingleNode("./FirstName") != null)
            {
                string FirstName = EmployeeRet.SelectSingleNode("./FirstName").InnerText;
                emp.FirstName = FirstName;
            }
            if (EmployeeRet.SelectSingleNode("./MiddleName") != null)
            {
                string MiddleName = EmployeeRet.SelectSingleNode("./MiddleName").InnerText;
                emp.MiddleName = MiddleName;
            }
            if (EmployeeRet.SelectSingleNode("./LastName") != null)
            {
                string LastName = EmployeeRet.SelectSingleNode("./LastName").InnerText;
                emp.LastName = LastName;
            }
            if (EmployeeRet.SelectSingleNode("./JobTitle") != null)
            {
                string JobTitle = EmployeeRet.SelectSingleNode("./JobTitle").InnerText;
                emp.JobTitle = JobTitle;
            }
            if (EmployeeRet.SelectSingleNode("./Phone") != null)
            {
                string Phone = EmployeeRet.SelectSingleNode("./Phone").InnerText;
                emp.Phone = Phone;
            }
            if (EmployeeRet.SelectSingleNode("./Mobile") != null)
            {
                string Mobile = EmployeeRet.SelectSingleNode("./Mobile").InnerText;
                emp.Mobile = Mobile;
            }
            if (EmployeeRet.SelectSingleNode("./Email") != null)
            {
                string Email = EmployeeRet.SelectSingleNode("./Email").InnerText;
                emp.Email = Email;
            }

            Area area = GetAreaFromEmployeeRet(EmployeeRet);
            emp.AreaId = area.Id;

            emp.Initials = GetInitialsFromEmployeeRet(EmployeeRet);

            db.SaveChanges();

            AddOrUpdateUserProfile(emp);
        }

        private static Area GetAreaFromEmployeeRet(XmlNode EmployeeRet)
        {
            RotoTrackDb db = new RotoTrackDb();
            Area area = new Area();
            Config config = db.Configs.First();

            string areaInitials = config.DefaultAreaInitials;
            if (db.Areas.Any(f => f.Name.StartsWith(areaInitials)))
            {
                area = db.Areas.First(f => f.Name.StartsWith(areaInitials));
            }
            else
            {
                Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Could not find the default " + areaInitials + " area!");
            }

            XmlNodeList DataExtRetList = EmployeeRet.SelectNodes("./DataExtRet");
            if (DataExtRetList != null)
            {
                for (int i = 0; i < DataExtRetList.Count; i++)
                {
                    XmlNode DataExtRet = DataExtRetList.Item(i);
                    string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                    string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    if (DataExtName == "Roto Location")
                    {
                        string areaPrefix = DataExtValue;
                        if (db.Areas.Any(f => f.Name.StartsWith(areaPrefix)))
                        {
                            area = db.Areas.First(f => f.Name.StartsWith(areaPrefix));
                            return area;
                        }
                    }
                }
            }

            return area;
        }

        private static string GetInitialsFromEmployeeRet(XmlNode EmployeeRet)
        {
            RotoTrackDb db = new RotoTrackDb();
            string initials = "UNKNOWN";

            XmlNodeList DataExtRetList = EmployeeRet.SelectNodes("./DataExtRet");
            if (DataExtRetList != null)
            {
                for (int i = 0; i < DataExtRetList.Count; i++)
                {
                    XmlNode DataExtRet = DataExtRetList.Item(i);
                    string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                    string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    if (DataExtName == "Initials")
                    {
                        initials = DataExtValue;
                    }
                }
            }

            return initials;
        }        

        private static void AddOrUpdateUserProfile(Employee emp)
        {
            UserProfile u = null;
            RotoTrackDb db = new RotoTrackDb();

            // If user already exists, simply update the attributes except for Username and password
            if (db.UserProfiles.Any(f => f.QBListId == emp.QBListId))
            {
                u = db.UserProfiles.First(f => f.QBListId == emp.QBListId);

                u.Email = emp.Email;
                u.AreaId = emp.AreaId;
                u.Initials = emp.Initials;
                u.FirstName = emp.FirstName;
                u.IsActive = emp.IsActive;
                u.JobTitle = emp.JobTitle;
                u.LastName = emp.LastName;
                u.MiddleName = emp.MiddleName;
                u.Mobile = emp.Mobile;
                u.Name = emp.Name;
                u.Phone = emp.Phone;

                db.SaveChanges();
            }
            // Else if employee is active, see if we can automatically create a new user profile
            else if (emp.IsActive)
            {
                string username = "";
                if ((emp.Email != null) && (emp.Email.Length > 0))
                {
                    username = emp.Email;
                }
                else if ((emp.FirstName.Length > 0) && (emp.LastName.Length > 0))
                {
                    username = emp.FirstName.ToLower() + "." + emp.LastName.ToLower() + "@roto-versal.com";
                }

                // If username is set, then we can create a new user.  Let's make them a Technician by default.  Password default to 'rototrack'
                if (username != "")
                {
                    try
                    {
                        RotoTrackDbUtils.CreateUserWithRoles(
                            username,
                            "rototrack",
                            new[] { "Technician" },
                            new
                            {
                                Email = emp.Email,
                                AreaId = emp.AreaId,
                                Initials = emp.Initials,
                                FirstName = emp.FirstName,
                                IsActive = emp.IsActive,
                                JobTitle = emp.JobTitle,
                                LastName = emp.LastName,
                                MiddleName = emp.MiddleName,
                                Mobile = emp.Mobile,
                                Name = emp.Name,
                                Phone = emp.Phone,
                                QBListID = emp.QBListId
                            }
                            );
                    }
                    catch (Exception e)
                    {
                        string evLogTxt = "";
                        evLogTxt = "Error creating a new user! " + e.Message + "\r\n";
                        Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + evLogTxt);
                    }
                }
            }
        }
    }
}
