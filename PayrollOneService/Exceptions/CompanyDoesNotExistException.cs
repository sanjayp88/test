using System;

namespace PayrollOneService.Exceptions
{
    public class CompanyDoesNotExistException : Exception
    {
        public CompanyDoesNotExistException()
        {}

        public CompanyDoesNotExistException(string message)
            : base(message)
        {}
    }
}
