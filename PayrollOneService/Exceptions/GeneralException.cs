using System;

namespace PayrollOneService.Exceptions
{
    public class GeneralException : Exception
    {
        public GeneralException()
        { }

        public GeneralException(string message)
            : base(message)
        { }
    }
}
