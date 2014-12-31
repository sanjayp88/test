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

        public virtual List<Payroll> GetPayrolls(Company company)
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
    }
}
