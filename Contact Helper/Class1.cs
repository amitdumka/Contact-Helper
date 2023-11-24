using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Microsoft.Maui.Controls.Internals.Profile;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Contact_Helper
{
    public class APIServer
    {

        public static string AuthKey = null;

        private static string InstallationId = null;

        public static ReturnData ReturnData = null;
        public static string BaseUrl = "https://account-asia-south1.truecaller.com/v2";
        public static string OTPReguest_url = "https://account-asia-south1.truecaller.com/v2/sendOnboardingOtp";
        private static HttpClient _client;
        private static JsonSerializerOptions _serializerOptions;

        private static readonly string authorizationKey;

        public APIServer()
        {
            _client = GetAuthClient();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }
        private static HttpClient GetAuthClient()
        {
            if (_client != null)
                return _client;
            var handler2 = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            };
            _client = new HttpClient(handler2);
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("accept-encoding", "gzip");
            _client.DefaultRequestHeaders.Add("user-agent", "Truecaller/11.75.5 (Android;10)");
            _client.DefaultRequestHeaders.Add("clientsecret", "lvc22mp3l1sfv6ujg83rd17btt");

            return _client;
        }

        private static HttpClient GetClient()
        {
            //HttpsClientHandlerService handler = new HttpsClientHandlerService();
            HttpsClientHandlerService handler = new HttpsClientHandlerService();
            var handler2 = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            };
            HttpClient client = new HttpClient(handler2)
            {
                //HttpClient client = new HttpClient(handler.GetPlatformMessageHandler());// new HttpClient();
                BaseAddress = new Uri(APIServer.BaseUrl)
            };// new HttpClient();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }



        public static async Task<bool> LoginRequestAsync(string phoneNumber)
        {
            var cCode = "IN";
            var dCode = "+91";
            Uri uri = new Uri(OTPReguest_url);

            LoginData data = new LoginData
            {
                PhoneNumber = phoneNumber,
                countryCode = cCode,
                DialingCode = dCode,
                Region = "region-2",
                SequenceNo = 2,
                InstallationDetails = new InstallationDetails
                {
                    Language = "en",
                    App = new AppData { BuildVersion = "5", MajorVersion = "11", MinorVersion = "7", Store = "GOOGLE_STORE" },
                    Device = new Device
                    {
                        Language = "en",
                        DeviceId = RandomString(16),
                        Manufacturer = "OnePlus",
                        MobileServices = new string[] { "GSM" },
                        Model = "OnePlus 8T",
                        OsName = "Android",
                        OsVersion = "10"
                    }
                }
            };

            try
            {

              var data2 = "{\"countryCode\": IN,\"dialingCode\": +91, \"installationDetails\": {\"app\": {\"buildVersion\": 5,\"majorVersion\": 11,\"minorVersion\": 7,\"store\": \"GOOGLE_PLAY\",},\"device\": {\"deviceId\": await generate_random_string(16),\"language\": \"en\", \"manufacturer\": device[\"manufacturer\"],\"model\": device[\"model\"], \"osName\": \"Android\",\"osVersion\": \"10\",\"mobileServices\": [\"GMS\"],},\"language\": \"en\",},\"phoneNumber\": 7779997556,\"region\": \"region-2\",\"sequenceNo\": 2,}";

            // string json = JsonSerializer.Serialize<LoginData>(data, _serializerOptions);
             StringContent content = new StringContent(data2, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;

            _client = GetAuthClient();
            response = await _client.PostAsync(uri, content);


            if (response.IsSuccessStatusCode)
            {
                Notify.NotifyVShort("Save successfully");
                //var obj = await response.Content.ReadFromJsonAsync<T>();
                return true;
            }
            else
            {
                Notify.NotifyVShort($"Failed Get otp, {response.ReasonPhrase}");
                return false;
            }
        }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
                Notify.NotifyVShort($"Error, {ex.Message}");
                return false;
            }
}
public static async Task<bool> OTPVerifAsync(string phoneNumber, string otp)
{
    var url = "https://account-asia-south1.truecaller.com/v1/verifyOnboardingOtp";
    var requestId = "";

    var cCode = "+91";
    var dCode = "In";

    Uri uri = new Uri(url);
    string post_data = $" {{ \"countryCode\": {cCode}, \"dialingCode\": {dCode},\"phoneNumber\": {phoneNumber},\"requestId\": json_data[\"requestId\"],\"token\": {otp} }}";

    try
    {
        string json = post_data;// JsonSerializer.Serialize<LoginData>(post_data, _serializerOptions);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = null;


        response = await _client.PostAsync(uri, content);


        if (response.IsSuccessStatusCode)
        {
            Notify.NotifyVShort("Save successfully");
            AuthKey = await response.Content.ReadAsStringAsync();

            return true;
        }
        else
        {
            Notify.NotifyVShort($"Failed Get otp, {response.ReasonPhrase}");
            return false;
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine(@"\tERROR {0}", ex.Message);
        Notify.NotifyVShort($"Error, {ex.Message}");
        return false;
    }
}


public static async Task<ReturnData> SearchNumber(string countryCode, string phoneNumber, string searchType)
{
    // a1i0G--jKJWQUkNVMDc1aAXSfd_UU4ILsbSjOe991oDag9TfXtINJuEiS72ToXQC
    string url = "";
    Uri uri = new Uri(url);
    var headers = $"{{ \"content-type\": \"application/json; charset=UTF-8\", \"accept-encoding\": \"gzip\", \"user-agent\": \"Truecaller/11.75.5 (Android;10)\", \"Authorization\": f\"Bearer {InstallationId}\" }}";
    var pms = $"{{ \"q\": {phoneNumber}, \"countryCode\": {countryCode}, \"type\": 4, \"locAddr\": \"\", \"placement\": \"SEARCHRESULTS,HISTORY,DETAILS\", \"encoding\": \"json\" }}";

    try
    {
        try
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", InstallationId);

            HttpResponseMessage response = await _client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<ReturnData>(content);
                return data;
            }
            else
            {
                Notify.NotifyLong($"\tERROR {response.StatusCode} # {response.ReasonPhrase}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"\tERROR {0}", ex.Message);
            Notify.NotifyLong($"\tERROR {ex.Message}");
            return null;
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine(@"\tERROR {0}", ex.Message);
        Notify.NotifyVShort($"Error, {ex.Message}");
        return null;
    }
}
private static Random random = new Random();

public static string RandomString(int length)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
}
    }
    public class AppData
{
    public string BuildVersion { get; set; }
    public string MajorVersion { get; set; }
    public string MinorVersion { get; set; }
    public string Store { get; set; }
}
public class Device
{
    public string DeviceId { get; set; }// await generate_random_string(16),
    public string Language { get; set; }// public string enpublic string ,
    public string Manufacturer { get; set; }// device[public string manufacturerpublic string ],
    public string Model { get; set; }// device[public string modelpublic string ],
    public string OsName { get; set; }// public string Androidpublic string ,
    public string OsVersion { get; set; }// public string 10public string ,
    public string[] MobileServices { get; set; }// [public string GMSpublic string ],
}
public class InstallationDetails
{
    public AppData App { get; set; }
    public Device Device { get; set; }
    public string Language { get; set; } = "en";
}

public class LoginData
{
    public string countryCode { get; set; }
    public string DialingCode { get; set; }
    public InstallationDetails InstallationDetails { get; set; }


    public string PhoneNumber { get; set; }
    public string Region { get; set; }
    public int SequenceNo { get; set; }
}

public class Headers
{
    public string Content_Type { get; set; } = "application/json; charset=UTF-8";
    public string Accept_Encoding { get; set; } = "gzip";
    public string User_Agent { get; set; } = "Truecaller/11.75.5 (Android;10)";
    public string Clientsecret { get; set; } = "lvc22mp3l1sfv6ujg83rd17btt";
}

public class Conts
{
    public const string Headers = "{ \"content-type\": \"application/json; charset=UTF-8\",        \"accept-encoding\": \"gzip\",        \"user-agent\": \"Truecaller/11.75.5 (Android;10)\",        \"clientsecret\": \"lvc22mp3l1sfv6ujg83rd17btt\",    }";
}

public class Util
{
    public static string GetPhoneCountryCode(string phoneNumber)
    {
        return "+91";
    }

    public static string GetRandomDevice(int len = 10)
    {

        return "";
    }
}



public class HttpsClientHandlerService
{
    public HttpMessageHandler GetPlatformMessageHandler()
    {
#if ANDROID
        var handler = new Xamarin.Android.Net.AndroidMessageHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (cert != null && cert.Issuer.Equals("CN=localhost"))
                return true;
            return errors == System.Net.Security.SslPolicyErrors.None;
        };
        return handler;
#elif IOS
        var handler = new NSUrlSessionHandler
        {
            TrustOverrideForUrl = IsHttpsLocalhost
        };
        return handler;
#else
        throw new PlatformNotSupportedException("Only Android and iOS supported.");
#endif
    }

#if IOS
    public bool IsHttpsLocalhost(NSUrlSessionHandler sender, string url, Security.SecTrust trust)
    {
        if (url.StartsWith("https://localhost"))
            return true;
        return false;
    }
#endif
}












}

