using rototrack_data_access;
using rototrack_model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using zochnet_utils;

namespace QBMigrationTool
{
    public class BillDAL
    {
        // This method will look at all BillLines modified within the last 180 days (created or changed), and then look at all
        // BillLines in the database that were modified in that same timer period.  If any exist in the database, but not in the list from QB,
        // then it must have been deleted from QuickBooks, so delete it from our database.
        public static void RemoveDeleted()
        {
            RotoTrackDb db = new RotoTrackDb();

            string fromDateTime = XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), -180, false);
            string toDateTime = XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, false);
            
            List<BillLine> bls = db.BillLines.ToList();
            XmlDocument doc = BuildQueryRequest(fromDateTime, toDateTime, bls);
            string response = QBUtils.DoRequest(doc);
            RemoveDeletedBasedOnResponse(db, fromDateTime, response, bls);              
        }

        private static void RemoveDeletedBasedOnResponse(RotoTrackDb db, string fromDateTime, string response, List<BillLine> bls)
        {
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList BillQueryRsList = responseXmlDoc.GetElementsByTagName("BillQueryRs");
            if (BillQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = BillQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    List<string> txnIds = new List<string>();
                    XmlNodeList BillRetList = responseNode.SelectNodes("//BillRet");//XPath Query
                    for (int i = 0; i < BillRetList.Count; i++)
                    {
                        XmlNode BillRet = BillRetList.Item(i);
                        if (BillRet != null)
                        {
                            string TxnID = BillRet.SelectSingleNode("./TxnID").InnerText;
                            txnIds.Add(TxnID);
                        }
                    }

                    txnIds.Sort();
                    DateTime fromDate;
                    if (DateTime.TryParse(fromDateTime, out fromDate))
                    {
                        foreach (BillLine bl in bls.ToList())
                        {
                            if (!txnIds.Contains(bl.BillTxnId))
                            {
                                if (bl.BillModified >= fromDate)
                                {
                                    if (!bl.WorkOrderListID.Contains("_deleted"))
                                    {
                                        Logging.RototrackErrorLog("Update WorkOrderListID to _deleted for Bill Lines with  BillTxnID: " + bl.BillTxnId);
                                        bl.WorkOrderListID = bl.WorkOrderListID + "_deleted";
                                        db.Entry(bl).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }                        
        }

        public static XmlDocument BuildQueryRequest(string fromModifiedDate, string toModifiedDate)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("BillQueryRq");
            parent.AppendChild(queryElement);

            XmlElement dateRangeFilter = doc.CreateElement("ModifiedDateRangeFilter");
            queryElement.AppendChild(dateRangeFilter);
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));                        
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "IncludeLineItems", "1"));

            return doc;
        }

        public static XmlDocument BuildQueryRequest(string fromModifiedDate, string toModifiedDate, List<BillLine> bls)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("BillQueryRq");
            parent.AppendChild(queryElement);

            /*
            XmlElement dateRangeFilter = doc.CreateElement("TxnDateRangeFilter");
            queryElement.AppendChild(dateRangeFilter);
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromTxnDate", fromModifiedDate));
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToTxnDate", toModifiedDate)); 
            //dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "DateMacro", "LastCalendarYearToDate"));            
            */

            XmlElement dateRangeFilter = doc.CreateElement("ModifiedDateRangeFilter");
            queryElement.AppendChild(dateRangeFilter);
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate)); 

            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "IncludeRetElement", "TxnID"));

            return doc;
        }

        public static void HandleResponse(string response)
        {
            WalkBillQueryRs(response);
        }

        private static void WalkBillQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList BillQueryRsList = responseXmlDoc.GetElementsByTagName("BillQueryRs");
            if (BillQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = BillQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList BillRetList = responseNode.SelectNodes("//BillRet");//XPath Query
                    for (int i = 0; i < BillRetList.Count; i++)
                    {
                        XmlNode BillRet = BillRetList.Item(i);
                        BillDAL.WalkBillRetForQuery(BillRet);
                    }
                }
            }
        }

        private static void WalkBillRetForQuery(XmlNode BillRet)
        {
            if (BillRet == null) return;

            RotoTrackDb db = new RotoTrackDb();

            //Go through all the elements of BillRet
            //Get value of TxnID
            string TxnID = BillRet.SelectSingleNode("./TxnID").InnerText;
            //Get value of TimeCreated
            string TimeCreated = BillRet.SelectSingleNode("./TimeCreated").InnerText;
            //Get value of TimeModified
            string TimeModified = BillRet.SelectSingleNode("./TimeModified").InnerText;
            //Get value of EditSequence
            string EditSequence = BillRet.SelectSingleNode("./EditSequence").InnerText;
            //Get value of TxnNumber
            if (BillRet.SelectSingleNode("./TxnNumber") != null)
            {
                string TxnNumber = BillRet.SelectSingleNode("./TxnNumber").InnerText;
            }

            //Get all field values for APAccountRef aggregate
            XmlNode APAccountRef = BillRet.SelectSingleNode("./APAccountRef");
            if (APAccountRef != null)
            {
                //Get value of ListID
                if (BillRet.SelectSingleNode("./APAccountRef/ListID") != null)
                {
                    string ListID = BillRet.SelectSingleNode("./APAccountRef/ListID").InnerText;
                }
                //Get value of FullName
                if (BillRet.SelectSingleNode("./APAccountRef/FullName") != null)
                {
                    string FullName = BillRet.SelectSingleNode("./APAccountRef/FullName").InnerText;
                }
            }
            //Done with field values for APAccountRef aggregate

            //Get value of TxnDate
            string TxnDate = BillRet.SelectSingleNode("./TxnDate").InnerText;
            //Get value of DueDate
            if (BillRet.SelectSingleNode("./DueDate") != null)
            {
                string DueDate = BillRet.SelectSingleNode("./DueDate").InnerText;
            }

            //Get value of AmountDue         
            string AmountDue = "";
            if (BillRet.SelectSingleNode("./AmountDue") != null)
            {
                AmountDue = BillRet.SelectSingleNode("./AmountDue").InnerText;
            }

            //Get all field values for CurrencyRef aggregate
            XmlNode CurrencyRef = BillRet.SelectSingleNode("./CurrencyRef");
            if (CurrencyRef != null)
            {
                //Get value of ListID
                if (BillRet.SelectSingleNode("./CurrencyRef/ListID") != null)
                {
                    string ListID = BillRet.SelectSingleNode("./CurrencyRef/ListID").InnerText;
                }
                //Get value of FullName
                if (BillRet.SelectSingleNode("./CurrencyRef/FullName") != null)
                {
                    string FullName = BillRet.SelectSingleNode("./CurrencyRef/FullName").InnerText;
                }
            }
            //Done with field values for CurrencyRef aggregate

            //Get all field values for VendorRef aggregate
            //Get value of ListID
            string VendorListID = "";
            if (BillRet.SelectSingleNode("./VendorRef/ListID") != null)
            {
                VendorListID = BillRet.SelectSingleNode("./VendorRef/ListID").InnerText;
            }
            //Get value of FullName
            if (BillRet.SelectSingleNode("./VendorRef/FullName") != null)
            {
                string FullName = BillRet.SelectSingleNode("./VendorRef/FullName").InnerText;
            }
            //Done with field values for VendorRef aggregate

            //Get value of ExchangeRate
            if (BillRet.SelectSingleNode("./ExchangeRate") != null)
            {
                string ExchangeRate = BillRet.SelectSingleNode("./ExchangeRate").InnerText;
            }
            //Get value of AmountDueInHomeCurrency
            if (BillRet.SelectSingleNode("./AmountDueInHomeCurrency") != null)
            {
                string AmountDueInHomeCurrency = BillRet.SelectSingleNode("./AmountDueInHomeCurrency").InnerText;
            }
            //Get value of RefNumber
            if (BillRet.SelectSingleNode("./RefNumber") != null)
            {
                string RefNumber = BillRet.SelectSingleNode("./RefNumber").InnerText;
            }
            //Get all field values for TermsRef aggregate
            XmlNode TermsRef = BillRet.SelectSingleNode("./TermsRef");
            if (TermsRef != null)
            {
                //Get value of ListID
                if (BillRet.SelectSingleNode("./TermsRef/ListID") != null)
                {
                    string ListID = BillRet.SelectSingleNode("./TermsRef/ListID").InnerText;
                }
                //Get value of FullName
                if (BillRet.SelectSingleNode("./TermsRef/FullName") != null)
                {
                    string FullName = BillRet.SelectSingleNode("./TermsRef/FullName").InnerText;
                }
            }
            //Done with field values for TermsRef aggregate

            //Get value of Memo
            if (BillRet.SelectSingleNode("./Memo") != null)
            {
                string Memo = BillRet.SelectSingleNode("./Memo").InnerText;
            }
            //Get value of IsPaid
            if (BillRet.SelectSingleNode("./IsPaid") != null)
            {
                string IsPaid = BillRet.SelectSingleNode("./IsPaid").InnerText;
            }
            //Get value of ExternalGUID
            if (BillRet.SelectSingleNode("./ExternalGUID") != null)
            {
                string ExternalGUID = BillRet.SelectSingleNode("./ExternalGUID").InnerText;
            }

            //Walk list of ExpenseLineRet aggregates
            XmlNodeList ExpenseLineRetList = BillRet.SelectNodes("./ExpenseLineRet");
            if (ExpenseLineRetList != null)
            {
                for (int i = 0; i < ExpenseLineRetList.Count; i++)
                {
                    XmlNode ExpenseLineRet = ExpenseLineRetList.Item(i);

                    //Get all field values for CustomerRef aggregate
                    string CustomerListID = "";
                    XmlNode CustomerRef = ExpenseLineRet.SelectSingleNode("./CustomerRef");
                    if (CustomerRef != null)
                    {
                        //Get value of ListID
                        if (ExpenseLineRet.SelectSingleNode("./CustomerRef/ListID") != null)
                        {
                            CustomerListID = ExpenseLineRet.SelectSingleNode("./CustomerRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (ExpenseLineRet.SelectSingleNode("./CustomerRef/FullName") != null)
                        {
                            string FullName = ExpenseLineRet.SelectSingleNode("./CustomerRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for CustomerRef aggregate

                    // Skip the rest of this iteration if no CustomerListID (associated work order)
                    if (CustomerListID == "") continue;

                    //Get value of TxnLineID
                    string TxnLineID = ExpenseLineRet.SelectSingleNode("./TxnLineID").InnerText;

                    // Find existing or create new BillLine entry
                    BillLine bl = FindOrCreateBillLine(db, TxnLineID, TxnID, TimeCreated, TimeModified, EditSequence, TxnDate, AmountDue);
                    bl.WorkOrderListID = CustomerListID;
                    bl.VendorListID = VendorListID;

                    string Description = "";

                    //Get all field values for AccountRef aggregate
                    XmlNode AccountRef = ExpenseLineRet.SelectSingleNode("./AccountRef");
                    if (AccountRef != null)
                    {
                        //Get value of ListID
                        if (ExpenseLineRet.SelectSingleNode("./AccountRef/ListID") != null)
                        {
                            string ListID = ExpenseLineRet.SelectSingleNode("./AccountRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (ExpenseLineRet.SelectSingleNode("./AccountRef/FullName") != null)
                        {
                            string FullName = ExpenseLineRet.SelectSingleNode("./AccountRef/FullName").InnerText;
                            Description = FullName + " - ";
                        }
                    }
                    //Done with field values for AccountRef aggregate

                    //Get value of Memo
                    if (ExpenseLineRet.SelectSingleNode("./Memo") != null)
                    {
                        string Memo = ExpenseLineRet.SelectSingleNode("./Memo").InnerText;
                        Description += Memo;
                    }
                    bl.Description = Description;

                    //Get value of Amount
                    if (ExpenseLineRet.SelectSingleNode("./Amount") != null)
                    {
                        string Amount = ExpenseLineRet.SelectSingleNode("./Amount").InnerText;
                        decimal amount;
                        if (Decimal.TryParse(Amount, out amount))
                        {
                            bl.Amount = amount;
                            bl.UnitCost = amount;
                            bl.Quantity = 1;
                        }
                    }

                    //Get all field values for ClassRef aggregate
                    XmlNode ClassRef = ExpenseLineRet.SelectSingleNode("./ClassRef");
                    if (ClassRef != null)
                    {
                        //Get value of ListID
                        if (ExpenseLineRet.SelectSingleNode("./ClassRef/ListID") != null)
                        {
                            string ListID = ExpenseLineRet.SelectSingleNode("./ClassRef/ListID").InnerText;
                            bl.AreaListID = ListID;
                        }
                        //Get value of FullName
                        if (ExpenseLineRet.SelectSingleNode("./ClassRef/FullName") != null)
                        {
                            string FullName = ExpenseLineRet.SelectSingleNode("./ClassRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for ClassRef aggregate

                    //Get value of BillableStatus
                    if (ExpenseLineRet.SelectSingleNode("./BillableStatus") != null)
                    {
                        string BillableStatus = ExpenseLineRet.SelectSingleNode("./BillableStatus").InnerText;
                        bl.BillableStatus = BillableStatus;
                    }

                    db.SaveChanges();
                }
            }

            XmlNodeList ORItemLineRetListChildren = BillRet.SelectNodes("./*");
            for (int i = 0; i < ORItemLineRetListChildren.Count; i++)
            {
                XmlNode Child = ORItemLineRetListChildren.Item(i);
                if (Child.Name == "ItemLineRet")
                {
                    //Get all field values for CustomerRef aggregate
                    string CustomerListID = "";
                    XmlNode CustomerRef = Child.SelectSingleNode("./CustomerRef");
                    if (CustomerRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./CustomerRef/ListID") != null)
                        {
                            CustomerListID = Child.SelectSingleNode("./CustomerRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./CustomerRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./CustomerRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for CustomerRef aggregate

                    // Skip this entity if no associated customer (work order)
                    if (CustomerListID == "") continue;

                    //Get value of TxnLineID
                    string TxnLineID = Child.SelectSingleNode("./TxnLineID").InnerText;

                    // Find existing or create new BillLine entry
                    BillLine bl = FindOrCreateBillLine(db, TxnLineID, TxnID, TimeCreated, TimeModified, EditSequence, TxnDate, AmountDue);
                    bl.WorkOrderListID = CustomerListID;
                    bl.VendorListID = VendorListID;

                    //Get all field values for ItemRef aggregate
                    XmlNode ItemRef = Child.SelectSingleNode("./ItemRef");
                    if (ItemRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./ItemRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./ItemRef/ListID").InnerText;
                            bl.ItemListID = ListID;
                        }
                    }
                    //Done with field values for ItemRef aggregate

                    //Get all field values for InventorySiteRef aggregate
                    XmlNode InventorySiteRef = Child.SelectSingleNode("./InventorySiteRef");
                    if (InventorySiteRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./InventorySiteRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./InventorySiteRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./InventorySiteRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./InventorySiteRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for InventorySiteRef aggregate

                    //Get all field values for InventorySiteLocationRef aggregate
                    XmlNode InventorySiteLocationRef = Child.SelectSingleNode("./InventorySiteLocationRef");
                    if (InventorySiteLocationRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./InventorySiteLocationRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./InventorySiteLocationRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./InventorySiteLocationRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./InventorySiteLocationRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for InventorySiteLocationRef aggregate

                    //Get value of Desc
                    if (Child.SelectSingleNode("./Desc") != null)
                    {
                        string Desc = Child.SelectSingleNode("./Desc").InnerText;
                        bl.Description = Desc;
                    }
                    //Get value of Quantity
                    if (Child.SelectSingleNode("./Quantity") != null)
                    {
                        string Quantity = Child.SelectSingleNode("./Quantity").InnerText;
                        int quantity;
                        if (Int32.TryParse(Quantity, out quantity))
                        {
                            bl.Quantity = quantity;
                        }
                    }
                    //Get value of UnitOfMeasure
                    if (Child.SelectSingleNode("./UnitOfMeasure") != null)
                    {
                        string UnitOfMeasure = Child.SelectSingleNode("./UnitOfMeasure").InnerText;
                    }
                    //Get all field values for OverrideUOMSetRef aggregate
                    XmlNode OverrideUOMSetRef = Child.SelectSingleNode("./OverrideUOMSetRef");
                    if (OverrideUOMSetRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./OverrideUOMSetRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./OverrideUOMSetRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./OverrideUOMSetRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./OverrideUOMSetRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for OverrideUOMSetRef aggregate

                    //Get value of Cost
                    if (Child.SelectSingleNode("./Cost") != null)
                    {
                        string Cost = Child.SelectSingleNode("./Cost").InnerText;
                        decimal unitCost;
                        if (Decimal.TryParse(Cost, out unitCost))
                        {
                            bl.UnitCost = unitCost;
                        }
                    }
                    //Get value of Amount
                    if (Child.SelectSingleNode("./Amount") != null)
                    {
                        string Amount = Child.SelectSingleNode("./Amount").InnerText;
                        decimal amount;
                        if (Decimal.TryParse(Amount, out amount))
                        {
                            bl.Amount = amount;
                        }
                    }

                    //Get all field values for ClassRef aggregate
                    XmlNode ClassRef = Child.SelectSingleNode("./ClassRef");
                    if (ClassRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./ClassRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./ClassRef/ListID").InnerText;
                            bl.AreaListID = ListID;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./ClassRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./ClassRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for ClassRef aggregate

                    //Get value of BillableStatus
                    if (Child.SelectSingleNode("./BillableStatus") != null)
                    {
                        string BillableStatus = Child.SelectSingleNode("./BillableStatus").InnerText;
                        bl.BillableStatus = BillableStatus;
                    }

                    db.SaveChanges();
                }

                if (Child.Name == "ItemGroupLineRet")
                {
                    //Get value of TxnLineID
                    string TxnLineID = Child.SelectSingleNode("./TxnLineID").InnerText;

                    //Get all field values for ItemGroupRef aggregate
                    //Get value of ListID
                    if (Child.SelectSingleNode("./ItemGroupRef/ListID") != null)
                    {
                        string ListID = Child.SelectSingleNode("./ItemGroupRef/ListID").InnerText;
                    }
                    //Get value of FullName
                    if (Child.SelectSingleNode("./ItemGroupRef/FullName") != null)
                    {
                        string FullName = Child.SelectSingleNode("./ItemGroupRef/FullName").InnerText;
                    }
                    //Done with field values for ItemGroupRef aggregate

                    //Get value of Desc
                    if (Child.SelectSingleNode("./Desc") != null)
                    {
                        string Desc = Child.SelectSingleNode("./Desc").InnerText;
                    }
                    //Get value of Quantity
                    if (Child.SelectSingleNode("./Quantity") != null)
                    {
                        string Quantity = Child.SelectSingleNode("./Quantity").InnerText;
                    }
                    //Get value of UnitOfMeasure
                    if (Child.SelectSingleNode("./UnitOfMeasure") != null)
                    {
                        string UnitOfMeasure = Child.SelectSingleNode("./UnitOfMeasure").InnerText;
                    }
                    //Get all field values for OverrideUOMSetRef aggregate
                    XmlNode OverrideUOMSetRef = Child.SelectSingleNode("./OverrideUOMSetRef");
                    if (OverrideUOMSetRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./OverrideUOMSetRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./OverrideUOMSetRef/ListID").InnerText;
                        }
                        //Get value of FullName
                        if (Child.SelectSingleNode("./OverrideUOMSetRef/FullName") != null)
                        {
                            string FullName = Child.SelectSingleNode("./OverrideUOMSetRef/FullName").InnerText;
                        }
                    }
                    //Done with field values for OverrideUOMSetRef aggregate

                    //Get value of TotalAmount
                    string TotalAmount = Child.SelectSingleNode("./TotalAmount").InnerText;
                    //Walk list of ItemLineRet aggregates
                    XmlNodeList ItemLineRetList = Child.SelectNodes("./ItemLineRet");
                    if (ItemLineRetList != null)
                    {
                        for (int j = 0; j < ItemLineRetList.Count; j++)
                        {
                            XmlNode ItemLineRet = ItemLineRetList.Item(j);

                            //Get all field values for CustomerRef aggregate
                            string CustomerListID = "";
                            XmlNode CustomerRef = ItemLineRet.SelectSingleNode("./CustomerRef");
                            if (CustomerRef != null)
                            {
                                //Get value of ListID
                                if (ItemLineRet.SelectSingleNode("./CustomerRef/ListID") != null)
                                {
                                    CustomerListID = ItemLineRet.SelectSingleNode("./CustomerRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (ItemLineRet.SelectSingleNode("./CustomerRef/FullName") != null)
                                {
                                    string FullName = ItemLineRet.SelectSingleNode("./CustomerRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for CustomerRef aggregate

                            // Skip the rest if no associated customer (work order)
                            if (CustomerListID == "") continue;

                            //Get value of TxnLineID
                            string TxnLineID2 = ItemLineRet.SelectSingleNode("./TxnLineID").InnerText;

                            // Find existing or create new BillLine entry
                            BillLine bl = FindOrCreateBillLine(db, TxnLineID2, TxnID, TimeCreated, TimeModified, EditSequence, TxnDate, AmountDue);
                            bl.WorkOrderListID = CustomerListID;
                            bl.VendorListID = VendorListID;

                            //Get all field values for ItemRef aggregate
                            XmlNode ItemRef = ItemLineRet.SelectSingleNode("./ItemRef");
                            if (ItemRef != null)
                            {
                                //Get value of ListID
                                if (ItemLineRet.SelectSingleNode("./ItemRef/ListID") != null)
                                {
                                    string ListID = ItemLineRet.SelectSingleNode("./ItemRef/ListID").InnerText;
                                    bl.ItemListID = ListID;
                                }
                                //Get value of FullName
                                if (ItemLineRet.SelectSingleNode("./ItemRef/FullName") != null)
                                {
                                    string FullName = ItemLineRet.SelectSingleNode("./ItemRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for ItemRef aggregate

                            //Get all field values for InventorySiteRef aggregate
                            XmlNode InventorySiteRef = ItemLineRet.SelectSingleNode("./InventorySiteRef");
                            if (InventorySiteRef != null)
                            {
                                //Get value of ListID
                                if (ItemLineRet.SelectSingleNode("./InventorySiteRef/ListID") != null)
                                {
                                    string ListID = ItemLineRet.SelectSingleNode("./InventorySiteRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (ItemLineRet.SelectSingleNode("./InventorySiteRef/FullName") != null)
                                {
                                    string FullName = ItemLineRet.SelectSingleNode("./InventorySiteRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for InventorySiteRef aggregate

                            //Get all field values for InventorySiteLocationRef aggregate
                            XmlNode InventorySiteLocationRef = ItemLineRet.SelectSingleNode("./InventorySiteLocationRef");
                            if (InventorySiteLocationRef != null)
                            {
                                //Get value of ListID
                                if (ItemLineRet.SelectSingleNode("./InventorySiteLocationRef/ListID") != null)
                                {
                                    string ListID = ItemLineRet.SelectSingleNode("./InventorySiteLocationRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (ItemLineRet.SelectSingleNode("./InventorySiteLocationRef/FullName") != null)
                                {
                                    string FullName = ItemLineRet.SelectSingleNode("./InventorySiteLocationRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for InventorySiteLocationRef aggregate

                            //Get value of Desc
                            if (ItemLineRet.SelectSingleNode("./Desc") != null)
                            {
                                string Desc = ItemLineRet.SelectSingleNode("./Desc").InnerText;
                                bl.Description = Desc;
                            }
                            //Get value of Quantity
                            if (ItemLineRet.SelectSingleNode("./Quantity") != null)
                            {
                                string Quantity = ItemLineRet.SelectSingleNode("./Quantity").InnerText;
                                int quantity;
                                if (Int32.TryParse(Quantity, out quantity))
                                {
                                    bl.Quantity = quantity;
                                }
                            }
                            //Get value of UnitOfMeasure
                            if (ItemLineRet.SelectSingleNode("./UnitOfMeasure") != null)
                            {
                                string UnitOfMeasure = ItemLineRet.SelectSingleNode("./UnitOfMeasure").InnerText;
                            }
                            //Get all field values for OverrideUOMSetRef aggregate
                            XmlNode OverrideUOMSetRef2 = ItemLineRet.SelectSingleNode("./OverrideUOMSetRef");
                            if (OverrideUOMSetRef2 != null)
                            {
                                //Get value of ListID
                                if (ItemLineRet.SelectSingleNode("./OverrideUOMSetRef/ListID") != null)
                                {
                                    string ListID = ItemLineRet.SelectSingleNode("./OverrideUOMSetRef/ListID").InnerText;
                                }
                                //Get value of FullName
                                if (ItemLineRet.SelectSingleNode("./OverrideUOMSetRef/FullName") != null)
                                {
                                    string FullName = ItemLineRet.SelectSingleNode("./OverrideUOMSetRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for OverrideUOMSetRef aggregate

                            //Get value of Cost
                            if (ItemLineRet.SelectSingleNode("./Cost") != null)
                            {
                                string Cost = ItemLineRet.SelectSingleNode("./Cost").InnerText;
                                decimal cost;
                                if (Decimal.TryParse(Cost, out cost))
                                {
                                    bl.UnitCost = cost;
                                }
                            }
                            //Get value of Amount
                            if (ItemLineRet.SelectSingleNode("./Amount") != null)
                            {
                                string Amount = ItemLineRet.SelectSingleNode("./Amount").InnerText;
                                decimal amount;
                                if (Decimal.TryParse(Amount, out amount))
                                {
                                    bl.Amount = amount;
                                }
                            }

                            //Get all field values for ClassRef aggregate
                            XmlNode ClassRef = ItemLineRet.SelectSingleNode("./ClassRef");
                            if (ClassRef != null)
                            {
                                //Get value of ListID
                                if (ItemLineRet.SelectSingleNode("./ClassRef/ListID") != null)
                                {
                                    string ListID = ItemLineRet.SelectSingleNode("./ClassRef/ListID").InnerText;
                                    bl.AreaListID = ListID;
                                }
                                //Get value of FullName
                                if (ItemLineRet.SelectSingleNode("./ClassRef/FullName") != null)
                                {
                                    string FullName = ItemLineRet.SelectSingleNode("./ClassRef/FullName").InnerText;
                                }
                            }
                            //Done with field values for ClassRef aggregate

                            //Get value of BillableStatus
                            if (ItemLineRet.SelectSingleNode("./BillableStatus") != null)
                            {
                                string BillableStatus = ItemLineRet.SelectSingleNode("./BillableStatus").InnerText;
                                bl.BillableStatus = BillableStatus;
                            }

                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        private static BillLine FindOrCreateBillLine(RotoTrackDb db, string TxnLineID, string TxnID, string TimeCreated, string TimeModified, string EditSequence, string TxnDate, string AmountDue)
        {
            BillLine bl = null;
            if (db.BillLines.Any(f => f.TxnLineId == TxnLineID))
            {
                bl = db.BillLines.First(f => f.TxnLineId == TxnLineID);
            }
            else
            {
                bl = new BillLine();
                db.BillLines.Add(bl);
            }

            bl.TxnLineId = TxnLineID;
            bl.BillTxnId = TxnID;
            DateTime createdDate;
            if (DateTime.TryParse(TimeCreated, out createdDate)) bl.BillCreated = createdDate;
            DateTime modifiedDate;
            if (DateTime.TryParse(TimeModified, out modifiedDate)) bl.BillModified = modifiedDate;
            bl.BillEditSequence = EditSequence;
            DateTime txnDate;
            if (DateTime.TryParse(TxnDate, out txnDate)) bl.BillTxnDate = txnDate;
            decimal amountDue;
            if (Decimal.TryParse(AmountDue, out amountDue)) bl.BillAmountDue = amountDue;

            return bl;
        }

    }
}
