using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cloudbase.OpenstackNova
{
    public class Flavor
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IList<Link> links;
    }
}
