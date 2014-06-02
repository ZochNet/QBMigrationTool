using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QBMigrationTool
{
    public class DSRDal
    {
        private void BuildSyncDSRRequests(ArrayList req)
        {
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
                                BuildVehicleAddMileageRequest(req, se, sd);
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
                                BuildTimeTrackingAddRequest(req, se, sd, sd.ServiceTypeId, se.RegularHours);
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
                                BuildTimeTrackingAddRequest(req, se, sd, sd.OTServiceTypeId, se.OTHours);
                            }
                        }
                    }
                }
            }
        }

    }
}
