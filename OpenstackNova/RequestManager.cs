using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace Cloudbase.OpenstackNova
{
    public class RequestManager
    {
        private Identity _identity { get; set; }


        public RequestManager(Identity identity)
        {
            this._identity = identity;
        }

        public JObject Get(string uri, string endpoint)
        {
            JObject responseObject = null;
            IdentityManager identityManager = new IdentityManager(this._identity);
            var authData = identityManager.GetAuthData(endpoint);

            string requestUrl = authData.Endpoint + uri;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
            request.Method = "GET";
            request.Headers.Add("X-Auth-Token", authData.AuthToken);
            request.ContentType = "application/json";
            System.Web.Script.Serialization.JavaScriptSerializer oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
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
            if (statusCode.Equals(HttpStatusCode.OK) && response.ContentLength > 0)
            {
                byte[] responseBody = { };
                JsonTextReader reader = new JsonTextReader(new StreamReader(response.GetResponseStream()));
                JsonSerializer se = new JsonSerializer();
                responseObject = (JObject)se.Deserialize(reader);
            }
            response.Close();
            return responseObject;
        }

        public JObject Action(string uri, string endpoint, string body, HttpStatusCode expectedCode)
        {
            IdentityManager identityManager = new IdentityManager(this._identity);
            var authData = identityManager.GetAuthData("nova");
            JObject responseObject = null;
            string requestUrl = authData.Endpoint + uri;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
            request.Method = "POST";
            request.Headers.Add("X-Auth-Token", authData.AuthToken);
            request.ContentType = "application/json";

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
                Console.WriteLine("Error: ", ex.Message);
            }
            statusCode = response.StatusCode;

            if (statusCode.Equals(expectedCode) && response.ContentLength > 0)
            {
                byte[] responseBody = { };
                JsonTextReader reader = new JsonTextReader(new StreamReader(response.GetResponseStream()));
                JsonSerializer se = new JsonSerializer();
                responseObject = (JObject)se.Deserialize(reader);
            }
            response.Close();
            return responseObject;
        }

        public FileInfo Download(string imageId, string outputPath,string endpoint)
        {
            IdentityManager identityManager = new IdentityManager(this._identity);
            var authData = identityManager.GetAuthData(endpoint);
            var uri = new Uri(string.Format("{0}/v2/images/{1}/file", authData.Endpoint, imageId));
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Headers.Add("X-Auth-Token", authData.AuthToken);
            request.Method = "GET";
            {
                var resp = request.GetResponse();
                using (Stream stream = resp.GetResponseStream())
                using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] MyBuffer = new byte[4096];
                    int BytesRead;
                    while (0 < (BytesRead = stream.Read(MyBuffer, 0, MyBuffer.Length)))
                    {
                        fs.Write(MyBuffer, 0, BytesRead);
                    }

                }

            }
            return null;
        }
        public FileInfo Download(string imageId, string outputPath,string endpoint, Func<decimal, bool> progressCallback)
        {
            IdentityManager identityManager = new IdentityManager(this._identity);
            var authData = identityManager.GetAuthData(endpoint);
            var uri = new Uri(string.Format("{0}/v2/images/{1}/file", authData.Endpoint, imageId));
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Headers.Add("X-Auth-Token", authData.AuthToken);
            request.Method = "GET";

            var resp = request.GetResponse();
            var progress = 0;
            var progressStep = 1;
            var allBytes = 0;
            var MB10 = 1024 * 1024 * 10;
            using (Stream stream = resp.GetResponseStream())
            using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] MyBuffer = new byte[4096];
                int BytesRead;
                while (0 < (BytesRead = stream.Read(MyBuffer, 0, MyBuffer.Length)))
                {
                    fs.Write(MyBuffer, 0, BytesRead); if (progress != 100)
                    {
                        allBytes += BytesRead;
                        if (allBytes / MB10 > progress * progressStep)
                        {
                            progress++;
                            progressCallback(progress);
                        }
                    }
                }
            }
            progressCallback(100);
            return null;
        }

        public JObject Head(string uri1, string endpoint)
        {
            IdentityManager identityManager = new IdentityManager(this._identity);
            var authData = identityManager.GetAuthData(endpoint);
            var uri = new Uri(string.Format("{0}/v2/images/{1}/file", authData.Endpoint));
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Headers.Add("X-Auth-Token", authData.AuthToken);
            request.Method = "HEAD";
            return null;
        }

        public void Delete(string uri,string endpoint)
        {
            JObject responseObject = null;
            IdentityManager identityManager = new IdentityManager(this._identity);
            var authData = identityManager.GetAuthData(endpoint);

            string requestUrl = authData.Endpoint + uri;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
            request.Method = "DELETE";
            request.Headers.Add("X-Auth-Token", authData.AuthToken);
            request.ContentType = "application/json";
            System.Web.Script.Serialization.JavaScriptSerializer oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
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
            if (statusCode.Equals(HttpStatusCode.OK) && response.ContentLength > 0)
            { 
            }
        }
    }
}
