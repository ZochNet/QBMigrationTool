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
    public class VendorCreditDAL
    {        
        public static void RemoveDeleted()
        {
            XmlDocument doc = BuildDeletedRequest();
            string response = QBUtils.DoRequest(doc);
            HandleDeletedResponse(response);
        }

        public static XmlDocument BuildDeletedRequest()
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement TxnDeletedQueryRq = doc.CreateElement("TxnDeletedQueryRq");
            parent.AppendChild(TxnDeletedQueryRq);
            TxnDeletedQueryRq.AppendChild(XmlUtils.MakeSimpleElem(doc, "TxnDelType", "VendorCredit"));

            return doc;
        }
        
        public static XmlDocument BuildQueryRequest(string fromModifiedDate, string toModifiedDate)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("VendorCreditQueryRq");
            parent.AppendChild(queryElement);

            XmlElement dateRangeFilter = doc.CreateElement("ModifiedDateRangeFilter");
            queryElement.AppendChild(dateRangeFilter);
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            dateRangeFilter.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));                        
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "IncludeLineItems", "1"));

            return doc;
        }

        public static void HandleDeletedResponse(string response)
        {
            WalkTxnDeletedQueryRs(response);
        }
        
        public static void HandleResponse(string response)
        {
            WalkVendorCreditQueryRs(response);
        }

        public static void WalkTxnDeletedQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList TxnDeletedQueryRsList = responseXmlDoc.GetElementsByTagName("TxnDeletedQueryRs");
            if (TxnDeletedQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = TxnDeletedQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList TxnDeletedRetList = responseNode.SelectNodes("//TxnDeletedRet");//XPath Query
                    for (int i = 0; i < TxnDeletedRetList.Count; i++)
                    {
                        XmlNode TxnDeletedRet = TxnDeletedRetList.Item(i);
                        WalkTxnDeletedRet(TxnDeletedRet);
                    }
                }
            }
        }

        private static void WalkTxnDeletedRet(XmlNode TxnDeletedRet)
        {
            if (TxnDeletedRet == null) return;

            //Go through all the elements of TxnDeletedRet
            //Get value of TxnDelType
            string TxnDelType = TxnDeletedRet.SelectSingleNode("./TxnDelType").InnerText;
            //Get value of TxnID
            string TxnID = TxnDeletedRet.SelectSingleNode("./TxnID").InnerText;
            //Get value of TimeCreated
            string TimeCreated = TxnDeletedRet.SelectSingleNode("./TimeCreated").InnerText;
            //Get value of TimeDeleted
            string TimeDeleted = TxnDeletedRet.SelectSingleNode("./TimeDeleted").InnerText;
            //Get value of RefNumber
            if (TxnDeletedRet.SelectSingleNode("./RefNumber") != null)
            {
                string RefNumber = TxnDeletedRet.SelectSingleNode("./RefNumber").InnerText;
            }

            RotoTrackDb db = new RotoTrackDb();
            List<VendorCreditLine> vclList = db.VendorCreditLines.Where(f => f.VendorCreditTxnId == TxnID).ToList();
            foreach (VendorCreditLine vcl in vclList.ToList())
            {
                db.VendorCreditLines.Remove(vcl);
            }
            db.SaveChanges();

        }

        private static void WalkVendorCreditQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList VendorCreditQueryRsList = responseXmlDoc.GetElementsByTagName("VendorCreditQueryRs");
            if (VendorCreditQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = VendorCreditQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList VendorCreditRetList = responseNode.SelectNodes("//VendorCreditRet");//XPath Query
                    for (int i = 0; i < VendorCreditRetList.Count; i++)
                    {
                        XmlNode VendorCreditRet = VendorCreditRetList.Item(i);
                        VendorCreditDAL.WalkVendorCreditRetForQuery(VendorCreditRet);
                    }
                }
            }
        }

        private static void WalkVendorCreditRetForQuery(XmlNode VendorCreditRet)
        {
            if (VendorCreditRet == null) return;

            RotoTrackDb db = new RotoTrackDb();

            //Go through all the elements of VendorCreditRet
            //Get value of TxnID
            string TxnID = VendorCreditRet.SelectSingleNode("./TxnID").InnerText;

            // New or modified objects will return all current line items, so remove any existing ones first and then recreate them.
            RemoveExistingLineItems(db, TxnID);

            //Get value of TimeCreated
            string TimeCreated = VendorCreditRet.SelectSingleNode("./TimeCreated").InnerText;
            //Get value of TimeModified
            string TimeModified = VendorCreditRet.SelectSingleNode("./TimeModified").InnerText;
            //Get value of EditSequence
            string EditSequence = VendorCreditRet.SelectSingleNode("./EditSequence").InnerText;
            //Get value of TxnNumber
            if (VendorCreditRet.SelectSingleNode("./TxnNumber") != null)
            {
                string TxnNumber = VendorCreditRet.SelectSingleNode("./TxnNumber").InnerText;
            }

            //Get all field values for APAccountRef aggregate
            XmlNode APAccountRef = VendorCreditRet.SelectSingleNode("./APAccountRef");
            if (APAccountRef != null)
            {
                //Get value of ListID
                if (VendorCreditRet.SelectSingleNode("./APAccountRef/ListID") != null)
                {
                    string ListID = VendorCreditRet.SelectSingleNode("./APAccountRef/ListID").InnerText;
                }
                //Get value of FullName
                if (VendorCreditRet.SelectSingleNode("./APAccountRef/FullName") != null)
                {
                    string FullName = VendorCreditRet.SelectSingleNode("./APAccountRef/FullName").InnerText;
                }
            }
            //Done with field values for APAccountRef aggregate

            //Get value of TxnDate
            string TxnDate = VendorCreditRet.SelectSingleNode("./TxnDate").InnerText;
            //Get value of DueDate
            if (VendorCreditRet.SelectSingleNode("./DueDate") != null)
            {
                string DueDate = VendorCreditRet.SelectSingleNode("./DueDate").InnerText;
            }

            //Get value of AmountDue         
            string AmountDue = "";
            if (VendorCreditRet.SelectSingleNode("./CreditAmount") != null)
            {
                AmountDue = VendorCreditRet.SelectSingleNode("./CreditAmount").InnerText;
            }

            //Get all field values for CurrencyRef aggregate
            XmlNode CurrencyRef = VendorCreditRet.SelectSingleNode("./CurrencyRef");
            if (CurrencyRef != null)
            {
                //Get value of ListID
                if (VendorCreditRet.SelectSingleNode("./CurrencyRef/ListID") != null)
                {
                    string ListID = VendorCreditRet.SelectSingleNode("./CurrencyRef/ListID").InnerText;
                }
                //Get value of FullName
                if (VendorCreditRet.SelectSingleNode("./CurrencyRef/FullName") != null)
                {
                    string FullName = VendorCreditRet.SelectSingleNode("./CurrencyRef/FullName").InnerText;
                }
            }
            //Done with field values for CurrencyRef aggregate

            //Get all field values for VendorRef aggregate
            //Get value of ListID
            string VendorListID = "";
            if (VendorCreditRet.SelectSingleNode("./VendorRef/ListID") != null)
            {
                VendorListID = VendorCreditRet.SelectSingleNode("./VendorRef/ListID").InnerText;
            }
            //Get value of FullName
            if (VendorCreditRet.SelectSingleNode("./VendorRef/FullName") != null)
            {
                string FullName = VendorCreditRet.SelectSingleNode("./VendorRef/FullName").InnerText;
            }
            //Done with field values for VendorRef aggregate

            //Get value of ExchangeRate
            if (VendorCreditRet.SelectSingleNode("./ExchangeRate") != null)
            {
                string ExchangeRate = VendorCreditRet.SelectSingleNode("./ExchangeRate").InnerText;
            }
            //Get value of AmountDueInHomeCurrency
            if (VendorCreditRet.SelectSingleNode("./AmountDueInHomeCurrency") != null)
            {
                string AmountDueInHomeCurrency = VendorCreditRet.SelectSingleNode("./AmountDueInHomeCurrency").InnerText;
            }
            //Get value of RefNumber
            if (VendorCreditRet.SelectSingleNode("./RefNumber") != null)
            {
                string RefNumber = VendorCreditRet.SelectSingleNode("./RefNumber").InnerText;
            }
            //Get all field values for TermsRef aggregate
            XmlNode TermsRef = VendorCreditRet.SelectSingleNode("./TermsRef");
            if (TermsRef != null)
            {
                //Get value of ListID
                if (VendorCreditRet.SelectSingleNode("./TermsRef/ListID") != null)
                {
                    string ListID = VendorCreditRet.SelectSingleNode("./TermsRef/ListID").InnerText;
                }
                //Get value of FullName
                if (VendorCreditRet.SelectSingleNode("./TermsRef/FullName") != null)
                {
                    string FullName = VendorCreditRet.SelectSingleNode("./TermsRef/FullName").InnerText;
                }
            }
            //Done with field values for TermsRef aggregate

            //Get value of Memo
            if (VendorCreditRet.SelectSingleNode("./Memo") != null)
            {
                string Memo = VendorCreditRet.SelectSingleNode("./Memo").InnerText;
            }
            //Get value of IsPaid
            if (VendorCreditRet.SelectSingleNode("./IsPaid") != null)
            {
                string IsPaid = VendorCreditRet.SelectSingleNode("./IsPaid").InnerText;
            }
            //Get value of ExternalGUID
            if (VendorCreditRet.SelectSingleNode("./ExternalGUID") != null)
            {
                string ExternalGUID = VendorCreditRet.SelectSingleNode("./ExternalGUID").InnerText;
            }

            //Walk list of ExpenseLineRet aggregates
            XmlNodeList ExpenseLineRetList = VendorCreditRet.SelectNodes("./ExpenseLineRet");
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

                    // Find existing or create new VendorCreditLine entry
                    VendorCreditLine vcl = FindOrCreateVendorCreditLine(db, TxnLineID, TxnID, TimeCreated, TimeModified, EditSequence, TxnDate, AmountDue);
                    vcl.WorkOrderListID = CustomerListID;
                    vcl.VendorListID = VendorListID;

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
                    vcl.Description = Description;

                    //Get value of Amount
                    if (ExpenseLineRet.SelectSingleNode("./Amount") != null)
                    {
                        string Amount = ExpenseLineRet.SelectSingleNode("./Amount").InnerText;
                        decimal amount;
                        if (Decimal.TryParse(Amount, out amount))
                        {
                            vcl.Amount = amount;
                            vcl.UnitCost = amount;
                            vcl.Quantity = 1.0M;
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
                            vcl.AreaListID = ListID;
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
                        vcl.BillableStatus = BillableStatus;
                    }

                    db.SaveChanges();
                }
            }

            XmlNodeList ORItemLineRetListChildren = VendorCreditRet.SelectNodes("./*");
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

                    // Find existing or create new VendorCreditLine entry
                    VendorCreditLine vcl = FindOrCreateVendorCreditLine(db, TxnLineID, TxnID, TimeCreated, TimeModified, EditSequence, TxnDate, AmountDue);
                    vcl.WorkOrderListID = CustomerListID;
                    vcl.VendorListID = VendorListID;

                    //Get all field values for ItemRef aggregate
                    XmlNode ItemRef = Child.SelectSingleNode("./ItemRef");
                    if (ItemRef != null)
                    {
                        //Get value of ListID
                        if (Child.SelectSingleNode("./ItemRef/ListID") != null)
                        {
                            string ListID = Child.SelectSingleNode("./ItemRef/ListID").InnerText;
                            vcl.ItemListID = ListID;
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
                        vcl.Description = Desc;
                    }
                    //Get value of Quantity
                    if (Child.SelectSingleNode("./Quantity") != null)
                    {
                        string Quantity = Child.SelectSingleNode("./Quantity").InnerText;
                        decimal quantity;
                        if (Decimal.TryParse(Quantity, out quantity))
                        {
                            vcl.Quantity = quantity;
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
                            vcl.UnitCost = unitCost;
                        }
                    }
                    //Get value of Amount
                    if (Child.SelectSingleNode("./Amount") != null)
                    {
                        string Amount = Child.SelectSingleNode("./Amount").InnerText;
                        decimal amount;
                        if (Decimal.TryParse(Amount, out amount))
                        {
                            vcl.Amount = amount;
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
                            vcl.AreaListID = ListID;
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
                        vcl.BillableStatus = BillableStatus;
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

                            // Find existing or create new VendorCreditLine entry
                            VendorCreditLine vcl = FindOrCreateVendorCreditLine(db, TxnLineID2, TxnID, TimeCreated, TimeModified, EditSequence, TxnDate, AmountDue);
                            vcl.WorkOrderListID = CustomerListID;
                            vcl.VendorListID = VendorListID;

                            //Get all field values for ItemRef aggregate
                            XmlNode ItemRef = ItemLineRet.SelectSingleNode("./ItemRef");
                            if (ItemRef != null)
                            {
                                //Get value of ListID
                                if (ItemLineRet.SelectSingleNode("./ItemRef/ListID") != null)
                                {
                                    string ListID = ItemLineRet.SelectSingleNode("./ItemRef/ListID").InnerText;
                                    vcl.ItemListID = ListID;
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
                                vcl.Description = Desc;
                            }
                            //Get value of Quantity
                            if (ItemLineRet.SelectSingleNode("./Quantity") != null)
                            {
                                string Quantity = ItemLineRet.SelectSingleNode("./Quantity").InnerText;
                                decimal quantity;
                                if (Decimal.TryParse(Quantity, out quantity))
                                {
                                    vcl.Quantity = quantity;
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
                                    vcl.UnitCost = cost;
                                }
                            }
                            //Get value of Amount
                            if (ItemLineRet.SelectSingleNode("./Amount") != null)
                            {
                                string Amount = ItemLineRet.SelectSingleNode("./Amount").InnerText;
                                decimal amount;
                                if (Decimal.TryParse(Amount, out amount))
                                {
                                    vcl.Amount = amount;
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
                                    vcl.AreaListID = ListID;
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
                                vcl.BillableStatus = BillableStatus;
                            }

                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        private static void RemoveExistingLineItems(RotoTrackDb db, string TxnID)
        {
            List<VendorCreditLine> blList = db.VendorCreditLines.Where(f => f.VendorCreditTxnId == TxnID).ToList();
            foreach (VendorCreditLine vcl in blList.ToList())
            {
                db.VendorCreditLines.Remove(vcl);
            }
            db.SaveChanges();
        }

        private static VendorCreditLine FindOrCreateVendorCreditLine(RotoTrackDb db, string TxnLineID, string TxnID, string TimeCreated, string TimeModified, string EditSequence, string TxnDate, string AmountDue)
        {
            VendorCreditLine vcl = null;
            if (db.VendorCreditLines.Any(f => f.TxnLineId == TxnLineID))
            {
                vcl = db.VendorCreditLines.First(f => f.TxnLineId == TxnLineID);
            }
            else
            {
                vcl = new VendorCreditLine();
                db.VendorCreditLines.Add(vcl);
            }

            vcl.TxnLineId = TxnLineID;
            vcl.VendorCreditTxnId = TxnID;
            DateTime createdDate;
            if (DateTime.TryParse(TimeCreated, out createdDate)) vcl.VendorCreditCreated = createdDate;
            DateTime modifiedDate;
            if (DateTime.TryParse(TimeModified, out modifiedDate)) vcl.VendorCreditModified = modifiedDate;
            vcl.VendorCreditEditSequence = EditSequence;
            DateTime txnDate;
            if (DateTime.TryParse(TxnDate, out txnDate)) vcl.VendorCreditTxnDate = txnDate;
            decimal amountDue;
            if (Decimal.TryParse(AmountDue, out amountDue)) vcl.VendorCreditAmountDue = amountDue;

            return vcl;
        }

    }
}
