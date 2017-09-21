using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using HMS.Net.Http;
using System.IO;
using System.Xml;

namespace hccPlayer
{
	public class HybridWebView : View
	{
		Action<string> action;

		public static readonly BindableProperty UriProperty = BindableProperty.Create (
			propertyName: "Uri",
			returnType: typeof(string),
			declaringType: typeof(HybridWebView),
			defaultValue: default(string));
        public static readonly BindableProperty NavigatingProperty = BindableProperty.Create(
            propertyName: "Navigating",
            returnType: typeof(bool),
            declaringType: typeof(HybridWebView),
            defaultValue: false);

        public string Uri {
			get { return (string)GetValue (UriProperty); }
			set { SetValue (UriProperty, value); }
		}
        public bool Navigating
        {
            get { return (bool)GetValue(NavigatingProperty); }
            set {
                SetValue(NavigatingProperty, value);
                // this.IsVisible = !value;
            }
        }

        public static BindableProperty EvaluateJavascriptProperty =
        BindableProperty.Create(nameof(EvaluateJavascript), typeof(Func<string, Task<string>>), typeof(HybridWebView), null, BindingMode.OneWayToSource);

        public Func<string, Task<string>> EvaluateJavascript
        {
            get { return (Func<string, Task<string>>)GetValue(EvaluateJavascriptProperty); }
            set { SetValue(EvaluateJavascriptProperty, value); }
        }

        public static BindableProperty NavigateProperty =
            BindableProperty.Create(nameof(Navigate), typeof(Func<string, string>), typeof(HybridWebView), null, BindingMode.OneWayToSource);

        public Func<string, string> Navigate
        {
            get { return (Func<string, string>)GetValue(NavigateProperty); }
            set { SetValue(NavigateProperty, value); }
        }


        public void OnWebViewNavigationCompleted()
        {
            Navigating = false;
        }
        public void RegisterAction (Action<string> callback)
		{
			action = callback;
		}

		public void Cleanup ()
		{
			action = null;
		}

