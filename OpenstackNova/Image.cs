using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cloudbase.OpenstackNova
{
    public class OpenstackImage
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string DiskFormat { get; set; }

        public string ContainerFormat { get; set; }

        public long Size { get; set; }

        public long CheckSum { get; set; }

        public string Location { get; set; }

        public string Status { get; set; }

        public bool IsPublic { get; set; }

        public string ParentInstanceId { get; set; } //metadata - instance_uuid
        public IList<Link> links;
    }
}
