namespace react_app.Apilo
{
    public class ApiloAccessTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(RefreshToken);
    }
}