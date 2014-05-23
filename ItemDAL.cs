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
    public class ItemDAL
    {
        public static XmlDocument BuildItemQueryRequest(string fromModifiedDate, string toModifiedDate)
        {
            XmlDocument doc = XmlUtils.MakeRequestDocument();
            XmlElement parent = XmlUtils.MakeRequestParentElement(doc);
            XmlElement queryElement = doc.CreateElement("ItemQueryRq");
            parent.AppendChild(queryElement);

            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "FromModifiedDate", fromModifiedDate));
            queryElement.AppendChild(XmlUtils.MakeSimpleElem(doc, "ToModifiedDate", toModifiedDate));

            return doc;
        }

        public static void HandleResponse(string response)
        {
            WalkItemQueryRs(response);
        }

        private static void WalkItemQueryRs(string response)
        {
            //Parse the response XML string into an XmlDocument
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(response);

            //Get the response for our request
            XmlNodeList ItemQueryRsList = responseXmlDoc.GetElementsByTagName("ItemQueryRs");
            if (ItemQueryRsList.Count == 1) //Should always be true since we only did one request in this sample
            {
                XmlNode responseNode = ItemQueryRsList.Item(0);
                //Check the status code, info, and severity
                XmlAttributeCollection rsAttributes = responseNode.Attributes;
                string statusCode = rsAttributes.GetNamedItem("statusCode").Value;
                string statusSeverity = rsAttributes.GetNamedItem("statusSeverity").Value;
                string statusMessage = rsAttributes.GetNamedItem("statusMessage").Value;

                //status code = 0 all OK, > 0 is warning
                if (Convert.ToInt32(statusCode) >= 0)
                {
                    XmlNodeList ORList = responseNode.SelectNodes("*");//XPath Query
                    for (int i = 0; i < ORList.Count; i++)
                    {
                        XmlNode OR = ORList.Item(i);
                        WalkOR(OR);
                    }
                }
            }
        }

        private static void WalkOR(XmlNode OR)
        {
            if (OR == null) return;

            RotoTrackDb db = new RotoTrackDb();

            //Go through all the elements of OR
            //Get all field values for ItemServiceRet aggregate 
            //XmlNode ItemServiceRet = OR.SelectSingleNode("./ItemServiceRet");
            if (OR.Name == "ItemServiceRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of FullName
                string FullName = OR.SelectSingleNode("./FullName").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, FullName, IsActive, "ItemService");
                db.SaveChanges();
                                /*
                //Get all field values for ClassRef aggregate 
                XmlNode ClassRef = OR.SelectSingleNode("./ItemServiceRet/ClassRef");
                if (ClassRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemServiceRet/ClassRef/ListID") != null)
                    {
                        string ClassListID = OR.SelectSingleNode("./ItemServiceRet/ClassRef/ListID").InnerText;
                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemServiceRet/ClassRef/FullName") != null)
                    {
                        string ClassFullName = OR.SelectSingleNode("./ItemServiceRet/ClassRef/FullName").InnerText;
                    }
                }
                //Done with field values for ClassRef aggregate

                //Get all field values for ParentRef aggregate 
                XmlNode ParentRef = OR.SelectSingleNode("./ItemServiceRet/ParentRef");
                if (ParentRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemServiceRet/ParentRef/ListID") != null)
                    {
                        string ParentListID = OR.SelectSingleNode("./ItemServiceRet/ParentRef/ListID").InnerText;
                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemServiceRet/ParentRef/FullName") != null)
                    {
                        string ParentFullName = OR.SelectSingleNode("./ItemServiceRet/ParentRef/FullName").InnerText;
                    }
                }
                //Done with field values for ParentRef aggregate

                //Get value of Sublevel
                string Sublevel = OR.SelectSingleNode("./ItemServiceRet/Sublevel").InnerText;
                //Get all field values for UnitOfMeasureSetRef aggregate 
                XmlNode UnitOfMeasureSetRef = OR.SelectSingleNode("./ItemServiceRet/UnitOfMeasureSetRef");
                if (UnitOfMeasureSetRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemServiceRet/UnitOfMeasureSetRef/ListID") != null)
                    {
                        string UnitOfMeasureListID = OR.SelectSingleNode("./ItemServiceRet/UnitOfMeasureSetRef/ListID").InnerText;
                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemServiceRet/UnitOfMeasureSetRef/FullName") != null)
                    {
                        string UnitOfMeasureFullName = OR.SelectSingleNode("./ItemServiceRet/UnitOfMeasureSetRef/FullName").InnerText;
                    }
                }
                //Done with field values for UnitOfMeasureSetRef aggregate

                //Get all field values for SalesTaxCodeRef aggregate 
                XmlNode SalesTaxCodeRef = OR.SelectSingleNode("./ItemServiceRet/SalesTaxCodeRef");
                if (SalesTaxCodeRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemServiceRet/SalesTaxCodeRef/ListID") != null)
                    {
                        string SalesTaxCodeListID = OR.SelectSingleNode("./ItemServiceRet/SalesTaxCodeRef/ListID").InnerText;
                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemServiceRet/SalesTaxCodeRef/FullName") != null)
                    {
                        string SalesTaxCodeFullName = OR.SelectSingleNode("./ItemServiceRet/SalesTaxCodeRef/FullName").InnerText;
                    }
                }
                //Done with field values for SalesTaxCodeRef aggregate

                XmlNodeList ORSalesPurchaseChildren = OR.SelectNodes("./ItemServiceRet/*");
                for (int i = 0; i < ORSalesPurchaseChildren.Count; i++)
                {
                    XmlNode Child = ORSalesPurchaseChildren.Item(i);
                    if (Child.Name == "SalesOrPurchase")
                    {
                        //Get value of Desc
                        if (Child.SelectSingleNode("./Desc") != null)
                        {
                            string Desc = Child.SelectSingleNode("./Desc").InnerText;

                        }
                        XmlNodeList ORPriceChildren = Child.SelectNodes("./*");
                        for (int i = 0; i < ORPriceChildren.Count; i++)
                        {
                            XmlNode Child = ORPriceChildren.Item(i);
                            if (Child.Name == "Price")
                            {
                            }

                            if (Child.Name == "PricePercent")
                            {
                            }


                        }

                        //Get all field values for AccountRef aggregate 
                        XmlNode AccountRef = Child.SelectSingleNode("./AccountRef");
                        if (AccountRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./AccountRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./AccountRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./AccountRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./AccountRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for AccountRef aggregate


                    }

                    if (Child.Name == "SalesAndPurchase")
                    {
                        //Get value of SalesDesc
                        if (Child.SelectSingleNode("./SalesDesc") != null)
                        {
                            string SalesDesc = Child.SelectSingleNode("./SalesDesc").InnerText;

                        }
                        //Get value of SalesPrice
                        if (Child.SelectSingleNode("./SalesPrice") != null)
                        {
                            string SalesPrice = Child.SelectSingleNode("./SalesPrice").InnerText;

                        }
                        //Get all field values for IncomeAccountRef aggregate 
                        XmlNode IncomeAccountRef = Child.SelectSingleNode("./IncomeAccountRef");
                        if (IncomeAccountRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./IncomeAccountRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./IncomeAccountRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./IncomeAccountRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./IncomeAccountRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for IncomeAccountRef aggregate

                        //Get value of PurchaseDesc
                        if (Child.SelectSingleNode("./PurchaseDesc") != null)
                        {
                            string PurchaseDesc = Child.SelectSingleNode("./PurchaseDesc").InnerText;

                        }
                        //Get value of PurchaseCost
                        if (Child.SelectSingleNode("./PurchaseCost") != null)
                        {
                            string PurchaseCost = Child.SelectSingleNode("./PurchaseCost").InnerText;

                        }
                        //Get all field values for ExpenseAccountRef aggregate 
                        XmlNode ExpenseAccountRef = Child.SelectSingleNode("./ExpenseAccountRef");
                        if (ExpenseAccountRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./ExpenseAccountRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./ExpenseAccountRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./ExpenseAccountRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./ExpenseAccountRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for ExpenseAccountRef aggregate

                        //Get all field values for PrefVendorRef aggregate 
                        XmlNode PrefVendorRef = Child.SelectSingleNode("./PrefVendorRef");
                        if (PrefVendorRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./PrefVendorRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./PrefVendorRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./PrefVendorRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./PrefVendorRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for PrefVendorRef aggregate


                    }


                }

                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemServiceRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemServiceRet/ExternalGUID").InnerText;

                }
                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemServiceRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;
                    }
                }
                */
            }
            //Done with field values for ItemServiceRet aggregate

            //Get all field values for ItemNonInventoryRet aggregate 
            //XmlNode ItemNonInventoryRet = OR.SelectSingleNode("./ItemNonInventoryRet");
            if (OR.Name ==  "ItemNonInventoryRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of FullName
                string FullName = OR.SelectSingleNode("./FullName").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, FullName, IsActive, "ItemNonInventory");
                db.SaveChanges();

                /*
                //Get all field values for ClassRef aggregate 
                XmlNode ClassRef = OR.SelectSingleNode("./ItemNonInventoryRet/ClassRef");
                if (ClassRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemNonInventoryRet/ClassRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemNonInventoryRet/ClassRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemNonInventoryRet/ClassRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemNonInventoryRet/ClassRef/FullName").InnerText;

                    }

                }
                //Done with field values for ClassRef aggregate

                //Get all field values for ParentRef aggregate 
                XmlNode ParentRef = OR.SelectSingleNode("./ItemNonInventoryRet/ParentRef");
                if (ParentRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemNonInventoryRet/ParentRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemNonInventoryRet/ParentRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemNonInventoryRet/ParentRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemNonInventoryRet/ParentRef/FullName").InnerText;

                    }

                }
                //Done with field values for ParentRef aggregate

                //Get value of Sublevel
                string Sublevel = OR.SelectSingleNode("./ItemNonInventoryRet/Sublevel").InnerText;
                //Get value of ManufacturerPartNumber
                if (OR.SelectSingleNode("./ItemNonInventoryRet/ManufacturerPartNumber") != null)
                {
                    string ManufacturerPartNumber = OR.SelectSingleNode("./ItemNonInventoryRet/ManufacturerPartNumber").InnerText;

                }
                //Get all field values for UnitOfMeasureSetRef aggregate 
                XmlNode UnitOfMeasureSetRef = OR.SelectSingleNode("./ItemNonInventoryRet/UnitOfMeasureSetRef");
                if (UnitOfMeasureSetRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemNonInventoryRet/UnitOfMeasureSetRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemNonInventoryRet/UnitOfMeasureSetRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemNonInventoryRet/UnitOfMeasureSetRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemNonInventoryRet/UnitOfMeasureSetRef/FullName").InnerText;

                    }

                }
                //Done with field values for UnitOfMeasureSetRef aggregate

                //Get all field values for SalesTaxCodeRef aggregate 
                XmlNode SalesTaxCodeRef = OR.SelectSingleNode("./ItemNonInventoryRet/SalesTaxCodeRef");
                if (SalesTaxCodeRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemNonInventoryRet/SalesTaxCodeRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemNonInventoryRet/SalesTaxCodeRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemNonInventoryRet/SalesTaxCodeRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemNonInventoryRet/SalesTaxCodeRef/FullName").InnerText;

                    }

                }
                //Done with field values for SalesTaxCodeRef aggregate

                XmlNodeList ORSalesPurchaseChildren = OR.SelectNodes("./ItemNonInventoryRet/*");
                for (int i = 0; i < ORSalesPurchaseChildren.Count; i++)
                {
                    XmlNode Child = ORSalesPurchaseChildren.Item(i);
                    if (Child.Name == "SalesOrPurchase")
                    {
                        //Get value of Desc
                        if (Child.SelectSingleNode("./Desc") != null)
                        {
                            string Desc = Child.SelectSingleNode("./Desc").InnerText;

                        }
                        XmlNodeList ORPriceChildren = Child.SelectNodes("./*");
                        for (int i = 0; i < ORPriceChildren.Count; i++)
                        {
                            XmlNode Child = ORPriceChildren.Item(i);
                            if (Child.Name == "Price")
                            {
                            }

                            if (Child.Name == "PricePercent")
                            {
                            }


                        }

                        //Get all field values for AccountRef aggregate 
                        XmlNode AccountRef = Child.SelectSingleNode("./AccountRef");
                        if (AccountRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./AccountRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./AccountRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./AccountRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./AccountRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for AccountRef aggregate


                    }

                    if (Child.Name == "SalesAndPurchase")
                    {
                        //Get value of SalesDesc
                        if (Child.SelectSingleNode("./SalesDesc") != null)
                        {
                            string SalesDesc = Child.SelectSingleNode("./SalesDesc").InnerText;

                        }
                        //Get value of SalesPrice
                        if (Child.SelectSingleNode("./SalesPrice") != null)
                        {
                            string SalesPrice = Child.SelectSingleNode("./SalesPrice").InnerText;

                        }
                        //Get all field values for IncomeAccountRef aggregate 
                        XmlNode IncomeAccountRef = Child.SelectSingleNode("./IncomeAccountRef");
                        if (IncomeAccountRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./IncomeAccountRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./IncomeAccountRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./IncomeAccountRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./IncomeAccountRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for IncomeAccountRef aggregate

                        //Get value of PurchaseDesc
                        if (Child.SelectSingleNode("./PurchaseDesc") != null)
                        {
                            string PurchaseDesc = Child.SelectSingleNode("./PurchaseDesc").InnerText;

                        }
                        //Get value of PurchaseCost
                        if (Child.SelectSingleNode("./PurchaseCost") != null)
                        {
                            string PurchaseCost = Child.SelectSingleNode("./PurchaseCost").InnerText;

                        }
                        //Get all field values for ExpenseAccountRef aggregate 
                        XmlNode ExpenseAccountRef = Child.SelectSingleNode("./ExpenseAccountRef");
                        if (ExpenseAccountRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./ExpenseAccountRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./ExpenseAccountRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./ExpenseAccountRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./ExpenseAccountRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for ExpenseAccountRef aggregate

                        //Get all field values for PrefVendorRef aggregate 
                        XmlNode PrefVendorRef = Child.SelectSingleNode("./PrefVendorRef");
                        if (PrefVendorRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./PrefVendorRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./PrefVendorRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./PrefVendorRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./PrefVendorRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for PrefVendorRef aggregate


                    }


                }

                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemNonInventoryRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemNonInventoryRet/ExternalGUID").InnerText;

                }
                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemNonInventoryRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    }

                }

                */
            }
            //Done with field values for ItemNonInventoryRet aggregate
            

            //Get all field values for ItemOtherChargeRet aggregate 
            //XmlNode ItemOtherChargeRet = OR.SelectSingleNode("./ItemOtherChargeRet");
            if (OR.Name == "ItemOtherChargeRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of FullName
                string FullName = OR.SelectSingleNode("./FullName").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, FullName, IsActive, "ItemOtherCharge");
                db.SaveChanges();

                /*
                //Get all field values for ClassRef aggregate 
                XmlNode ClassRef = OR.SelectSingleNode("./ItemOtherChargeRet/ClassRef");
                if (ClassRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemOtherChargeRet/ClassRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemOtherChargeRet/ClassRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemOtherChargeRet/ClassRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemOtherChargeRet/ClassRef/FullName").InnerText;

                    }

                }
                //Done with field values for ClassRef aggregate

                //Get all field values for ParentRef aggregate 
                XmlNode ParentRef = OR.SelectSingleNode("./ItemOtherChargeRet/ParentRef");
                if (ParentRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemOtherChargeRet/ParentRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemOtherChargeRet/ParentRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemOtherChargeRet/ParentRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemOtherChargeRet/ParentRef/FullName").InnerText;

                    }

                }
                //Done with field values for ParentRef aggregate

                //Get value of Sublevel
                string Sublevel = OR.SelectSingleNode("./ItemOtherChargeRet/Sublevel").InnerText;
                //Get all field values for SalesTaxCodeRef aggregate 
                XmlNode SalesTaxCodeRef = OR.SelectSingleNode("./ItemOtherChargeRet/SalesTaxCodeRef");
                if (SalesTaxCodeRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemOtherChargeRet/SalesTaxCodeRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemOtherChargeRet/SalesTaxCodeRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemOtherChargeRet/SalesTaxCodeRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemOtherChargeRet/SalesTaxCodeRef/FullName").InnerText;

                    }

                }
                //Done with field values for SalesTaxCodeRef aggregate

                XmlNodeList ORSalesPurchaseChildren = OR.SelectNodes("./ItemOtherChargeRet/*");
                for (int i = 0; i < ORSalesPurchaseChildren.Count; i++)
                {
                    XmlNode Child = ORSalesPurchaseChildren.Item(i);
                    if (Child.Name == "SalesOrPurchase")
                    {
                        //Get value of Desc
                        if (Child.SelectSingleNode("./Desc") != null)
                        {
                            string Desc = Child.SelectSingleNode("./Desc").InnerText;

                        }
                        XmlNodeList ORPriceChildren = Child.SelectNodes("./*");
                        for (int i = 0; i < ORPriceChildren.Count; i++)
                        {
                            XmlNode Child = ORPriceChildren.Item(i);
                            if (Child.Name == "Price")
                            {
                            }

                            if (Child.Name == "PricePercent")
                            {
                            }


                        }

                        //Get all field values for AccountRef aggregate 
                        XmlNode AccountRef = Child.SelectSingleNode("./AccountRef");
                        if (AccountRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./AccountRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./AccountRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./AccountRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./AccountRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for AccountRef aggregate


                    }

                    if (Child.Name == "SalesAndPurchase")
                    {
                        //Get value of SalesDesc
                        if (Child.SelectSingleNode("./SalesDesc") != null)
                        {
                            string SalesDesc = Child.SelectSingleNode("./SalesDesc").InnerText;

                        }
                        //Get value of SalesPrice
                        if (Child.SelectSingleNode("./SalesPrice") != null)
                        {
                            string SalesPrice = Child.SelectSingleNode("./SalesPrice").InnerText;

                        }
                        //Get all field values for IncomeAccountRef aggregate 
                        XmlNode IncomeAccountRef = Child.SelectSingleNode("./IncomeAccountRef");
                        if (IncomeAccountRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./IncomeAccountRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./IncomeAccountRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./IncomeAccountRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./IncomeAccountRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for IncomeAccountRef aggregate

                        //Get value of PurchaseDesc
                        if (Child.SelectSingleNode("./PurchaseDesc") != null)
                        {
                            string PurchaseDesc = Child.SelectSingleNode("./PurchaseDesc").InnerText;

                        }
                        //Get value of PurchaseCost
                        if (Child.SelectSingleNode("./PurchaseCost") != null)
                        {
                            string PurchaseCost = Child.SelectSingleNode("./PurchaseCost").InnerText;

                        }
                        //Get all field values for ExpenseAccountRef aggregate 
                        XmlNode ExpenseAccountRef = Child.SelectSingleNode("./ExpenseAccountRef");
                        if (ExpenseAccountRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./ExpenseAccountRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./ExpenseAccountRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./ExpenseAccountRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./ExpenseAccountRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for ExpenseAccountRef aggregate

                        //Get all field values for PrefVendorRef aggregate 
                        XmlNode PrefVendorRef = Child.SelectSingleNode("./PrefVendorRef");
                        if (PrefVendorRef != null)
                        {
                            //Get value of ListID
                            if (Child.SelectSingleNode("./PrefVendorRef/ListID") != null)
                            {
                                string ListID = Child.SelectSingleNode("./PrefVendorRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (Child.SelectSingleNode("./PrefVendorRef/FullName") != null)
                            {
                                string FullName = Child.SelectSingleNode("./PrefVendorRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for PrefVendorRef aggregate


                    }


                }

                //Get value of SpecialItemType
                if (OR.SelectSingleNode("./ItemOtherChargeRet/SpecialItemType") != null)
                {
                    string SpecialItemType = OR.SelectSingleNode("./ItemOtherChargeRet/SpecialItemType").InnerText;

                }
                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemOtherChargeRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemOtherChargeRet/ExternalGUID").InnerText;

                }
                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemOtherChargeRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    }

                }

                */
            }
            //Done with field values for ItemOtherChargeRet aggregate

            //Get all field values for ItemInventoryRet aggregate 
            //XmlNode ItemInventoryRet = OR.SelectSingleNode("./ItemInventoryRet");
            if (OR.Name == "ItemInventoryRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of FullName
                string FullName = OR.SelectSingleNode("./FullName").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, FullName, IsActive, "ItemInventory");
                db.SaveChanges();

                /*
                //Get all field values for ClassRef aggregate 
                XmlNode ClassRef = OR.SelectSingleNode("./ItemInventoryRet/ClassRef");
                if (ClassRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryRet/ClassRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryRet/ClassRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryRet/ClassRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryRet/ClassRef/FullName").InnerText;

                    }

                }
                //Done with field values for ClassRef aggregate

                //Get all field values for ParentRef aggregate 
                XmlNode ParentRef = OR.SelectSingleNode("./ItemInventoryRet/ParentRef");
                if (ParentRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryRet/ParentRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryRet/ParentRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryRet/ParentRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryRet/ParentRef/FullName").InnerText;

                    }

                }
                //Done with field values for ParentRef aggregate

                //Get value of Sublevel
                string Sublevel = OR.SelectSingleNode("./ItemInventoryRet/Sublevel").InnerText;
                //Get value of ManufacturerPartNumber
                if (OR.SelectSingleNode("./ItemInventoryRet/ManufacturerPartNumber") != null)
                {
                    string ManufacturerPartNumber = OR.SelectSingleNode("./ItemInventoryRet/ManufacturerPartNumber").InnerText;

                }
                //Get all field values for UnitOfMeasureSetRef aggregate 
                XmlNode UnitOfMeasureSetRef = OR.SelectSingleNode("./ItemInventoryRet/UnitOfMeasureSetRef");
                if (UnitOfMeasureSetRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryRet/UnitOfMeasureSetRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryRet/UnitOfMeasureSetRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryRet/UnitOfMeasureSetRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryRet/UnitOfMeasureSetRef/FullName").InnerText;

                    }

                }
                //Done with field values for UnitOfMeasureSetRef aggregate

                //Get all field values for SalesTaxCodeRef aggregate 
                XmlNode SalesTaxCodeRef = OR.SelectSingleNode("./ItemInventoryRet/SalesTaxCodeRef");
                if (SalesTaxCodeRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryRet/SalesTaxCodeRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryRet/SalesTaxCodeRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryRet/SalesTaxCodeRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryRet/SalesTaxCodeRef/FullName").InnerText;

                    }

                }
                //Done with field values for SalesTaxCodeRef aggregate

                //Get value of SalesDesc
                if (OR.SelectSingleNode("./ItemInventoryRet/SalesDesc") != null)
                {
                    string SalesDesc = OR.SelectSingleNode("./ItemInventoryRet/SalesDesc").InnerText;

                }
                //Get value of SalesPrice
                if (OR.SelectSingleNode("./ItemInventoryRet/SalesPrice") != null)
                {
                    string SalesPrice = OR.SelectSingleNode("./ItemInventoryRet/SalesPrice").InnerText;

                }
                //Get all field values for IncomeAccountRef aggregate 
                XmlNode IncomeAccountRef = OR.SelectSingleNode("./ItemInventoryRet/IncomeAccountRef");
                if (IncomeAccountRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryRet/IncomeAccountRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryRet/IncomeAccountRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryRet/IncomeAccountRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryRet/IncomeAccountRef/FullName").InnerText;

                    }

                }
                //Done with field values for IncomeAccountRef aggregate

                //Get value of PurchaseDesc
                if (OR.SelectSingleNode("./ItemInventoryRet/PurchaseDesc") != null)
                {
                    string PurchaseDesc = OR.SelectSingleNode("./ItemInventoryRet/PurchaseDesc").InnerText;

                }
                //Get value of PurchaseCost
                if (OR.SelectSingleNode("./ItemInventoryRet/PurchaseCost") != null)
                {
                    string PurchaseCost = OR.SelectSingleNode("./ItemInventoryRet/PurchaseCost").InnerText;

                }
                //Get all field values for COGSAccountRef aggregate 
                XmlNode COGSAccountRef = OR.SelectSingleNode("./ItemInventoryRet/COGSAccountRef");
                if (COGSAccountRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryRet/COGSAccountRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryRet/COGSAccountRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryRet/COGSAccountRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryRet/COGSAccountRef/FullName").InnerText;

                    }

                }
                //Done with field values for COGSAccountRef aggregate

                //Get all field values for PrefVendorRef aggregate 
                XmlNode PrefVendorRef = OR.SelectSingleNode("./ItemInventoryRet/PrefVendorRef");
                if (PrefVendorRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryRet/PrefVendorRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryRet/PrefVendorRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryRet/PrefVendorRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryRet/PrefVendorRef/FullName").InnerText;

                    }

                }
                //Done with field values for PrefVendorRef aggregate

                //Get all field values for AssetAccountRef aggregate 
                XmlNode AssetAccountRef = OR.SelectSingleNode("./ItemInventoryRet/AssetAccountRef");
                if (AssetAccountRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryRet/AssetAccountRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryRet/AssetAccountRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryRet/AssetAccountRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryRet/AssetAccountRef/FullName").InnerText;

                    }

                }
                //Done with field values for AssetAccountRef aggregate

                //Get value of ReorderPoint
                if (OR.SelectSingleNode("./ItemInventoryRet/ReorderPoint") != null)
                {
                    string ReorderPoint = OR.SelectSingleNode("./ItemInventoryRet/ReorderPoint").InnerText;

                }
                //Get value of QuantityOnHand
                if (OR.SelectSingleNode("./ItemInventoryRet/QuantityOnHand") != null)
                {
                    string QuantityOnHand = OR.SelectSingleNode("./ItemInventoryRet/QuantityOnHand").InnerText;

                }
                //Get value of AverageCost
                if (OR.SelectSingleNode("./ItemInventoryRet/AverageCost") != null)
                {
                    string AverageCost = OR.SelectSingleNode("./ItemInventoryRet/AverageCost").InnerText;

                }
                //Get value of QuantityOnOrder
                if (OR.SelectSingleNode("./ItemInventoryRet/QuantityOnOrder") != null)
                {
                    string QuantityOnOrder = OR.SelectSingleNode("./ItemInventoryRet/QuantityOnOrder").InnerText;

                }
                //Get value of QuantityOnSalesOrder
                if (OR.SelectSingleNode("./ItemInventoryRet/QuantityOnSalesOrder") != null)
                {
                    string QuantityOnSalesOrder = OR.SelectSingleNode("./ItemInventoryRet/QuantityOnSalesOrder").InnerText;

                }
                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemInventoryRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemInventoryRet/ExternalGUID").InnerText;

                }
                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemInventoryRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    }

                }
                */

            }
            //Done with field values for ItemInventoryRet aggregate

            //Get all field values for ItemInventoryAssemblyRet aggregate 
            //XmlNode ItemInventoryAssemblyRet = OR.SelectSingleNode("./ItemInventoryAssemblyRet");
            if (OR.Name == "ItemInventoryAssemblyRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of FullName
                string FullName = OR.SelectSingleNode("./FullName").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, FullName, IsActive, "ItemInventoryAssembly");
                db.SaveChanges();

                /*
                //Get all field values for ClassRef aggregate 
                XmlNode ClassRef = OR.SelectSingleNode("./ItemInventoryAssemblyRet/ClassRef");
                if (ClassRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/ClassRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryAssemblyRet/ClassRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/ClassRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryAssemblyRet/ClassRef/FullName").InnerText;

                    }

                }
                //Done with field values for ClassRef aggregate

                //Get all field values for ParentRef aggregate 
                XmlNode ParentRef = OR.SelectSingleNode("./ItemInventoryAssemblyRet/ParentRef");
                if (ParentRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/ParentRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryAssemblyRet/ParentRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/ParentRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryAssemblyRet/ParentRef/FullName").InnerText;

                    }

                }
                //Done with field values for ParentRef aggregate

                //Get value of Sublevel
                string Sublevel = OR.SelectSingleNode("./ItemInventoryAssemblyRet/Sublevel").InnerText;
                //Get value of ManufacturerPartNumber
                if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/ManufacturerPartNumber") != null)
                {
                    string ManufacturerPartNumber = OR.SelectSingleNode("./ItemInventoryAssemblyRet/ManufacturerPartNumber").InnerText;

                }
                //Get all field values for UnitOfMeasureSetRef aggregate 
                XmlNode UnitOfMeasureSetRef = OR.SelectSingleNode("./ItemInventoryAssemblyRet/UnitOfMeasureSetRef");
                if (UnitOfMeasureSetRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/UnitOfMeasureSetRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryAssemblyRet/UnitOfMeasureSetRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/UnitOfMeasureSetRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryAssemblyRet/UnitOfMeasureSetRef/FullName").InnerText;

                    }

                }
                //Done with field values for UnitOfMeasureSetRef aggregate

                //Get all field values for SalesTaxCodeRef aggregate 
                XmlNode SalesTaxCodeRef = OR.SelectSingleNode("./ItemInventoryAssemblyRet/SalesTaxCodeRef");
                if (SalesTaxCodeRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/SalesTaxCodeRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryAssemblyRet/SalesTaxCodeRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/SalesTaxCodeRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryAssemblyRet/SalesTaxCodeRef/FullName").InnerText;

                    }

                }
                //Done with field values for SalesTaxCodeRef aggregate

                //Get value of SalesDesc
                if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/SalesDesc") != null)
                {
                    string SalesDesc = OR.SelectSingleNode("./ItemInventoryAssemblyRet/SalesDesc").InnerText;

                }
                //Get value of SalesPrice
                if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/SalesPrice") != null)
                {
                    string SalesPrice = OR.SelectSingleNode("./ItemInventoryAssemblyRet/SalesPrice").InnerText;

                }
                //Get all field values for IncomeAccountRef aggregate 
                XmlNode IncomeAccountRef = OR.SelectSingleNode("./ItemInventoryAssemblyRet/IncomeAccountRef");
                if (IncomeAccountRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/IncomeAccountRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryAssemblyRet/IncomeAccountRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/IncomeAccountRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryAssemblyRet/IncomeAccountRef/FullName").InnerText;

                    }

                }
                //Done with field values for IncomeAccountRef aggregate

                //Get value of PurchaseDesc
                if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/PurchaseDesc") != null)
                {
                    string PurchaseDesc = OR.SelectSingleNode("./ItemInventoryAssemblyRet/PurchaseDesc").InnerText;

                }
                //Get value of PurchaseCost
                if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/PurchaseCost") != null)
                {
                    string PurchaseCost = OR.SelectSingleNode("./ItemInventoryAssemblyRet/PurchaseCost").InnerText;

                }
                //Get all field values for COGSAccountRef aggregate 
                XmlNode COGSAccountRef = OR.SelectSingleNode("./ItemInventoryAssemblyRet/COGSAccountRef");
                if (COGSAccountRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/COGSAccountRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryAssemblyRet/COGSAccountRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/COGSAccountRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryAssemblyRet/COGSAccountRef/FullName").InnerText;

                    }

                }
                //Done with field values for COGSAccountRef aggregate

                //Get all field values for PrefVendorRef aggregate 
                XmlNode PrefVendorRef = OR.SelectSingleNode("./ItemInventoryAssemblyRet/PrefVendorRef");
                if (PrefVendorRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/PrefVendorRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryAssemblyRet/PrefVendorRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/PrefVendorRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryAssemblyRet/PrefVendorRef/FullName").InnerText;

                    }

                }
                //Done with field values for PrefVendorRef aggregate

                //Get all field values for AssetAccountRef aggregate 
                XmlNode AssetAccountRef = OR.SelectSingleNode("./ItemInventoryAssemblyRet/AssetAccountRef");
                if (AssetAccountRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/AssetAccountRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemInventoryAssemblyRet/AssetAccountRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/AssetAccountRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemInventoryAssemblyRet/AssetAccountRef/FullName").InnerText;

                    }

                }
                //Done with field values for AssetAccountRef aggregate

                //Get value of BuildPoint
                if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/BuildPoint") != null)
                {
                    string BuildPoint = OR.SelectSingleNode("./ItemInventoryAssemblyRet/BuildPoint").InnerText;

                }
                //Get value of QuantityOnHand
                if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/QuantityOnHand") != null)
                {
                    string QuantityOnHand = OR.SelectSingleNode("./ItemInventoryAssemblyRet/QuantityOnHand").InnerText;

                }
                //Get value of AverageCost
                if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/AverageCost") != null)
                {
                    string AverageCost = OR.SelectSingleNode("./ItemInventoryAssemblyRet/AverageCost").InnerText;

                }
                //Get value of QuantityOnOrder
                if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/QuantityOnOrder") != null)
                {
                    string QuantityOnOrder = OR.SelectSingleNode("./ItemInventoryAssemblyRet/QuantityOnOrder").InnerText;

                }
                //Get value of QuantityOnSalesOrder
                if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/QuantityOnSalesOrder") != null)
                {
                    string QuantityOnSalesOrder = OR.SelectSingleNode("./ItemInventoryAssemblyRet/QuantityOnSalesOrder").InnerText;

                }
                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemInventoryAssemblyRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemInventoryAssemblyRet/ExternalGUID").InnerText;

                }
                //Walk list of ItemInventoryAssemblyLine aggregates
                XmlNodeList ItemInventoryAssemblyLineList = OR.SelectNodes("./ItemInventoryAssemblyRet/ItemInventoryAssemblyLine");
                if (ItemInventoryAssemblyLineList != null)
                {
                    for (int i = 0; i < ItemInventoryAssemblyLineList.Count; i++)
                    {
                        XmlNode ItemInventoryAssemblyLine = ItemInventoryAssemblyLineList.Item(i);
                        //Get all field values for ItemInventoryRef aggregate 
                        //Get value of ListID
                        if (ItemInventoryAssemblyLine.SelectSingleNode("./ItemInventoryRef/ListID") != null)
                        {
                            string ListID = ItemInventoryAssemblyLine.SelectSingleNode("./ItemInventoryRef/ListID").InnerText;

                        }
                        //Get value of FullName
                        if (ItemInventoryAssemblyLine.SelectSingleNode("./ItemInventoryRef/FullName") != null)
                        {
                            string FullName = ItemInventoryAssemblyLine.SelectSingleNode("./ItemInventoryRef/FullName").InnerText;

                        }
                        //Done with field values for ItemInventoryRef aggregate

                        //Get value of Quantity
                        if (ItemInventoryAssemblyLine.SelectSingleNode("./Quantity") != null)
                        {
                            string Quantity = ItemInventoryAssemblyLine.SelectSingleNode("./Quantity").InnerText;

                        }

                    }

                }

                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemInventoryAssemblyRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    }

                }

                */
            }
            //Done with field values for ItemInventoryAssemblyRet aggregate

            //Get all field values for ItemFixedAssetRet aggregate 
            //XmlNode ItemFixedAssetRet = OR.SelectSingleNode("./ItemFixedAssetRet");
            if (OR.Name == "ItemFixedAssetRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, Name, IsActive, "ItemFixedAsset");
                db.SaveChanges();

                /*
                //Get all field values for ClassRef aggregate 
                XmlNode ClassRef = OR.SelectSingleNode("./ItemFixedAssetRet/ClassRef");
                if (ClassRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemFixedAssetRet/ClassRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemFixedAssetRet/ClassRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemFixedAssetRet/ClassRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemFixedAssetRet/ClassRef/FullName").InnerText;

                    }

                }
                //Done with field values for ClassRef aggregate

                //Get value of AcquiredAs
                string AcquiredAs = OR.SelectSingleNode("./ItemFixedAssetRet/AcquiredAs").InnerText;
                //Get value of PurchaseDesc
                string PurchaseDesc = OR.SelectSingleNode("./ItemFixedAssetRet/PurchaseDesc").InnerText;
                //Get value of PurchaseDate
                string PurchaseDate = OR.SelectSingleNode("./ItemFixedAssetRet/PurchaseDate").InnerText;
                //Get value of PurchaseCost
                if (OR.SelectSingleNode("./ItemFixedAssetRet/PurchaseCost") != null)
                {
                    string PurchaseCost = OR.SelectSingleNode("./ItemFixedAssetRet/PurchaseCost").InnerText;

                }
                //Get value of VendorOrPayeeName
                if (OR.SelectSingleNode("./ItemFixedAssetRet/VendorOrPayeeName") != null)
                {
                    string VendorOrPayeeName = OR.SelectSingleNode("./ItemFixedAssetRet/VendorOrPayeeName").InnerText;

                }
                //Get all field values for AssetAccountRef aggregate 
                XmlNode AssetAccountRef = OR.SelectSingleNode("./ItemFixedAssetRet/AssetAccountRef");
                if (AssetAccountRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemFixedAssetRet/AssetAccountRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemFixedAssetRet/AssetAccountRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemFixedAssetRet/AssetAccountRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemFixedAssetRet/AssetAccountRef/FullName").InnerText;

                    }

                }
                //Done with field values for AssetAccountRef aggregate

                //Get all field values for FixedAssetSalesInfo aggregate 
                XmlNode FixedAssetSalesInfo = OR.SelectSingleNode("./ItemFixedAssetRet/FixedAssetSalesInfo");
                if (FixedAssetSalesInfo != null)
                {
                    //Get value of SalesDesc
                    string SalesDesc = OR.SelectSingleNode("./ItemFixedAssetRet/FixedAssetSalesInfo/SalesDesc").InnerText;
                    //Get value of SalesDate
                    string SalesDate = OR.SelectSingleNode("./ItemFixedAssetRet/FixedAssetSalesInfo/SalesDate").InnerText;
                    //Get value of SalesPrice
                    if (OR.SelectSingleNode("./ItemFixedAssetRet/FixedAssetSalesInfo/SalesPrice") != null)
                    {
                        string SalesPrice = OR.SelectSingleNode("./ItemFixedAssetRet/FixedAssetSalesInfo/SalesPrice").InnerText;

                    }
                    //Get value of SalesExpense
                    if (OR.SelectSingleNode("./ItemFixedAssetRet/FixedAssetSalesInfo/SalesExpense") != null)
                    {
                        string SalesExpense = OR.SelectSingleNode("./ItemFixedAssetRet/FixedAssetSalesInfo/SalesExpense").InnerText;

                    }

                }
                //Done with field values for FixedAssetSalesInfo aggregate

                //Get value of AssetDesc
                if (OR.SelectSingleNode("./ItemFixedAssetRet/AssetDesc") != null)
                {
                    string AssetDesc = OR.SelectSingleNode("./ItemFixedAssetRet/AssetDesc").InnerText;

                }
                //Get value of Location
                if (OR.SelectSingleNode("./ItemFixedAssetRet/Location") != null)
                {
                    string Location = OR.SelectSingleNode("./ItemFixedAssetRet/Location").InnerText;

                }
                //Get value of PONumber
                if (OR.SelectSingleNode("./ItemFixedAssetRet/PONumber") != null)
                {
                    string PONumber = OR.SelectSingleNode("./ItemFixedAssetRet/PONumber").InnerText;

                }
                //Get value of SerialNumber
                if (OR.SelectSingleNode("./ItemFixedAssetRet/SerialNumber") != null)
                {
                    string SerialNumber = OR.SelectSingleNode("./ItemFixedAssetRet/SerialNumber").InnerText;

                }
                //Get value of WarrantyExpDate
                if (OR.SelectSingleNode("./ItemFixedAssetRet/WarrantyExpDate") != null)
                {
                    string WarrantyExpDate = OR.SelectSingleNode("./ItemFixedAssetRet/WarrantyExpDate").InnerText;

                }
                //Get value of Notes
                if (OR.SelectSingleNode("./ItemFixedAssetRet/Notes") != null)
                {
                    string Notes = OR.SelectSingleNode("./ItemFixedAssetRet/Notes").InnerText;

                }
                //Get value of AssetNumber
                if (OR.SelectSingleNode("./ItemFixedAssetRet/AssetNumber") != null)
                {
                    string AssetNumber = OR.SelectSingleNode("./ItemFixedAssetRet/AssetNumber").InnerText;

                }
                //Get value of CostBasis
                if (OR.SelectSingleNode("./ItemFixedAssetRet/CostBasis") != null)
                {
                    string CostBasis = OR.SelectSingleNode("./ItemFixedAssetRet/CostBasis").InnerText;

                }
                //Get value of YearEndAccumulatedDepreciation
                if (OR.SelectSingleNode("./ItemFixedAssetRet/YearEndAccumulatedDepreciation") != null)
                {
                    string YearEndAccumulatedDepreciation = OR.SelectSingleNode("./ItemFixedAssetRet/YearEndAccumulatedDepreciation").InnerText;

                }
                //Get value of YearEndBookValue
                if (OR.SelectSingleNode("./ItemFixedAssetRet/YearEndBookValue") != null)
                {
                    string YearEndBookValue = OR.SelectSingleNode("./ItemFixedAssetRet/YearEndBookValue").InnerText;

                }
                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemFixedAssetRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemFixedAssetRet/ExternalGUID").InnerText;

                }
                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemFixedAssetRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    }

                }
                */

            }
            //Done with field values for ItemFixedAssetRet aggregate

            //Get all field values for ItemSubtotalRet aggregate 
            //XmlNode ItemSubtotalRet = OR.SelectSingleNode("./ItemSubtotalRet");
            if (OR.Name == "ItemSubtotalRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, Name, IsActive, "ItemSubtotal");
                db.SaveChanges();

                /*
                //Get value of ItemDesc
                if (OR.SelectSingleNode("./ItemSubtotalRet/ItemDesc") != null)
                {
                    string ItemDesc = OR.SelectSingleNode("./ItemSubtotalRet/ItemDesc").InnerText;

                }
                //Get value of SpecialItemType
                if (OR.SelectSingleNode("./ItemSubtotalRet/SpecialItemType") != null)
                {
                    string SpecialItemType = OR.SelectSingleNode("./ItemSubtotalRet/SpecialItemType").InnerText;

                }
                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemSubtotalRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemSubtotalRet/ExternalGUID").InnerText;

                }
                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemSubtotalRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    }

                }
                */

            }
            //Done with field values for ItemSubtotalRet aggregate

            //Get all field values for ItemDiscountRet aggregate 
            //XmlNode ItemDiscountRet = OR.SelectSingleNode("./ItemDiscountRet");
            if (OR.Name == "ItemDiscountRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of FullName
                string FullName = OR.SelectSingleNode("./FullName").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, FullName, IsActive, "ItemDiscount");
                db.SaveChanges();

                /*
                //Get all field values for ClassRef aggregate 
                XmlNode ClassRef = OR.SelectSingleNode("./ItemDiscountRet/ClassRef");
                if (ClassRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemDiscountRet/ClassRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemDiscountRet/ClassRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemDiscountRet/ClassRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemDiscountRet/ClassRef/FullName").InnerText;

                    }

                }
                //Done with field values for ClassRef aggregate

                //Get all field values for ParentRef aggregate 
                XmlNode ParentRef = OR.SelectSingleNode("./ItemDiscountRet/ParentRef");
                if (ParentRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemDiscountRet/ParentRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemDiscountRet/ParentRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemDiscountRet/ParentRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemDiscountRet/ParentRef/FullName").InnerText;

                    }

                }
                //Done with field values for ParentRef aggregate

                //Get value of Sublevel
                string Sublevel = OR.SelectSingleNode("./ItemDiscountRet/Sublevel").InnerText;
                //Get value of ItemDesc
                if (OR.SelectSingleNode("./ItemDiscountRet/ItemDesc") != null)
                {
                    string ItemDesc = OR.SelectSingleNode("./ItemDiscountRet/ItemDesc").InnerText;

                }
                //Get all field values for SalesTaxCodeRef aggregate 
                XmlNode SalesTaxCodeRef = OR.SelectSingleNode("./ItemDiscountRet/SalesTaxCodeRef");
                if (SalesTaxCodeRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemDiscountRet/SalesTaxCodeRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemDiscountRet/SalesTaxCodeRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemDiscountRet/SalesTaxCodeRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemDiscountRet/SalesTaxCodeRef/FullName").InnerText;

                    }

                }
                //Done with field values for SalesTaxCodeRef aggregate

                XmlNodeList ORDiscountRateChildren = OR.SelectNodes("./ItemDiscountRet/*");
                for (int i = 0; i < ORDiscountRateChildren.Count; i++)
                {
                    XmlNode Child = ORDiscountRateChildren.Item(i);
                    if (Child.Name == "DiscountRate")
                    {
                    }

                    if (Child.Name == "DiscountRatePercent")
                    {
                    }


                }

                //Get all field values for AccountRef aggregate 
                XmlNode AccountRef = OR.SelectSingleNode("./ItemDiscountRet/AccountRef");
                if (AccountRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemDiscountRet/AccountRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemDiscountRet/AccountRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemDiscountRet/AccountRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemDiscountRet/AccountRef/FullName").InnerText;

                    }

                }
                //Done with field values for AccountRef aggregate

                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemDiscountRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemDiscountRet/ExternalGUID").InnerText;

                }
                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemDiscountRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    }

                }

                */
            }
            //Done with field values for ItemDiscountRet aggregate

            //Get all field values for ItemPaymentRet aggregate 
            //XmlNode ItemPaymentRet = OR.SelectSingleNode("./ItemPaymentRet");
            if (OR.Name == "ItemPaymentRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, Name, IsActive, "ItemPayment");
                db.SaveChanges();

                /*
                //Get all field values for ClassRef aggregate 
                XmlNode ClassRef = OR.SelectSingleNode("./ItemPaymentRet/ClassRef");
                if (ClassRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemPaymentRet/ClassRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemPaymentRet/ClassRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemPaymentRet/ClassRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemPaymentRet/ClassRef/FullName").InnerText;

                    }

                }
                //Done with field values for ClassRef aggregate

                //Get value of ItemDesc
                if (OR.SelectSingleNode("./ItemPaymentRet/ItemDesc") != null)
                {
                    string ItemDesc = OR.SelectSingleNode("./ItemPaymentRet/ItemDesc").InnerText;

                }
                //Get all field values for DepositToAccountRef aggregate 
                XmlNode DepositToAccountRef = OR.SelectSingleNode("./ItemPaymentRet/DepositToAccountRef");
                if (DepositToAccountRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemPaymentRet/DepositToAccountRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemPaymentRet/DepositToAccountRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemPaymentRet/DepositToAccountRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemPaymentRet/DepositToAccountRef/FullName").InnerText;

                    }

                }
                //Done with field values for DepositToAccountRef aggregate

                //Get all field values for PaymentMethodRef aggregate 
                XmlNode PaymentMethodRef = OR.SelectSingleNode("./ItemPaymentRet/PaymentMethodRef");
                if (PaymentMethodRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemPaymentRet/PaymentMethodRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemPaymentRet/PaymentMethodRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemPaymentRet/PaymentMethodRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemPaymentRet/PaymentMethodRef/FullName").InnerText;

                    }

                }
                //Done with field values for PaymentMethodRef aggregate

                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemPaymentRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemPaymentRet/ExternalGUID").InnerText;

                }
                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemPaymentRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    }

                }

                */
            }
            //Done with field values for ItemPaymentRet aggregate

            //Get all field values for ItemSalesTaxRet aggregate 
            //XmlNode ItemSalesTaxRet = OR.SelectSingleNode("./ItemSalesTaxRet");
            if (OR.Name == "ItemSalesTaxRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, Name, IsActive, "ItemSalesTax");
                db.SaveChanges();

                /*
                //Get all field values for ClassRef aggregate 
                XmlNode ClassRef = OR.SelectSingleNode("./ItemSalesTaxRet/ClassRef");
                if (ClassRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemSalesTaxRet/ClassRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemSalesTaxRet/ClassRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemSalesTaxRet/ClassRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemSalesTaxRet/ClassRef/FullName").InnerText;

                    }

                }
                //Done with field values for ClassRef aggregate

                //Get value of ItemDesc
                if (OR.SelectSingleNode("./ItemSalesTaxRet/ItemDesc") != null)
                {
                    string ItemDesc = OR.SelectSingleNode("./ItemSalesTaxRet/ItemDesc").InnerText;

                }
                //Get value of TaxRate
                if (OR.SelectSingleNode("./ItemSalesTaxRet/TaxRate") != null)
                {
                    string TaxRate = OR.SelectSingleNode("./ItemSalesTaxRet/TaxRate").InnerText;

                }
                //Get all field values for TaxVendorRef aggregate 
                XmlNode TaxVendorRef = OR.SelectSingleNode("./ItemSalesTaxRet/TaxVendorRef");
                if (TaxVendorRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemSalesTaxRet/TaxVendorRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemSalesTaxRet/TaxVendorRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemSalesTaxRet/TaxVendorRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemSalesTaxRet/TaxVendorRef/FullName").InnerText;

                    }

                }
                //Done with field values for TaxVendorRef aggregate

                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemSalesTaxRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemSalesTaxRet/ExternalGUID").InnerText;

                }
                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemSalesTaxRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    }

                }

                */
            }
            //Done with field values for ItemSalesTaxRet aggregate

            //Get all field values for ItemSalesTaxGroupRet aggregate 
            //XmlNode ItemSalesTaxGroupRet = OR.SelectSingleNode("./ItemSalesTaxGroupRet");
            if (OR.Name == "ItemSalesTaxGroupRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, Name, IsActive, "ItemSalesTaxGroup");
                db.SaveChanges();

                /*
                //Get value of ItemDesc
                if (OR.SelectSingleNode("./ItemSalesTaxGroupRet/ItemDesc") != null)
                {
                    string ItemDesc = OR.SelectSingleNode("./ItemSalesTaxGroupRet/ItemDesc").InnerText;

                }
                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemSalesTaxGroupRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemSalesTaxGroupRet/ExternalGUID").InnerText;

                }
                //Walk list of ItemSalesTaxRef aggregates
                XmlNodeList ItemSalesTaxRefList = OR.SelectNodes("./ItemSalesTaxGroupRet/ItemSalesTaxRef");
                if (ItemSalesTaxRefList != null)
                {
                    for (int i = 0; i < ItemSalesTaxRefList.Count; i++)
                    {
                        XmlNode ItemSalesTaxRef = ItemSalesTaxRefList.Item(i);
                        //Get value of ListID
                        if (ItemSalesTaxRef.SelectSingleNode("./ListID") != null)
                        {
                            string ListID = ItemSalesTaxRef.SelectSingleNode("./ListID").InnerText;

                        }
                        //Get value of FullName
                        if (ItemSalesTaxRef.SelectSingleNode("./FullName") != null)
                        {
                            string FullName = ItemSalesTaxRef.SelectSingleNode("./FullName").InnerText;

                        }

                    }

                }

                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemSalesTaxGroupRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;

                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;

                    }

                }

                */
            }
            //Done with field values for ItemSalesTaxGroupRet aggregate

            //Get all field values for ItemGroupRet aggregate 
            //XmlNode ItemGroupRet = OR.SelectSingleNode("./ItemGroupRet");
            if (OR.Name == "ItemGroupRet")
            {
                //Get value of ListID
                string ListID = OR.SelectSingleNode("./ListID").InnerText;
                //Get value of TimeCreated
                string TimeCreated = OR.SelectSingleNode("./TimeCreated").InnerText;
                //Get value of TimeModified
                string TimeModified = OR.SelectSingleNode("./TimeModified").InnerText;
                //Get value of EditSequence
                string EditSequence = OR.SelectSingleNode("./EditSequence").InnerText;
                //Get value of Name
                string Name = OR.SelectSingleNode("./Name").InnerText;
                //Get value of BarCodeValue
                if (OR.SelectSingleNode("./BarCodeValue") != null)
                {
                    string BarCodeValue = OR.SelectSingleNode("./BarCodeValue").InnerText;
                }
                //Get value of IsActive
                string IsActive = "false";
                if (OR.SelectSingleNode("./IsActive") != null)
                {
                    IsActive = OR.SelectSingleNode("./IsActive").InnerText;
                }

                Item o = ItemDAL.FindOrCreateItem(db, ListID, EditSequence, TimeCreated, TimeModified, Name, Name, IsActive, "ItemGroup");
                db.SaveChanges();

                /*
                //Get value of ItemDesc
                if (OR.SelectSingleNode("./ItemGroupRet/ItemDesc") != null)
                {
                    string ItemDesc = OR.SelectSingleNode("./ItemGroupRet/ItemDesc").InnerText;

                }
                //Get all field values for UnitOfMeasureSetRef aggregate 
                XmlNode UnitOfMeasureSetRef = OR.SelectSingleNode("./ItemGroupRet/UnitOfMeasureSetRef");
                if (UnitOfMeasureSetRef != null)
                {
                    //Get value of ListID
                    if (OR.SelectSingleNode("./ItemGroupRet/UnitOfMeasureSetRef/ListID") != null)
                    {
                        string ListID = OR.SelectSingleNode("./ItemGroupRet/UnitOfMeasureSetRef/ListID").InnerText;

                    }
                    //Get value of FullName
                    if (OR.SelectSingleNode("./ItemGroupRet/UnitOfMeasureSetRef/FullName") != null)
                    {
                        string FullName = OR.SelectSingleNode("./ItemGroupRet/UnitOfMeasureSetRef/FullName").InnerText;

                    }

                }
                //Done with field values for UnitOfMeasureSetRef aggregate

                //Get value of IsPrintItemsInGroup
                if (OR.SelectSingleNode("./ItemGroupRet/IsPrintItemsInGroup") != null)
                {
                    string IsPrintItemsInGroup = OR.SelectSingleNode("./ItemGroupRet/IsPrintItemsInGroup").InnerText;

                }
                //Get value of SpecialItemType
                if (OR.SelectSingleNode("./ItemGroupRet/SpecialItemType") != null)
                {
                    string SpecialItemType = OR.SelectSingleNode("./ItemGroupRet/SpecialItemType").InnerText;

                }
                //Get value of ExternalGUID
                if (OR.SelectSingleNode("./ItemGroupRet/ExternalGUID") != null)
                {
                    string ExternalGUID = OR.SelectSingleNode("./ItemGroupRet/ExternalGUID").InnerText;

                }
                //Walk list of ItemGroupLine aggregates
                XmlNodeList ItemGroupLineList = OR.SelectNodes("./ItemGroupRet/ItemGroupLine");
                if (ItemGroupLineList != null)
                {
                    for (int i = 0; i < ItemGroupLineList.Count; i++)
                    {
                        XmlNode ItemGroupLine = ItemGroupLineList.Item(i);
                        //Get all field values for ItemRef aggregate 
                        XmlNode ItemRef = ItemGroupLine.SelectSingleNode("./ItemRef");
                        if (ItemRef != null)
                        {
                            //Get value of ListID
                            if (ItemGroupLine.SelectSingleNode("./ItemRef/ListID") != null)
                            {
                                string ListID = ItemGroupLine.SelectSingleNode("./ItemRef/ListID").InnerText;

                            }
                            //Get value of FullName
                            if (ItemGroupLine.SelectSingleNode("./ItemRef/FullName") != null)
                            {
                                string FullName = ItemGroupLine.SelectSingleNode("./ItemRef/FullName").InnerText;

                            }

                        }
                        //Done with field values for ItemRef aggregate

                        //Get value of Quantity
                        if (ItemGroupLine.SelectSingleNode("./Quantity") != null)
                        {
                            string Quantity = ItemGroupLine.SelectSingleNode("./Quantity").InnerText;

                        }
                        //Get value of UnitOfMeasure
                        if (ItemGroupLine.SelectSingleNode("./UnitOfMeasure") != null)
                        {
                            string UnitOfMeasure = ItemGroupLine.SelectSingleNode("./UnitOfMeasure").InnerText;
                        }
                    }
                }

                //Walk list of DataExtRet aggregates
                XmlNodeList DataExtRetList = OR.SelectNodes("./ItemGroupRet/DataExtRet");
                if (DataExtRetList != null)
                {
                    for (int i = 0; i < DataExtRetList.Count; i++)
                    {
                        XmlNode DataExtRet = DataExtRetList.Item(i);
                        //Get value of OwnerID
                        if (DataExtRet.SelectSingleNode("./OwnerID") != null)
                        {
                            string OwnerID = DataExtRet.SelectSingleNode("./OwnerID").InnerText;
                        }
                        //Get value of DataExtName
                        string DataExtName = DataExtRet.SelectSingleNode("./DataExtName").InnerText;
                        //Get value of DataExtType
                        string DataExtType = DataExtRet.SelectSingleNode("./DataExtType").InnerText;
                        //Get value of DataExtValue
                        string DataExtValue = DataExtRet.SelectSingleNode("./DataExtValue").InnerText;
                    }
                }
                */
            }
                        
            //Done with field values for ItemGroupRet aggregate
        }
        
        private static Item FindOrCreateItem(RotoTrackDb db, string QBListId, string QBEditSequence, string TimeCreated, string TimeModified, string Name, string FullName, string IsActive, string ItemType)
        {
            Item o = null;
            if (db.Items.Any(f => f.QBListId == QBListId))
            {
                o = db.Items.First(f => f.QBListId == QBListId);
            }
            else
            {
                o = new Item();
                db.Items.Add(o);
            }

            o.QBListId = QBListId;
            o.QBEditSequence = QBEditSequence;
            DateTime createdDate;
            if (DateTime.TryParse(TimeCreated, out createdDate)) o.TimeCreated = createdDate;
            DateTime modifiedDate;
            if (DateTime.TryParse(TimeModified, out modifiedDate)) o.TimeModified = modifiedDate;
            o.Name = Name;
            o.FullName = FullName;
            o.IsActive = (IsActive == "true") ? true : false;
            o.ItemType = ItemType;

            return o;
        }

    }
}
