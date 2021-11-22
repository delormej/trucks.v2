using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

namespace Trucks.Panther
{
    public class PantherClient
    {
        private const string pantherBaseUrl = "http://fleetweb.pantherpremium.com";
        private string sessionId;
        private DateTime sessionExpires;
        private string company;
        private string password;

        public string Company { get { return company;} }

        HttpClientHandler clientHandler;
        HttpClient client;

        public static string GetLocalFileName(SettlementHistory settlement)
        {
            return GetLocalFileName(settlement.CompanyId.ToString(), settlement.SettlementId);   
        }

        private static string GetLocalFileName(string companyId, string settlementId)
        {
            return Path.Combine(companyId, settlementId + ".xls");
        }

        public PantherClient(string company, string password)
        {
            this.company = company;
            this.password = password;
            clientHandler = new HttpClientHandler();
            clientHandler.CookieContainer = new CookieContainer();            
            client = new HttpClient(clientHandler);
        }
    
        public IAsyncEnumerable<KeyValuePair<string, SettlementHistory>> DownloadSettlementsAsync(Func<SettlementHistory, bool> filter, int max = 10)
        {
            // Method can't be marked async with IAsyncEnumerable return type, so manually waiting
            var task = GetSettlementsAsync();
            task.Wait();
            List<SettlementHistory> settlements = task.Result;

            List<SettlementHistory> selectSettlements = settlements.Where(filter)
                .OrderByDescending(s => s.SettlementDate)
                .Take(max)
                .ToList();

            return DownloadSettlementsAsync(selectSettlements);
        }

        public async IAsyncEnumerable<KeyValuePair<string, SettlementHistory>> DownloadSettlementsAsync(List<SettlementHistory> settlementsToDownload)
        {
            foreach (SettlementHistory settlement in settlementsToDownload)
            {
                string xls = await DownloadSettlementReportAsync(settlement.SettlementId);
                System.Console.WriteLine($"Downloaded {settlement.SettlementId}: {xls}");
                
                var kvp = new KeyValuePair<string, SettlementHistory>(xls, settlement);

                yield return kvp;
            }
            yield break;
        }        

        public async Task<List<SettlementHistory>> GetSettlementsAsync()
        {
            bool loggedIn = await LoginAsync();
            if (!loggedIn)
                throw new ApplicationException("Unable to login with credentials.");
            
            string payrollHistHtml = await GetPayrollHistAsync();
            
            PayrollHistHtmlParser parser = new PayrollHistHtmlParser(company);
            List<SettlementHistory> settlements = parser.Parse(payrollHistHtml);
            
            return settlements;            
        }

        /// <summary>
        /// Downloads an Excel file to disk and returns the path of the saved file.
        /// </summary>
        public async Task<string> DownloadSettlementReportAsync(string checkNumber)
        {
            Directory.CreateDirectory(company);
            string uri = pantherBaseUrl + $"/Financial/DownloadSettlementReport?ChkNo={checkNumber}";
            byte[] bytes = await client.GetByteArrayAsync(uri);
            string filename = GetLocalFileName(company, checkNumber);
            File.WriteAllBytes(filename, bytes);
            return filename;
        }

        private async Task<bool> LoginAsync()
        {
            if (sessionExpires > DateTime.Now)
                return true;

            bool isLoggedIn = false;
            
            string loginUrl = pantherBaseUrl + "/Login/Login";
            string content = $"UserID={company}&Password={password}&RememberMe=false";
            StringContent httpContent = new StringContent(content);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            
            var response = await client.PostAsync(loginUrl, httpContent);
            response.EnsureSuccessStatusCode();

            var contents = await response.Content.ReadAsStringAsync();
            CookieCollection cookieCollection = clientHandler.CookieContainer.GetCookies(new Uri(loginUrl));
            foreach (Cookie cookie in cookieCollection)
            {
                if (cookie.Name == "session-id")
                {
                    sessionId = cookie.Value;
                    sessionExpires = cookie.Expires;
                    isLoggedIn = true;
                    break;
                }
            }
            return isLoggedIn;
        }

        private Task<string> GetPayrollHistAsync()
        {
            string uri = pantherBaseUrl + "/Financial/PayrollHist";
            return client.GetStringAsync(uri);
        }        

        private bool Exists(SettlementHistory settlement)
        {
            return File.Exists(GetLocalFileName(settlement));
        }        
    }
}