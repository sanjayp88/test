using System.Runtime.Serialization;

namespace PayrollOneService.API
{
    [DataContract]
    public enum PayFrequency
    {
        [EnumMember]
        Weekly = 0,
        
        [EnumMember]
        Monthly = 1
    }
}
