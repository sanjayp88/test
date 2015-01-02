using System.Runtime.Serialization;

namespace PayrollOneService.API
{
    [DataContract]
    public class EmployeeCompanyId
    {

        [DataMember]
        public string EmployeeId { get; set; }

        [DataMember]
        public Company Company { get; set; }
    }
}
