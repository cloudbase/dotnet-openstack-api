using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cloudbase.OpenstackNova
{
    class InstanceRequestBody
    {
        public string name { get; set; }

        public string imageRef { get; set; }

        public string key_name { get; set; }

        public string flavorRef { get; set; }
    }
}
