namespace Trucks
{
    public class ConvertState 
    {
        public SettlementHistory Settlement { get; set;}
        public int ConversionJobId { get; set; }
        public string LocalXlsPath { get; set; }
        public string CloudPath { get; set; }
        public DateTime UploadTimestampUtc { get; set; }
        public DateTime ConvertTimestampUtc { get; set; }
    }    
}