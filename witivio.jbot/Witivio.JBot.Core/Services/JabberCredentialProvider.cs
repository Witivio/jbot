using System;
using System.Threading.Tasks;
using Witivio.JBot.Core.Models;

namespace Witivio.JBot.Core.Services
{

    public interface IJabberCredentialProvider
    {
        Task<JabberCredential> GetAsync();
    }
    class JabberCredentialProvider : IJabberCredentialProvider
    {
        public async Task<JabberCredential> GetAsync()
        {
            throw new NotImplementedException();
        }
    }
}
