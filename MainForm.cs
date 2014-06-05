
/*-----------------------------------------------------------
 * QBMigrationTool :    Tool for migrating data and ad-hoc 
 *                      querying of QB file
 *
 * Description: Tool for migrating data from QB to custom DB
 *              and for ad-hoc querying of the QB file.
 *               
 * Created: 5/27/2014
 *
 * Copyright © 2014 Unified Communications Inc. All rights reserved.
 *
 *----------------------------------------------------------
 */ 
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using Interop.QBXMLRP2;
using System.Collections.Generic;
using rototrack_data_access;
using rototrack_model;
using System.Linq;
using System.Timers;
using zochnet_utils;

namespace QBMigrationTool
{
	/// <summary>
	/// CustomerAddForm shows how to invoke QuickBooks qbXMLRP COM object
	/// It uses .NET to create qbXML request and parse qbXML response
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
    {
        #region Member Data
        private System.Timers.Timer aTimer;
		private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button Exit;
        private TextBox tbResults;
        private Button btnGetQBListID;
        private TextBox tbWorkOrderNumber;
        private Label label2;
        private Button btnRunXml;
        private TextBox tbXML;
        private Label label4;
        private Button btnAutoSync;
        private Label labelBuildType;
        private NumericUpDown numericUpDownSyncDuration;
        private Label label5;
        private Button btnSyncNow;
        private Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
        #endregion

        #region Main Form Code
        public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
            numericUpDownSyncDuration.Value = 3;
            numericUpDownSyncDuration.Maximum = 30;
            numericUpDownSyncDuration.Minimum = 1;

            labelBuildType.Text = RototrackConfig.GetBuildType();
            //aTimer = new System.Timers.Timer(10000);
            aTimer = new System.Timers.Timer((double)numericUpDownSyncDuration.Value * 100000);
            aTimer.Elapsed += aTimer_Elapsed;
		}
        
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		static void Main() 
		{
			Application.Run(new MainForm());
		}
        #endregion

