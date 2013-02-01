using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Cloudbase.OpenstackCommon
{
    public class IdentityManager
    {
        private Identity _identity { get; set; }
    
        public IdentityManager(Identity identity)
        {
            this._identity = identity;
        }
    
        public AuthData GetAuthData(string openstackComponentName)
        {
            var authData = new AuthData();
            var uri = new Uri(string.Format("{0}/tokens", _identity.AuthEndpoint));
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/json";
            var bodyObject = new RequestBodyWrapper()
            {
                auth = new RequestBody()
                {
                    passwordCredentials = new PasswordCredentials()
                    {
                        password = _identity.Password,
                        username = _identity.Username,
                    },
                    tenantName = _identity.TenantName
                }
            };
            System.Web.Script.Serialization.JavaScriptSerializer oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string body = oSerializer.Serialize(bodyObject);

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(body);
            request.ContentLength = bytes.Length;
            System.IO.Stream os = request.GetRequestStream();
            os.Write(bytes, 0, bytes.Length); //Push it out there
            os.Close();

            HttpStatusCode statusCode;
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
            }
            statusCode = response.StatusCode;
            if (statusCode.Equals(HttpStatusCode.OK) )
            {
                try
                {
                    byte[] responseBody = { };
                    JsonTextReader reader = new JsonTextReader(new StreamReader(response.GetResponseStream()));
                    reader.Read();
                    JsonSerializer se = new JsonSerializer();
                    JObject parsedData = (JObject)se.Deserialize(reader);
                    var responseContent = parsedData["access"];
                     authData.AuthToken = responseContent["token"]["id"].ToString();
                    _identity.TenantId = responseContent["token"]["tenant"]["id"].ToString();
                    foreach (var endpoint in responseContent["serviceCatalog"])
                    {
                        var subEndpoint = endpoint["endpoints"];
                        var endpointName = endpoint["name"].ToString();
                        if (endpointName.Equals(openstackComponentName))
                        {
                            var endpointUrl = subEndpoint.FirstOrDefault()["publicURL"].ToString();
                            authData.Endpoint = endpointUrl;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occured ", ex.Message);
                }
            }
            response.Close();
            return authData;
        }
        
        private class RequestBodyWrapper
        {
            public RequestBody auth { get; set; }
        }

        private class RequestBody
        {
            public PasswordCredentials passwordCredentials { get; set; }

            public string tenantName { get; set; }
        }

        private class PasswordCredentials
        {
            public string username { get; set; }

            public string password { get; set; }
        }


    }
}
