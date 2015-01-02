using System.Runtime.Serialization;

namespace PayrollOneService.API
{
    [DataContract]
    public class ProcessPayrollResult
    {

        [DataMember]
        public NetPaymentLine NetPaymentLine { get; set; }

        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string FailureMessage { get; set; }

        [DataMember]
        public string EmployeePayrollId { get; set; }
    }
}
