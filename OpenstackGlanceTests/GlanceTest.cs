using Cloudbase.OpenstackCommon;
using Cloudbase.OpenstackGlance;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cloudabase.OpenstackGlanceTests
{
    [TestClass]
    public class GlanceTest

    {
        [TestMethod]
        public void UploadImage()
        {
            string path = @"E:\AtlasVHDS\Snapshot-8e2f.ami";
            FileInfo image = new FileInfo(path);
            Identity identity = new Identity()
            {
                AuthEndpoint = "http://192.168.1.149:5000/v2.0",
                Password = "Passw0rd",
                TenantName = "admin",
                Username = "admin",
            };
            ImageManager imageManager = new ImageManager(identity);
            OpenstackImage img = new OpenstackImage()
            {
                Name = "Test" + Guid.NewGuid().ToString().Substring(0, 4),
                DiskFormat = image.Extension,
                Size = image.Length,


            };
           OpenstackImage obj= imageManager.UploadImage(image);
        }
    }
}
