using System;
using System.Collections.Generic;
using System.Text;

namespace OffersInfoFromAllegroApi.Dto
{
    public class FileToCompareDto
    {
        public string Signature { get; set; }
        public decimal Price { get; set; }
        public decimal AllegroPrice { get; set; }

        public bool Is1Difference => Math.Abs((Price - AllegroPrice)) > Convert.ToDecimal(0.01);
        public bool Is50Difference => Math.Abs((Price - AllegroPrice)) / Price > Convert.ToDecimal(0.5);
        public decimal DifferenceValue => Price - AllegroPrice;
    }
}
