using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AttendanceSystem.Infrastructure.Dto
{
    public class AttendanceLogVM
    {
        public ICollection<AttendanceLogItem> Items { get; set; } = [];
    }

    public class AttendanceLogItem
    {
        public long SessionId { get; set; }
        public string? Source { get; set; }
        public long OriginalId { get; set; }
        public string PersonalCode { get; set; }

        public DateTime WorkDate { get; set; }

        public DateTime? InDateTime { get; set; }
        public long? InRawId { get; set; }
        public string IsInManual { get; set; }

        public DateTime? OutDateTime { get; set; }
        public long? OutRawId { get; set; }
        public string IsOutManual { get; set; }

        public int? DurationMinutes { get; set; }

        public string IsIncomplete { get; set; }

        public DateTime BuiltAt { get; set; }

        public string IsActive { get; set; }

        public long? UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserAvatar { get; set; }
    }

    public class MonthlyAttendanceReportVM
    {
        public ICollection<MonthlyAttendanceReportItem> Items { get; set; } = new List<MonthlyAttendanceReportItem>();
    }

    public class MonthlyAttendanceReportItem
    {
        public long? UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserAvatar { get; set; }

        
        public int? TotalWorkDays { get; set; }

        public int? DurationMinutes { get; set; }
        public int? TotalOverTimeMinutes { get; set; }
        public int? TotalDeficitMinutes { get; set; }
    }

    public class EmployeeMonthlyAttendanceReportVM
    {
        public long UserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string FullName { get; set; }

        public List<EmployeeMonthlyAttendanceReportItem> Items { get; set; } = [];
    }

    public class EmployeeMonthlyAttendanceReportItem
    {
        public DateTime? Date { get; set; }
        public string? ShamsiDate { get; set; }
        public int? TotalMinutesPresent { get; set; }
        public int? AttendanceRecordsCount { get; set; }
        public List<EmployeeMonthlyAttendanceReportItemTrafficItem> TrafficItems { get; set; } = [];
    }

    public class EmployeeMonthlyAttendanceReportItemTrafficItem
    {
        public DateTime? InDateTime { get; set; }
        public bool IsInManual { get; set; }

        public DateTime? OutDateTime { get; set; }
        public bool IsOutManual { get; set; }
        public int? TotalDuration { get; set; }
    }

    public class LiveAttendanceStatusVM
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string UserAvatar { get; set; }

        public bool IsPresent { get; set; }

        public List<EmployeeMonthlyAttendanceReportItemTrafficItem> TrafficItems { get; set; } = [];
    }

    public class EditAttendanceRecordsVM
    {
        public bool IsRecordsDeleted { get; set; }

        public long UserId { get; set; }

        public DateTime WorkDate { get; set; }

        public string Reason { get; set; }

        public List<AttendanceLogItem> LogItems { get; set; } = [];
    }

    public class EditAttendanceRecordVM
    {
        public long UserId { get; set; }
        public DateOnly WorkDate { get; set; }
        public long SessionId { get; set; }
        public DateTime? EntranceTime { get; set; }
        public DateTime? ExitTime { get; set; }

    }

    #region Attendance Full Report 

    public class AttendanceFullReport
    {
        [JsonPropertyName("report")]
        public string Report { get; set; }

        [JsonPropertyName("employee_info")]
        public EmployeeInfo EmployeeInfo { get; set; }

        [JsonPropertyName("date_range")]
        public DateRange DateRange { get; set; }

        [JsonPropertyName("total_records")]
        public int TotalRecords { get; set; }

        [JsonPropertyName("items")]
        public List<AttendanceItem> Items { get; set; }
    }

    public class EmployeeInfo
    {
        [JsonPropertyName("personal_code")]
        public int PersonalCode { get; set; }

        [JsonPropertyName("internal_emp_id")]
        public int InternalEmpId { get; set; }
    }

    public class DateRange
    {
        [JsonPropertyName("start")]
        public string Start { get; set; }

        [JsonPropertyName("end")]
        public string End { get; set; }
    }

    public class AttendanceItem
    {
        [JsonPropertyName("WorkDate")]
        public string WorkDate { get; set; }

        [JsonPropertyName("PersonalCode")]
        public string PersonalCode { get; set; }

        [JsonPropertyName("FullName")]
        public string FullName { get; set; }

        [JsonPropertyName("GroupName")]
        public string GroupName { get; set; }

        [JsonPropertyName("ShiftStartTime")]
        public string ShiftStartTime { get; set; }

        [JsonPropertyName("ShiftEndTime")]
        public string ShiftEndTime { get; set; }

        [JsonPropertyName("FirstEnterTime")]
        public string FirstEnterTime { get; set; }

        [JsonPropertyName("LastExitTime")]
        public string LastExitTime { get; set; }

        [JsonPropertyName("TotalPresentMins")]
        public int TotalPresentMins { get; set; }

        [JsonPropertyName("DelayMins")]
        public int DelayMins { get; set; }

        [JsonPropertyName("EarlyExitMins")]
        public int EarlyExitMins { get; set; }

        [JsonPropertyName("IntraDayShortfallMins")]
        public int IntraDayShortfallMins { get; set; }

        [JsonPropertyName("TotalDeficitMins")]
        public int TotalDeficitMins { get; set; }

        [JsonPropertyName("ApprovedOTMins")]
        public int ApprovedOTMins { get; set; }

        [JsonPropertyName("PendingOTMins")]
        public int PendingOTMins { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; }
    }


    #endregion

    #region Api Helper Classes

    public class FullAttendanceReportRequest
    {
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public string? PersonalCode { get; set; } = null;
    }

    public class ManualAttendanceRequest
    {
        [JsonPropertyName("personal_code")]
        public long PersonalCode { get; set; }

        [JsonPropertyName("work_date")]
        public string WorkDate { get; set; }

        [JsonPropertyName("times_string")]
        public string TimesString { get; set; }

        [JsonPropertyName("reason_id")]
        public int ReasonId { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("created_by_id")]
        public long CreatedById { get; set; }
    }

    public class AttendanceRecordsWithRangeRequest
    {
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public string? PersonalCode { get; set; } = null;
    }

    public class ApiResult
    {
        public string Result { get; set; }
        public string? Message { get; set; }
    }

    public class AttendanceFullReportApiResult
    {
        public ApiResult ApiResult { get; set; }
        public AttendanceFullReport? Data { get; set; }
    }

    public class RangeFunctionApiResult
    {
        public ApiResult ApiResult { get; set; }
        public AttendanceLogVM? Data { get; set; }
    }

    #endregion
}
