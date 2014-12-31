using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;

namespace PayrollOneService
{
    public class PayrollOneService
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(PayrollOneService));
        private readonly IEmployerFactory _employerFactory;

        public PayrollOneService(IEmployerFactory employerFactory)
        {
            _employerFactory = employerFactory;
            _log.Debug("Payroll Service ctor");
        }

        public virtual List<API.Payroll> GetPayrolls(API.Company company)
        {
            try
            {
                var context = new PayrollOneDataContext(_employerFactory.SqlConnectionString(company));
                return context.Payrolls.Select(p => new API.Payroll
                {
                    Description = p.Description,
                    Frequency = p.Frequency
                }).ToList();
            }
            catch(Exception ex)
            {
                _log.Error("GetPayrolls: " + ex.Message, ex);
                throw;
            }
        }

        public List<EmployeeValidation> Validate(API.Company company, List<API.Employee> employees, string payrollDescription)
        {
            var employeesToValidate = employees.ToList();
            var result = new List<EmployeeValidation>();
            var employeeRecords = new List<API.Employee>();

            using(var employer = _employerFactory.OpenEmployer(company))
            {
                var payroll = employer.Payrolls.FirstOrDefault(x => x.Description == payrollDescription);
                if(payroll != null)
                {
                    employeeRecords.AddRange(payroll.Employees.Where(e => employeesToValidate.Select(x => x.AgencyId).Contains(e.Tag1) ||
                        employeesToValidate.Select(x => x.Id).Contains(e.Code)).Select(e => new EmployeeProxy(e)
                        {
                            Company = company,
                            PayFrequency = (payroll.Frequency == PayrollOneAPI.PayFrequency.Month)
                                ? PayFrequency.Monthly : PayFrequency.Weekly
                        }).ToArray());
                }
            }

            foreach(var employee in employeesToValidate)
            {
                Payroll.API.Employee employeeRecord = null;

                // check by agency id if we have it
                if(!string.IsNullOrEmpty(employee.AgencyId))
                {
                    employeeRecord = employeeRecords.FirstOrDefault(e => e.AgencyId == employee.AgencyId);
                }
                // check by payrollid if we have it
                else if(!string.IsNullOrEmpty(employee.Id))
                {
                    employeeRecord = employeeRecords.FirstOrDefault(e => e.Id == employee.Id);
                }

                result.Add(new EmployeeValidation { Valid = employeeRecord != null, Candidate = employee, Record = employeeRecord });

            }

            return result.ToList();
        }


        public virtual List<EmployeeValidation> Persist(API.Company company, string payrollDescription, List<Payroll.API.Employee> employees)
        {
            try
            {
                var employeesToPersist = employees.ToList();
                var result = (from employee in employeesToPersist
                              where string.IsNullOrEmpty(employee.NiNumber) && string.IsNullOrEmpty(employee.AgencyId)
                              select new EmployeeValidation
                              {
                                  Candidate = employee,
                                  Valid = false,
                                  ValidationMessage = "No NI Number or Agency ID"
                              }).ToList();

                foreach (var employeeToRemove in result.Select(e => e.Candidate))
                    employeesToPersist.Remove(employeeToRemove);

                using (var employer = _employerFactory.OpenEmployer(company))
                {
                    var payroll = employer.Payrolls.First(x => x.Description == payrollDescription);

                    foreach (var employeeToPersist in employeesToPersist)
                    {
                        try
                        {
                            var employee = FindEmployee(employer, employeeToPersist.Id);

                            if (employee == null)
                            {
                                employee = payroll.NewEmployee();
                            }
                            else
                            {
                                employee.BeginEdit();
                            }

                            employee.Forenames = employeeToPersist.FirstName;
                            employee.Surname = employeeToPersist.Surname;

                            employee.NINumber = (employeeToPersist.NiNumber.Trim().Length == 9 &&
                                                 employeeToPersist.NiNumber.Trim().ToUpper().Substring(0, 2) != "TN")
                                ? employeeToPersist.NiNumber.Trim()
                                : string.Empty;

                            var addressLines =
                                new[]
                                {
                                    employeeToPersist.Address1, employeeToPersist.Address2, employeeToPersist.Address3,
                                    employeeToPersist.Address4
                                }.
                                    Where(line => !string.IsNullOrWhiteSpace(line)).Select(l => l.Replace("\r", " ").
                                        Replace("\n", " ").Replace("`", "'").Replace("#", "").Trim
                                        ()).ToList();

                            employee.Tag1 = employeeToPersist.AgencyId;
                            employee.TaxCode = employeeToPersist.TaxCode;
                            employee.Address1 = (addressLines.Count > 0) ? addressLines[0] : string.Empty;
                            employee.Address2 = (addressLines.Count > 1) ? addressLines[1] : string.Empty;
                            employee.Address3 = (addressLines.Count > 2) ? addressLines[2] : string.Empty;
                            employee.Address4 = (addressLines.Count > 3) ? addressLines[3] : string.Empty;
                            employee.PostCode = employeeToPersist.PostCode;
                            employee.Title = employeeToPersist.Title;
                            employee.Joined = employeeToPersist.StartDate;
                            employee.DOB = employeeToPersist.DOB;
                            employee.SortCode = employeeToPersist.BankSortCode;
                            employee.AccountNumber = employeeToPersist.BankAccountNumber;
                            employee.EMail = employeeToPersist.Email;
                            employee.MobilePhone = employeeToPersist.Mobile;
                            employee.HomePhone = employeeToPersist.LandLine;
                            employee.BankName = employeeToPersist.BankName;
                            employee.NITable = employeeToPersist.NICategory;
                            employee.TaxBasis = employeeToPersist.Week1Basis;

                            employee.Update();

                            employeeToPersist.Id = employee.Code;

                            result.Add(new EmployeeValidation
                            {
                                Candidate = employeeToPersist,
                                Record = new EmployeeProxy(employee),
                                Valid = true,
                                ValidationMessage = string.Empty
                            });
                        }
                        catch (Exception ex)
                        {
                            result.Add(new EmployeeValidation
                            {
                                Candidate = employeeToPersist,
                                Valid = false,
                                ValidationMessage =
                                    PayrollOneAPI.Helper.GetExplanation(ex)
                                        .Aggregate(string.Empty, (accumulate, item) => accumulate + " " + item.Text)
                            });

                            break;
                        }
                    }
                }

                return result.ToList();
            }
            catch(Exception ex)
            {
                _log.Error("Persist: " + ex.Message, ex);
                throw;
            }
        }

        private PayrollAPI.Employee FindEmployee(PayrollOneAPI.Employer employer, string id)
        {
            return employer.Payrolls.SelectMany(p => p.Employees).FirstOrDefault(e => e.Code == id);
        }

        public virtual List<Payroll.API.Employee> GetAll(bool includeLeavers)
        {
            try
            {

                var employees = new List<API.Employee>();

                foreach(var company in _employerFactory.Connections.Select(c => c.Company))
                    employees.AddRange(GetAllForCompany(company, includeLeavers));

                return employees;
            }
            catch(Exception ex)
            {
                _log.Error("List: " + ex.Message, ex);
                throw;
            }

        }

        public virtual List<API.Employee> GetAllForCompany(Payroll.API.Company company, bool includeLeavers = false)
        {
            try
            {
                var employeeList = new List<API.Employee>();
                using(var employer = _employerFactory.OpenEmployer(company))
                {
                    foreach(var payroll in employer.Payrolls)
                    {
                        employeeList.AddRange(payroll.Employees.Select(e => new EmployeeProxy(e)
                        {
                            Company = company,
                            PayFrequency = (payroll.Frequency == PayrollOneAPI.PayFrequency.Month)
                                ? PayFrequency.Monthly : PayFrequency.Weekly,
                            PayrollDescription =  payroll.Description
                        }).ToArray());
                    }
                }

                return includeLeavers ? employeeList : employeeList.Where(e => !e.HasLeft).ToList();
            }
            catch(Exception ex)
            {
                _log.Error("GetAllForCompany: " + ex.Message, ex);
                throw;
            }

        }

        public virtual List<API.Employee> GetAllForCompanyPayroll(API.Company company, string payrollDescription, bool includeLeavers = false)
        {
            try
            {
                var employeeList = new List<API.Employee>();
                using(var employer = _employerFactory.OpenEmployer(company))
                {
                    var payroll = employer.Payrolls.FirstOrDefault(x => x.Description == payrollDescription);
                    if(payroll != null)
                    {
                        employeeList.AddRange(payroll.Employees.Select(e => new EmployeeProxy(e)
                        {
                            Company = company,
                            PayFrequency = (payroll.Frequency == PayrollOneAPI.PayFrequency.Month)
                                ? PayFrequency.Monthly : PayFrequency.Weekly,
                            PayrollDescription = payroll.Description
                        }).ToArray());
                    }
                }

                return includeLeavers ? employeeList : employeeList.Where(e => !e.HasLeft).ToList();
            }
            catch(Exception ex)
            {
                _log.Error("GetAllForCompanyPayroll: " + ex.Message, ex);
                throw;
            }

        }

        public virtual void PlaceEmployeesOnHoldExcept(List<EmployeeCompanyId> exceptEmployees)
        {
            try
            {
                foreach(var grouping in exceptEmployees.GroupBy(g => g.Company))
                    using(var employer = _employerFactory.OpenEmployer(grouping.Key))
                        foreach(var employee in employer.Payrolls.SelectMany(p => p.Employees))
                            employee.Dormant = grouping.All(i => i.EmployeeId != employee.Code);
            }
            catch(Exception ex)
            {
                _log.Error("PlaceEmployeesOnHoldExcept: " + ex.Message, ex);
                throw;
            }

        }

        public virtual void ResetData(Payroll.API.Company company)
        {

            _log.Error("ResetData not implemented");

            // throw new NotImplementedException();
        }

        public ProcessPayrollAggregateResult ProcessPayroll(API.Company company, List<GrossPaymentLine> grossPaymentLines, string payrollDescription, bool returnNetPaymentData)
        {
            var result = new ProcessPayrollAggregateResult
            {
                Results = new List<ProcessPayrollResult>(),
                Success = true,
                FailureMessage = string.Empty
            };

            var grossPaymentsList = grossPaymentLines.ToList();

            using(var employer = _employerFactory.OpenEmployer(company))
            {
                try
                {
                    var payroll = employer.Payrolls.First(x => x.Description == payrollDescription);

                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    var addPaymentResult = AddEmployeePayments(employer, payroll, grossPaymentsList, result.Results);
                    _log.DebugFormat("Created payments for {0} employees in {1} ms (batch {2})", grossPaymentsList.Count(), stopWatch.ElapsedMilliseconds, 1);

                    if(!addPaymentResult.Item1)
                    {
                        result.Success = false;
                        result.FailureMessage = addPaymentResult.Item2;
                        return result;
                    }

                    stopWatch = new Stopwatch();
                    stopWatch.Start();

                    if(grossPaymentsList.Count() > 250)
                        payroll.Calculate();
                    else
                        foreach(var employee in grossPaymentsList.Select(i => FindEmployee(employer, i.EmployeeCompanyId.EmployeeId)))
                            payroll.CalculateEmployee(employee);

                    _log.DebugFormat("Calculated payroll for {0} employees in {1} ms (batch {2})", grossPaymentsList.Count(), stopWatch.ElapsedMilliseconds, 1);

                    result.Results.AddRange(GetCalculatedData(company, grossPaymentsList, returnNetPaymentData, payroll, grossPaymentsList.Count));
                }
                catch(Exception ex)
                {
                    var explanation = PayrollOneAPI.Helper.GetExplanation(ex).ToString();

                    _log.DebugFormat("Error processing {0} : {1}", ex.Message, explanation);

                    result.Success = false;
                    result.FailureMessage = explanation;
                    return result;
                }
            }

            return result;
        }

        private IEnumerable<ProcessPayrollResult> GetCalculatedData(Company company, IEnumerable<GrossPaymentLine> grossPaymentLines, bool returnNetPaymentData, PayrollOneAPI.Payroll payroll, int lineCount)
        {
            var context = new PayrollOneClassesDataContext(_employerFactory.SqlConnectionString(company));

            var periodPayElements = context.PeriodPayElements.Where(e =>
                e.Frequency == Enum.GetName(typeof(PayrollAPI.PayFrequency), payroll.Frequency) &&
                    e.PAYEYear == payroll.CurrentYear &&
                    e.Period == payroll.CurrentPeriod).ToList();

            var p11TaxItemsForYear = context.P11Taxes.Where(e =>
                e.Frequency == Enum.GetName(typeof(PayrollAPI.PayFrequency), payroll.Frequency) &&
                    e.PAYEYear == payroll.CurrentYear).ToList();

            var p11NIItemsforYear = context.P11NIs.Where(e =>
                e.Frequency == Enum.GetName(typeof(PayrollAPI.PayFrequency), payroll.Frequency) &&
                    e.PAYEYear == payroll.CurrentYear).ToList();

            var p45TaxablePayItems = context.Employees.Select(e => new P45Data
            {
                EmployeeID = e.EmployeeID,
                PayToDateRaw = e.P45TaxablePay,
                TaxToDateRaw = e.P45Tax,
                P45Received = e.P45Received,
                P46FilledIn = e.P46FilledIn,
                P6Received = e.P6Received
            });

            if(returnNetPaymentData)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var employees = context.Employees.ToDictionary(e => e.EmployeeCode, e => e.EmployeeID);

                var results = CreatePayslips(grossPaymentLines.Select(g => new Tuple<int, GrossPaymentLine>(employees[g.EmployeeCompanyId.EmployeeId], g)),
                    periodPayElements, p11TaxItemsForYear, p11NIItemsforYear, p45TaxablePayItems, company, payroll).ToList();
                _log.DebugFormat("Created payslips for {0} employees in {1} ms (batch {2})", lineCount,
                    stopWatch.ElapsedMilliseconds, 1);
                return results;
            }
            return grossPaymentLines.Select(e => new ProcessPayrollResult
            {
                Success = true,
                EmployeePayrollId = e.EmployeeCompanyId.EmployeeId
            }).ToList();
        }

        private IEnumerable<ProcessPayrollResult> CreatePayslips(IEnumerable<Tuple<int, GrossPaymentLine>> grossPaymentLines, IEnumerable<PeriodPayElement> periodPayElements, IEnumerable<P11Tax> p11TaxesForYear, IEnumerable<P11NI> p11NIsForYear, IEnumerable<P45Data> p45TaxablePayItems, API.Company company, PayrollAPI.Payroll payroll)
        {
            return grossPaymentLines.Select(e => CreatePayslip(e, periodPayElements, p11TaxesForYear, p11NIsForYear, p45TaxablePayItems, company, payroll));
        }

        private ProcessPayrollResult CreatePayslip(Tuple<int, GrossPaymentLine> grossPaymentLine, IEnumerable<PeriodPayElement> periodPayElements, IEnumerable<P11Tax> p11TaxesForYear, IEnumerable<P11NI> p11NIsForYear, IEnumerable<P45Data> p45TaxablePayItems, API.Company company, PayrollAPI.Payroll payroll)
        {
            var netPaymentLine = new NetPaymentLine();

            try
            {
                var payElements = periodPayElements.Where(e => e.EmployeeID == grossPaymentLine.Item1).ToArray();

                var p11TaxForYear = p11TaxesForYear.Where(t => t.EmployeeID == grossPaymentLine.Item1).ToArray();
                var p11NIForYear = p11NIsForYear.Where(t => t.EmployeeID == grossPaymentLine.Item1).ToArray();

                var p11Tax = p11TaxForYear.FirstOrDefault(e => e.Period == payroll.CurrentPeriod);
                var p11NI = p11NIForYear.Where(e => e.Period == payroll.CurrentPeriod);

                var p45TaxablePayItem = p45TaxablePayItems.FirstOrDefault(e => e.EmployeeID == grossPaymentLine.Item1);

                var p45PayTD = p45TaxablePayItem == null ? 0 : p45TaxablePayItem.PayToDate;
                var p45TaxTD = p45TaxablePayItem == null ? 0 : p45TaxablePayItem.TaxToDate;

                netPaymentLine.EmployeeCompanyId = new EmployeeCompanyId
                {
                    Company = company,
                    EmployeeId = grossPaymentLine.Item2.EmployeeCompanyId.EmployeeId
                };

                netPaymentLine.EES = Convert.ToDouble(p11NI.Sum(n => n.NIEE));
                netPaymentLine.ERS = Convert.ToDouble(p11NI.Sum(n => n.NIER));
                //netPaymentLine.Net = Convert.ToDouble(payElements.Sum(e => e.Pay));
                netPaymentLine.Net = Convert.ToDouble(payElements.Sum(e => e.PayValue));
                netPaymentLine.IncomeTax = Convert.ToDouble(p11Tax == null ? 0 : p11Tax.Tax);

                netPaymentLine.EESNITD = Convert.ToDouble(p11NIForYear.Sum(p => p.NIEE));

                netPaymentLine.TaxableGrossTD = Convert.ToDouble(p11TaxForYear.Sum(t => t.TaxablePay) + p45PayTD);
                netPaymentLine.TaxTD = Convert.ToDouble(p11TaxForYear.Sum(t => t.Tax) + p45TaxTD);

                if(p11Tax == null)
                {
                    var employee = payroll.Employees.Single(e => e.ID == grossPaymentLine.Item1);
                    netPaymentLine.TaxCode = employee.TaxCode;
                    netPaymentLine.TaxBasis = employee.TaxBasis;
                }
                else
                {
                    netPaymentLine.TaxCode = p11Tax.TaxCode;
                    //netPaymentLine.TaxBasis = p11Tax.TaxBasis;
                    netPaymentLine.TaxBasis = p11Tax.TaxBasis == 1;
                }


                if(payElements.Any(e => e.Variation == "LOAN"))
                    netPaymentLine.SLRPaymentCurrent =
                        payElements.Where(p => p.Variation == "LOAN").Sum(e => e.NetPay.GetValueOrDefault());

                return new ProcessPayrollResult { Success = true, EmployeePayrollId = grossPaymentLine.Item2.EmployeeCompanyId.EmployeeId, NetPaymentLine = netPaymentLine }; ;
            }
            catch(Exception ex)
            {
                return new ProcessPayrollResult { Success = false, FailureMessage = PayrollOneAPI.Helper.GetExplanation(ex).ToString(), EmployeePayrollId = grossPaymentLine.Item2.EmployeeCompanyId.EmployeeId };
            }
        }

        private Tuple<bool, string> AddEmployeePayments(PayrollOneAPI.Employer employer, PayrollOneAPI.Payroll payroll, IEnumerable<GrossPaymentLine> grossPaymentLines, List<ProcessPayrollResult> results)
        {
            try
            {
                var str = new StringBuilder("Employee Code\tBASIC:Pay\tNONTAX:Pay\n");

                foreach(var line in grossPaymentLines)
                {
                    str.Append(String.Format("{0}\t{1}\t{2}\n", new[]
                    {
                        line.EmployeeCompanyId.EmployeeId.ToString(),
                        line.TaxablePay.ToString("F"),
                        line.NonTaxablePay.ToString("F")
                    }));
                }

                employer.GetPendingMessages();
                if(!payroll.StreamTimesheets(str.ToString()))
                {
                    return new Tuple<bool, string>(false,
                        employer.GetPendingMessages().Aggregate(string.Empty, (s, item) => s + "\r\n" + item.Text));
                }

                return new Tuple<bool, string>(true, string.Empty);
            }
            catch(Exception ex)
            {
                return new Tuple<bool, string>(false, PayrollOneAPI.Helper.GetExplanation(ex).ToString());
            }
        }

        public string TestConnection()
        {
            try
            {
                foreach(var result in _employerFactory.Connections.Select(c => c.Company).Select(TestConnection).Where(result => result != "OK"))
                    return result;

                return "OK";
            }
            catch(Exception ex)
            {
                _log.Error("TestConnection: " + ex.Message, ex);
                throw;
            }

        }

        public string TestConnection(Payroll.API.Company company)
        {
            try
            {
                using(var employer = _employerFactory.OpenEmployer(company))
                {
                    var payroll = employer.Payrolls[0];
                }
            }
            catch(Exception ex)
            {
                _log.Error(ex.Message, ex);
                return ex.Message;
            }

            return "OK";
        }

        public void CreateCompany(API.Company company)
        {
            try
            {
                _employerFactory.CreateCompany(company);
            }
            catch(Exception ex)
            {
                _log.Error("CreateCompany: " + ex.Message, ex);
                throw;
            }

        }

        public void DeleteCompany(string companyName)
        {
            try
            {
                _employerFactory.DeleteCompany(companyName);
            }
            catch(Exception ex)
            {
                _log.Error("DeleteCompany: " + ex.Message, ex);
                throw;
            }

        }

        public List<API.Company> GetCompanies()
        {
            try
            {
                return _employerFactory.GetCompanies();
            }
            catch(Exception ex)
            {
                _log.Error("GetCompanies: " + ex.Message, ex);
                throw;
            }

        }

        public void Ping()
        {
            try
            {
                return;
            }
            catch(Exception ex)
            {
                _log.Error("Ping: " + ex.Message, ex);
                throw;
            }
        }

        public void Dispose()
        { }

        public static IEnumerable<IEnumerable<T>> ToChunks<T>(IEnumerable<T> enumerable, int chunkSize)
        {
            var itemsReturned = 0;
            var list = enumerable.ToList(); // Prevent multiple execution of IEnumerable.
            var count = list.Count;
            while(itemsReturned < count)
            {
                var currentChunkSize = Math.Min(chunkSize, count - itemsReturned);
                yield return list.GetRange(itemsReturned, currentChunkSize);
                itemsReturned += currentChunkSize;
            }
        }
}
