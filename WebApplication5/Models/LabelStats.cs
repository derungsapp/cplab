namespace WebApplication5.Models
{
    public class LabelStats
    {
        public string Label { get; set; }
        public int TotalRecords { get; set; }
        public int CorrectRecords { get; set; }
        public int WrongRecords => TotalRecords - CorrectRecords;
    }
}