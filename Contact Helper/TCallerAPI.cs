namespace Contact_Helper
{
    public static class SessionData
    {
        public static string PhoneNumber { get; set; }
        public static string InstallId { get; set; }
        public static string CCode { get; set; }
        public static string Status { get; set; }

        public static string RequestId { get; set; }

        public static string ResonseData {  get; set; }

    }
    public class TCallerAPI
    {
        
        //public static bool SessionToJsonFile() { }
        //public static bool JsonFileToSession() { }
        
        public static async Task<bool> DoLoginCheckAsync(string PhoneNumber)
        {
            var lc = await APIServer.LoginCheckAsync(PhoneNumber);
            if (lc != null)
            {
                if (lc.Status == "error")
                {
                    Notify.NotifyVShort(lc.ErrMsg);
                    return false;
                }
                SessionData.PhoneNumber = lc.PhoneNumber;
                SessionData.InstallId = lc.InstallationId;
                SessionData.CCode = lc.CCode;
                SessionData.Status = "LoggedIn";
                return true;

            }
            else { return false; }
        }

        public static Task<SearchData> SearchNumber(string PhoneNumber, bool name, bool email, bool raw)
        {
            return APIServer.SearchNumber(PhoneNumber, name, email, raw);
        }

        public static async Task<bool> DoLogin(string PhoneNumber)
        {
            var ld = await APIServer.LoginRequestAsync(PhoneNumber);
            if (ld != null)
            {
                if (ld.Status != "error" && ld.Data != null)
                {
                    SessionData.PhoneNumber = "+" + ld.Data.ParsedPhoneNumber;
                    SessionData.RequestId = ld.Data.RequestId;
                    Notify.NotifyVLong($"OTP is {ld.Data.Message} through {ld.Data.Method} for MobileNo +{ld.Data.ParsedPhoneNumber}, and it {ld.Status} ");
                    return true;
                }
                else return false;
            }
            return false;
        }

        public static async Task<bool> VerifyOTP(string otp)
        {
            var ld = await APIServer.OTPVerifAsync(SessionData.PhoneNumber, otp, SessionData.RequestId);
            if (ld != null)
            {
                if (ld.Status != "error")
                {
                    SessionData.InstallId = ld.InstallationId;
                    SessionData.ResonseData = ld.Response;
                    Notify.NotifyVLong($"Otp is verifed and logged in, {ld.Status}");
                    return true;

                }else return false;
            }
            else return false;
        }

    }
    public class LoginCheck
    {
        public string? ErrMsg { get; set; }
        public string? CCode { get; set; }
        public string? InstallationId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Status { get; set; }

    }
    public class DataOTP
    {

        public string Domain { get; set; }
        public string Message { get; set; }
        public string Method { get; set; }
        public string ParsedCountryCode { get; set; }
        public string ParsedPhoneNumber { get; set; }
        public string RequestId { get; set; }
        public int Status { get; set; }
        public int TokenTtl { get; set; }
    }
    public class LoginOtp
    {
        public string Status { get; set; }
        public DataOTP Data { get; set; }

    }

    public class LoginReturn
    {
        public string Status { get; set; }
        public string InstallationId { get; set; }
        public string Response { get; set; }


    }

}

