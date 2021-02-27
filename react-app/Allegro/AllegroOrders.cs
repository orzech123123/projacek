using System;
using System.Collections.Generic;

namespace react_app.Apaczka
{
    public class AllegroCheckoutFormsResponse
    {
        public List<AllegroCheckoutForm> CheckoutForms { get; set; }
    }

    public class AllegroCheckoutForm
    {
        public string Id { get; set; }
        public List<AllegroCheckoutFormLineItem> LineItems { get; set; }
    }

    public class AllegroCheckoutFormLineItem
    {
        public string Id { get; set; }
        public AllegroCheckoutFormOffer Offer { get; set; }

    }

    public class AllegroCheckoutFormOffer
    {
        public string Id { get; set; }
        public string Name { get; set; }

    }


    public class AllegroSaleOffer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public AllegroSaleOfferExternal External { get; set; }

    }
    public class AllegroSaleOfferExternal
    {
        public string Id { get; set; }

    }
}
