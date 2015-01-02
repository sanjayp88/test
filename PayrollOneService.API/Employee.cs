using System;
using System.Runtime.Serialization;
using Thrift.Common;

namespace PayrollOneService.API
{
    [DataContract]
    public class Employee
    {
        
        //[DataMember]
        public DateTime StartDate
        {
            get { return StartDate_ISO8601.ISO8601ToUtc(); }
            set { StartDate_ISO8601 = value.Date.ToISO8601(); }
        }

        //[DataMember]
        public DateTime DOB
        {
            get { return DOB_ISO8601.ISO8601ToUtc(); }
            set { DOB_ISO8601 = value.Date.ToISO8601(); }
        }

        //[DataMember]
        public DateTime? LeavingDate
        {
            get
            {
                var date = LeavingDate_ISO8601.ISO8601ToUtc();

                if (date == DateTime.MinValue)
                    return null;

                return date;
            }
            set
            {
                if (value != null && value.Value.Date.Year > 2010)
                    LeavingDate_ISO8601 = value.Value.Date.ToISO8601();
                else
                    LeavingDate_ISO8601 = null;
            }
        }

        //[DataMember]
        public bool HasLeft
        {
            get { return (LeavingDate.HasValue && LeavingDate.Value < DateTime.Now.Date); }
        }

        //[DataMember]
        public bool QueryAccountDetails
        {
            get
            {
                if (BankAccountName == null)
                    return true;

                return (BankAccountName.IndexOf(Surname) < 0);
            }
        }

        //[DataMember]
        public string DisplayName
        {
            get { return string.Format("{0}, {1}", Surname.Trim(), FirstName.Trim()); }
        }

        [DataMember]
        public string FirstName { get; set; }
       
        [DataMember]
        public string Surname { get; set; }
        
        [DataMember]
        public string NiNumber { get; set; }
        
        [DataMember]
        public string NICategory { get; set; }
        
        [DataMember]
        public string Id { get; set; }
        
        [DataMember]
        public string AgencyId { get; set; }
       
        [DataMember]
        public string Title { get; set; }
        
        [DataMember]
        public string StartDate_ISO8601 { get; set; }
        
        [DataMember]
        public string DOB_ISO8601 { get; set; }
        
        [DataMember]
        public string Address1 { get; set; }
        
        [DataMember]
        public string Address2 { get; set; }
       
        [DataMember]
        public string Address3 { get; set; }
        
        [DataMember]
        public string Address4 { get; set; }
        
        [DataMember]
        public string PostCode { get; set; }
        
        [DataMember]
        public string Email { get; set; }
        
        [DataMember]
        public string Mobile { get; set; }
        
        [DataMember]
        public string LandLine { get; set; }
        
        [DataMember]
        public string BankName { get; set; }
        
        [DataMember]
        public string BankSortCode { get; set; }
        
        [DataMember]
        public string BankAccountNumber { get; set; }
        
        [DataMember]
        public string BankAccountName { get; set; }
        
        [DataMember]
        public bool Week1Basis { get; set; }
        
        [DataMember]
        public string TaxCode { get; set; }
        
        [DataMember]
        public double P45Gross { get; set; }
        
        [DataMember]
        public double P45Tax { get; set; }
        
        [DataMember]
        public double TotalTaxTD { get; set; }
        
        [DataMember]
        public double TaxableGrossTD { get; set; }
        
        [DataMember]
        public double TotalGrossTD { get; set; }
        
        [DataMember]
        public double NICGrossTD { get; set; }
        
        [DataMember]
        public string RDSKey { get; set; }
        
        [DataMember]
        public PayFrequency PayFrequency { get; set; }
        
        [DataMember]
        public Company Company { get; set; }
        
        [DataMember]
        public string LeavingDate_ISO8601 { get; set; }
        
        [DataMember]
        public string EmployeeCompanyName { get; set; }

        [DataMember]
        public string PayrollDescription { get; set; }
    }
}