		public void InvokeAction (string data)
		{
			if (action == null || data == null) {
				return;
			}
           
            action.Invoke(data);

		}
        private HttpCachedServer.iHttpServer ws;
        private ISql SQL;
        private SqLiteCache sqLiteCache = null;
        public HttpCachedClient hc;
        private string baseUrl;
        public string startServer(string baseUrl, ISql SQL)
        {
            this.baseUrl = baseUrl;
            if (this.baseUrl.EndsWith("/",StringComparison.CurrentCultureIgnoreCase))
                this.baseUrl = this.baseUrl.Substring(0, this.baseUrl.Length - 1);

            this.SQL = SQL; //  Xamarin.Forms.DependencyService.Get<iSQL>();
            this.sqLiteCache = new SqLiteCache(SQL, "");
            this.hc = new HttpCachedClient(this.sqLiteCache);
            

            this.ws = Xamarin.Forms.DependencyService.Get<HttpCachedServer.iHttpServer>();
            this.ws.baseUrl = baseUrl;

            this.ws.getContentDelegate = (url) => {
                Byte[] content = null;
                System.Diagnostics.Debug.WriteLine("getContentDelegate " + url + " " + ws.url_port_external + " " + this.baseUrl);
                if (url.StartsWith(ws.url_port_local,StringComparison.CurrentCultureIgnoreCase))
                {
                    content = this.ws.GetLocalContent(url);
                }
                else if (url.StartsWith(ws.url_port_external, StringComparison.CurrentCultureIgnoreCase) || url.StartsWith(ws.url_port, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (url.StartsWith(ws.url_port_external, StringComparison.CurrentCultureIgnoreCase))
                    {
                        url = url.Substring(ws.url_port_external.Length);
                    }
                    else 
                    {
                        // url=""http://localhost:8080/external/a.tile.openstreetmap.org/11/1514/862.png"
                        // ws.url_port_external="http://www.leaflon.test/external/"
                        // this.baseUrl="http://www.leaflon.test"
                        url = url.Substring(ws.url_port.Length);
                        if (!url.StartsWith(this.baseUrl, StringComparison.CurrentCultureIgnoreCase))
                        {
                            url = hcc.HccUtil.url_join(this.baseUrl, url);
                        }
                    }
                    if( !url.StartsWith("http://",StringComparison.CurrentCultureIgnoreCase) && !url.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
                        url = hcc.HccUtil.url_join("http://" , url);

                    System.Diagnostics.Debug.WriteLine("GetCachedStream " + url);
                    // get from the HttpCachedClient
                    hc.isOffline = true;
                    Task.Run(async () =>
                    {
                        try
                        {
                            HccResponse hccResponse = await hc.GetCachedStreamAsync(url);
                            {
                                if (hccResponse.hccInfo.fromDb == false)
                                    System.Diagnostics.Debug.WriteLine("GetCachedStream " + url + " NOT from DB");
                                if (hccResponse.stream != null)
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        hccResponse.stream.CopyTo(ms);
                                        content = pathRelUrls(ms.ToArray());
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            content = Encoding.UTF8.GetBytes("Error: " + ex.Message);
                        }
                    }).Wait();
                }
                else if (url.StartsWith(ws.url_port_site,StringComparison.CurrentCultureIgnoreCase))
                {
                    // http://localhost:8080/site/index.html
                    url = url.Substring(ws.url_port_site.Length - 1);

                    // get from the HttpCachedClient
                    this.hc = new HttpCachedClient(this.sqLiteCache);

                    Task.Run(async () =>
                    {
                        try
                        {
                            HccResponse hccResponse = await hc.GetCachedStreamAsync(url);
                            {
                                if (hccResponse.hccInfo.fromDb == false)
                                    System.Diagnostics.Debug.WriteLine("GetCachedStream " + url + " NOT from DB");
                                if (hccResponse.stream != null)
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        hccResponse.stream.CopyTo(ms);
                                        content = pathRelUrls(ms.ToArray());
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            content = Encoding.UTF8.GetBytes("Error: " + ex.Message);
                        }
                    }).Wait();
                    
                }
                return content;
            };

            this.ws.Run();

            return this.ws.url_port; 
        }
        public string getServerSite()
        {
            return this.ws.url_port_site;
        }
        public string getServer()
        {
            return this.ws.url_port;
        }
        public Boolean UtilHasProtocol(string url)
        {
            return url.StartsWith("http://") || url.StartsWith("https://");

        }
        public byte[] pathRelUrls(byte[] bytes)
        {
            string html = Encoding.UTF8.GetString(bytes,0,bytes.Length);
            Byte[] htmlbytes = null;

            using (MemoryStream readStream = new MemoryStream(bytes))
            {
                using (MemoryStream writeStream = new MemoryStream())
                {
                    try
                    {
                        PostProcess(readStream, writeStream);
                        htmlbytes = writeStream.ToArray();
                    }
                    catch (Exception)
                    {
                        // ToDO log the error
                        htmlbytes = bytes;
                    }
                }
            }
            return htmlbytes; //  Encoding.UTF8.GetBytes(html);
        }
        // https://stackoverflow.com/questions/1502450/xmlreader-i-need-to-edit-an-element-and-produce-a-new-one
        private void PostProcess(Stream inStream, Stream outStream)
        {
            var writeSettings = new XmlWriterSettings() { Indent = true, IndentChars = " " };
            XmlReaderSettings readSettings = new XmlReaderSettings();
            readSettings.DtdProcessing = DtdProcessing.Ignore;

            using (var reader = XmlReader.Create(inStream, readSettings))
            using (var writer = XmlWriter.Create(outStream, writeSettings))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            writer.WriteStartElement(reader.Prefix, reader.Name, reader.NamespaceURI);
                            writer.WriteAttributes(reader, true);

                            //
                            // check if this is the node you want, inject attributes here.
                            //
                            if(reader.Name == "link" )
                            {
                                string src = reader.GetAttribute("src");
                                if (!src.StartsWith("http://") && !src.StartsWith("https://"))
                                {
                                    writer.WriteAttributeString("src",this.ws.url_port + src);
                                }
                            }
                            if (reader.IsEmptyElement)
                            {
                                writer.WriteEndElement();
                            }
                            break;

                        case XmlNodeType.Text:
                            writer.WriteString(reader.Value);
                            break;

                        case XmlNodeType.EndElement:
                            writer.WriteFullEndElement();
                            break;

                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.ProcessingInstruction:
                            writer.WriteProcessingInstruction(reader.Name, reader.Value);
                            break;

                        case XmlNodeType.SignificantWhitespace:
                            writer.WriteWhitespace(reader.Value);
                            break;
                    }
                }
            }
        }
        public void stopServer()
        {
            this.ws.Stop();
        }
    }
}
