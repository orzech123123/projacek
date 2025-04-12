namespace react_app.Apilo
{
    public class ApiloSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Url { get; set; }
        public string AuthCode { get; set; }

        public string Base64Bearer => Base64Encode($"{ClientId}:{ClientSecret}");

        public string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
