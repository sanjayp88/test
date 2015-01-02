using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PayrollOneService
{
    public class XXXEmployeeProxy : API.Employee
    {
        public XXXEmployeeProxy()
        {
        }

        public XXXEmployeeProxy(PayrollAPI.Employee employee)
        {
            var employee1 = employee;

            FirstName = employee1.Forenames;
            Surname = employee1.Surname;
            NiNumber = employee1.NINumber;
            Id = employee1.Code;
            AgencyId = employee1.Tag1;
            TaxCode = employee1.TaxCode;
            Address1 = employee1.Address1;
            Address2 = employee1.Address2;
            Address3 = employee1.Address3;
            Address4 = employee1.Address4;
            PostCode = employee1.PostCode;
            Title = employee.Title;
            StartDate = employee.Joined;
            DOB = employee.DOB;
            BankSortCode = employee.SortCode;
            BankAccountNumber = employee.AccountNumber;
            BankAccountName = employee1.Forenames.Substring(0, 1) + ", " + employee1.Surname;
            Email = employee1.EMail;
            Mobile = employee1.MobilePhone;
            LandLine = employee1.HomePhone;
            BankName = employee1.BankName;
            NICategory = employee1.NITable;
            Week1Basis = employee1.TaxBasis;
            LeavingDate = employee1.Left;
            EmployeeCompanyName = employee1.Tag4;
        }
    }
}
