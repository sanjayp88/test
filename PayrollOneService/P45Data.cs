namespace PayrollOneService
{
    public class P45Data
    {
        public int EmployeeID { get; set; }

        public decimal PayToDate
        {
            get
            {
                return UseRawValues
                    ? (PayToDateRaw.HasValue ? PayToDateRaw.Value : 0)
                    : 0;
            }

        }
        public decimal TaxToDate
        {
            get
            {
                return UseRawValues
                    ? (TaxToDateRaw.HasValue ? TaxToDateRaw.Value : 0)
                    : 0;
            }
        }

        public bool UseRawValues
        {
            get
            {
                return (P45Received.HasValue && P45Received.Value == 1) ||
                       (P46FilledIn.HasValue && P46FilledIn.Value == 1) ||
                       (P6Received.HasValue && P6Received.Value == 1);
            }
        }

        public decimal? PayToDateRaw { get; set; }
        public decimal? TaxToDateRaw { get; set; }
        public int? P45Received { get; set; }
        public int? P46FilledIn { get; set; }
        public int? P6Received { get; set; }
    }
}