/*
 {
    "searchResult": "{\n  \"status_code\": 200,\n  \"data\": {\n    \"data\": [\n      {\n        \"id\": \"nFVPNP7DT4D4JidoIZAFVA==\",\n        \"name\": \"Deogarh Amit Kumar\",\n        \"score\": 0.3010747,\n        \"access\": \"PUBLIC\",\n        \"enhanced\": true,\n        \"phones\": [\n          {\n            \"e164Format\": \"+9831213339\",\n            \"numberType\": \"FIXED_LINE\",\n            \"nationalFormat\": \"31213339\",\n            \"dialingCode\": 98,\n            \"countryCode\": \"IR\",\n            \"carrier\": \"TCI\",\n            \"type\": \"openPhone\"\n          }\n        ],\n        \"addresses\": [\n          {\n            \"city\": \"Isfahan\",\n            \"countryCode\": \"IR\",\n            \"type\": \"address\"\n          }\n        ],\n        \"internetAddresses\": [],\n        \"badges\": [],\n        \"tags\": [],\n        \"nameFeedback\": {\n          \"nameSource\": 1,\n          \"nameElectionAlgo\": \"1\"\n        },\n        \"cacheTtl\": 1296000000,\n        \"sources\": [],\n        \"searchWarnings\": [],\n        \"surveys\": [\n          {\n            \"id\": \"3b5c458f-ca99-411b-b10d-eedbed25b6a1\",\n            \"frequency\": 86400,\n            \"passthroughData\": \"eyAiOCI6ICIxIiwgIjIiOiAiRGVvZ2FyaCBBbWl0IEt1bWFyIiwgIjMiOiAiMyIsICI1IjogIjEiLCAiNCI6ICJ1Z2MiIH0=\",\n            \"perNumberCooldown\": 31536000,\n            \"dynamicContentAccessKey\": \"\"\n          },\n          {\n            \"id\": \"151bda50-4e80-434c-90b7-83b67bf7b1fd\",\n            \"frequency\": 86400,\n            \"passthroughData\": \"eyAiOCI6ICIxIiwgIjIiOiAiRGVvZ2FyaCBBbWl0IEt1bWFyIiwgIjMiOiAiMyIsICI1IjogIjEiLCAiNCI6ICJ1Z2MiIH0=\",\n            \"perNumberCooldown\": 31536000,\n            \"dynamicContentAccessKey\": \"\"\n          }\n        ],\n        \"commentsStats\": {\n          \"showComments\": false\n        },\n        \"manualCallerIdPrompt\": false,\n        \"ns\": 7\n      }\n    ],\n    \"provider\": \"ss-nu\",\n    \"stats\": {\n      \"sourceStats\": []\n    }\n  }\n}",
    "status": "ok"
}

{
    "searchResult": {
        "data": {
            "data": [
                {
                    "access": "PUBLIC",
                    "addresses": [
                        {
                            "city": "Isfahan",
                            "countryCode": "IR",
                            "type": "address"
                        }
                    ],
                    "badges": [],
                    "cacheTtl": 1296000000,
                    "commentsStats": {
                        "showComments": false
                    },
                    "enhanced": true,
                    "id": "nFVPNP7DT4D4JidoIZAFVA==",
                    "internetAddresses": [],
                    "manualCallerIdPrompt": false,
                    "name": "Deogarh Amit Kumar",
                    "nameFeedback": {
                        "nameElectionAlgo": "1",
                        "nameSource": 1
                    },
                    "ns": 7,
                    "phones": [
                        {
                            "carrier": "TCI",
                            "countryCode": "IR",
                            "dialingCode": 98,
                            "e164Format": "+9831213339",
                            "nationalFormat": "31213339",
                            "numberType": "FIXED_LINE",
                            "type": "openPhone"
                        }
                    ],
                    "score": 0.3010747,
                    "searchWarnings": [],
                    "sources": [],
                    "surveys": [
                        {
                            "dynamicContentAccessKey": "",
                            "frequency": 86400,
                            "id": "3b5c458f-ca99-411b-b10d-eedbed25b6a1",
                            "passthroughData": "eyAiNCI6ICJ1Z2MiLCAiMiI6ICJEZW9nYXJoIEFtaXQgS3VtYXIiLCAiNSI6ICIxIiwgIjgiOiAiMSIsICIzIjogIjMiIH0=",
                            "perNumberCooldown": 31536000
                        },
                        {
                            "dynamicContentAccessKey": "",
                            "frequency": 86400,
                            "id": "151bda50-4e80-434c-90b7-83b67bf7b1fd",
                            "passthroughData": "eyAiNCI6ICJ1Z2MiLCAiMiI6ICJEZW9nYXJoIEFtaXQgS3VtYXIiLCAiNSI6ICIxIiwgIjgiOiAiMSIsICIzIjogIjMiIH0=",
                            "perNumberCooldown": 31536000
                        }
                    ],
                    "tags": []
                }
            ],
            "provider": "ss-nu",
            "stats": {
                "sourceStats": []
            }
        },
        "status_code": 200
    },
    "status": "ok"
}
 
 */

