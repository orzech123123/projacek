using System;
using System.Collections.Generic;

namespace react_app.Allegro
{
    public class AllegroAccessTokenR
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(RefreshToken);
    }

    public class AllegroCheckoutForms
    {
        public List<AllegroCheckoutForm> CheckoutForms { get; set; }
    }

    public class AllegroCheckoutForm
    {
        public string Id { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<AllegroCheckoutFormLineItem> LineItems { get; set; }
    }

    public class AllegroCheckoutFormLineItem
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public AllegroCheckoutFormOffer Offer { get; set; }

    }

    public class AllegroCheckoutFormOffer
    {
        public string Id { get; set; }
        public string Name { get; set; }

    }

    public class AllegroSaleOffers
    {
        public List<AllegroSaleOffer> Offers { get; set; }
    }

    public class AllegroSaleOffer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public AllegroSaleOfferPublication Publication {get;set;}
        public AllegroSaleOfferExternal External { get; set; }
        public AllegroSaleOfferStock Stock { get; set; }

        public string Signature => External?.Id;
    }

    public class AllegroSaleOfferPublication
{
        public AllegroSaleOfferStatus Status { get; set; }

    }

    public enum AllegroSaleOfferStatus
    {
        Inactive,
        Active,
        Activating,
        Ended
    }

    public class AllegroSaleOfferExternal
    {
        public string Id { get; set; }
    }

    public class AllegroSaleOfferStock
    {
        public int Available { get; set; }
    }

    public class AllegroOfferCommand
    {
        public string Id { get; set; }
        public AllegroOfferCommandTaskCount TaskCount {get;set; }
    }

    public class AllegroOfferCommandTaskCount
    {
        public int? Total { get; set; }
        public int? Success { get; set; }
        public int? Failed { get; set; }
    }
}