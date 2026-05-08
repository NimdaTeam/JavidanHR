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

        public List<AttendanceLogItem>? LogItems { get; set; } = [];
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

    #region Attendance Report V2
    public class AttendanceReport
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }

        [JsonPropertyName("personal_code")]
        public string? PersonalCode { get; set; }

        [JsonPropertyName("total_days")]
        public int TotalDays { get; set; }

        [JsonPropertyName("data")]
        public List<AttendanceRecord> Data { get; set; }
    }

    public class AttendanceRecord
    {
        [JsonPropertyName("personal_code")]
        public string PersonalCode { get; set; }

        [JsonPropertyName("work_date")]
        public string WorkDate { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("group_name")]
        public string GroupName { get; set; }

        [JsonPropertyName("day_type")]
        public string DayType { get; set; }

        [JsonPropertyName("holiday_description")]
        public string? HolidayDescription { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("shift_start")]
        public string ShiftStart { get; set; }

        [JsonPropertyName("shift_end")]
        public string ShiftEnd { get; set; }

        [JsonPropertyName("first_enter")]
        public string FirstEnter { get; set; }

        [JsonPropertyName("last_exit")]
        public string LastExit { get; set; }

        [JsonPropertyName("total_present_mins")]
        public int TotalPresentMins { get; set; }

        [JsonPropertyName("delay_mins")]
        public int DelayMins { get; set; }

        [JsonPropertyName("early_exit_mins")]
        public int EarlyExitMins { get; set; }

        [JsonPropertyName("deficit_mins")]
        public int DeficitMins { get; set; }

        [JsonPropertyName("approved_ot_mins")]
        public int ApprovedOtMins { get; set; }

        [JsonPropertyName("pending_ot_mins")]
        public int PendingOtMins { get; set; }

        [JsonPropertyName("physical_extra_mins")]
        public int PhysicalExtraMins { get; set; }

        [JsonPropertyName("in_shift_presence_mins")]
        public int InShiftPresenceMins { get; set; }

        [JsonPropertyName("shift_mins")]
        public int ShiftMins { get; set; }

        [JsonPropertyName("total_present_time")]
        public string TotalPresentTime { get; set; }

        [JsonPropertyName("delay_time")]
        public string DelayTime { get; set; }

        [JsonPropertyName("deficit_time")]
        public string DeficitTime { get; set; }

        [JsonPropertyName("approved_ot_time")]
        public string ApprovedOtTime { get; set; }

        [JsonPropertyName("total_present_hours")]
        public double TotalPresentHours { get; set; }

        [JsonPropertyName("approved_ot_hours")]
        public double ApprovedOtHours { get; set; }

        [JsonPropertyName("deficit_hours")]
        public double DeficitHours { get; set; }

        [JsonPropertyName("session_count")]
        public int SessionCount { get; set; }

        [JsonPropertyName("sessions")]
        public List<Session> Sessions { get; set; }

        // مرخصی
        [JsonPropertyName("leave_type")]
        public string? LeaveType { get; set; }

        [JsonPropertyName("leave_reason")]
        public string? LeaveReason { get; set; }

        [JsonPropertyName("leave_start_time")]
        public string? LeaveStartTime { get; set; }

        [JsonPropertyName("leave_end_time")]
        public string? LeaveEndTime { get; set; }

        [JsonPropertyName("leave_minutes")]
        public int? LeaveMinutes { get; set; }

        [JsonPropertyName("leave_time")]
        public string LeaveTime { get; set; }

        [JsonPropertyName("leave_status")]
        public string? LeaveStatus { get; set; }

        // مأموریت
        [JsonPropertyName("mission_type")]
        public string? MissionType { get; set; }

        [JsonPropertyName("mission_location")]
        public string? MissionLocation { get; set; }

        [JsonPropertyName("mission_purpose")]
        public string? MissionPurpose { get; set; }

        [JsonPropertyName("mission_start_time")]
        public string? MissionStartTime { get; set; }

        [JsonPropertyName("mission_end_time")]
        public string? MissionEndTime { get; set; }

        [JsonPropertyName("mission_minutes")]
        public int? MissionMinutes { get; set; }

        [JsonPropertyName("mission_time")]
        public string MissionTime { get; set; }

        [JsonPropertyName("mission_status")]
        public string? MissionStatus { get; set; }
    }

    public class Session
    {
        [JsonPropertyName("in_time")]
        public string? InTime { get; set; }

        [JsonPropertyName("out_time")]
        public string? OutTime { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; }

        [JsonPropertyName("duration_time")]
        public string DurationTime { get; set; }

        [JsonPropertyName("duration_hours")]
        public double DurationHours { get; set; }

        [JsonPropertyName("is_incomplete")]
        public bool IsIncomplete { get; set; }

        [JsonPropertyName("is_manual_in")]
        public bool IsManualIn { get; set; }

        [JsonPropertyName("is_manual_out")]
        public bool IsManualOut { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("punch_count")]
        public int PunchCount { get; set; }

        [JsonPropertyName("session_sequence")]
        public int SessionSequence { get; set; }
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
        public string PersonalCode { get; set; }

        [JsonPropertyName("work_date")]
        public string WorkDate { get; set; }

        [JsonPropertyName("times_string")]
        public string TimesString { get; set; }

        [JsonPropertyName("reason_id")]
        public int ReasonId { get; set; }

        [JsonPropertyName("description_base")]
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
