using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QBMigrationTool
{
    public class Customer
    {
        public string ListID { get; set; }
        public string Name { get; set; }
        public ShipTo ShipTo { get; set; }
        public Site Site { get; set; }
    }
}
