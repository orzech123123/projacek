using System;
using System.Collections.Generic;

namespace react_app.Allegro
{
    public class AllegroAccessTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(RefreshToken);
    }

    public class AllegroCheckoutFormsResponse
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

    public class AllegroSaleOffer
    {
        public string Name { get; set; }
        public AllegroSaleOfferExternal External { get; set; }

    }

    public class AllegroSaleOfferExternal
    {
        public string Id { get; set; }

    }
}
