using System;
using System.Collections.Generic;

namespace Service.Exceptions
{
    public class ValidationException: Exception
    {
        public ValidationException(IEnumerable<ValidationException> errors):base(errors.ToString())
        {

        }

        public ValidationException(string message) : base(message)
        {

        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
