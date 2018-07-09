using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using Xunit;
using docs.host;
using Microsoft.AspNetCore.Http;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Configuration;
using System.Globalization;
using System.Threading;

namespace docs_host.Test
{
    public class MethodTest
    {
        private readonly string fileFolder = "C:/Users/t-zhiliu/Documents/GitHub/azure-docs-pr/_site/articles";
        private readonly string branch = "master";
        private readonly string commit = "0";
        private readonly string version = "1.0";

        private static string CollectionDocument = "Documents";
        private static string CollectionBase = "BaseInfo";


        public MethodTest()
        {
            DocumentDBRepo<BaseInfo>.Initialize(CollectionBase);
            DocumentDBRepo<Document>.Initialize(CollectionDocument);
            BlobStorage.Initialize();
        }

        [Fact]
        public async Task PutDocTest()
        {
            var httpContext = new DefaultHttpContext();
            List<Document> documents = new List<Document>();
            var allfiles = Directory.GetFiles(fileFolder, "*.json", SearchOption.AllDirectories);

            
            foreach (var file in allfiles)
            {
                StreamReader sr = new StreamReader(file);
                string srContent = sr.ReadToEnd();
                sr.Close();

                dynamic ob = JsonConvert.DeserializeObject(srContent);
                Document d = new Document
                {
                    Locale = ob.locale ?? "en-us",
                    Url = ob.redirectionUrl ?? "/azure/fake",
                    Version = version,
                    Commit = commit,
                    Blob = "Empty"
                };
                d.Id = ob.id ?? Method.IdGenerator(d.Locale, d.Url, d.Version, d.Commit);

                documents.Add(d);
            }
            
            string content = JsonConvert.SerializeObject(documents);
            byte[] byteArray = Encoding.ASCII.GetBytes(content);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                httpContext.Request.Body = stream;
                await Method.PutDoc(httpContext);
            }
        }

        [Fact]
        public async Task FindDocTest()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "branch", "master" },
                { "basename", "/azure" },
                { "url", "/azure/azure-resource-manager/resource-group-template-deploy" },
                { "locale", "en-us" },
                { "version", "1.0" }
            };

            var httpContext = new DefaultHttpContext();
            string content = JsonConvert.SerializeObject(dic);
            byte[] byteArray = Encoding.ASCII.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);

            httpContext.Request.Body = stream;
            await Method.FindDoc(httpContext);
        }

        [Fact]
        public async Task PutBlobTest()
        {
            var httpContext = new DefaultHttpContext();
            List<string> contents = new List<string>();
            var allfiles = Directory.GetFiles(fileFolder, "*.json", SearchOption.AllDirectories);

            foreach (var file in allfiles)
            {
                StreamReader sr = new StreamReader(file);
                string srContent = sr.ReadToEnd();
                sr.Close();

                dynamic ob = JsonConvert.DeserializeObject(srContent);
                if (ob["content"] == null)
                {
                    continue;
                }
                string c = ob.content;

                contents.Add(c);
            }

            string content = JsonConvert.SerializeObject(contents);
            byte[] byteArray = Encoding.ASCII.GetBytes(content);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                httpContext.Request.Body = stream;
                await Method.PutBlob(httpContext);
            }
        }

        [Fact]
        public async Task HasBlobTest()
        {
            var httpContext = new DefaultHttpContext();
            string[] hashes = new string[] { "--84MqDZvRtnEyllAQtL2plumzI", "-3LuApQ44GeMKIGRIjBn_-1JZrs" };
            
            string content = JsonConvert.SerializeObject(hashes);
            byte[] byteArray = Encoding.ASCII.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);

            httpContext.Request.Body = stream;
            await Method.HasBlob(httpContext);
        }
    }
}
