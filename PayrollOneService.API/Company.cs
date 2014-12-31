using System.Runtime.Serialization;

namespace PayrollOneService.API
{
    [DataContract]
    public class Company
    {
        [DataMember]
        public string CompanyKey { get; set; }

        [DataMember]
        public bool IsMyPSU { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string ApplicationMode { get; set; }

        [DataMember]
        public string BankSortCode { get; set; }

        [DataMember]
        public string BankAccountNumber { get; set; }

        [DataMember]
        public string TraderFileSharePath { get; set; }

        [DataMember]
        public string PaymentFileType { get; set; }

        [DataMember]
        public string OmbrosDBServer { get; set; }

        [DataMember]
        public string OmbrosDBDatabaseName { get; set; }

        [DataMember]
        public string OmbrosDBUserName { get; set; }

        [DataMember]
        public string OmbrosDBPassword { get; set; }

        [DataMember]
        public string Address1 { get; set; }

        [DataMember]
        public string Address2 { get; set; }

        [DataMember]
        public string Address3 { get; set; }

        [DataMember]
        public string Address4 { get; set; }

        [DataMember]
        public string Postcode { get; set; }

        [DataMember]
        public string InvoiceLogoFileName { get; set; }

        [DataMember]
        public string SmsSenderId { get; set; }

        [DataMember]
        public string EmailAddress { get; set; }

        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string BankReference { get; set; }

        [DataMember]
        public string ProductName { get; set; }

        [DataMember]
        public bool SendPortalEmails { get; set; }
    }
}