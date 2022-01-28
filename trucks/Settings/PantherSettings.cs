namespace Trucks
{
    public class Company
    {
        public string CompanyId { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

    }

    public class PantherSettings
    {
        public const string Section = "Panther";
        public string Url { get; set; }
        public IEnumerable<Company> Companies { get; set; }
    }
}