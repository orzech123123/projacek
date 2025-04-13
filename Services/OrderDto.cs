namespace react_app.Services
{
    public class OrderDto
    {
        //public DateTime Date { get; set; }
        public string ProviderOrderId { get; set; }
        public OrderProviderType ProviderType { get; set; }
        //public string Name { get; set; }
        public string Codes { get; set; }

        public int Quantity { get; set; }

        public bool IsValid => !string.IsNullOrEmpty(ProviderOrderId) &&
            //!string.IsNullOrEmpty(Name) &&
            Quantity > 0 &&
            !string.IsNullOrEmpty(Codes);
    }
}
