using System;
using System.Threading.Tasks;

namespace OffersInfoFromAllegroApi
{
    class Program
    {
        static async Task Main(string[] args)
        {

            AllegroApi allegroApi = new AllegroApi(
                "clientId",
                "clientSecret",
                "deviceCode"
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
