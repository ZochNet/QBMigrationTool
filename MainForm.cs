
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
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading;

namespace QBMigrationTool
{
	/// <summary>
	/// CustomerAddForm shows how to invoke QuickBooks qbXMLRP COM object
	/// It uses .NET to create qbXML request and parse qbXML response
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
    {
        #region Member Data
        private bool stopUpdateAmountThread;
        private Thread updateAmountThread = null;
        private bool updateAmountThreadBusy = false;
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
        private Button buttonSyncWorkOrders;
        private Button buttonSyncSalesReps;
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
            numericUpDownSyncDuration.Value = 10;
            numericUpDownSyncDuration.Maximum = 30;
            numericUpDownSyncDuration.Minimum = 1;

            labelBuildType.Text = RototrackConfig.GetBuildType();
            //aTimer = new System.Timers.Timer(10000);
            aTimer = new System.Timers.Timer((double)numericUpDownSyncDuration.Value * 60.0 * 1000.0);
            aTimer.Elapsed += aTimer_Elapsed;

            this.stopUpdateAmountThread = false;

            // Initialize web security for creating users
            try
            {
                RotoTrackDbUtils.InitializeWebSecurityIfNotAlready();                
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to access database.  Application will now exit.  Please make sure the database is accessible and try again.");
                throw(e);
            }                        
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
            MainForm mf = null;
            try
            {
                mf = new MainForm();
                Application.Run(mf);
            }
            catch (Exception)
            {
                Application.ExitThread();
            }
            			
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
            this.buttonSyncWorkOrders = new System.Windows.Forms.Button();
            this.buttonSyncSalesReps = new System.Windows.Forms.Button();
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
            // buttonSyncWorkOrders
            // 
            this.buttonSyncWorkOrders.Location = new System.Drawing.Point(120, 37);
            this.buttonSyncWorkOrders.Name = "buttonSyncWorkOrders";
            this.buttonSyncWorkOrders.Size = new System.Drawing.Size(75, 23);
            this.buttonSyncWorkOrders.TabIndex = 34;
            this.buttonSyncWorkOrders.Text = "Sync WOs";
            this.buttonSyncWorkOrders.UseVisualStyleBackColor = true;
            this.buttonSyncWorkOrders.Click += new System.EventHandler(this.buttonSyncWorkOrders_Click);
            // 
            // buttonSyncSalesReps
            // 
            this.buttonSyncSalesReps.Location = new System.Drawing.Point(218, 37);
            this.buttonSyncSalesReps.Name = "buttonSyncSalesReps";
            this.buttonSyncSalesReps.Size = new System.Drawing.Size(108, 23);
            this.buttonSyncSalesReps.TabIndex = 35;
            this.buttonSyncSalesReps.Text = "Sync Sales Reps";
            this.buttonSyncSalesReps.UseVisualStyleBackColor = true;
            this.buttonSyncSalesReps.Click += new System.EventHandler(this.buttonSyncSalesReps_Click);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(801, 794);
            this.Controls.Add(this.buttonSyncSalesReps);
            this.Controls.Add(this.buttonSyncWorkOrders);
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

        private bool UpdateWorkOrder(int woID)
        {
            RotoTrackDb db = new RotoTrackDb();
            WorkOrder wo = db.WorkOrders.Find(woID);

            XmlDocument doc = WorkOrderDAL.BuildModRq(wo);
            string response = QBUtils.DoRequest(doc);
            bool status = WorkOrderDAL.HandleModResponse(response);

            if (!status)
            {
                Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Failed to update work order for WO#: " + wo.WorkOrderNumber + " ListID: " + wo.QBListId + ".  Setting NeedToUpdateQB flag to true to try again.");
                wo.NeedToUpdateQB = true;
                db.Entry(wo).State = EntityState.Modified;
                db.SaveChanges();
                return false;
            }
            else
            {
                return true;
            }
        }

        private void UpdateSalesRep(int woID)
        {
            RotoTrackDb db = new RotoTrackDb();
            WorkOrder wo = db.WorkOrders.Find(woID);

            XmlDocument doc = SiteAndAdditionalInfoDAL.BuildUpdateSalesRepRq(wo);
            string response = QBUtils.DoRequest(doc);            
        }

        private void UpdateSiteAndAdditionalInfo(int woID)
        {
            RotoTrackDb db = new RotoTrackDb();
            WorkOrder wo = db.WorkOrders.Find(woID);

            XmlDocument doc = SiteAndAdditionalInfoDAL.BuildUpdateRq(wo);
            string response = QBUtils.DoRequest(doc);
            bool status = SiteAndAdditionalInfoDAL.HandleResponse(response);

            if (!status)
            {
                Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Failed to update site and additional info for WO#: " + wo.WorkOrderNumber + " ListID: " + wo.QBListId + ".  Setting NeedToUpdateQB flag to true to try again.");
                wo.NeedToUpdateQB = true;
                db.Entry(wo).State = EntityState.Modified;
                db.SaveChanges();
            }
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

        private bool RemoveTimeTracking(int ttID)
        {
            RotoTrackDb db = new RotoTrackDb();
            DeletedTimeTracking tt = db.DeletedTimeTrackings.Find(ttID);

            XmlDocument doc = TimeTrackingDAL.BuildRemoveRq(tt.QBTxnId);
            string response = QBUtils.DoRequest(doc);
            return TimeTrackingDAL.HandleRemoveResponse(response);
        }

        private bool RemoveVehicleMileage(int mtID)
        {
            RotoTrackDb db = new RotoTrackDb();
            DeletedMileageTracking mt = db.DeletedMileageTrackings.Find(mtID);

            XmlDocument doc = VehicleMileageDAL.BuildRemoveRq(mt.QBTxnId);
            string response = QBUtils.DoRequest(doc);
            return VehicleMileageDAL.HandleRemoveResponse(response);
        }

        private string SyncDataHelper(XmlDocument doc)
        {                      
            string response = QBUtils.DoRequest(doc);

            return response;
        }

        private int GetNumUnsyncedWorkOrders()
        {
            RotoTrackDb db = new RotoTrackDb();

            int count = 0;
            var workorders = db.Database.SqlQuery<int>("select ID from WorkOrders where NeedToUpdateQB=1").ToList();                        
            count += workorders.Count();

            workorders = db.Database.SqlQuery<int>("select ID from WorkOrders where QBListId is null").ToList();
            count += workorders.Count();

            return count;
        }

        private int GetNumUnsyncedDSRs()
        {
            RotoTrackDb db = new RotoTrackDb();

            int count = 0;
            var dsrs = db.Database.SqlQuery<int>("select Id from DSRs where Status = 3 and IsSynchronizedWithQB = 0").ToList();
            count += dsrs.Count();

            dsrs = db.Database.SqlQuery<int>("select d.Id from DSRs d inner join ServiceEntries se on se.DSRId = d.Id where d.IsSynchronizedWithQB = 1 and (se.QBListIdForMileage is null or se.QBListIdForOTHours is null or se.QBListIdForRegularHours is null)").ToList();
            count += dsrs.Count();

            return count; 
        }

        private void SetMileageToNA(int seID)
        {
            RotoTrackDb db = new RotoTrackDb();
            ServiceEntry se = db.ServiceEntries.Find(seID);
            se.QBListIdForMileage = "N/A";
            db.Entry(se).State = EntityState.Modified;
            db.SaveChanges();
        }

        private void SetRegularHoursToNA(int seID)
        {
            RotoTrackDb db = new RotoTrackDb();
            ServiceEntry se = db.ServiceEntries.Find(seID);
            se.QBListIdForRegularHours = "N/A";
            db.Entry(se).State = EntityState.Modified;
            db.SaveChanges();
        }

        private void SetOTHoursToNA(int seID)
        {
            RotoTrackDb db = new RotoTrackDb();
            ServiceEntry se = db.ServiceEntries.Find(seID);
            se.QBListIdForOTHours = "N/A";
            db.Entry(se).State = EntityState.Modified;
            db.SaveChanges();
        }

        #endregion

        #region Main Sync Functions
        private void DoSync()
        {
            DateTime nowTime = DateTime.Now;
            string dayOfWeek = nowTime.DayOfWeek.ToString();
            DateTime startTime = DateTime.Today.AddHours(18).AddMinutes(45);
            DateTime endTime = DateTime.Today.AddHours(21).AddMinutes(15);
            
            ClearStatus();
            SetStatus("");

            //AppendStatus(dayOfWeek);
            AppendStatus("NOTE: Sync is disabled on Thursdays from " + startTime.ToString() + " to " + endTime.ToString());
            AppendStatus(Environment.NewLine);
            if (dayOfWeek == "Thursday" && nowTime > startTime && nowTime < endTime)
            {
                AppendStatus("Sync is disabled on Thursdays from " + startTime.ToString() + " to " + endTime.ToString());
                AppendStatus(Environment.NewLine);
                return;
            }
            
            SyncWorkOrders();
            SyncDSRs();
            RemoveDSRTimeAndMileage();

            int numUnsyncedWorkOrders = GetNumUnsyncedWorkOrders();
            int numUnsyncedDSRs = GetNumUnsyncedDSRs();
            if (numUnsyncedWorkOrders > 0)
            {
                Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "There are " + numUnsyncedWorkOrders.ToString() + " Work Orders that failed to sync with QB!");
            }
            if (numUnsyncedDSRs > 0)
            {
                Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "There are " + numUnsyncedDSRs.ToString() + " DSRs that failed to sync with QB!");
            }

            SyncQBData();
            
            AppendStatus("Sync Completed: " + DateTime.Now.ToString());
            AppendStatus(Environment.NewLine);
        }

