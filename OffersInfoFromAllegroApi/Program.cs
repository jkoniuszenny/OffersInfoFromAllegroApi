using System;
using System.Threading.Tasks;

namespace OffersInfoFromAllegroApi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AllegroApi allegroApi = new AllegroApi(
                "195b6f90cf0d4fe58c531d4e94398655",
                "KJEMA69eqzpn1uSzB9aFuQm7DmLMG9MobYzF02dYEOkgiCRteGlMB6fKqCFzWu5P",
                "xZ3kLHP6XBLTESvLv1Q1is4Vi3bUAje2"
                );


            await allegroApi.RefreshToken();

            await allegroApi.ReadOffers();
        }
    }
}
