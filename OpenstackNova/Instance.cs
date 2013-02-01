using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cloudbase.OpenstackNova
{
    public class Instance
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string tenant_id { get; set; }

        public string user_id { get; set; }

        public string created { get; set; }

        public string hostId { get; set; }

        public InstanceStatus status { get; set; }

        public int progress { get; set; }
        
        public string FlavorName { get; set; }
        
        public string FlavorId { get; set; }
    }
}
