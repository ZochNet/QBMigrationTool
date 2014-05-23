using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QBMigrationTool
{
    public class RototrackConfig
    {
        public static string GetBuildType()
        {
            string buildType = "UNKNOWN";

            #if DEV_ROTO
            buildType = "DEV_ROTO";
            #elif DEV_GUARD
            buildType = "DEV_GUARD";
            #elif LIVE_ROTO
            buildType = "LIVE_ROTO";
            #elif LIVE_GUARD
            buildType = "LIVE_GUARD";
            #endif

            return buildType;
        }

    }
}
