using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Trucks.Excel
{
    public class ConversionJob
    {
        public ZamzarResult Result { get; set; }
        public string Company { get; set; }
        public string SettlementId { get; set; }
        public DateTime SettlementDate { get; set; }
    }
}