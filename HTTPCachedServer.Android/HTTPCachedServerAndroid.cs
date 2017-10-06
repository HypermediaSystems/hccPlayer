﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

using HttpCachedServer.Droid.Implementation;
using Android.Content.Res;
using System.IO;
using System.Net.Sockets;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(HTTPCachedServerDroid))]
namespace HttpCachedServer.Droid.Implementation
{
    public class HTTPCachedServerDroid : HttpCachedServer.HttpBaseServer, HttpCachedServer.iHttpServer
    {
        private readonly HttpListener _listener = new HttpListener();
        // private readonly Func<HttpListenerRequest, Byte[]> _responderMethod;


        public HTTPCachedServerDroid()
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                "Needs Windows XP SP2, Server 2003 or later.");

            string ipAddress = GetLocalIPAddress();
            this.url = "http://" + ipAddress;
            this.port = 8080;

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("prefixes");

            _listener.Prefixes.Add(this.url + ":" + this.port + "/");

            // _responderMethod = SendResponse;
            _listener.Start();
        }
        public Byte[] GetContent(HttpListenerRequest request)
        {
            Byte[] content = this.getContentDelegate?.Invoke(request.Url.ToString());
            if (content == null || content.Length == 0)
            {
                if (request.Url.ToString().StartsWith(this.url_port))
                    content = this.GetLocalContent(request.Url.AbsolutePath.Substring(1));
            }
            return content;
        }
        public Byte[] GetLocalContent(string requestedUrl)
        {
            while (requestedUrl.StartsWith("/") || requestedUrl.StartsWith("\\"))
                requestedUrl = requestedUrl.Substring(1);

            Byte[] content;
            try
            {
                AssetManager assets = Forms.Context.Assets;
                using (StreamReader sr = new StreamReader(assets.Open("Content/" + requestedUrl)))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        sr.BaseStream.CopyTo(ms);
                        content = ms.ToArray();
                    }
                }
            }
            catch (Java.Lang.Exception ex)
            {
                // ToDo log the exception
                content = Encoding.UTF8.GetBytes(this.BuildError(ex.Message + "<br/>" + ex.JniPeerMembers.JniPeerTypeName, requestedUrl));
            }
            catch (Exception ex)
            {
                // ToDo log the exception
                content = Encoding.UTF8.GetBytes(this.BuildError(ex.Message + "<br/>", url));
            }
            return content;
        }
        public void Run()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                // string rstr = _responderMethod(ctx.Request);
                                // byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                byte[] buf = this.GetContent(ctx.Request);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch (Exception ex)
                            {
                                // ToDO log this error
                                string err = ex.Message;
                            }
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
        public void ClearCache()
        {
            // ??
        }
        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return ""; ;
        }
    }
}
