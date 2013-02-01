using Cloudbase.OpenstackCommon;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Cloudbase.OpenstackNova
{
    public class InstanceManager
    {
        private Identity _identity { get; set; }

        public InstanceManager(Identity identity)
        {
            this._identity = identity;
        }

        public IList<Instance> ListInstances()
        {
            return ListInstances(_identity);
        }

        public IList<Instance> ListInstances(Identity identity)
        {
            IList<Instance> list = new List<Instance>();
            RequestManager requestManager = new RequestManager(identity);
            var uri = "/servers/detail";
            JObject response = requestManager.Get(uri, "nova");
            var tempinstances = response["servers"];
            foreach (var tempinstance in tempinstances)
            {
                var instance = new Instance()
                {
                    Id = tempinstance["id"].ToString(),
                    Name = tempinstance["name"].ToString(),
                    FlavorName = new FlavorManager(identity).Get(tempinstance["flavor"]["id"].ToString(), identity).Name
                };
                list.Add(instance);
            }
            return list;
        }

        public Instance Get(string id, Identity identity)
        {
            Instance instance = null;
            RequestManager requestManager = new RequestManager(identity);
            var uri = String.Format("/servers/{0}", id);
            JObject response = requestManager.Get(uri, "nova");
            var tempinstance = response["server"];
            instance = new Instance()
            {
                Id = tempinstance["id"].ToString(),
                Name = tempinstance["name"].ToString()
            };
            return instance;
        }

        public string CreateSnapshot(string instanceId)
        {
            return CreateSnapshot(instanceId, _identity);
        }

        public string CreateSnapshot(string instanceId, Identity identity)
        {
            var snapshotName = "Snapshot-" + Guid.NewGuid().ToString().Substring(0, 4);
            IList<OpenstackImage> list = new List<OpenstackImage>();
            RequestManager requestManager = new RequestManager(identity);
            var uri = string.Format("/servers/{0}/action", instanceId);

            var bodyObject = new RequestBodyWrapper()
            {
                createImage = new RequestBody()
                {
                    name = snapshotName,
                    metadata = new Metadata()
                    {
                        CreatedBy = "Created by Atlas"
                    }
                }
            };
            System.Web.Script.Serialization.JavaScriptSerializer oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string body = oSerializer.Serialize(bodyObject);
            JObject response = requestManager.Action(uri, "nova", body, HttpStatusCode.Accepted);
            return snapshotName;
        }

        public string CreateSnapshot(string instanceId, Func<decimal, bool> progressCallback)
        {
            string snapshotName = CreateSnapshot(instanceId, _identity);
            bool snapshotFinished = false;
            var progress = 0;
            var progressStep = 5;
            ImageManager imageManager = new ImageManager(_identity);
            if (snapshotName != null)
            {
                OpenstackImage imageParent = imageManager.GetImage(new OpenstackImage() { Name = snapshotName });
                while (!snapshotFinished)
                {
                    OpenstackImage image = imageManager.GetImage(imageParent.Id);
                    if (image.Status.ToLower() == "active")
                    {
                        snapshotFinished = true;
                        progress = 100;
                    }
                    if (image.Status.ToLower() == "error")
                    {
                        throw new Exception("Snapshot failed to create!");
                    }
                    if (progress != 100)
                    {
                        if (!snapshotFinished && progress + progressStep < 99)
                            progress += progressStep;
                    }
                    progressCallback(progress);
                    Thread.Sleep(10000);
                }
            }
            return snapshotName;
        }

        public Instance GetInstance(string instanceId)
        {
            return GetInstance(instanceId, _identity);
        }
        public Instance GetInstance(string instanceId, Identity identity)
        {
            Instance instance;
            RequestManager requestManager = new RequestManager(identity);
            var uri = string.Format("/servers/{0}", instanceId);
            JObject response = requestManager.Get(uri, "nova");
            var tempinstance = response["server"];
            if (response != null)
            {
                instance = new Instance()
               {
                   Id = tempinstance["id"].ToString(),
                   Name = tempinstance["name"].ToString(),
                   status = (InstanceStatus)Enum.Parse(typeof(InstanceStatus), tempinstance["status"].ToString()),

               };

                return instance;
            }
            return null;
        }

        public Instance GetInstance(Instance instance)
        {
            return GetInstance(instance, _identity);
        }

        public Instance GetInstance(Instance instance, Identity identity)
        {
            return GetInstance(instance.Id, _identity);
        }

        public Instance CreateInstance(string instanceName, string imageId, string keypairName, string flavorId)
        {
            return CreateInstance(instanceName, imageId, keypairName, flavorId, _identity);
        }

        public Instance CreateInstance(string instanceName, string imageId, string keypairName, string flavorId, Func<decimal, bool> progressCallback)
        {
            Instance instance = CreateInstance(instanceName, imageId, keypairName, flavorId, _identity);
            bool instanceFinished = false;
            var progress = 0;
            var progressStep = 5;
            InstanceManager instanceManager = new InstanceManager(_identity);
            if (instanceName != null)
            {
                Instance currentInstance = instanceManager.GetInstance(new Instance() { Name = instanceName });
                while (!instanceFinished)
                {
                    currentInstance = instanceManager.GetInstance(currentInstance.Id);
                    if (currentInstance.status == InstanceStatus.ACTIVE)
                    {
                        instanceFinished = true;
                        progress = 100;
                    }
                    if (currentInstance.status == InstanceStatus.ERROR)
                    {
                        throw new Exception("Instance failed to create!");
                    }
                    if (progress != 100)
                    {
                        if (!instanceFinished && progress + progressStep < 98)
                            progress += progressStep;
                    }
                    progressCallback(progress);
                    Thread.Sleep(10000);
                }
            }
            return instance;
        }

        public Instance CreateInstance(string instanceName, string imageId, string keypairName, string flavorId, Identity identity)
        {
            RequestManager requestManager = new RequestManager(identity);
            var uri = string.Format("/servers");

            var bodyObject = new InstanceRequestBodyWrapper()
            {
                server = new InstanceRequestBody()
                {
                    name = instanceName,
                    imageRef = imageId,
                    key_name = keypairName,
                    flavorRef = flavorId,
                }
            };
            System.Web.Script.Serialization.JavaScriptSerializer oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string body = oSerializer.Serialize(bodyObject);
            JObject response = requestManager.Post(uri, body, "nova");
            
          
            if (response != null)
            {
                var tempinstance = response["server"];
                var instance = new Instance()
                {
                    Id = tempinstance["id"].ToString(),
                };

                return instance;
            }
            return null;
        }

        public class RequestBodyWrapper
        {
            public RequestBody createImage { get; set; }
        }

        public class RequestBody
        {
            public string name { get; set; }

            public Metadata metadata { get; set; }
        }

        public class Metadata
        {
            public string CreatedBy { get; set; }
        }
    }
}
