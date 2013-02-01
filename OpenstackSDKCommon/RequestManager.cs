using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace Cloudbase.OpenstackCommon
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
            if (statusCode.Equals(HttpStatusCode.OK))
            {
                JsonTextReader reader = new JsonTextReader(new StreamReader(response.GetResponseStream()));
                JsonSerializer se = new JsonSerializer();
                responseObject = (JObject)se.Deserialize(reader);
            }
            response.Close();
            return responseObject;
        }

        public JObject Upload(FileInfo file, string uri, string endpoint)
        {
            IdentityManager identityManager = new IdentityManager(this._identity);
            var authData = identityManager.GetAuthData(endpoint);
            string requestUrl = authData.Endpoint + uri;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.Headers.Add("X-Auth-Token", authData.AuthToken);
            request.Headers.Add("x-image-meta-container_format", "bare");
            request.Headers.Add("x-image-meta-is_public", "True");
            request.Headers.Add("x-image-meta-disk_format", "qcow2");
            request.Headers.Add("x-image-meta-name", "bigtestcode2");
            request.Headers.Add("x-image-meta-size", file.Length.ToString());
            request.ContentLength = file.Length;
            request.KeepAlive = true;
            request.Timeout = 600000000;
            var uploadedSize = 0;
            request.AllowWriteStreamBuffering = false;
            try
            {
                JObject responseObject = null;
                using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader bReader = new BinaryReader(stream);
                    using (Stream os = request.GetRequestStream())
                    {
                        int fourMb = 1024 * 1024 * 4;
                        while (uploadedSize < file.Length)
                        {
                            byte[] range = bReader.ReadBytes(fourMb);
                            os.Write(range, 0, range.Length);
                            uploadedSize += range.Length;
                        }
                    }
                }
                HttpWebResponse response;
                HttpStatusCode statusCode;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {

                    response = (HttpWebResponse)ex.Response;
                }
                statusCode = response.StatusCode;
                if (statusCode.Equals(HttpStatusCode.OK)||statusCode.Equals(HttpStatusCode.Created))
                {
                    byte[] responseBody = { };
                    JsonTextReader reader = new JsonTextReader(new StreamReader(response.GetResponseStream()));
                    JsonSerializer se = new JsonSerializer();
                    responseObject = (JObject)se.Deserialize(reader);
                }
                return responseObject;
            }
            catch (Exception ex)
            {
                var up = uploadedSize;
                throw;
            }
        }
        public JObject Upload(FileInfo file, string uri, string endpoint, Func<decimal, bool> progressCallback)
        {
            IdentityManager identityManager = new IdentityManager(this._identity);
            var authData = identityManager.GetAuthData(endpoint);
            string requestUrl = authData.Endpoint + uri;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.Headers.Add("X-Auth-Token", authData.AuthToken);
            request.Headers.Add("x-image-meta-container_format", "bare");
            request.Headers.Add("x-image-meta-is_public", "True");
            request.Headers.Add("x-image-meta-disk_format", "qcow2");
            request.Headers.Add("x-image-meta-name", "bigtestcode2");
            request.Headers.Add("x-image-meta-size", file.Length.ToString());
            request.ContentLength = file.Length;
            request.KeepAlive = true;
            request.Timeout = 600000000;
            var uploadedSize = 0;
            request.AllowWriteStreamBuffering = false;
            try
            {
                JObject responseObject = null;
                using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader bReader = new BinaryReader(stream);
                    using (Stream os = request.GetRequestStream())
                    {
                        int fourMb = 1024 * 1024 * 4;
                        var progress = 0;
                        var previousProgress = 2;
                        while (uploadedSize < file.Length)
                        {
                           
                            byte[] range = bReader.ReadBytes(fourMb);
                            os.Write(range, 0, range.Length);
                            uploadedSize += range.Length;
                            progress = (int)Math.Floor((double)(uploadedSize / (int)file.Length * 100));
                            if (progress%previousProgress==0&& progress!=0)
                            {
                                progressCallback(progress);
                            }
                        }
                    }

                }
                HttpWebResponse response;
                HttpStatusCode statusCode;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {

                    response = (HttpWebResponse)ex.Response;
                }
                statusCode = response.StatusCode;
                if (statusCode.Equals(HttpStatusCode.OK))
                {
                    byte[] responseBody = { };
                    JsonTextReader reader = new JsonTextReader(new StreamReader(response.GetResponseStream()));
                    JsonSerializer se = new JsonSerializer();
                    responseObject = (JObject)se.Deserialize(reader);
                }

                progressCallback(100);
                return responseObject;
            }
            catch (Exception ex)
            {
                var up = uploadedSize;
                throw;
            }
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

            if (statusCode.Equals(expectedCode))
            {
                byte[] responseBody = { };
                JsonTextReader reader = new JsonTextReader(new StreamReader(response.GetResponseStream()));
                JsonSerializer se = new JsonSerializer();
                responseObject = (JObject)se.Deserialize(reader);
            }
            response.Close();
            return responseObject;
        }

        public FileInfo Download(string imageId, string outputPath, string endpoint)
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

        public FileInfo Download(string imageId, string outputPath, string endpoint, Func<decimal, bool> progressCallback)
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

        public void Delete(string uri, string endpoint)
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
            if (statusCode.Equals(HttpStatusCode.OK))
            {
            }
        }


        public JObject Post(string uri, string body, string endpoint)
        {
            JObject responseObject = null;
            IdentityManager identityManager = new IdentityManager(this._identity);
            var authData = identityManager.GetAuthData(endpoint);
            string requestUrl = authData.Endpoint + uri;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
            request.Method = "POST";
            request.Headers.Add("X-Auth-Token", authData.AuthToken);
            request.ContentType = "application/json";
            HttpStatusCode statusCode;
            HttpWebResponse response;


            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(body);
            request.ContentLength = bytes.Length;
            System.IO.Stream os = request.GetRequestStream();
            os.Write(bytes, 0, bytes.Length);
            os.Close();
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
            }
            statusCode = response.StatusCode;
            if (statusCode.Equals(HttpStatusCode.Accepted) || statusCode.Equals(HttpStatusCode.OK))
            {
                JsonTextReader reader = new JsonTextReader(new StreamReader(response.GetResponseStream()));
                JsonSerializer se = new JsonSerializer();
                responseObject = (JObject)se.Deserialize(reader);
            }
            response.Close();
            return responseObject;
        }


    }
}
