using System.Text.Json.Serialization;

namespace Contact_Helper
{
    public class SearchData
    {
        public SearchResult SearchResult { get; set; }
        public string Status { get; set; }
    }
    public class SearchResult
    {
        //public int? Status_Code {  get; set; }    
        public RootData Data { get; set; }
    }


    public class ReturnData
    {
        [JsonPropertyName("status_code")]
        public int? StatusCode { get; set; }

        [JsonPropertyName("data")]
        public RootData Data { get; set; }
    }
    public class RootData
    {
        [JsonPropertyName("data")]
        public List<Data> Data { get; set; }
        [JsonPropertyName("provider")]
        public string Provider { get; set; }

        [JsonPropertyName("stats")]
        public Stats Stats { get; set; }


    }


    public class Data
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("imId")]
        public string ImId { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("score")]
        public double? Score { get; set; }

        [JsonPropertyName("access")]
        public string Access { get; set; }

        [JsonPropertyName("enhanced")]
        public bool? Enhanced { get; set; }


        [JsonPropertyName("phones")]
        public List<Phone> Phones { get; set; }

        [JsonPropertyName("addresses")]
        public List<Addresses> Addresses { get; set; }

        [JsonPropertyName("internetAddresses")]
        public List<InternetAddress> InternetAddresses { get; set; }

        [JsonPropertyName("badges")]
        public List<string> Badges { get; set; }

        [JsonPropertyName("tags")]
        public List<object> Tags { get; set; }

        [JsonPropertyName("cacheTtl")]
        public int? CacheTtl { get; set; }

        [JsonPropertyName("sources")]
        public List<object> Sources { get; set; }

        [JsonPropertyName("searchWarnings")]
        public List<object> SearchWarnings { get; set; }

        [JsonPropertyName("surveys")]
        public List<Survey> Surveys { get; set; }

        [JsonPropertyName("commentsStats")]
        public CommentsStats CommentsStats { get; set; }

        [JsonPropertyName("manualCallerIdPrompt")]
        public bool? ManualCallerIdPrompt { get; set; }

        [JsonPropertyName("ns")]
        public int? Ns { get; set; }
    }

    public class Stats
    {
        [JsonPropertyName("sourceStats")]
        public List<object> SourceStats { get; set; }
    }
    public class Phone
    {
        [JsonPropertyName("e164Format")]
        public string E164Format { get; set; }

        [JsonPropertyName("numberType")]
        public string NumberType { get; set; }

        [JsonPropertyName("nationalFormat")]
        public string NationalFormat { get; set; }

        [JsonPropertyName("dialingCode")]
        public int? DialingCode { get; set; }

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("carrier")]
        public string Carrier { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
    public class Survey
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("frequency")]
        public int? Frequency { get; set; }

        [JsonPropertyName("passthroughData")]
        public string PassthroughData { get; set; }

        [JsonPropertyName("perNumberCooldown")]
        public int? PerNumberCooldown { get; set; }

        [JsonPropertyName("dynamicContentAccessKey")]
        public string DynamicContentAccessKey { get; set; }
    }

    public class InternetAddress
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("service")]
        public string Service { get; set; }

        [JsonPropertyName("caption")]
        public string Caption { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Addresses
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("timeZone")]
        public string TimeZone { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class CommentsStats
    {
        [JsonPropertyName("showComments")]
        public bool? ShowComments { get; set; }
    }

}
