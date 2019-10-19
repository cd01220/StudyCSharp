using System;
using System.Collections.Generic;
using System.Text;

namespace Changes.ViewModels
{
    public class ApiKey
    {
        public string PublicKey { get; set; }   // okex API Key
        public string PrivateKey { get; set; }  // okex Secret
        public string Passphrase { get; set; }  // okex Passphrase
    }
}
