using System.Collections.Generic;
using System.ServiceModel;

namespace PayrollOneService.API
{
    [ServiceContract]
    public interface IPayrollOneService
    {
        [OperationContract]
        List<Payroll> GetPayrolls(Company company);
    }
}
