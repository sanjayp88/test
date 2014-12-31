using System.Collections.Generic;
using PayrollAPI;

namespace PayrollOneService
{
    public interface IEmployerFactory
    {
        IEnumerable<Connection> Connections { get; }
        Employer OpenEmployer(API.Company company);
        string SqlConnectionString(API.Company company);

        List<API.Company> GetCompanies();
        void DeleteCompany(string companyName);
        void CreateCompany(API.Company company);
    }
}