/*
 
 
 
 
 {
  "status_code": 200,
  "data": {
    "data": [
      {
        "id": "vTp4mdQysp5VZLkDNEMaNg==",
        "name": "Shalini Sah",
        "imId": "1h9i33e9bkmww",
        "gender": "UNKNOWN",
        "image": "https://images-noneu.truecallerstatic.com/myview/1/5c3cb39d190                                                                                                                                                             22b7bc8bc69ee79ea8a4b/1",
        "score": 0.9,
        "access": "PUBLIC",
        "enhanced": true,
        "phones": [
          {
            "e164Format": "+918409201476",
            "numberType": "MOBILE",
            "nationalFormat": "084092 01476",
            "dialingCode": 91,
            "countryCode": "IN",
            "carrier": "Airtel",
            "type": "openPhone"
          }
        ],
        "addresses": [
          {
            "address": "IN",
            "city": "Bihar",
            "countryCode": "IN",
            "timeZone": "+05:30",
            "type": "address"
          }
        ],
        "internetAddresses": [
          {
            "id": "salini.vandana@gmail.com",
            "service": "email",
            "caption": "Shalini Sah",
            "type": "internetAddress"
          }
        ],
        "badges": [
          "user"
        ],
        "tags": [],
        "cacheTtl": 1296000000,
        "sources": [],
        "searchWarnings": [],
        "surveys": [
          {
            "id": "3b5c458f-ca99-411b-b10d-eedbed25b6a1",
            "frequency": 86400,
            "passthroughData": "eyAiMiI6ICJTaGFsaW5pIFNhaCIsICI4IjogIjAiLCAiNCI6                                                                                                                                                             ICJwZiIsICIzIjogIjMiIH0=",
            "perNumberCooldown": 31536000,
            "dynamicContentAccessKey": ""
          },
          {
            "id": "151bda50-4e80-434c-90b7-83b67bf7b1fd",
            "frequency": 86400,
            "passthroughData": "eyAiMiI6ICJTaGFsaW5pIFNhaCIsICI4IjogIjAiLCAiNCI6                                                                                                                                                             ICJwZiIsICIzIjogIjMiIH0=",
            "perNumberCooldown": 31536000,
            "dynamicContentAccessKey": ""
          }
        ],
        "commentsStats": {
          "showComments": false
        },
        "manualCallerIdPrompt": false,
        "ns": 0
      }
    ],
    "provider": "ss-nu",
    "stats": {
      "sourceStats": []
    }
  }
}

 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 */
