using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cloudbase.OpenstackCommon;
using System.IO;
namespace Cloudbase.OpenstackNova
{
    public class KeypairManager
    {
        private Identity _identity { get; set; }

        public KeypairManager(Identity identity)
        {
            this._identity = identity;
        }

        public void CreateKeypair(string name, FileInfo output)
        {
            CreateKeypair(name, _identity, output);
        }

        public void CreateKeypair(string name, Identity identity, FileInfo output)
        {
            Keypair keypair = new Keypair();
            RequestManager requestManager = new RequestManager(identity);

            Keypair bodyObject = new Keypair()
            {
                name = name
            };
            System.Web.Script.Serialization.JavaScriptSerializer oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string body = oSerializer.Serialize(bodyObject);

            var uri = String.Format("/os-keypairs");
            JObject response = requestManager.Post(uri, "{\"keypair\":" + body + "}", "nova");
            keypair.publicKey = response["keypair"]["public_key"].ToString();
            keypair.privateKey = response["keypair"]["private_key"].ToString();
            keypair.userId = response["keypair"]["user_id"].ToString();
            keypair.name = response["keypair"]["name"].ToString();
            keypair.fingerprint = response["keypair"]["fingerprint"].ToString();
            StreamWriter stream = new StreamWriter(output.FullName + name + ".pem");
            stream.Write(keypair.privateKey);
            stream.Close();
        }

        public IList<Keypair> ListKeypairs()
        {
            return ListKeypairs(_identity);
        }
  
        public  IList<Keypair>  ListKeypairs(Identity identity)
        {
            IList<Keypair> list = new List<Keypair>();
            RequestManager requestManager = new RequestManager(identity);
            var uri = "/os-keypairs";
            JObject response = requestManager.Get(uri, "nova");
            var tempinstances = response["keypairs"];
            foreach (var tempinstance in tempinstances)
            {
                var tmp = tempinstance["keypair"];
                var keyPair = new Keypair()
                {
                    name = tmp["name"].ToString(),
                    publicKey = tmp["public_key"].ToString(),
                    fingerprint=tmp["fingerprint"].ToString(),
                };
                list.Add(keyPair);
            }
            return list;
        }

    }
}
