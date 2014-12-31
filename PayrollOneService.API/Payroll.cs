using System.Runtime.Serialization;

namespace PayrollOneService.API
{
    [DataContract]
    public class Payroll
    {
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Frequency { get; set; }
    }
}