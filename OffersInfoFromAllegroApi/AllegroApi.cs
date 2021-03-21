using OffersInfoFromAllegroApi.Dto;
using OfficeOpenXml;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace OffersInfoFromAllegroApi
{
    public class AllegroApi
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _deviceCode;
        private readonly string _refreshToken;

        private OfferInputDto OfferInputDto;
        private List<FileToCompareDto> FileToCompare;
        private string ActualToken;

        public AllegroApi(string clientId, string clientSecret, string deviceCode)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _deviceCode = deviceCode;

            _refreshToken = GetRefreshTokenFromFile();
        }


        public async Task<bool> Validate()
        {
            if (!File.Exists(@$"Produkty.xlsx"))
            {
                Console.WriteLine("Brak pliku z produktami do porówania bądź błedna nazwa. Właściwa to: Produkty.xlsx");
                return false;
            }

            if (!File.Exists(@$"Token.txt"))
            {
                Console.WriteLine("Brak pliku z tokenem bądź błedna nazwa. Właściwa to: Token.txt");
                return false;
            }


            return true;
        }


        public async Task<string> GenerateToken()
        {
            string basicAuth = Base64Encode($"{_clientId}:{_clientSecret}");

            RestClient restClient = new RestClient($"https://allegro.pl/auth/oauth/token?grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Adevice_code&device_code={_deviceCode}");
            restClient.AddDefaultHeader("Authorization", $"Basic {basicAuth}");

            RestRequest restRequest = new RestRequest();
            var respons = await restClient.GetAsync<AuthJsonOutputDto>(restRequest);

            return respons.access_token;
        }

        public async Task RefreshToken()
        {
            string basicAuth = Base64Encode($"{_clientId}:{_clientSecret}");

            RestClient restClient = new RestClient($"https://allegro.pl/auth/oauth/token?grant_type=refresh_token&refresh_token={_refreshToken}");
            restClient.AddDefaultHeader("Authorization", $"Basic {basicAuth}");

            RestRequest restRequest = new RestRequest();
            var respons = await restClient.GetAsync<AuthJsonOutputDto>(restRequest);

            if (respons.access_token == null)
            {
                Console.WriteLine("Nie udało się pobrać nowego tokenu.");
                return;
            }

            SetRefreshTokenFromFile(respons.refresh_token);

            ActualToken = respons.access_token;
        }

        public async Task ReadOffersFromAllegro()
        {
            int limit = 1000;
            int offset = 0;

            RestClient restClient = new RestClient($"https://api.allegro.pl/sale/offers?limit={limit}&offset={offset}");
            restClient.AddDefaultHeader("Authorization", $"Bearer {ActualToken}");
            restClient.AddDefaultHeader("Accept", $"application/vnd.allegro.public.v1+json");


            RestRequest restRequest = new RestRequest();
            OfferInputDto = await restClient.GetAsync<OfferInputDto>(restRequest);

            int totalCount = OfferInputDto.totalCount;

            for (int i = 1; i < (int)(totalCount / limit) + 1; i++)
            {
                offset = limit * i;

                restClient = new RestClient($"https://api.allegro.pl/sale/offers?limit={limit}&offset={offset}");
                restClient.AddDefaultHeader("Authorization", $"Bearer {ActualToken}");
                restClient.AddDefaultHeader("Accept", $"application/vnd.allegro.public.v1+json");

                var tmpInput = await restClient.GetAsync<OfferInputDto>(restRequest);

                OfferInputDto.offers.AddRange(tmpInput.offers);

                await Task.Delay(1000);
            }

        }

        public async Task ReadOffersFromFile()
        {
            FileInfo fi = new FileInfo(@$"Produkty.xlsx");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var excelFile = new ExcelPackage(fi);

            var ws = excelFile.Workbook.Worksheets[0];

            var start = ws.Dimension.Start;
            var end = ws.Dimension.End;

            FileToCompare = new List<FileToCompareDto>();

            for (int row = start.Row + 1; row <= end.Row; row++)
            {
                string signature = ws.Cells[row, 1].Text;

                decimal price = 0;
                Decimal.TryParse(ws.Cells[row, 3].Text, out price);

                Offer offerFromAllegro = OfferInputDto.offers.FirstOrDefault(f => f.external?.id == signature);

                if (offerFromAllegro is { } && price != 0)
                {
                    FileToCompare.Add(new FileToCompareDto()
                    {
                        Signature = signature,
                        Price = price,
                        AllegroPrice = offerFromAllegro.sellingMode.price.amount
                    });
                }

            }
            await Task.CompletedTask;
        }


        public async Task PrepareExcelFromAllegro()
        {
            FileInfo xlsTmpFileName = new FileInfo($"ProduktyAllegro_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss")}.xlsx");
            ExcelPackage excelFile = new ExcelPackage(xlsTmpFileName);

            if (excelFile.Workbook.Worksheets.Count == 0)
            {
                excelFile.Workbook.Worksheets.Add("Allegro");
            }


            var ws = excelFile.Workbook.Worksheets[0];

            ws.Cells[1, 1].Value = "Sygnatura";
            ws.Cells[1, 2].Value = "Cena";

            int row = 2;

            foreach (var item in OfferInputDto.offers.Where(w=>w.external is { }))
            {
                ws.Cells[row, 1].Value = item.external.id;
                ws.Cells[row, 2].Value = item.sellingMode.price.amount;

                row++;
            }

            FileStream fs = new FileStream(xlsTmpFileName.FullName, FileMode.Create);
            excelFile.SaveAs(fs);

            fs.Close();

            await Task.CompletedTask;
        }

        public async Task PrepareExcelCompareFile()
        {
            FileInfo xlsTmpFileName = new FileInfo($"ProduktyPoPorownianiu_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss")}.xlsx");
            ExcelPackage excelFile = new ExcelPackage(xlsTmpFileName);

            if (excelFile.Workbook.Worksheets.Count == 0)
            {
                excelFile.Workbook.Worksheets.Add("Arkusz1");
            }
           

            var ws = excelFile.Workbook.Worksheets[0];

            ws.Cells[1, 1].Value = "Sygnatura";
            ws.Cells[1, 2].Value = "Cena Subiekt";
            ws.Cells[1, 3].Value = "Cena Allegro";
            ws.Cells[1, 4].Value = "Różnica co najmniej grosz";
            ws.Cells[1, 5].Value = "Różnica co najmniej 50%";
            ws.Cells[1, 6].Value = "Różnica wartość";

            foreach (var item in FileToCompare)
            {
                ws.Cells[FileToCompare.IndexOf(item) + 2, 1].Value = item.Signature;
                ws.Cells[FileToCompare.IndexOf(item) + 2, 2].Value = item.Price;
                ws.Cells[FileToCompare.IndexOf(item) + 2, 3].Value = item.AllegroPrice;
                ws.Cells[FileToCompare.IndexOf(item) + 2, 4].Value = item.Is1Difference ? "Tak" : "";
                ws.Cells[FileToCompare.IndexOf(item) + 2, 5].Value = item.Is50Difference ? "Tak" : "";
                ws.Cells[FileToCompare.IndexOf(item) + 2, 6].Value = item.DifferenceValue;
            }

            FileStream fs = new FileStream(xlsTmpFileName.FullName, FileMode.Create);
            excelFile.SaveAs(fs);

            fs.Close();

            await Task.CompletedTask;

        }








        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private string GetRefreshTokenFromFile()
        {
            string fileName = "Token.txt";
            return File.ReadAllText(fileName); ;
        }

        private void SetRefreshTokenFromFile(string refreshToken)
        {
            string fileName = "Token.txt";
            File.WriteAllText(fileName, refreshToken);
        }
    }
}
