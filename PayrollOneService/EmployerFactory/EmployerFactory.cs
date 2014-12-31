using System;
using System.Collections.Generic;
using System.Linq;
using PayrollAPI;

namespace PayrollOneService
{
    public class EmployerFactory : IEmployerFactory
    {
        private readonly List<String> _licenseKeys;
        private readonly PayrollEngine _payrollEngine;
        private readonly IEnumerable<Connection> _connections;

        public EmployerFactory(string exePath, IEnumerable<string> licenseKeys, IEnumerable<Connection> connections)
        {
            _payrollEngine = new PayrollEngine(exePath);
            _licenseKeys = licenseKeys.ToList();
            _connections = connections;
        }

        public IEnumerable<Connection> Connections
        {
            get { return _connections; }
        }

        public Employer OpenEmployer(API.Company company)
        {
            return _payrollEngine.OpenEmployer(Connections.First(c => c.Company == company).PR1ConnectionString,
                _licenseKeys);
        }

        public string SqlConnectionString(API.Company company)
        {
            return Connections.First(c => c.Company == company).SqlConnectionString;
        }

        public List<API.Company> GetCompanies()
        {
            return _connections.Select(c => c.Company).ToList();
        }

        public void DeleteCompany(string companyName)
        {
            throw new NotImplementedException();
        }

        public void CreateCompany(API.Company company)
        {
            throw new NotImplementedException();
        }
    }
}
