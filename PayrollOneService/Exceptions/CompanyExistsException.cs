using System;

namespace PayrollOneService.Exceptions
{
    public class CompanyExistsException : Exception
    {
        public CompanyExistsException()
        {}

        public CompanyExistsException(string message) : base(message)
        {}
    }
}
