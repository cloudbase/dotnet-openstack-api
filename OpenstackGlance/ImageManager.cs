using Cloudbase.OpenstackCommon;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Cloudbase.OpenstackGlance
{
    public class ImageManager
    {
        public Identity _identity { get; set; }

        public ImageManager(Identity identity)
        {
            this._identity = identity;
        }

        public IList<OpenstackImage> ListImages()
        {
            return ListImages(_identity);
        }

        public IList<OpenstackImage> ListImages(Identity identity)
        {
            IList<OpenstackImage> list = new List<OpenstackImage>();
            RequestManager requestManager = new RequestManager(identity);
            var uri = "/images/detail";
            JObject response = requestManager.Get(uri, "nova");
            var tempinstances = response["images"];
            foreach (var tempinstance in tempinstances)
            {
                var instance = new OpenstackImage()
                {
                    Id = tempinstance["id"].ToString(),
                    Name = tempinstance["name"].ToString(),
                    Status = tempinstance["status"].ToString(),
                };
                list.Add(instance);
            }
            return list;
        }

        public OpenstackImage GetImage(string imageId)
        {
            return GetImage(imageId, _identity);
        }

        public OpenstackImage GetImage(string imageId, Identity identity)
        {
            OpenstackImage image;
            RequestManager requestManager = new RequestManager(identity);
            var uri = string.Format("/v2/images/{0}", imageId);
            JObject response = requestManager.Get(uri, "glance");
            var tempinstance = response;
            if (response != null)
            {
                image = new OpenstackImage()
                {
                    Id = tempinstance["id"].ToString(),
                    Name = tempinstance["name"].ToString(),
                    Status = tempinstance["status"].ToString(),
                };
                if (tempinstance["container_format"] != null)
                    image.ContainerFormat = tempinstance["container_format"].ToString();
                if (tempinstance["disk_format"] != null)
                    image.DiskFormat = tempinstance["disk_format"].ToString();
                return image;
            }
            return null;
        }
        public OpenstackImage GetImage(OpenstackImage imageId)
        {
            return GetImage(imageId, _identity);
        }

        public OpenstackImage GetImage(OpenstackImage image, Identity identity)
        {
            //TODO: get the image from glance without listing all of them, using filtered request by name
            var image2 = ListImages().Where((x) => x.Name.Equals(image.Name)).ToList().FirstOrDefault();
            return GetImage(image2.Id);
        }

        public FileInfo DownloadImage(string imageId, string outputPath)
        {
            return DownloadImage(imageId, outputPath, _identity);
        }

        public FileInfo DownloadImage(string imageId, string outputPath, Identity identity)
        {
            RequestManager requestManager = new RequestManager(identity);
            var uri = string.Format("/v2/images/{0}/file", imageId);
            FileInfo result = requestManager.Download(imageId, outputPath, "glance");
            return result;
        }
        public FileInfo DownloadImage(string imageId, string outputPath, Func<decimal, bool> progressCallback)
        {
            RequestManager requestManager = new RequestManager(_identity);
            var uri = string.Format("/v2/images/{0}/file", imageId);
            FileInfo result = requestManager.Download(imageId, outputPath, "glance", progressCallback);
            return result;
        }

        public OpenstackImage UploadImage(FileInfo image)
        {
            return UploadImage(image, _identity);

        }

        public OpenstackImage UploadImage(FileInfo image, Identity identity)
        {
            RequestManager requestManager = new RequestManager(_identity);
            var uri = string.Format("/v1/images");
            JObject result = requestManager.Upload(image, uri, "glance");
            var jImage = result["image"];
            OpenstackImage returnedImage = new OpenstackImage()
            {
                CheckSum = jImage["checksum"].ToString(),
                ContainerFormat = jImage["container_format"].ToString(),
                DiskFormat = jImage["disk_format"].ToString(),
                Id = jImage["id"].ToString(),
                IsPublic = jImage["is_public"].ToString() == "true" ? true : false,
                Name = jImage["name"].ToString(),
                Size = long.Parse(jImage["size"].ToString()),
                Status = jImage["status"].ToString(),
            };

            return returnedImage;
        }

        public OpenstackImage UploadImage(FileInfo image, Func<decimal, bool> progressCallback)
        {
            RequestManager requestManager = new RequestManager(_identity);
            var uri = string.Format("/v1/images");
            JObject result = requestManager.Upload(image, uri, "glance", progressCallback);
            var jImage = result["image"];
            OpenstackImage returnedImage = new OpenstackImage()
            {
                CheckSum = jImage["checksum"].ToString(),
                ContainerFormat = jImage["container_format"].ToString(),
                DiskFormat = jImage["disk_format"].ToString(),
                Id = jImage["id"].ToString(),
                IsPublic = jImage["is_public"].ToString() == "true" ? true : false,
                Name = jImage["name"].ToString(),
                Size = long.Parse(jImage["size"].ToString()),
                Status = jImage["status"].ToString(),
            };

            return returnedImage;
        }

        public void Delete(string imageId)
        {
            RequestManager requestManager = new RequestManager(_identity);
            var uri = string.Format("/v2/images/{0}", imageId);
            requestManager.Delete(uri, "glance");
            ImageManager imageManager = new ImageManager(_identity);
            var deleteFinished = false;
            if (imageId != null)
            {
                while (!deleteFinished)
                {
                    OpenstackImage image = imageManager.GetImage(imageId);
                    if (image != null)
                    {
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        deleteFinished = true;
                    }
                }
            }
        }

    }
}
