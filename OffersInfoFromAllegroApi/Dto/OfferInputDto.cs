using System;
using System.Collections.Generic;
using System.Text;

namespace OffersInfoFromAllegroApi.Dto
{
    public class Category
    {
        public string id { get; set; }
    }

    public class PrimaryImage
    {
        public string url { get; set; }
    }

    public class Price
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
    }

    public class SellingMode
    {
        public string format { get; set; }
        public Price price { get; set; }
        public decimal minimalPrice { get; set; }
        public decimal startingPrice { get; set; }
    }

    public class SaleInfo
    {
        public decimal currentPrice { get; set; }
        public int biddersCount { get; set; }
    }

    public class Stats
    {
        public int watchersCount { get; set; }
        public int visitsCount { get; set; }
    }

    public class Stock
    {
        public int available { get; set; }
        public int sold { get; set; }
    }

    public class Publication
    {
        public string status { get; set; }
        public object startingAt { get; set; }
        public DateTime startedAt { get; set; }
        public object endingAt { get; set; }
        public object endedAt { get; set; }
    }

    public class Warranty
    {
        public string id { get; set; }
    }

    public class ReturnPolicy
    {
        public string id { get; set; }
    }

    public class ImpliedWarranty
    {
        public string id { get; set; }
    }

    public class AfterSalesServices
    {
        public Warranty warranty { get; set; }
        public ReturnPolicy returnPolicy { get; set; }
        public ImpliedWarranty impliedWarranty { get; set; }
    }

    public class ShippingRates
    {
        public string id { get; set; }
    }

    public class Delivery
    {
        public ShippingRates shippingRates { get; set; }
    }

    public class External
    {
        public string id { get; set; }
    }

    public class Offer
    {
        public string id { get; set; }
        public string name { get; set; }
        public Category category { get; set; }
        public PrimaryImage primaryImage { get; set; }
        public SellingMode sellingMode { get; set; }
        public SaleInfo saleInfo { get; set; }
        public Stats stats { get; set; }
        public Stock stock { get; set; }
        public Publication publication { get; set; }
        public AfterSalesServices afterSalesServices { get; set; }
        public object additionalServices { get; set; }
        public External external { get; set; }
        public Delivery delivery { get; set; }
    }

    public class OfferInputDto
    {
        public List<Offer> offers { get; set; }
        public int count { get; set; }
        public int totalCount { get; set; }
    }

}
