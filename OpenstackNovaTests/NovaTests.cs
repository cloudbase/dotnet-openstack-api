using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cloudbase.OpenstackNova;
using Newtonsoft.Json.Linq;
using System.IO;
using Cloudbase.OpenstackCommon;

namespace Cloudabase.OpenstackNovaTests
{
    [TestClass]
    public class NovaTests
    {

        [TestMethod]
        public void ListFlavors()
        {
            Identity identity = new Identity()
            {
                AuthEndpoint = "http://192.168.1.149:5000/v2.0",
                Password = "Passw0rd",
                TenantName = "admin",
                Username = "admin",
            };

            FlavorManager flavorManager = new FlavorManager(identity);
            IList<Flavor> flavors = flavorManager.List();

            Assert.IsTrue(flavors.Count != 0);
        }

        [TestMethod]
        public void ListInstances()
        {
            Identity identity = new Identity()
            {
                AuthEndpoint = "http://192.168.1.149:5000/v2.0",
                Password = "Passw0rd",
                TenantName = "admin",
                Username = "admin",

            };

            InstanceManager instanceManager = new InstanceManager(identity);
            IList<Instance> instances = instanceManager.ListInstances();

            Assert.IsTrue(instances.Count != 0);
        }

        [TestMethod]
        public void ListImages()
        {
            Identity identity = new Identity()
            {
                AuthEndpoint = "http://192.168.1.149:5000/v2.0",
                Password = "Passw0rd",
                TenantName = "admin",
                Username = "admin",

            };

            ImageManager imageManager = new ImageManager(identity);
            IList<OpenstackImage> instances = imageManager.ListImages();
            Assert.IsTrue(instances.Count != 0);
        }
        //id=31b0ded6-9431-48ec-b114-286980341279
        [TestMethod]
        public void CreateSnapshot()
        {
            Identity identity = new Identity()
            {
                AuthEndpoint = "http://192.168.1.149:5000/v2.0",
                Password = "Passw0rd",
                TenantName = "admin",
                Username = "admin",
            };
            InstanceManager instanceManager = new InstanceManager(identity);
            var firstInstaceId = instanceManager.ListInstances(identity)[0].Id;
            var snapshotName = instanceManager.CreateSnapshot(firstInstaceId, (x) => { return true; });
            ImageManager imageManager = new ImageManager(identity);
            OpenstackImage image = imageManager.GetImage(new OpenstackImage() { Name = snapshotName });
            Assert.AreNotSame(image, null);
        }
        [TestMethod]
        public void DeleteSnapshot()
        {
            Identity identity = new Identity()
            {
                AuthEndpoint = "http://192.168.1.149:5000/v2.0",
                Password = "Passw0rd",
                TenantName = "admin",
                Username = "admin",
            };
            InstanceManager instanceManager = new InstanceManager(identity);
            var firstInstaceId = "a9a8ba2c-fc26-4ecc-8419-ca9b7cf326a2";
            var snapshotName = instanceManager.CreateSnapshot(firstInstaceId, (x) => { return true; });
            ImageManager imageManager = new ImageManager(identity);
            OpenstackImage image = imageManager.GetImage(new OpenstackImage() { Name = snapshotName });
            Assert.AreNotSame(image, null);
            imageManager.Delete(image.Id);
            image = imageManager.GetImage(new OpenstackImage() { Name = snapshotName });
            Assert.AreEqual(image, null);
        }
        [TestMethod]
        public void DownloadSnapshot()
        {
            Identity identity = new Identity()
            {
                AuthEndpoint = "http://192.168.1.118:5000/v2.0",
                Password = "Passw0rd",
                TenantName = "admin",
                Username = "admin",
            };
            InstanceManager instanceManager = new InstanceManager(identity);
            var firstInstaceId = "f71e50ab-cf33-4491-b6f4-8887b29b2c22";
            var snapshotName = instanceManager.CreateSnapshot(firstInstaceId, (x) => { return true; });
            ImageManager imageManager = new ImageManager(identity);
            OpenstackImage image = imageManager.GetImage(new OpenstackImage() { Name = snapshotName });
            Assert.AreNotSame(image, null);
            image = imageManager.GetImage(image.Id);
            imageManager.DownloadImage(image.Id, @"E:\\AtlasVHDS\\" + image.Name + "." + image.DiskFormat);
            imageManager.Delete(image.Id);
            image = imageManager.GetImage(new OpenstackImage() { Name = snapshotName });
            Assert.AreEqual(image, null);
        }

        [TestMethod]
        public void PostKeypair()
        {
            Identity identity = new Identity()
            {
                AuthEndpoint = "http://192.168.1.149:5000/v2.0",
                Password = "Passw0rd",
                TenantName = "admin",
                Username = "admin",

            };
            FileInfo workingFolder = new FileInfo(@"E:\AtlasVHDS\");
            KeypairManager imageManager = new KeypairManager(identity);
            var name = "testkeypair"+Guid.NewGuid().ToString().Substring(0,4);
            imageManager.CreateKeypair(name, workingFolder);

        }
    }
}