        private void SyncSalesReps()
        {
            RotoTrackDb db = new RotoTrackDb();
            List<WorkOrder> woList = null;

            AppendStatus("Syncing Work Order Sales Reps...");
                        
            woList = db.WorkOrders.Where(wo => wo.statusValue != (int)WorkOrderStatus.Inactive).ToList();
            foreach (WorkOrder wo in woList)
            {
                UpdateSalesRep(wo.Id);                
            }

            AppendStatus("Done");
            AppendStatus(Environment.NewLine);
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
                    if (UpdateWorkOrder(wo.Id))
                    {
                        UpdateSiteAndAdditionalInfo(wo.Id);
                    }
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
                    if (UpdateWorkOrder(wo.Id))
                    {
                        UpdateSiteAndAdditionalInfo(wo.Id);
                    }
                }
            }

            // Next, all the work orders that need to be updated in Quickbooks where QBListId is not null
            woList = db.WorkOrders.Where(wo => wo.NeedToUpdateQB == true && wo.QBListId != null).ToList();
            foreach (WorkOrder wo in woList)
            {
                // Update EditSequence for the workorder and then update the work order and site.
                UpdateWorkOrderEditSequence(wo.Id);
                if (UpdateWorkOrder(wo.Id))
                {
                    UpdateSiteAndAdditionalInfo(wo.Id);
                }
            }
              
            // Finally, in case we encounter the quirk where we try to add a work order, but it's already in Quickbooks, let's get the ListID and EditSequence 
            // from QuickBooks and save it
            woList = db.WorkOrders.Where(wo => wo.QBListId == null && wo.NeedToUpdateQB == true).ToList();
            foreach (WorkOrder wo in woList)
            {
                CustomerDAL.UpdateAlreadyExistingWorkOrder(wo.Id);
                if (UpdateWorkOrder(wo.Id))
                {
                    UpdateSiteAndAdditionalInfo(wo.Id);
                }
            }

            AppendStatus("Done");
            AppendStatus(Environment.NewLine);
        }
               
        private void SyncDSRs()
        {
            AppendStatus("Syncing DSRs...");

            RotoTrackDb db = new RotoTrackDb();

            int approvedVal = (int)DSRStatus.Approved;
            List<DSRLite> dsrList = db.DSRs.Where(f => f.IsSynchronizedWithQB == false && f.statusValue == approvedVal).Select(f => new DSRLite { Id = f.Id, WorkOrderId = f.WorkOrderId, WorkOrderGUID = f.WorkOrderGUID, Created = f.Created, Modified = f.Modified, DateWorked = f.DateWorked, TechnicianId = f.TechnicianId, IsSynchronizedWithQB = f.IsSynchronizedWithQB, statusValue = f.statusValue }).ToList();                        
            foreach (DSRLite dsr in dsrList)
            {
                List<ServiceEntry> serviceEntryList = db.ServiceEntries.Where(f => f.DSRId == dsr.Id).ToList();
                foreach (ServiceEntry se in serviceEntryList.ToList())
                {
                    ServiceDetail sd = db.ServiceDetails.Find(se.ServiceDetailId);
                    if (se.QBListIdForMileage == null)
                    {
                        if (se.Mileage == 0)
                        {
                            SetMileageToNA(se.Id);
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
                            SetRegularHoursToNA(se.Id);
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
                            SetOTHoursToNA(se.Id);
                        }
                        else
                        {
                            AddTimeTracking(se.Id, se.ServiceDetailId, sd.OTServiceTypeId, se.OTHours);
                        }
                    }
                }

                // Update status of DSR
                DSR dsrToUpdate = db.DSRs.Find(dsr.Id);
                dsrToUpdate.IsSynchronizedWithQB = true;
                db.Entry(dsrToUpdate).State = EntityState.Modified;
                db.SaveChanges();

            }

            AppendStatus("Done");
            AppendStatus(Environment.NewLine);
        }

        private void RemoveDSRTimeAndMileage()
        {
            AppendStatus("Removing Deleted DSR Time and Mileage...");

            RotoTrackDb db = new RotoTrackDb();

            List<DeletedTimeTracking> deletedTimeList = db.DeletedTimeTrackings.Where(f => f.IsSynchronizedWithQB == false).ToList();
            foreach (DeletedTimeTracking deletedTime in deletedTimeList)
            {
                if (RemoveTimeTracking(deletedTime.Id))
                {
                    deletedTime.IsSynchronizedWithQB = true;
                    db.Entry(deletedTime).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            List<DeletedMileageTracking> deletedMileageList = db.DeletedMileageTrackings.Where(f => f.IsSynchronizedWithQB == false).ToList();
            foreach (DeletedMileageTracking deletedMileage in deletedMileageList)
            {
                if (RemoveVehicleMileage(deletedMileage.Id))
                {
                    deletedMileage.IsSynchronizedWithQB = true;
                    db.Entry(deletedMileage).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            // Due to buggy mileage handling in QB, remove any deleted mileage trackings that still exist.
            db.Database.ExecuteSqlCommand("delete from MileageTrackings where QBTxnId in (select QBTxnId from DeletedMileageTrackings)");
            
            AppendStatus("Done");
            AppendStatus(Environment.NewLine);
        }

        private void ExportItemList()
        {
            AppendStatus("Exporting Item List...");

            string csv = BuildItemListCsv();
            string filePath = "C:\\Roto-Versal-Public\\Accounts\\WO\\";
            System.IO.Directory.CreateDirectory(filePath);
            string outFile = filePath + "ItemList.csv";
            System.IO.File.WriteAllText(outFile, csv.ToString());

            AppendStatus("Done");
            AppendStatus(Environment.NewLine);
        }

        private string BuildItemListCsv()
        {
            RotoTrackDb db = new RotoTrackDb();
            List<Item> ItemData = db.Items.Where(f => (f.ItemType == "ItemInventory" || f.ItemType == "ItemInventoryAssembly") && f.IsActive).ToList();
            string csv = "Type, Name, Description, Average Cost" + Environment.NewLine;
            foreach (Item item in ItemData)
            {
                string type = "";
                string name = "";
                string description = "";
                string averageCost = "";

                if (item.ItemType != null) type = item.ItemType.Replace("'", "''").Replace("\"", "\"\"");
                if (item.Name != null) name = item.Name.Replace("'", "''").Replace("\"", "\"\"");
                if (item.Description != null) description = item.Description.Replace("'", "''").Replace("\"", "\"\"");
                averageCost = item.AverageCost.ToString();

                csv += "\"" + type + "\",";
                csv += "\"" + name + "\",";
                csv += "\"" + description + "\",";                
                csv += "\"" + averageCost + "\"" + Environment.NewLine;
            }
            return csv;
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
            AppendStatus("Sync Classes (Areas)...");              
            doc = ClassDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            ClassDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);

            AppendStatus("Sync Employees...");

            //string fromDateTimeOld = XmlUtils.GetAdjustedDateAsQBString(fromModifiedDate, -600, false);
            doc = EmployeeDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime, ownerID);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            EmployeeDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);

            AppendStatus("Sync Customers...");              
            doc = CustomerDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime, ownerID);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            CustomerDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);

            AppendStatus("Sync Customer Types...");              
            doc = CustomerTypeDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            CustomerTypeDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);

            AppendStatus("Sync Vehicles...");              
            doc = VehicleDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            VehicleDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);

            AppendStatus("Sync Job Types...");              
            doc = JobTypeDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            JobTypeDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);

            AppendStatus("Sync Time Tracking...");              
            doc = TimeTrackingDAL.BuildQueryRequest(fromDateTime, toDateTime);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            TimeTrackingDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);
            
            // Exception for VehicleMileage due to bug in Quickbooks where querying against modified date gets all the mileage--so have to use transaction date, back 30 days.
            // "yyyy-MM-ddTHH:mm:ssK" or "yyyy-MM-dd"
            AppendStatus("Sync Vehicle Mileage...");              
            string fromDateOnly = XmlUtils.GetAdjustedDateAsQBString(fromModifiedDate, -180, true);
            string toDateOnly = XmlUtils.GetAdjustedDateAsQBString(DateTime.Now.ToShortDateString(), 1, true);
            doc = VehicleMileageDAL.BuildQueryRequest(fromDateOnly, toDateOnly);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            VehicleMileageDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);
                     
            //doc = VehicleMileageDAL.BuildQueryRequest2(fromDateTime, toDateTime);
            //response = SyncDataHelper(doc, "Vehicle Mileage");
            //VehicleMileageDAL.HandleResponse(response);
            AppendStatus("Sync Items...");                  
            doc = ItemDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            //doc = ItemDAL.BuildQueryRequest(activeStatus);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            ItemDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);

            AppendStatus("Sync Vendors...");              
            doc = VendorDAL.BuildQueryRequest(activeStatus, fromDateTime, toDateTime);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            VendorDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);

            AppendStatus("Sync Bills...");              
            doc = BillDAL.BuildQueryRequest(fromDateTime, toDateTime);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            BillDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);
            AppendStatus("Removing deleted Bills...");
            BillDAL.RemoveDeleted();
            AppendStatus("Done" + Environment.NewLine);

            AppendStatus("Sync Sales Orders...");              
            doc = SalesOrderDAL.BuildQueryRequest(fromDateTime, toDateTime);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            SalesOrderDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);
            AppendStatus("Removing deleted Sales Orders...");
            SalesOrderDAL.RemoveDeleted();
            AppendStatus("Done" + Environment.NewLine);

            AppendStatus("Sync Invoices...");

            //doc = InvoiceDAL.BuildQueryRequest(XmlUtils.GetAdjustedDateAsQBString(fromModifiedDate, -600, false), toDateTime);
            doc = InvoiceDAL.BuildQueryRequest(fromDateTime, toDateTime);
            response = SyncDataHelper(doc);
            AppendStatus("Done" + Environment.NewLine + "Processing...");
            InvoiceDAL.HandleResponse(response);
            AppendStatus("Done" + Environment.NewLine);
            AppendStatus("Removing deleted Invoices...");
            InvoiceDAL.RemoveDeleted();
            AppendStatus("Done" + Environment.NewLine);
            
            AppConfig.SetLastSyncTime(DateTime.Now);
        }

        private void UpdateEstDollarAmountForAllWorkOrders(bool force=false)
        {
            if (this.updateAmountThreadBusy)
            {
                AppendStatus("NOTE: Update Dollar amounts already in progress.  Skipping.");
                AppendStatus(Environment.NewLine);
            }
            else
            {
                bool doUpdate = false;
                DateTime nowTime = DateTime.Now;
                DateTime startTime = DateTime.Today.AddHours(2).AddMinutes(45);
                DateTime endTime = DateTime.Today.AddHours(3).AddMinutes(0);
                
                if (nowTime > startTime && nowTime < endTime)
                {
                    doUpdate = true;
                    AppendStatus("Update Dollar amounts starting within allowed time slot: "+ DateTime.Now.ToString());
                    AppendStatus(Environment.NewLine);
                }
                else if (force)
                {
                    doUpdate = true;
                    AppendStatus("Update Dollar amounts is being forced started now: " + DateTime.Now.ToString());
                    AppendStatus(Environment.NewLine);
                }
                else
                {
                    doUpdate = false;
                    AppendStatus("Update Dollar amounts, triggered by autosync, is only enabled nightly from " + startTime.ToString() + " to " + endTime.ToString() + ". Skipping.");
                    AppendStatus(Environment.NewLine);
                }

                if (doUpdate)
                {
                    this.updateAmountThread = new Thread(new ThreadStart(this.UpdateEstDollarAmountForAllWorkOrdersThread));
                    this.updateAmountThread.Start();
                }
            }
        }

        private void UpdateEstDollarAmountForAllWorkOrdersThread()
        {
            this.updateAmountThreadBusy = true;
            
            RotoTrackDb db = new RotoTrackDb();

            // Get active work orders that have a QBListID set
            List<WorkOrder> woList = db.WorkOrders.Where(wo => wo.QBListId != null && (wo.statusValue == (int)WorkOrderStatus.Open || wo.statusValue == (int)WorkOrderStatus.PreClose || wo.statusValue == (int)WorkOrderStatus.PendingApproval || wo.statusValue == (int)WorkOrderStatus.ReadyToInvoice || wo.statusValue == (int)WorkOrderStatus.Invoiced)).ToList();

            //List<WorkOrder> woList = db.WorkOrders.Where(wo => wo.QBListId != null && (wo.statusValue == (int)WorkOrderStatus.Invoiced) && wo.Id > 4013 && wo.Id < 4654).OrderBy(f => f.Id).ToList();

            int totalWo = woList.Count;
            int currentWo = 1;

            //for (int i = currentWo; i < totalWo; i++) {
            foreach (WorkOrder wo in woList) {
                //WorkOrder wo = woList.ToArray()[i];
                AppendStatus("Syncing " + currentWo.ToString() + " of " + totalWo.ToString() + Environment.NewLine);

                if (wo.Status == WorkOrderStatus.Invoiced)
                {
                    UpdateInvoiceSubtotal(wo.Id);
                }
                else
                {
                    UpdateEstDollarAmountForWorkOrder(wo.Id);
                }

                currentWo++;

                if (this.stopUpdateAmountThread)
                {
                    this.updateAmountThread.Abort();
                }
            }

            AppendStatus("Done" + Environment.NewLine);
            this.updateAmountThreadBusy = false;
        }
        /*
        public static IEnumerable<t> DistinctBy<t>(this IEnumerable<t> list, Func<t, object> propertySelector)
        {
            return list.GroupBy(propertySelector).Select(x => x.First());
        }
        */
        public class DistinctInvoice
        {
            public string TxnId { get; set; }
            public DateTime TxnDate { get; set; }
            public string WorkOrderListId { get; set; }
            public double Subtotal { get; set; }
        }

        private void UpdateInvoiceSubtotal(int woID)
        {
            RotoTrackDb db = new RotoTrackDb();

            WorkOrder wo = db.WorkOrders.Find(woID);

            // Do invoice subtotal
            double invoiceSubtotal = 0.0;
            if (db.Invoices.Any(f => f.WorkOrderListID == wo.QBListId))
            {
                string query = "select i.TxnId, i.TxnDate, i.WorkOrderListID, i.Subtotal from invoices i inner join (select max(i.id) as id from Invoices i inner join WorkOrders w on w.QBListId = i.WorkOrderListID where i.TxnDate <> '1900-01-01 00:00:00.000' and i.WorkOrderListID = '" + wo.QBListId + "' group by i.TxnId, i.WorkOrderListID) iv on iv.id = i.Id";

                List<DistinctInvoice> invoices = db.Database.SqlQuery<DistinctInvoice>(query).ToList();

                //List<Invoice> invoiceList = db.Invoices.Where(f => f.WorkOrderListID == wo.QBListId).ToList();
                foreach (DistinctInvoice invoice in invoices)
                {
                    invoiceSubtotal += invoice.Subtotal;
                    wo.InvoiceCreated = invoice.TxnDate;
                }
            }

            wo.InvoiceSubtotal = invoiceSubtotal;

            db.Entry(wo).State = EntityState.Modified;
            db.SaveChanges();
        }

        private void UpdateEstDollarAmountForWorkOrder(int woID)
        {
            RotoTrackDb db = new RotoTrackDb();

            WorkOrder wo = db.WorkOrders.Find(woID);
            string woListID = wo.QBListId;

            List<TimeTracking> timetrackings;
            timetrackings = db.TimeTrackings.Where(f => f.QBWorkOrderListID == woListID).ToList();

            List<MileageTracking> mileagetrackings;
            mileagetrackings = db.MileageTrackings.Where(f => f.QBWorkOrderListID == woListID).ToList();

            List<BillLine> billLines;
            billLines = db.BillLines.Where(f => f.WorkOrderListID == woListID).ToList();

            List<SalesOrderLine> soLines;
            soLines = db.SalesOrderLines.Where(f => f.WorkOrderListID == woListID).ToList();

            var workorders = db.WorkOrders.ToList();
            var users = db.UserProfiles.ToList();
            var servicetypes = db.ServiceTypes.ToList();
            var customers = db.Customers.ToList();
            var vehicles = db.Vehicles.ToList();
            var mileagerates = db.MileageRates.ToList();
            var vendors = db.Vendors.ToList();
            var items = db.Items.ToList();

            var tt_results =
                from tt in timetrackings
                from workorder in workorders.Where(f => f.QBListId == tt.QBWorkOrderListID).DefaultIfEmpty()
                from customer in customers.Where(f => f.Id == (workorder == null ? -1 : workorder.CustomerId)).DefaultIfEmpty()
                from user in users.Where(f => f.QBListId == tt.QBEmployeeListID).DefaultIfEmpty()
                from servicetype in servicetypes.Where(f => f.QBListId == tt.QBServiceTypeListID).DefaultIfEmpty()
                select new
                {
                    Id = tt.Id,
                    Job = ((workorder == null) || (customer == null)) ? "" : (customer.Name + ":" + workorder.WorkOrderNumber),
                    Type = "Time",
                    BillableStatus = tt.BillableStatus,
                    Date = tt.DateWorked,
                    Item = (servicetype == null) ? "" : servicetype.Name,
                    Description = (user == null) ? "" : user.FirstName + " " + user.LastName,
                    Unit = tt.HoursWorked + (tt.MinutesWorked / 60.0M),
                    Rate = (servicetype == null) ? 0M : decimal.Parse(servicetype.Price) * 1.0M,
                    Amount = (servicetype == null) ? 0M : decimal.Parse(servicetype.Price) * (tt.HoursWorked + (tt.MinutesWorked / 60.0M)) * 1.0M,
                    SalePrice = (servicetype == null) ? 0M : decimal.Parse(servicetype.Price) * (tt.HoursWorked + (tt.MinutesWorked / 60.0M)) * 1.0M,
                    Comments = ""
                };

            var mileage_results =
                from mileagetracking in mileagetrackings
                from workorder in workorders.Where(f => f.QBListId == mileagetracking.QBWorkOrderListID).DefaultIfEmpty()
                from customer in customers.Where(f => f.Id == (workorder == null ? -1 : workorder.CustomerId)).DefaultIfEmpty()
                from vehicle in vehicles.Where(f => f.QBListId == mileagetracking.QBVehicleListID).DefaultIfEmpty()
                from mileagerate in mileagerates.Where(f => f.QBListId == mileagetracking.QBMileageRateListID).DefaultIfEmpty()
                select new
                {
                    Id = mileagetracking.Id,
                    Job = ((workorder == null) || (customer == null)) ? "" : (customer.Name + ":" + workorder.WorkOrderNumber),
                    Type = "Mileage",
                    BillableStatus = mileagetracking.BillableStatus,
                    Date = mileagetracking.TripStartDate,
                    Item = (mileagerate == null) ? "" : mileagerate.Name,
                    Description = (vehicle == null) ? "" : vehicle.Name,
                    Unit = mileagetracking.TotalMiles * 1.0M,
                    Rate = (mileagerate == null) ? 0.0M : (mileagerate.Rate * 1.0M),
                    Amount = mileagetracking.BillableAmount * 1.0M,
                    SalePrice = mileagetracking.BillableAmount * 1.0M,
                    Comments = mileagetracking.Notes
                };

            var bill_results =
                from billline in billLines
                from workorder in workorders.Where(f => f.QBListId == billline.WorkOrderListID).DefaultIfEmpty()
                from customer in customers.Where(f => f.Id == (workorder == null ? -1 : workorder.CustomerId)).DefaultIfEmpty()
                from item in items.Where(f => f.QBListId == billline.ItemListID).DefaultIfEmpty()
                from vendor in vendors.Where(f => f.QBListId == billline.VendorListID).DefaultIfEmpty()
                select new
                {
                    Id = billline.Id,
                    Job = ((workorder == null) || (customer == null)) ? "" : (customer.Name + ":" + workorder.WorkOrderNumber),
                    Type = ((billline.Amount < 0) ? "Credit" : "Bill"),
                    BillableStatus = billline.BillableStatus,
                    Date = billline.BillTxnDate,
                    Item = (item == null) ? "" : item.Name,
                    Description = billline.Description,
                    Unit = ((billline.Quantity == 0 && billline.Amount > 0 && billline.UnitCost > 0) ? (billline.Amount / billline.UnitCost) : billline.Quantity * 1.0M),
                    Rate = billline.UnitCost * 1.0M,
                    Amount = billline.Amount * 1.0M,
                    //SalePrice = ((billline.Amount < 0) ? billline.Amount * 1.0M : 0.0M),
                    SalePrice = CalculateSalePrice(billline, item),
                    Comments = (vendor == null) ? "" : vendor.Name
                };

            var so_results =
                from soline in soLines
                from workorder in workorders.Where(f => f.QBListId == soline.WorkOrderListID).DefaultIfEmpty()
                from customer in customers.Where(f => f.Id == (workorder == null ? -1 : workorder.CustomerId)).DefaultIfEmpty()
                select new
                {
                    Id = soline.Id,
                    Job = ((workorder == null) || (customer == null)) ? "" : (customer.Name + ":" + workorder.WorkOrderNumber),
                    Type = "Sales Order",
                    BillableStatus = soline.BillableStatus,
                    Date = soline.SalesOrderTxnDate,
                    Item = soline.ItemName,
                    Description = soline.Description,
                    Unit = soline.Quantity * 1.0M,
                    Rate = soline.UnitCost * 1.0M,
                    Amount = soline.Amount * 1.0M,
                    SalePrice = soline.Amount * 1.0M,
                    Comments = ""
                };

            var workordersummaryAll = tt_results
                .Concat(mileage_results)
                .Concat(bill_results)
                .Concat(so_results)
                .AsQueryable();

            decimal totalAmount = workordersummaryAll.Sum(f => f.Amount);
            decimal totalSalePrice = workordersummaryAll.Sum(f => f.SalePrice);

            wo.EstDollarAmount = (double)totalSalePrice;

            if (wo.CloseStatus == CloseStatus.Warranty)
            {
                wo.EstDollarAmount *= -1.0;
            }
                      
            db.Entry(wo).State = EntityState.Modified;
            db.SaveChanges();
        }

        private decimal CalculateSalePrice(BillLine billline, Item item)
        {
            if (billline == null || item == null) return 0.0M;

            if (billline.Amount < 0)
            {
                return billline.Amount * 1.0M;
            }
            else
            {
                if (billline.BillableStatus == "NotBillable")
                {
                    return 0.0M;
                }
                else
                {
                    decimal salePrice = 0.0M;
                    switch (item.Name.ToUpper())
                    {
                        case "PER DIEM":
                            salePrice = billline.Amount;
                            break;
                        case "SALES TAX":
                            salePrice = billline.Amount;
                            break;
                        case "LODG":
                            salePrice = billline.Amount * 1.15M;
                            break;
                        case "AIRF":
                            salePrice = billline.Amount * 1.15M;
                            break;
                        case "MEALS":
                            salePrice = billline.Amount * 1.15M;
                            break;
                        case "FRT":
                            salePrice = billline.Amount * 1.15M;
                            break;
                        case "FEE":
                            salePrice = billline.Amount * 1.15M;
                            break;
                        case "CORECHG":
                            salePrice = billline.Amount * 1.15M;
                            break;
                        case "TRAVEL":
                            salePrice = billline.Amount * 1.15M;
                            break;
                        case "SPEC EQUIP":
                            salePrice = billline.Amount * 1.20M;
                            break;
                        default:
                            salePrice = billline.Amount * 1.25M;
                            break;
                    }
                    return salePrice;
                }
            }
            //return ((billline.Amount < 0) ? billline.Amount * 1.0M : 0.0M);
        }
        #endregion         

        #region GUI Event Processing
        void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            aTimer.Enabled = false;
            try
            {
#if !LIVE_ROTO_DB_SYNC
                    DoSync();
                    ExportItemList();
#else

                UpdateEstDollarAmountForAllWorkOrders();
#endif
            }
            catch (Exception ex)
            {
                Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Exception occurred and ok to ignore and try again.  Exception details are: " + ex.ToString());
                AppendStatus("Exception occurred!");
            }
            aTimer.Enabled = true;
        }

        private void btn_SyncNow_Click(object sender, EventArgs e)
        {
            try
            {
#if !LIVE_ROTO_DB_SYNC
                    DoSync();
                    ExportItemList();                    
#else
                UpdateEstDollarAmountForAllWorkOrders(true);
#endif
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                AppendStatus(errorMessage);
            }
            catch (Exception ex)
            {
                Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Exception occurred and ok to ignore and try again.  Exception details are: " + ex.ToString());
                AppendStatus("Exception occurred!");
            }
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
                aTimer.Interval = (double)numericUpDownSyncDuration.Value * 60.0 * 1000.0;
                btnSyncNow.Enabled = false;
                numericUpDownSyncDuration.Enabled = false;                
                btnAutoSync.Text = "Disable Sync";

                try
                {
#if !LIVE_ROTO_DB_SYNC
                    DoSync();
                    ExportItemList();
#else
                    UpdateEstDollarAmountForAllWorkOrders();
#endif
                }
                catch (Exception ex)
                {
                    Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Exception occurred and ok to ignore and try again.  Exception details are: " + ex.ToString());
                    AppendStatus("Exception occurred!");
                }
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
            this.stopUpdateAmountThread = true;
            this.Close();
        }
        #endregion

        private void buttonSyncWorkOrders_Click(object sender, EventArgs e)
        {
            try
            {
                ClearStatus();
                SyncWorkOrders();
            }
            catch (Exception ex)
            {
                Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Exception occurred and ok to ignore and try again.  Exception details are: " + ex.ToString());
                AppendStatus("Exception occurred!");
            }
        }

        private void buttonSyncSalesReps_Click(object sender, EventArgs e)
        {
            try
            {
                ClearStatus();
                SyncSalesReps();
            }
            catch (Exception ex)
            {
                Logging.RototrackErrorLog("QBMigrationTool: " + RototrackConfig.GetBuildType() + ": " + "Exception occurred and ok to ignore and try again.  Exception details are: " + ex.ToString());
                AppendStatus("Exception occurred!");
            }
        }

    }
}
