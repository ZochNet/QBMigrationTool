using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rototrack_data_access;
using rototrack_model;

namespace QBMigrationTool
{
    public class AppConfig
    {
        public static string GetLastSyncTime()
        {
            DateTime lastSync = DateTime.MinValue;
            RotoTrackDb db = new RotoTrackDb();
            Config config = db.Configs.First();
            if (config != null)
            {
                if (config.LastFullRefreshQB != null)
                {
                    lastSync = config.LastFullRefreshQB;
                }
            }
            string fromModifiedDate = lastSync.ToString("yyyy-MM-ddTHH:mm:ssK");

            return fromModifiedDate;
        }

        public static void SetLastSyncTime(DateTime lastSync)
        {
            RotoTrackDb db = new RotoTrackDb();
            Config config = db.Configs.First();
            if (config != null)
            {
                config.LastFullRefreshQB = lastSync;
                db.SaveChanges();
            }
        }
    }
}
