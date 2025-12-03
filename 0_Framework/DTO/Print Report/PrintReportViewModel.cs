namespace _0_Framework.DTO.Print_Report
{
    public class PrintReportViewModel
    {
        public string SubSystemTitle { get; set; }

        public string ReportTitle { get; set; }

        public string ReportCity { get; set; }

        public string ReportYear { get; set; }

        public string ReportMonth { get; set; }

        public string FirstValueTableHeader { get; set; }

        public string SecondValueTableHeader { get; set; }

        public List<ReportRow> Rows { get; set; }
    }

    public class ReportRow
    {
        public string? Label { get; set; }
        public string? Unit { get; set; }
        public string? FirstValue { get; set; }
        public string? SecondValue { get; set; }
    }
}
