using _0_Framework.Utilities.Helpers;
using AttendanceSystem.Infrastructure.Dto;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using _0_Framework.Utilities.Security;
using Microsoft.VisualBasic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AttendanceSystem.Infrastructure.ApiHelper
{
    public  class AttendanceSystemApiHelper
    {
        private protected readonly string AttendanceApiUrl = "https://att.hsu.ac.ir/";
        private protected readonly string AttendanceApiKey = "MyVEAPIhMdnhkmOssw431We";
        private readonly ILogger<AttendanceSystemApiHelper> _logger;

        public AttendanceSystemApiHelper( ILogger<AttendanceSystemApiHelper> logger)
        {
            _logger = logger;
        }

        public async Task<RangeFunctionApiResult> GetRangeApiResult(AttendanceRecordsWithRangeRequest request)
        {
            try
            {
                _logger.LogInformation("-------------------------------------------------------------------------------");
                _logger.LogInformation($"started function GetRangeApiResult with parameters: from={request.From} | to={request.To} | personalCode= {request.PersonalCode ?? "null"} at {DateTime.Now}");

                var baseAddress = AttendanceApiUrl + "sessions/range?";

                using var client = new HttpClient();

                if (!string.IsNullOrWhiteSpace(request.From))
                {
                    baseAddress += $"from={request.From}";
                }
                else
                {
                    baseAddress += $"from={DateTime.Now.GetStartOfDate()}";
                }

                if (!string.IsNullOrWhiteSpace(request.To))
                {
                    baseAddress += $"&to={request.To}";
                }
                else
                {
                    baseAddress += $"&to={DateTime.Now.GetEndOfDate()}";
                }

                if (!string.IsNullOrWhiteSpace(request.PersonalCode))
                {
                    baseAddress += $"&personal_code={request.PersonalCode}";
                }

                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Add("X-API-Key", AttendanceApiKey);

                var result = await client.GetFromJsonAsync<AttendanceLogVM>(client.BaseAddress);

               
                _logger.LogInformation($"Ended function GetRangeApiResult with result: SUCCESS at {DateTime.Now}");
                _logger.LogInformation("-------------------------------------------------------------------------------");

                return new RangeFunctionApiResult()
                {
                    ApiResult = new ApiResult()
                    {
                        Result = "success"
                    },
                    Data = result
                };
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Ended function GetRangeApiResult with result: FAIL at {DateTime.Now}");
                _logger.LogInformation("-------------------------------------------------------------------------------");

                return new RangeFunctionApiResult()
                {
                    ApiResult = new ApiResult()
                    {
                        Result = "Failed"
                    }
                };
            }
        }

        public async Task<AttendanceReport?> GetFullAttendanceReport(FullAttendanceReportRequest request)
        {
            try
            {
                _logger.LogInformation("-------------------------------------------------------------------------------");
                _logger.LogInformation($"started function GetRangeApiResult with parameters: from={request.From} | to={request.To} | personalCode= {request.PersonalCode ?? "null"} at {DateTime.Now}");

                var baseAddress = AttendanceApiUrl + "reports/daily-attendance?";

                using var client = new HttpClient();

                if (!string.IsNullOrWhiteSpace(request.From))
                {
                    baseAddress += $"start_date={Strings.Left(request.From.SanitizeString(),10)}";
                }
                else
                {
                    baseAddress += $"start_date={Strings.Left(DateTime.Now.GetStartOfDate(),10)}";
                }

                if (!string.IsNullOrWhiteSpace(request.To))
                {
                    baseAddress += $"&end_date={Strings.Left(request.To.SanitizeString(), 10)}";
                }
                else
                {
                    baseAddress += $"&end_date={Strings.Left(DateTime.Now.GetEndOfDate(), 10)}";
                }

                if (!string.IsNullOrWhiteSpace(request.PersonalCode))
                {
                    baseAddress += $"&personal_code={request.PersonalCode}";
                }

                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Add("X-API-Key", AttendanceApiKey);

                var result = await client.GetFromJsonAsync<AttendanceReport>(client.BaseAddress);


                _logger.LogInformation($"Ended function GetRangeApiResult with result: SUCCESS at {DateTime.Now}");
                _logger.LogInformation("-------------------------------------------------------------------------------");

                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Ended function GetRangeApiResult with result: FAIL at {DateTime.Now}");
                _logger.LogInformation("-------------------------------------------------------------------------------");

                return null;
            }
        }

        public async Task<ApiResult> PostManualAttendanceRequest(ManualAttendanceRequest request)
        {
            try
            {
                _logger.LogInformation("-------------------------------------------------------------------------------");
                _logger.LogInformation($"started function PostManualAttendanceRequest with parameters: personalCode={request.PersonalCode} | workDate={request.WorkDate} | times= {request.TimesString} at {DateTime.Now}");

                var baseAddress = AttendanceApiUrl + "attendance/manual";

                using var client = new HttpClient();

                var jsonData = JsonSerializer.Serialize(request);

                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                client.BaseAddress = new Uri(baseAddress);
                client.DefaultRequestHeaders.Add("X-API-Key", AttendanceApiKey);

                var result = await client.PostAsync(client.BaseAddress, content);

                if (result.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Ended function PostManualAttendanceRequest with status: SUCCESS at {DateTime.Now}");
                    _logger.LogInformation("-------------------------------------------------------------------------------");
                    return new ApiResult()
                    {
                        Result = "success"
                    };
                }

                _logger.LogInformation($"Ended function PostManualAttendanceRequest with status: Failed at {DateTime.Now}");
                _logger.LogInformation("-------------------------------------------------------------------------------");
                return new ApiResult()
                {
                    Result = "failed"
                };
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Ended function PostManualAttendanceRequest with status: Failed at {DateTime.Now}");
                _logger.LogInformation("-------------------------------------------------------------------------------");

                return new ApiResult()
                {
                    Result = "failed"
                };
            }
        }
    }
}
