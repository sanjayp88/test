using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using log4net;
using PayrollOneService;
using PayrollOneService.API;
using Company = PayrollOneService.API.Company;
using Payroll = PayrollOneService.API.Payroll;
using Service = PayrollOneService.PayrollOneService;
using Employee = PayrollOneService.API.Employee;

namespace PayrollOneWCFService
{
    public class PayrollOneService : IPayrollOneService
    {
        private readonly ILog _log;
        private readonly Service _service;

        public PayrollOneService()
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = LogManager.GetLogger(typeof(PayrollOneService));
            _service = new Service(EmployerFactory);
        }

        private string Environment
        {
            get { return ConfigurationManager.AppSettings["Environment"]; }
        }

        private string PayrollServiceDBServer
        {
            get { return ConfigurationManager.AppSettings[Environment + "PayrollServiceDBServer"]; }
        }

        private string PayrollServiceDBName
        {
            get { return ConfigurationManager.AppSettings[Environment + "PayrollServiceDBName"]; }
        }

        private string PayrollServiceDBUsername
        {
            get { return ConfigurationManager.AppSettings[Environment + "PayrollServiceDBUsername"]; }
        }

        private string PayrollServiceDBPassword
        {
            get { return ConfigurationManager.AppSettings[Environment + "PayrollServiceDBPassword"]; }
        }

        private IEmployerFactory EmployerFactory
        {
            get
            {

                var env = ConfigurationManager.AppSettings["Environment"];
                var employerFactory = new EmployerFactoryDb(
                    _log,
                    ConfigurationManager.AppSettings[env + "PR1_EXE_PATH"],
                    ConfigurationManager.AppSettings[env + "PR1_LICENSE_KEYS"].Split("|".ToCharArray()),
                    PayrollServiceDBServer,
                    PayrollServiceDBName,
                    PayrollServiceDBUsername,
                    PayrollServiceDBPassword);
                return employerFactory;
            }
        }

        public List<Payroll> GetPayrolls(Company company)
        {
            try
            {
                return _service.GetPayrolls(company);
            }
            catch (Exception ex)
            {
                throw new FaultException("GetPayrolls: " + ex.Message);
            }
        }

        public List<EmployeeValidation> Validate(Company company, List<Employee> employees,
            string payrollDescription)
        {
            try
            {
                return _service.Validate(company, employees, payrollDescription);
            }
            catch (Exception ex)
            {
                throw new FaultException("Validate: " + ex.Message);
            }
        }

        public List<EmployeeValidation> Persist(Company company, string payrollDescription, List<Employee> employees)
        {
            try
            {
                return _service.Persist(company, payrollDescription, employees);
            }
            catch (Exception ex)
            {
                throw new FaultException("Persist: " + ex.Message);
            }
        }
        
        public List<Employee> GetAll(bool includeLeavers)
        {
            try
            {
                return _service.GetAll(includeLeavers);
            }
            catch (Exception ex)
            {
                throw new FaultException("GetAll: " + ex.Message);
            }
        }

        public List<Employee> GetAllForCompany(Company company, bool includeLeavers = false)
        {
            try
            {
                return _service.GetAllForCompany(company, includeLeavers);

            }
            catch (Exception ex)
            {
                throw new FaultException("GetAllForCompany: " + ex.Message);
            }
        }
       
        public List<Employee> GetAllForCompanyPayroll(Company company, string payrollDescription,
            bool includeLeavers = false)
        {
            try
            {
                var employees = _service.GetAllForCompanyPayroll(company, payrollDescription, includeLeavers);
                return employees;
            }
            catch (Exception ex)
            {
                throw new FaultException("GetAllForCompanyPayroll: " + ex.Message);
            }
        }

        public void PlaceEmployeesOnHoldExcept(List<EmployeeCompanyId> exceptEmployees)
        {
            try
            {
                _service.PlaceEmployeesOnHoldExcept(exceptEmployees);
            }
            catch (Exception ex)
            {
                throw new FaultException("PlaceEmployeesOnHoldExcept: " + ex.Message);
            }
        }

        public ProcessPayrollAggregateResult ProcessPayroll(Company company, List<GrossPaymentLine> grossPaymentLines,
            string payrollDescription, bool returnNetPaymentData)
        {
            try
            {
                return _service.ProcessPayroll(company, grossPaymentLines, payrollDescription, returnNetPaymentData);
            }
            catch (Exception ex)
            {
                throw new FaultException("ProcessPayroll: " + ex.Message);
            }
        }

        public string TestConnection()
        {
            try
            {
                return _service.TestConnection();
            }
            catch (Exception ex)
            {
                throw new FaultException("TestConnection: " + ex.Message);
            }
        }

        public List<Company> GetCompanies()
        {
            try
            {
                return _service.GetCompanies();
            }
            catch (Exception ex)
            {
                throw new FaultException("GetCompanies: " + ex.Message);
            }
        }

        public void CreateCompany(Company company)
        {
            try
            {
                 _service.CreateCompany(company);
            }
            catch (Exception ex)
            {
                throw new FaultException("CreateCompany: " + ex.Message);
            }
        }

        public void ResetData(Company company)
        {
            try
            {
                _service.ResetData(company);
            }
            catch (Exception ex)
            {
                throw new FaultException("ResetData: " + ex.Message);
            }
        }
    }
}
