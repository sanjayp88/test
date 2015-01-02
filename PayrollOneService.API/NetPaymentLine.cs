using System.Runtime.Serialization;

namespace PayrollOneService.API
{
    [DataContract]
    public class NetPaymentLine
    {

        [DataMember]
        public EmployeeCompanyId EmployeeCompanyId { get; set; }

        [DataMember]
        public double EES { get; set; }

        [DataMember]
        public double ERS { get; set; }

        [DataMember]
        public double Net { get; set; }

        [DataMember]
        public double IncomeTax { get; set; }

        [DataMember]
        public int ProcessWeek { get; set; }

        [DataMember]
        public double TaxCredit { get; set; }

        [DataMember]
        public double EESNITD { get; set; }

        [DataMember]
        public double GrossTD { get; set; }

        [DataMember]
        public double TaxableGrossTD { get; set; }

        [DataMember]
        public double TaxTD { get; set; }

        [DataMember]
        public double SLRPaymentCurrent { get; set; }

        [DataMember]
        public string TaxCode { get; set; }

        [DataMember]
        public bool TaxBasis { get; set; }
    }
}