        #region Windows Form Designer generated code
        /// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.label3 = new System.Windows.Forms.Label();
            this.Exit = new System.Windows.Forms.Button();
            this.tbResults = new System.Windows.Forms.TextBox();
            this.btnGetQBListID = new System.Windows.Forms.Button();
            this.tbWorkOrderNumber = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnRunXml = new System.Windows.Forms.Button();
            this.tbXML = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnAutoSync = new System.Windows.Forms.Button();
            this.labelBuildType = new System.Windows.Forms.Label();
            this.numericUpDownSyncDuration = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.btnSyncNow = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSyncDuration)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(368, 14);
            this.label3.TabIndex = 13;
            this.label3.Text = "Note: You need to have QuickBooks with a company file opened.";
            // 
            // Exit
            // 
            this.Exit.Location = new System.Drawing.Point(669, 12);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(120, 24);
            this.Exit.TabIndex = 12;
            this.Exit.Text = "Exit";
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // tbResults
            // 
            this.tbResults.Location = new System.Drawing.Point(12, 149);
            this.tbResults.Multiline = true;
            this.tbResults.Name = "tbResults";
            this.tbResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbResults.Size = new System.Drawing.Size(774, 633);
            this.tbResults.TabIndex = 15;
            // 
            // btnGetQBListID
            // 
            this.btnGetQBListID.Location = new System.Drawing.Point(302, 65);
            this.btnGetQBListID.Name = "btnGetQBListID";
            this.btnGetQBListID.Size = new System.Drawing.Size(186, 23);
            this.btnGetQBListID.TabIndex = 19;
            this.btnGetQBListID.Text = "Get QBListID And EditSeq";
            this.btnGetQBListID.UseVisualStyleBackColor = true;
            this.btnGetQBListID.Click += new System.EventHandler(this.btnGetQBListID_Click);
            // 
            // tbWorkOrderNumber
            // 
            this.tbWorkOrderNumber.Location = new System.Drawing.Point(58, 67);
            this.tbWorkOrderNumber.Name = "tbWorkOrderNumber";
            this.tbWorkOrderNumber.Size = new System.Drawing.Size(238, 20);
            this.tbWorkOrderNumber.TabIndex = 20;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "WO #";
            // 
            // btnRunXml
            // 
            this.btnRunXml.Location = new System.Drawing.Point(310, 95);
            this.btnRunXml.Name = "btnRunXml";
            this.btnRunXml.Size = new System.Drawing.Size(75, 23);
            this.btnRunXml.TabIndex = 22;
            this.btnRunXml.Text = "Run XML";
            this.btnRunXml.UseVisualStyleBackColor = true;
            this.btnRunXml.Click += new System.EventHandler(this.btnRunXml_Click);
            // 
            // tbXML
            // 
            this.tbXML.Location = new System.Drawing.Point(66, 97);
            this.tbXML.Name = "tbXML";
            this.tbXML.Size = new System.Drawing.Size(238, 20);
            this.tbXML.TabIndex = 23;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "QBXML";
            // 
            // btnAutoSync
            // 
            this.btnAutoSync.Location = new System.Drawing.Point(12, 9);
            this.btnAutoSync.Name = "btnAutoSync";
            this.btnAutoSync.Size = new System.Drawing.Size(102, 23);
            this.btnAutoSync.TabIndex = 28;
            this.btnAutoSync.Text = "Enable Auto Sync";
            this.btnAutoSync.UseVisualStyleBackColor = true;
            this.btnAutoSync.Click += new System.EventHandler(this.btnAutoSync_Click);
            // 
            // labelBuildType
            // 
            this.labelBuildType.AutoSize = true;
            this.labelBuildType.Location = new System.Drawing.Point(717, 132);
            this.labelBuildType.Name = "labelBuildType";
            this.labelBuildType.Size = new System.Drawing.Size(69, 13);
            this.labelBuildType.TabIndex = 29;
            this.labelBuildType.Text = "<Build Type>";
            // 
            // numericUpDownSyncDuration
            // 
            this.numericUpDownSyncDuration.Location = new System.Drawing.Point(120, 12);
            this.numericUpDownSyncDuration.Name = "numericUpDownSyncDuration";
            this.numericUpDownSyncDuration.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownSyncDuration.TabIndex = 30;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(246, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(144, 13);
            this.label5.TabIndex = 31;
            this.label5.Text = "Auto Sync Duration (minutes)";
            // 
            // btnSyncNow
            // 
            this.btnSyncNow.Location = new System.Drawing.Point(12, 38);
            this.btnSyncNow.Name = "btnSyncNow";
            this.btnSyncNow.Size = new System.Drawing.Size(75, 23);
            this.btnSyncNow.TabIndex = 32;
            this.btnSyncNow.Text = "Sync Now";
            this.btnSyncNow.UseVisualStyleBackColor = true;
            this.btnSyncNow.Click += new System.EventHandler(this.btn_SyncNow_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(647, 132);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 14);
            this.label1.TabIndex = 33;
            this.label1.Text = "Build Type: ";
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(801, 794);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSyncNow);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numericUpDownSyncDuration);
            this.Controls.Add(this.labelBuildType);
            this.Controls.Add(this.btnAutoSync);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbXML);
            this.Controls.Add(this.btnRunXml);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbWorkOrderNumber);
            this.Controls.Add(this.btnGetQBListID);
            this.Controls.Add(this.tbResults);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Exit);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QuickBooks Migration Tool";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSyncDuration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
        
        #region Helper Methods for updating GUI
        private void ClearStatus()
        {
            this.Invoke((MethodInvoker)delegate
            {
                tbResults.Clear();
            });
            this.Invoke((MethodInvoker)delegate
            {
                tbResults.Refresh();
            });
        }

        private void SetStatus(string value)
        {
            this.Invoke((MethodInvoker)delegate
            {
                tbResults.Text = value;
            });
        }

        private void AppendStatus(string value)
        {
            this.Invoke((MethodInvoker)delegate
            {
                tbResults.AppendText(value);
            });
        }
        #endregion
        
        #region Sync Helper Methods
        private void AddCustomerType(int biID)
        {
            RotoTrackDb db = new RotoTrackDb();
            BillingInstruction bi = db.BillingInstructions.Find(biID);

            XmlDocument doc = CustomerTypeDAL.BuildAddRq(bi);
            string response = QBUtils.DoRequest(doc);
            CustomerTypeDAL.HandleAddResponse(response);
        }

        private void AddWorkOrder(int woID)
        {
            RotoTrackDb db = new RotoTrackDb();
            WorkOrder wo = db.WorkOrders.Find(woID);

            XmlDocument doc = WorkOrderDAL.BuildAddRq(wo);
            string response = QBUtils.DoRequest(doc);
            WorkOrderDAL.HandleAddResponse(response);
        }

        private void UpdateWorkOrderEditSequence(int woID)
        {
            RotoTrackDb db = new RotoTrackDb();
            WorkOrder wo = db.WorkOrders.Find(woID);

            string ownerID = "0";
            XmlDocument doc = WorkOrderDAL.BuildQueryRequestForUpdate(wo.QBListId, ownerID);
            string response = QBUtils.DoRequest(doc);
            WorkOrderDAL.HandleResponseForUpdate(response);
        }

        private void UpdateWorkOrder(int woID)
        {
            RotoTrackDb db = new RotoTrackDb();
            WorkOrder wo = db.WorkOrders.Find(woID);

            XmlDocument doc = WorkOrderDAL.BuildModRq(wo);
            string response = QBUtils.DoRequest(doc);
            WorkOrderDAL.HandleModResponse(response);
        }

        private void UpdateSite(int woID)
        {
            RotoTrackDb db = new RotoTrackDb();
            WorkOrder wo = db.WorkOrders.Find(woID);

            XmlDocument doc = SiteDAL.BuildUpdateRq(wo);
            string response = QBUtils.DoRequest(doc);
        }

        private void AddVehicleMileage(int seID, int sdID)
        {
            RotoTrackDb db = new RotoTrackDb();
            ServiceEntry se = db.ServiceEntries.Find(seID);
            ServiceDetail sd = db.ServiceDetails.Find(sdID);

            XmlDocument doc = VehicleMileageDAL.BuildAddRq(se, sd);
            if (doc != null)
            {
                string response = QBUtils.DoRequest(doc);
                VehicleMileageDAL.HandleAddResponse(response);
            }
        }

        private void AddTimeTracking(int seID, int sdID, int serviceId, decimal hours)
        {
            RotoTrackDb db = new RotoTrackDb();
            ServiceEntry se = db.ServiceEntries.Find(seID);
            ServiceDetail sd = db.ServiceDetails.Find(sdID);

            XmlDocument doc = TimeTrackingDAL.BuildAddRq(se, sd, serviceId, hours);
            if (doc != null)
            {
                string response = QBUtils.DoRequest(doc);
                TimeTrackingDAL.HandleAddResponse(response);
            }
        }

        private string SyncDataHelper(XmlDocument doc, string EntityName)
        {
            AppendStatus("Query " + EntityName + "...");
            AppendStatus(Environment.NewLine);
            string response = QBUtils.DoRequest(doc);
            AppendStatus("Processing " + EntityName + "...");
            AppendStatus(Environment.NewLine);
            return response;
        }

        private int GetNumUnsyncedWorkOrders()
        {
            RotoTrackDb db = new RotoTrackDb();

            int count = 0;
            List<WorkOrder> workorders = db.WorkOrders.SqlQuery("select * from WorkOrders where NeedToUpdateQB=1").ToList();
            count += workorders.Count();

            workorders = db.WorkOrders.SqlQuery("select * from WorkOrders where QBListId is null").ToList();
            count += workorders.Count();

            return count;
        }

        private int GetNumUnsyncedDSRs()
        {
            RotoTrackDb db = new RotoTrackDb();

            var dsrs = db.DSRs.Where(f => f.IsSynchronizedWithQB == true).ToList();
            var serviceentries = db.ServiceEntries.Where(f => f.QBListIdForMileage == null || f.QBListIdForRegularHours == null || f.QBListIdForOTHours == null).ToList();

            var unsynced = dsrs.AsQueryable()
                        .Join(serviceentries, d => d.Id, s => s.DSRId, (d, s) => new
                        {
                            d.Id
                        });

            return unsynced.Count();
        }
        #endregion

        #region Main Sync Functions
        private void DoSync()
        {
            ClearStatus();
            SetStatus("");
            SyncWorkOrders();
            SyncDSRs();

            //int numUnsyncedWorkOrders = GetNumUnsyncedWorkOrders();
            //int numUnsyncedDSRs = GetNumUnsyncedDSRs();
            //if (numUnsyncedWorkOrders > 0)
            //{
            //    Logging.RototrackErrorLog("There are " + numUnsyncedWorkOrders.ToString() + " Work Orders that failed to sync with QB!");
            //}
            //if (numUnsyncedDSRs > 0)
            //{
            //    Logging.RototrackErrorLog("There are " + numUnsyncedDSRs.ToString() + " DSRs that failed to sync with QB!");
            //}

            SyncQBData();
        }

        private void SyncWorkOrders()
        {
            RotoTrackDb db = new RotoTrackDb();
            List<WorkOrder> woList = null;

            AppendStatus("Syncing Work Orders...");                    

            // First, all new CustomerTypes (Billing Instructions)
            List<BillingInstruction> biList = db.BillingInstructions.Where(bi => bi.QBListId == null).ToList();
            foreach (BillingInstruction bi in biList)
            {                
                AddCustomerType(bi.Id);

                // Find and add workorders that need to be added that referred to this
                woList = db.WorkOrders.Where(wo => wo.QBListId == null && wo.BillingInstructionsId == bi.Id).ToList();
                foreach (WorkOrder wo in woList)
                {
                    AddWorkOrder(wo.Id);
                    // Evertime we add a work order, we have to immediately follow up with an update to the work order and the site--QB quirkiness
                    // But first, get the latest copy from the database            
                    UpdateWorkOrder(wo.Id);
                    UpdateSite(wo.Id);
                }
            }

            // Next, all the new work orders that need to be added to Quickbooks.  
            woList = db.WorkOrders.Where(wo => wo.QBListId == null).ToList();
            foreach (WorkOrder wo in woList)
            {
                // Include any where the BillingInstructions QBListID is not null 
                // since we will take care of adding such workorders after the new BillingInstruction 
                // is successfully added to QB as a result of a request above
                BillingInstruction bi = db.BillingInstructions.Find(wo.BillingInstructionsId);
                if (bi.QBListId != null)
                {                    
                    AddWorkOrder(wo.Id);
                    // Evertime we add a work order, we have to immediately follow up with an update to the work order and the site--QB quirkiness
                    // But first, get the latest copy from the database            
                    UpdateWorkOrder(wo.Id);
                    UpdateSite(wo.Id);
                }
            }

            // Next, all the work orders that need to be updated in Quickbooks
            woList = db.WorkOrders.Where(wo => wo.NeedToUpdateQB == true).ToList();
            foreach (WorkOrder wo in woList)
            {
                // Update EditSequence for the workorder and then update the work order and site.
                UpdateWorkOrderEditSequence(wo.Id);
                UpdateWorkOrder(wo.Id);
                UpdateSite(wo.Id);
            }
              
            AppendStatus("Done");
            AppendStatus(Environment.NewLine);
        }
               
        private void SyncDSRs()
        {
            AppendStatus("Syncing DSRs...");

            RotoTrackDb db = new RotoTrackDb();

            List<DSR> dsrList = db.DSRs.Include("ServiceEntryList").Where(f => f.IsSynchronizedWithQB == false).ToList();
            foreach (DSR dsr in dsrList)
            {
                if (dsr.Status == DSRStatus.Approved)
                {
                    dsr.IsSynchronizedWithQB = true;
                    db.Entry(dsr).State = EntityState.Modified;
                    db.SaveChanges();

                    foreach (ServiceEntry se in dsr.ServiceEntryList.ToList())
                    {
                        ServiceDetail sd = db.ServiceDetails.Find(se.ServiceDetailId);
                        if (se.QBListIdForMileage == null)
                        {
                            if (se.Mileage == 0)
                            {
                                se.QBListIdForMileage = "N/A";
                                db.Entry(se).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {                                
                                AddVehicleMileage(se.Id, se.ServiceDetailId);
                            }
                        }
                        if (se.QBListIdForRegularHours == null)
                        {
                            if (se.RegularHours == 0)
                            {
                                se.QBListIdForRegularHours = "N/A";
                                db.Entry(se).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {                             
                                AddTimeTracking(se.Id, se.ServiceDetailId, sd.ServiceTypeId, se.RegularHours);
                            }
                        }
                        if (se.QBListIdForOTHours == null)
                        {
                            if (se.OTHours == 0)
                            {
                                se.QBListIdForOTHours = "N/A";
                                db.Entry(se).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {                             
                                AddTimeTracking(se.Id, se.ServiceDetailId, sd.OTServiceTypeId, se.OTHours);
                            }
                        }
                    }
                }
            }

            AppendStatus("Done");
            AppendStatus(Environment.NewLine);
        }

        private void SyncQBData()
        {
            XmlDocument doc = null;
            string response = "";
            string fromModifiedDate = AppConfig.GetLastSyncTime();

            string activeStatus = "All";
            string ownerID = "0";
            string fromDateTime = XmlUtils.GetAdjustedDateAsQBString(fromModifiedDate, -1, false);
            string toDateTime = XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, false);
                        
            // Sync all necessary data from QB
            doc = ClassDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            response = SyncDataHelper(doc, "Classes (Areas)");
            ClassDAL.HandleResponse(response);

            doc = EmployeeDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime, ownerID);
            response = SyncDataHelper(doc, "Employees");
            EmployeeDAL.HandleResponse(response);

            doc = CustomerDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime, ownerID);
            response = SyncDataHelper(doc, "Customers");
            CustomerDAL.HandleResponse(response);

            doc = CustomerTypeDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            response = SyncDataHelper(doc, "Customer Types");
            CustomerTypeDAL.HandleResponse(response);

            doc = VehicleDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            response = SyncDataHelper(doc, "Vehicles");
            VehicleDAL.HandleResponse(response);

            doc = JobTypeDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            response = SyncDataHelper(doc, "Job Types");
            JobTypeDAL.HandleResponse(response);

            doc = TimeTrackingDAL.BuildQueryRequest(fromDateTime, toDateTime);
            response = SyncDataHelper(doc, "Time Trackings");
            TimeTrackingDAL.HandleResponse(response);
            
            // Exception for VehicleMileage due to bug in Quickbooks where querying against modified date gets all the mileage--so have to use transaction date, back 30 days.
            // "yyyy-MM-ddTHH:mm:ssK" or "yyyy-MM-dd"
            string fromDateOnly = XmlUtils.GetAdjustedDateAsQBString(fromModifiedDate, -30, true);
            string toDateOnly = XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, true);
            doc = VehicleMileageDAL.BuildQueryRequest(fromDateOnly, toDateOnly);
            response = SyncDataHelper(doc, "Vehicle Mileage");
            VehicleMileageDAL.HandleResponse(response);
                     
            //doc = VehicleMileageDAL.BuildQueryRequest2(fromDateTime, toDateTime);
            //response = SyncDataHelper(doc, "Vehicle Mileage");
            //VehicleMileageDAL.HandleResponse(response);
                
            doc = ItemDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            response = SyncDataHelper(doc, "Items");
            ItemDAL.HandleResponse(response);

            doc = VendorDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            response = SyncDataHelper(doc, "Vendors");
            VendorDAL.HandleResponse(response);

            doc = BillDAL.BuildQueryRequest(fromDateTime, toDateTime);
            response = SyncDataHelper(doc, "Bills");
            BillDAL.HandleResponse(response);

            doc = SalesOrderDAL.BuildQueryRequest(fromDateTime, toDateTime);
            response = SyncDataHelper(doc, "Sales Orders");
            SalesOrderDAL.HandleResponse(response);

            doc = InvoiceDAL.BuildQueryRequest(fromDateTime, toDateTime);
            response = SyncDataHelper(doc, "Invoices");
            InvoiceDAL.HandleResponse(response);            

            AppConfig.SetLastSyncTime(DateTime.Now);

            AppendStatus("Done");
            AppendStatus(Environment.NewLine);
        }
        #endregion         

        #region GUI Event Processing
        void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            aTimer.Enabled = false;
            DoSync();
            aTimer.Enabled = true;
        }

        private void btn_SyncNow_Click(object sender, EventArgs e)
        {
            DoSync();
        }

        private void btnAutoSync_Click(object sender, EventArgs e)
        {
            if (aTimer.Enabled)
            {
                aTimer.Enabled = false;
                btnAutoSync.Text = "Enable Sync";
                btnSyncNow.Enabled = true;
                numericUpDownSyncDuration.Enabled = true;
            }
            else
            {
                aTimer.Interval = (double)numericUpDownSyncDuration.Value * 100000;
                btnSyncNow.Enabled = false;
                numericUpDownSyncDuration.Enabled = false;                
                btnAutoSync.Text = "Disable Sync";

                DoSync();
                aTimer.Enabled = true;
            }            
        }
              
        private void btnGetQBListID_Click(object sender, EventArgs e)
        {
            String woNum = tbWorkOrderNumber.Text.Trim();
            if (woNum.Length == 0)
            {
                MessageBox.Show("Please enter a Work Order Number.", "Input Validation");
                return;
            }
            try
            {
                string response = QBUtils.DoRequest(CustomerDAL.BuildCustomerQueryByWorkorderNum(woNum));
                tbResults.Text = "";
                string qbid = CustomerDAL.GetQBIDFromResponse(response);
                if (qbid != "")
                {
                    tbResults.AppendText(qbid);
                }
                else
                {
                    tbResults.AppendText("Not found!");
                }

                tbResults.AppendText(Environment.NewLine);

                string qbes = CustomerDAL.GetQBEditSequenceFromResponse(response);
                if (qbes != "")
                {
                    tbResults.AppendText(qbes);
                }
            }
            catch (Exception ex)
            {
                tbResults.Text = ex.Message;
            }
        }

        // <?xml version="1.0"?><?qbxml version="12.0"?><QBXML><QBXMLMsgsRq onError="stopOnError"><CustomerQueryRq requestID="1"><ActiveStatus>All</ActiveStatus><FromModifiedDate>2013-11-09T15:08:14</FromModifiedDate><OwnerID>0</OwnerID></CustomerQueryRq></QBXMLMsgsRq></QBXML>
        private void btnRunXml_Click(object sender, EventArgs e)
        {
            String rawXML = tbXML.Text.Trim();
            if (rawXML.Length == 0)
            {
                MessageBox.Show("Please enter QBXML.", "Input Validation");
                return;
            }
            try
            {
                string response = QBUtils.DoRequestRaw(rawXML);
                tbResults.Text = response;
            }
            catch (Exception ex)
            {
                tbResults.Text = ex.Message;
            }
        }

        private void Exit_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
        #endregion

    }
}
