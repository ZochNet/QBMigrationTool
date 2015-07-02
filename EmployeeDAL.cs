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
            if (EmployeeRet.SelectSingleNode("./HiredDate") != null)
            {
                string HiredDate = EmployeeRet.SelectSingleNode("./HiredDate").InnerText;
                DateTime hDate;
                if (DateTime.TryParse(HiredDate, out hDate))
                {
                    emp.HireDate = hDate;
                }
            }

            //Get all field values for EmployeePayrollInfo aggregate 
            XmlNode EmployeePayrollInfo = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo");
            if (EmployeePayrollInfo != null)
            {
                //Get value of PayPeriod
                if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/PayPeriod") != null)
                {
                    string PayPeriod = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/PayPeriod").InnerText;
                }
                //Get all field values for ClassRef aggregate 
                XmlNode ClassRef = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/ClassRef");
                if (ClassRef != null)
                {
                    //Get value of ListID
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/ClassRef/ListID") != null)
                    {
                        string ListID2 = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/ClassRef/ListID").InnerText;
                    }
                    //Get value of FullName
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/ClassRef/FullName") != null)
                    {
                        string FullName = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/ClassRef/FullName").InnerText;
                    }
                }
                //Done with field values for ClassRef aggregate

                XmlNodeList OREarningsChildren = EmployeeRet.SelectNodes("./EmployeePayrollInfo/*");
                for (int i = 0; i < OREarningsChildren.Count; i++)
                {
                    XmlNode Child = OREarningsChildren.Item(i);
                    if (Child.Name == "ClearEarnings")
                    {
                    }

                    if (Child.Name == "Earnings")
                    {
                        //Get all field values for PayrollItemWageRef aggregate 
                        //Get value of ListID
                        if (Child.SelectSingleNode("./PayrollItemWageRef/ListID") != null)
                        {
                            string ListID3 = Child.SelectSingleNode("./PayrollItemWageRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./PayrollItemWageRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./PayrollItemWageRef/FullName").InnerText;
                        }
                        //Done with field values for PayrollItemWageRef aggregate

                        XmlNodeList ORRateChildren = Child.SelectNodes("./*");
                        for (int j = 0; j < ORRateChildren.Count; j++)
                        {
                            XmlNode Child2 = ORRateChildren.Item(j);
                            if (Child2.Name == "Rate")
                            {
                            }

                            if (Child2.Name == "RatePercent")
                            {
                            }
                        }
                    }
                }

                //Get value of IsUsingTimeDataToCreatePaychecks
                if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/IsUsingTimeDataToCreatePaychecks") != null)
                {
                    string IsUsingTimeDataToCreatePaychecks = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/IsUsingTimeDataToCreatePaychecks").InnerText;
                }
                //Get value of UseTimeDataToCreatePaychecks
                if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/UseTimeDataToCreatePaychecks") != null)
                {
                    string UseTimeDataToCreatePaychecks = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/UseTimeDataToCreatePaychecks").InnerText;
                }
                //Get all field values for SickHours aggregate 
                XmlNode SickHours = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours");
                if (SickHours != null)
                {
                    //Get value of HoursAvailable
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/HoursAvailable") != null)
                    {
                        string HoursAvailable = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/HoursAvailable").InnerText;
                    }
                    //Get value of AccrualPeriod
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/AccrualPeriod") != null)
                    {
                        string AccrualPeriod = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/AccrualPeriod").InnerText;
                    }
                    //Get value of HoursAccrued
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/HoursAccrued") != null)
                    {
                        string HoursAccrued = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/HoursAccrued").InnerText;                        
                    }
                    //Get value of MaximumHours
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/MaximumHours") != null)
                    {
                        string MaximumHours = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/MaximumHours").InnerText;
                    }
                    //Get value of IsResettingHoursEachNewYear
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/IsResettingHoursEachNewYear") != null)
                    {
                        string IsResettingHoursEachNewYear = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/IsResettingHoursEachNewYear").InnerText;
                    }
                    //Get value of HoursUsed
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/HoursUsed") != null)
                    {
                        string HoursUsed = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/HoursUsed").InnerText;
                    }
                    //Get value of AccrualStartDate
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/AccrualStartDate") != null)
                    {
                        string AccrualStartDate = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/SickHours/AccrualStartDate").InnerText;
                    }
                }
                //Done with field values for SickHours aggregate

                //Get all field values for VacationHours aggregate 
                XmlNode VacationHours = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours");
                if (VacationHours != null)
                {
                    //Get value of HoursAvailable
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/HoursAvailable") != null)
                    {
                        string HoursAvailable = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/HoursAvailable").InnerText;
                        emp.VacationHoursOffer = 0;
                        int vacHoursOffer;
                        if (Int32.TryParse(HoursAvailable, out vacHoursOffer))
                        {
                            // mgruetzner-6/8/2015--Don't set this here any longer--it messes up the vacation calcs.  If we want to override the standard formula, do it in rototrack.
                            //emp.VacationHoursOffer = vacHoursOffer;
                        }
                    }
                    //Get value of AccrualPeriod
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/AccrualPeriod") != null)
                    {
                        string AccrualPeriod = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/AccrualPeriod").InnerText;
                    }
                    //Get value of HoursAccrued
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/HoursAccrued") != null)
                    {
                        string HoursAccrued = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/HoursAccrued").InnerText;                        
                    }
                    //Get value of MaximumHours
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/MaximumHours") != null)
                    {
                        string MaximumHours = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/MaximumHours").InnerText;
                    }
                    //Get value of IsResettingHoursEachNewYear
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/IsResettingHoursEachNewYear") != null)
                    {
                        string IsResettingHoursEachNewYear = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/IsResettingHoursEachNewYear").InnerText;
                    }
                    //Get value of HoursUsed
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/HoursUsed") != null)
                    {
                        string HoursUsed = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/HoursUsed").InnerText;
                    }
                    //Get value of AccrualStartDate
                    if (EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/AccrualStartDate") != null)
                    {
                        string AccrualStartDate = EmployeeRet.SelectSingleNode("./EmployeePayrollInfo/VacationHours/AccrualStartDate").InnerText;
                    }
                }
                //Done with field values for VacationHours aggregate
            }
            //Done with field values for EmployeePayrollInfo aggregate

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
