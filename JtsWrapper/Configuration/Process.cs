using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JtsWrapper.Configuration
{
    internal static class Process
    {
        public static string OperationId = ConfigurationManager.AppSettings["OperationId"];
        public static string LineSegmentId = ConfigurationManager.AppSettings["LineSegmentId"];
        public static string ProcessedBy = ConfigurationManager.AppSettings["ProcessedBy"];
        public static bool SimulationOn = ConfigurationManager.AppSettings["SimulationMode"].ToLower() == "on";
    }
}
