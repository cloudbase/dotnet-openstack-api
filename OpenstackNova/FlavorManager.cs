using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cloudbase.OpenstackCommon;
namespace Cloudbase.OpenstackNova
{
    public class FlavorManager
    {
        private Identity _identity { get; set; }

        public FlavorManager(Identity identity)
        {
            this._identity = identity;
        }

        public IList<Flavor> List()
        {
            return List(_identity);
        }

        public IList<Flavor> List(Identity identity)
        {
            IList<Flavor> list = new List<Flavor>();
            RequestManager requestManager = new RequestManager(identity);
            var uri = "/flavors/detail";
            JObject response = requestManager.Get(uri, "nova");
            var tempinstances = response["flavors"];
            foreach (var tempinstance in tempinstances)
            {
                var instance = new Flavor()
                {
                    Id = tempinstance["id"].ToString(),
                    Name = tempinstance["name"].ToString()
                };
                list.Add(instance);
            }
            return list;
        }

        public Flavor Get(string id, Identity identity)
        {
            Flavor flavor = new Flavor();
            RequestManager requestManager = new RequestManager(identity);
            var uri = String.Format("/flavors/{0}", id);
            JObject response = requestManager.Get(uri, "nova");
            flavor.Name = response["flavor"]["name"].ToString();
            flavor.Id = response["flavor"]["id"].ToString();
            return flavor;
        }

        public string GetFlavorId(string name)
        {
            return GetFlavorId(name, _identity);
        }

        public string GetFlavorId(string name, Identity identity)
        {
            var flavor = List().Where((x) => x.Name.Equals(name)).FirstOrDefault();
            return flavor.Id; 
        }
    }
}
