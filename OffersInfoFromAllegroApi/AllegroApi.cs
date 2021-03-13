using OffersInfoFromAllegroApi.Dto;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
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

        private string _actualToken;

        public AllegroApi(string clientId, string clientSecret, string deviceCode)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _deviceCode = deviceCode;

            _refreshToken = GetRefreshTokenFromFile();
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

            SetRefreshTokenFromFile(respons.refresh_token);

            _actualToken   = respons.access_token;
        }


        public async Task ReadOffers()
        {
            RestClient restClient = new RestClient($"https://api.allegro.pl/sale/offers?limit=1000");
            restClient.AddDefaultHeader("Authorization", $"Bearer {_actualToken}");
            restClient.AddDefaultHeader("Accept", $"application/vnd.allegro.public.v1+json");


            RestRequest restRequest = new RestRequest();
            var respons = await restClient.ExecuteAsync(restRequest);

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
            File.WriteAllText(fileName, refreshToken); ;
        }
    }
}
