using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using PayrollAPI;
using PayrollOneService.Exceptions;

namespace PayrollOneService
{
    public class EmployerFactoryDb : IEmployerFactory
    {
        public IEnumerable<Connection> Connections { get; private set; }
        private readonly string _server;
        private readonly string _database;
        private readonly string _username;
        private readonly string _password;
        private readonly ILog _log;

        private readonly List<String> _licenseKeys;
        private readonly PayrollEngine _payrollEngine;

        public readonly string DbPrefix = "PayrollOne_";

        private string PayrollServiceDBConnectionString
        {
            get
            {
                var connStr = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3}",
                    _server,
                    _database,
                    _username,
                    _password);

                return connStr;
            }
        }

        public EmployerFactoryDb(ILog log, string exePath, IEnumerable<string> licenseKeys, string server, string database, string username, string password)
        {
            _log = log;

            _payrollEngine = new PayrollEngine(exePath);
            _licenseKeys = licenseKeys.ToList();

            _server = server;
            _database = database;
            _username = username;
            _password = password;

            InitialiseConnections();
        }

        private void InitialiseConnections()
        {
            try
            {
                // initialise the Connections property
                Connections = GetCompanies().Select(c =>
                    new Connection
                    {
                        Company = new API.Company
                        {
                            CompanyKey = c.CompanyKey,
                            IsMyPSU = c.IsMyPSU,
                            ApplicationMode = c.ApplicationMode,
                            BankSortCode = c.BankSortCode,
                            BankAccountNumber = c.BankAccountNumber,
                            TraderFileSharePath = c.TraderFileSharePath,
                            PaymentFileType = c.PaymentFileType,
                            OmbrosDBServer = c.OmbrosDBServer,
                            OmbrosDBDatabaseName = c.OmbrosDBDatabaseName,
                            OmbrosDBPassword = c.OmbrosDBPassword,
                            OmbrosDBUserName = c.OmbrosDBUserName,
                            Address1 = c.Address1,
                            Address2 = c.Address2,
                            Address3 = c.Address3,
                            Address4 = c.Address4,
                            Postcode = c.Postcode,
                            InvoiceLogoFileName = c.InvoiceLogoFileName,
                            SmsSenderId = c.SmsSenderId,
                            EmailAddress = c.EmailAddress,
                            PhoneNumber = c.PhoneNumber,
                            DisplayName = c.DisplayName,
                            BankReference = c.BankReference,
                            ProductName = c.ProductName,
                            SendPortalEmails = c.SendPortalEmails

                        },
                        Database = string.Format("{0}{1}", DbPrefix, c.CompanyKey),
                        Server = _server,
                        Uid = _username,
                        Pwd = _password
                    });
            }
            catch(Exception ex)
            {
                _log.Error("EmployerFactoryDb InitialiseConnections()", ex);
                throw;
            }
        }

        public Employer OpenEmployer(API.Company company)
        {
            var PR1ConnectionString = Connections.First(c => c.Company.CompanyKey == company.CompanyKey).PR1ConnectionString;
            return _payrollEngine.OpenEmployer(PR1ConnectionString, _licenseKeys);
        }

        public string SqlConnectionString(API.Company company)
        {
            return Connections.First(c => c.Company.CompanyKey == company.CompanyKey).SqlConnectionString;
        }

        public void CreateCompany(API.Company company)
        {
            // create company in the payroll service database
            try
            {
                // soft delete company
                using(var context = new PayrollOneConfigDataContext(PayrollServiceDBConnectionString))
                {

                    var companyDb = context.Companies.FirstOrDefault(c => c.CompanyKey == company.CompanyKey && !c.Deleted);
                    if(companyDb != null)
                        throw new CompanyExistsException("Company " + company.CompanyKey + " already exists");

                    companyDb = new Company
                    {
                        CompanyKey = company.CompanyKey,
                        IsMyPSU = company.IsMyPSU,
                        Deleted = false,
                        ApplicationMode = company.ApplicationMode,
                        ProductName = company.ProductName,
                        SendPortalEmails = company.SendPortalEmails,
                        BankSortCode = company.BankSortCode,
                        BankAccountNumber = company.BankAccountNumber,
                        TraderFileSharePath = company.TraderFileSharePath,
                        PaymentFileType = company.PaymentFileType,
                        OmbrosDBServer = company.OmbrosDBServer,
                        OmbrosDBDatabaseName = company.OmbrosDBDatabaseName,
                        OmbrosDBPassword = company.OmbrosDBPassword,
                        OmbrosDBUserName = company.OmbrosDBUserName,
                        Address1 = company.Address1,
                        Address2 = company.Address2,
                        Address3 = company.Address3,
                        Address4 = company.Address4,
                        Postcode = company.Postcode,
                        InvoiceLogoFilename = company.InvoiceLogoFileName,
                        SMSSenderId = company.SmsSenderId,
                        EmailAddress = company.EmailAddress,
                        PhoneNumber = company.PhoneNumber,
                        DisplayName = company.DisplayName,
                        BankReference = company.BankReference
                    };
                    context.Companies.InsertOnSubmit(companyDb);

                    context.SubmitChanges();
                }

                // update the Connections property
                InitialiseConnections();

                ProvisionDatabase(company);
            }
            catch(CompanyExistsException ex)
            {
                _log.Error("CreateCompany: " + ex.Message, ex);
                throw new GeneralException(ex.Message);
            }
            catch(GeneralException ex)
            {
                _log.Error("CreateCompany: " + ex.Message, ex);
                throw;
            }
            catch(Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw new GeneralException(ex.Message);
            }
        }

        private void ProvisionDatabase(API.Company company)
        {
            var connection = Connections.FirstOrDefault(c => c.Company.CompanyKey == company.CompanyKey);
            if(connection == null)
                throw new Exception("Connection does not exist");

            // ensure database exists
            var sqlConnStr = string.Format("data source={0}; uid={1}; pwd={2};", _server, _username, _password);
            var databaseName = string.Format("{0}{1}", DbPrefix, company.CompanyKey);
            var sqlDbConnStr = string.Format("data source={0};database={1}; uid={2}; pwd={3};", _server, databaseName, _username, _password);

            ValidateDatabaseDoesNotExist(sqlConnStr, databaseName);
            CreateDatabase(sqlConnStr, databaseName);

            // make it a PayrollOne database
            CreateEmployer(connection.PR1ConnectionString, sqlDbConnStr);
        }

        private static void CreateDatabase(string sqlConnStr, string databaseName)
        {
            var conn = new SqlConnection(sqlConnStr);
            try
            {
                using(conn)
                {
                    var createDatabaseCommand = string.Format("Create Database {0};", databaseName);

                    using(var cmd = new SqlCommand(createDatabaseCommand, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                conn.Close();
            }
        }

        private void CreateEmployer(string pr1ConnStr, string sqlConnStr)
        {
            try
            {
                // populate database with PR1 data
                var employer = _payrollEngine.OpenEmployer(pr1ConnStr, _licenseKeys);

                // set a weekly and a monthly payroll
                var today = DateTime.Today;
                var currentMonthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

                var dayOfWeek = Convert.ToInt32(today.DayOfWeek);
                var weekEndDate = today.AddDays(7 - dayOfWeek);

                employer.NewPayroll(PayrollAPI.PayFrequency.Month, currentMonthEnd);
                employer.NewPayroll(PayrollAPI.PayFrequency.Week, weekEndDate);

                CreatePaymentCode(employer, sqlConnStr, "NONTAX", "Non Taxable Pay", 25,
                    false, false, false,
                    false, false, true, false);

                CreatePaymentCode(employer, sqlConnStr, "BASIC_SUPP", "Basic Supplemental Pay", 51,
                    true, true, true,
                    false, false, true, true);

                CreatePaymentCode(employer, sqlConnStr, "NONTAX_SUPP", "Non Taxable Supplemental Pay", 26,
                    false, false, false,
                    false, false, true, true);
            }
            catch(Exception ex)
            {
                _log.Error(ex.Message, ex);
                throw new GeneralException(ex.Message);
            }
        }

        private void CreatePaymentCode(Employer employer, string sqlConnStr, string code, string description, int sequence,
            bool beforeTax, bool beforeNI, bool beforePension,
            bool pensionContribution, bool standardPayment, bool variablePayment, bool excludeFromHolidayAccrual)
        {
            var paymentCode = employer.NewPaymentCode(code);
            paymentCode.BeginEdit();
            paymentCode.Description = description;
            paymentCode.Sequence = sequence;
            paymentCode.BeforeTax = beforeTax;
            paymentCode.BeforeNI = beforeNI;
            paymentCode.BeforePension = beforePension;

            paymentCode.Pension = pensionContribution;
            paymentCode.Regular = standardPayment;
            paymentCode.Variable = variablePayment;
            paymentCode.Update();

            // set exclude from Holiday Accrual property
            if(excludeFromHolidayAccrual)
                SetExcludeFromHolidayAccrual(sqlConnStr, code);
        }

        private static void SetExcludeFromHolidayAccrual(string sqlConnStr, string code)
        {
            var conn = new SqlConnection(sqlConnStr);
            try
            {
                using(conn)
                {
                    var excludeFromHolidayAccrualCommand =
                        string.Format("update PayElement set ExcludeHourlyReports=1 where ElementCode='{0}';", code);

                    using(var cmd = new SqlCommand(excludeFromHolidayAccrualCommand, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                conn.Close();
            }
        }

        private void ValidateDatabaseDoesNotExist(string sqlConnStr, string databaseName)
        {
            var conn = new SqlConnection(sqlConnStr);
            try
            {
                using(conn)
                {
                    var databaseExistsCommand = string.Format("SELECT COUNT(database_id) FROM sys.databases WHERE Name = '{0}'", databaseName);

                    using(var cmd = new SqlCommand(databaseExistsCommand, conn))
                    {
                        conn.Open();
                        var databaseId = (int)cmd.ExecuteScalar();
                        if(databaseId > 0)
                        {
                            throw new GeneralException(string.Format("SQL Database {0} already exists", databaseName));
                        }
                    }
                }
            }
            finally
            {
                conn.Close();
            }
        }

        public void DeleteCompany(string companyName)
        {
            try
            {
                // soft delete company
                using(var context = new PayrollOneConfigDataContext(PayrollServiceDBConnectionString))
                {
                    var companyList = context.Companies.Where(c => c.CompanyKey == companyName && !c.Deleted);

                    if (!companyList.Any())
                        throw new CompanyDoesNotExistException("Company " + companyName + " does not exist");

                    foreach(var company in companyList)
                        company.Deleted = true;

                    context.SubmitChanges();
                }

                // update the Connections property
                InitialiseConnections();
            }
            catch(Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        public List<API.Company> GetCompanies()
        {
            try
            {
                using (var context = new PayrollOneConfigDataContext(PayrollServiceDBConnectionString))
                {
                    return (from c in context.Companies where !c.Deleted
                            select new API.Company
                        {
                            CompanyKey = c.CompanyKey,
                            IsMyPSU = c.IsMyPSU,
                            Id = c.CompanyId,
                            ApplicationMode = c.ApplicationMode,
                            BankSortCode = c.BankSortCode,
                            BankAccountNumber = c.BankAccountNumber,
                            TraderFileSharePath = c.TraderFileSharePath,
                            PaymentFileType = c.PaymentFileType,
                            OmbrosDBServer = c.OmbrosDBServer,
                            OmbrosDBDatabaseName = c.OmbrosDBDatabaseName,
                            OmbrosDBPassword = c.OmbrosDBPassword,
                            OmbrosDBUserName = c.OmbrosDBUserName,
                            Address1 = c.Address1,
                            Address2 = c.Address2,
                            Address3 = c.Address3,
                            Address4 = c.Address4,
                            Postcode = c.Postcode,
                            InvoiceLogoFileName = c.InvoiceLogoFilename,
                            SmsSenderId = c.SMSSenderId,
                            EmailAddress = c.EmailAddress,
                            PhoneNumber = c.PhoneNumber,
                            BankReference = c.BankReference,
                            ProductName = c.ProductName,
                            SendPortalEmails = c.SendPortalEmails,
                            DisplayName = c.DisplayName
                        }).ToList();
                }
            }
            catch(Exception ex)
            {
                _log.Error("GetCompanies: " + ex.Message, ex);
                throw;
            }

        }
    }
}
