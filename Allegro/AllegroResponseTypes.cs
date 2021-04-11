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
        public AllegroSaleOfferCategory Category { get; set; }
        public AllegroSaleOfferLocation Location { get; set; }
        public AllegroSaleOfferPayments Payments { get; set; }
        public AllegroSaleOfferSellingMode SellingMode { get; set; }
        public AllegroSaleOfferDescription Description { get; set; }
        public AllegroSaleOfferDelivery Delivery { get; set; }
        public AllegroSaleOfferProduct Product { get; set; }
        public AllegroSaleOfferAfterSalesServices AfterSalesServices { get; set; }
        public List<AllegroSaleOfferParameter> Parameters { get; set; }
        public List<AllegroSaleOfferImage> Images { get; set; }

        public string Signature => External?.Id;
    }

    public class AllegroSaleOfferImage
    {
        public string Url { get; set; }
    }

    public class AllegroSaleOfferParameter
    {
        public string Id { get; set; }
        public List<string> ValuesIds { get; set; }
        public List<string> Values { get; set; }
        public AllegroSaleOfferParameterRangeValue RangeValue { get; set; }
    }

    public class AllegroSaleOfferParameterRangeValue
    {
        public string From { get; set; }
        public string To { get; set; }
    }

    public class AllegroSaleOfferProduct
    {
        public string Id { get; set; }
    }

    public class AllegroSaleOfferAfterSalesServices
    {
        public AllegroSaleOfferAfterSalesServicesId ImpliedWarranty { get; set; }
        public AllegroSaleOfferAfterSalesServicesId ReturnPolicy { get; set; }
        public AllegroSaleOfferAfterSalesServicesId Warranty { get; set; }
    }

    public class AllegroSaleOfferAfterSalesServicesId
    {
        public string Id { get; set; }
    }

    public class AllegroSaleOfferDelivery
    {
        public string ShipmentDate { get; set; }
        public string AdditionalInfo { get; set; }
        public string HandlingTime { get; set; }
        public AllegroSaleOfferDeliveryShippingRates ShippingRates { get; set; }
    }

    public class AllegroSaleOfferDeliveryShippingRates
    {
        public string Id { get; set; }
    }

    public class AllegroSaleOfferDescription
    {
        public List<AllegroSaleOfferDescriptionSection> Sections { get; set; }
    }

    public class AllegroSaleOfferDescriptionSection
    {
        public List<AllegroSaleOfferDescriptionSectionItem> Items { get; set; }
    }
    
    public class AllegroSaleOfferDescriptionSectionItem
    {
        public string Type { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
    }

    public class AllegroSaleOfferSellingMode
    {
        public string Format { get; set; }
        public AllegroSaleOfferSellingModePrice Price { get; set; }
        public AllegroSaleOfferSellingModePrice MinimalPrice { get; set; }
        public AllegroSaleOfferSellingModePrice StartingPrice { get; set; }
        public AllegroSaleOfferSellingModePrice NetPrice { get; set; }
    }

    public class AllegroSaleOfferSellingModePrice
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }

    public class AllegroSaleOfferPayments
    {
        public string Invoice { get; set; }
    }
    
    public class AllegroSaleOfferPublication
    {
        public AllegroSaleOfferStatus Status { get; set; }
    }

    public class AllegroSaleOfferCategory
    {
        public string Id { get; set; }
    }

    public class AllegroSaleOfferLocation
    {
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string PostCode { get; set; }
        public string Province { get; set; }
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
        public string Unit { get; set; }
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