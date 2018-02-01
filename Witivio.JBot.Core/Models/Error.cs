using System;
using System.Collections.Generic;
using System.Text;

namespace Witivio.JBot.Core.Models
{
    public class JBotError
    {
        public DateTime Date { get; set; }
        public Exception Exception { get; set; }
    }

    public abstract class ErrorProvider
    {
        private JBotError _error;
        public JBotError GetError()
        {
            return _error;
        }

        protected void SetError(Exception error)
        {
            _error = new JBotError { Date = DateTime.UtcNow, Exception = error };
        }
    }
}
