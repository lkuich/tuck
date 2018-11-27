using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Tuck
{
    public struct Report {
        public string Name;
        public int Size, Successes;
        public double AvgConfidence;
    }
    public class GSheets
    {
        private const string ApplicationName = "Tensorflow Reporting";
        private string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private const string spreadsheetId = "...";
        private SheetsService SheetsService { get; set; }
        
        public GSheets()
        {
            UserCredential credential;
            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            SheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }
        
        public void AddExcelSheet(string name)
        {
            var range = new ValueRange();
            range.Values = new List<IList<object>> {
                new List<object>() { "Name" },
                new List<object>() { "Successes" },
                new List<object>() { "Avg Confidence" }
            };

            AddExcelRow(name, range, 1);
        }

        public void AddReportExcel(string name, Report report, int row)
        {
            var range = new ValueRange();
                range.Values = new List<IList<object>> {
                new List<object>() { report.Name },
                new List<object>() { $"{report.Successes}/{report.Size}" },
                new List<object>() { report.AvgConfidence }
            };

            AddExcelRow(name, range, row);
        }

        private void AddExcelRow(string name, ValueRange range, int row)
        {
            range.MajorDimension = "COLUMNS";
            var valueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            var request = SheetsService.Spreadsheets.Values.Update(range, spreadsheetId, $"{name}!A{row}");
            request.ValueInputOption = valueInputOption;

            request.Execute();
        }
    }
}