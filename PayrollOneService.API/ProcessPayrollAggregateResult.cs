using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PayrollOneService.API
{
    [DataContract]
    public class ProcessPayrollAggregateResult
    {

        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public List<ProcessPayrollResult> Results { get; set; }

        [DataMember]
        public string FailureMessage { get; set; }
    }
}