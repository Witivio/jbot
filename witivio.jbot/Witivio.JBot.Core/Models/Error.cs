using System;
using System.Collections.Generic;
using System.Text;

namespace Witivio.JBot.Core.Models
{
    public class Error
    {
        public DateTime Date { get; set; }
        public Exception Exception { get; set; }
    }

    public abstract class ErrorProvider
    {
        private Error _error;
        public Error GetError()
        {
            return _error;
        }

        protected void SetError(Exception error)
        {
            _error = new Error { Date = DateTime.UtcNow, Exception = error };
        }
    }
}
