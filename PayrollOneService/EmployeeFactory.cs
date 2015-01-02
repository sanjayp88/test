using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayrollOneAPI = PayrollAPI;

namespace PayrollOneService.API
{
    public class EmployeeFactory
    {
        public static Employee CreateEmployee(PayrollAPI.Employee employee, Company company,
            PayrollOneAPI.PayFrequency payFrequency, string payrollDescription)
        {
            var e = CreateEmployee(employee);
            e.Company = company;
            e.PayFrequency = (payFrequency == PayrollOneAPI.PayFrequency.Month)
                ? PayFrequency.Monthly
                : PayFrequency.Weekly;
            e.PayrollDescription = payrollDescription;

            return e;
        }

        public static Employee CreateEmployee(PayrollAPI.Employee employee)
        {
            var e = new Employee
            {
                FirstName = employee.Forenames,
                Surname = employee.Surname,
                NiNumber = employee.NINumber,
                Id = employee.Code,
                AgencyId = employee.Tag1,
                TaxCode = employee.TaxCode,
                Address1 = employee.Address1,
                Address2 = employee.Address2,
                Address3 = employee.Address3,
                Address4 = employee.Address4,
                PostCode = employee.PostCode,
                Title = employee.Title,
                StartDate = employee.Joined,
                DOB = employee.DOB,
                BankSortCode = employee.SortCode,
                BankAccountNumber = employee.AccountNumber,
                BankAccountName = employee.Forenames.Substring(0,
                    1) + ", " + employee.Surname,
                Email = employee.EMail,
                Mobile = employee.MobilePhone,
                LandLine = employee.HomePhone,
                BankName = employee.BankName,
                NICategory = employee.NITable,
                Week1Basis = employee.TaxBasis,
                LeavingDate = employee.Left,
                EmployeeCompanyName = employee.Tag4,
            };

            return e;
        }
    }
}
