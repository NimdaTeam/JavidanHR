using Newtonsoft.Json;
using System.Text;
using _0_Framework.DTO;

namespace _0_Framework.Utilities.SMSSender
{
    public static class SMSSender
    {
        private static readonly string ApiKey = "ijI9mwgL5_BnV2FmPdQjLeMQWWotr7LMLS4NKPtv350=";
        private static readonly string Sender = "3000505";
        private static readonly string ApiUrl = "https://api2.ippanel.com/api/v1/sms/pattern/normal/send";

        public static async Task<OperationResult> SendSmsAsync(string phoneNumber, string type, object data)
        {
            // اعتبارسنجی ورودی‌ها
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return new OperationResult { IsSuccessful = false };

            if (data == null)
                return new OperationResult { IsSuccessful = false };

            var pattern = GetPattern(type);
            if (string.IsNullOrEmpty(pattern))
                return new OperationResult { IsSuccessful = false };

            using (var client = new HttpClient())
            {
                var body = new
                {
                    code = pattern,
                    sender = Sender,
                    recipient = phoneNumber,
                    variable = data
                };

                var jsonData = JsonConvert.SerializeObject(body, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Add("apikey", ApiKey);

                try
                {
                    var response = await client.PostAsync(ApiUrl, content);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic responseData = JsonConvert.DeserializeObject(responseBody);

                        if (responseData?.code != null && responseData?.data != null)
                        {
                            return new OperationResult
                            {
                                IsSuccessful = true,
                            };
                        }
                        else
                        {
                            return new OperationResult
                            {
                                IsSuccessful = false,
                            };
                        }
                    }
                    else
                    {
                        return new OperationResult
                        {
                            IsSuccessful = false,
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new OperationResult
                    {
                        IsSuccessful = false,
                        Exception = ex
                    };
                }
            }
        }

        private static string GetPattern(string type)
        {
            switch (type?.ToLower())
            {
                case "otp":
                    return "40g6jl867x6tciz";
                case "newrequest":
                    return "hxr94tqxl75iawv";
                case "newmessage":
                    return "4u56bw5hbmkcgaa";
                case "refer":
                    return "lz29nbch6ik6baf";
                default:
                    return "";
            }
        }
    }
}