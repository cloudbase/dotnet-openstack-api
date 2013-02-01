using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cloudbase.OpenstackNova
{
    public class Keypair
    {
        public string publicKey { get; set; }
        public string privateKey { get; set; }
        public string userId { get; set; }
        public string name { get; set; }
        public string fingerprint { get; set; }

    }
}
