using rototrack_model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QBMigrationTool
{
    class DSRLite
    {
        public int Id;
        public int WorkOrderId;
        public string WorkOrderGUID;
        public DateTime Created;
        public DateTime Modified;
        public DateTime DateWorked;
        public DSRStatus Status { get { return (DSRStatus)statusValue; } private set { } }
        public int TechnicianId;
        public bool IsSynchronizedWithQB;
        public int statusValue;
    }
}
