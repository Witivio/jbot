using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Witivio.JBot.Core.Models;
using Witivio.JBot.Core.Services;

namespace Witivio.JBot.Core
{
    public interface ICommunicationLinker : IDisposable
    {
        void Start();
        Error GetError();
    }

    public class CommunicationLinker : ErrorProvider, ICommunicationLinker
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
