namespace Trucks
{
    public class PantherSettings
    {
        public const string Section = "Panther";
        public string Url { get; set; }
        public IEnumerable<Company> Companies { get; set; }
    }
}