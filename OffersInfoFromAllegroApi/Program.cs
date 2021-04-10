using OffersInfoFromAllegroApi.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OffersInfoFromAllegroApi
{
    class Program
    {
        static async Task Main(string[] args)
        {

            AppSettings appSettings = new AppSettings();

            using (var reader = new StreamReader(Directory.GetCurrentDirectory() + "/appsettings.json"))
                appSettings = JsonSerializer.Deserialize<AppSettings>(reader.ReadToEnd());

            AllegroApi allegroApi = new AllegroApi(
                  appSettings.ClientId,
                  appSettings.ClientSecret,
                  appSettings.DeviceCode
                  );

            bool validate = await allegroApi.Validate();

            if (!validate)
            {
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Odnawiam token z Allegro ...");
            await allegroApi.RefreshToken();

            Console.WriteLine("Pobieram dane z Allegro ...");
            await allegroApi.ReadOffersFromAllegro();

            Console.WriteLine("Odczytuje dane z pliku ...");
            await allegroApi.ReadOffersFromFile();

            Console.WriteLine("Zapisuję plik z danymi z Allegro ...");
            await allegroApi.PrepareExcelFromAllegro();

            Console.WriteLine("Zapisuję plik z wynikami porówniania ...");
            await allegroApi.PrepareExcelCompareFile();

            Console.WriteLine("Wszystkie działania zakończone. Naciśnij dowolny przycisk aby zamnkąć.");

            Console.ReadKey();

        }
    }
}
