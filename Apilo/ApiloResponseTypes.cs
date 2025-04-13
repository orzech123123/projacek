using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace react_app.Apilo
{
    public class ApiloAccessTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(RefreshToken);
    }

    public class ApiloOrdersResponse
    {
        [JsonProperty("orders")]
        public List<ApiloOrder> Orders { get; set; }
    }

    public class ApiloOrder
    {
        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        //[JsonProperty("orderItems")]
        //public List<ApiloOrderItem> OrderItems { get; set; }
    }

    public class ApiloOrderDetailResponse
    {
        [JsonProperty("orderNotes")]
        public List<ApiloOrderNote> OrderNotes { get; set; }
    }

    public class ApiloOrderNote
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }


    //public class ApiloOrderItem
    //{
    //    [JsonProperty("originalName")]
    //    public string OriginalName { get; set; }

    //    [JsonProperty("originalCode")]
    //    public string OriginalCode { get; set; }

    //    // Removed other properties from ApiloOrderItem
    //}
}