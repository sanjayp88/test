using System.Runtime.Serialization;

namespace PayrollOneService.API
{
    [DataContract]
    public class GrossPaymentLine
    {

        [DataMember]
        public EmployeeCompanyId EmployeeCompanyId { get; set; }

        [DataMember]
        public double TaxablePay { get; set; }

        [DataMember]
        public double NonTaxablePay { get; set; }

        [DataMember]
        public PayFrequency PayFrequency { get; set; }
    }
}
