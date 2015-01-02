using System.Collections.Generic;
using System.ServiceModel;

namespace PayrollOneService.API
{
    [ServiceContract]
    public interface IPayrollOneService
    {
        [OperationContract]
        List<Payroll> GetPayrolls(Company company);

        [OperationContract]
        List<EmployeeValidation> Validate(Company company, List<API.Employee> employees, string payrollDescription);

        [OperationContract]
        List<EmployeeValidation> Persist(Company company, string payrollDescription, List<API.Employee> employees);

        [OperationContract]
        List<API.Employee> GetAll(bool includeLeavers);

        [OperationContract]
        List<API.Employee> GetAllForCompany(Company company, bool includeLeavers = false);

        [OperationContract]
        List<API.Employee> GetAllForCompanyPayroll(Company company, string payrollDescription, bool includeLeavers = false);

        [OperationContract]
        void PlaceEmployeesOnHoldExcept(List<EmployeeCompanyId> exceptEmployees);

        [OperationContract]
        ProcessPayrollAggregateResult ProcessPayroll(Company company, List<GrossPaymentLine> grossPaymentLines, string payrollDescription, bool returnNetPaymentData);

        [OperationContract]
        string TestConnection();

        [OperationContract]
        List<Company> GetCompanies();
    }
}
