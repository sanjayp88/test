using System.Runtime.Serialization;

namespace PayrollOneService.API
{
    [DataContract]
    public class EmployeeValidation
    {

        [DataMember]
        public bool Valid { get; set; }

        [DataMember]
        public Employee Candidate { get; set; }

        [DataMember]
        public Employee Record { get; set; }

        [DataMember]
        public string ValidationMessage { get; set; }
    }
}
