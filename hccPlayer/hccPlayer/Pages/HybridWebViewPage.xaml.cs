using Newtonsoft.Json.Linq;
// using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using System.Net.Http;
using HMS.Net.Http;

namespace hccPlayer
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HybridWebViewPage : ContentPage
    {
        
        public HybridWebViewPage(ISql SQL)
        {
            InitializeComponent();

            try
            {
                Task.Run(async () =>
                   await hybridWebView.startServer(SQL)
                ).Wait();

                hybridWebView.Uri = hcc.HccUtil.url_join(hybridWebView.getServer(), hybridWebView.getDefaultHTML());
            }
            catch (Exception ex)
            {
                string msg = "ERROR: " + ex.ToString();
                ModalDialog.showMessage(gridLayout, "HccPlayer", msg, ModalDialog.Buttons.OK,() => { });
                
                // ToDo log the error
                // throw;
            }

            // this is defined in the XAML file
            hybridWebView.RegisterAction(data => {
                try
                {
                    var jObject = JObject.Parse(data);
                    switch (jObject["cmd"].ToString().ToUpper())
                    {
                        case "OPENMENU":
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                openMenu();
                            });
                            break;
                    }
                }
                catch (Exception ex)
                {
                    DisplayAlert("Alert", "Hello " + data, "OK");
                }
            });
            List<Sample> samples = new List<Sample>();
            samples.Add(new Sample("Framed Divs", "HWM.JQuery('div').css('border-width','2px').css('border-style','solid').css('border-color','green')"));
            samples.Add(new Sample("set text and search",
                "HWM.JQuery('#lst-ib')" + Environment.NewLine +
                "    .css('background-color','yellow')" + Environment.NewLine +
                "    .text('abc');" + Environment.NewLine +
                "HWM.wait(100,function(jsonStr){ " + Environment.NewLine +
                "	HWM.JQuery('[name=\"btnK\"]').submit();" + Environment.NewLine +
                "});" + Environment.NewLine
             ));

            samples.Add(new Sample("goto hmsconv.com",
                "HWM.navigate('http://www.hmsconv.com');" + Environment.NewLine +
                "HWM.wait(2000,function(jsonStr){ " + Environment.NewLine +
                "	HWM.JQuery('[href=\"#portfolioModal1\"]').click();" + Environment.NewLine +
                "	HWM.wait(2000,function(jsonStr){ " + Environment.NewLine +
                "		HWM.JQuery('[class*=\"close-modal\"]').click();" + Environment.NewLine +
                "		});" + Environment.NewLine +
                "});" + Environment.NewLine
            ));


            cmbSamples.SelectedIndexChanged += (sender, args) =>
            {
                if (cmbSamples.SelectedIndex == -1)
                {

                }
                else
                {
                    code.Text = samples[cmbSamples.SelectedIndex].getCode();
                }
            };

            samples.Clear();
            var assembly = typeof(HybridWebViewPage).GetTypeInfo().Assembly;
            var res = assembly.GetManifestResourceNames();
            foreach (var r in res)
            {
                System.Diagnostics.Debug.WriteLine(r);
            }
            Stream stream = assembly.GetManifestResourceStream("hccPlayer.data.samples.xml");
            stream2samples(stream, samples);
#if false
            string curDesc = "";
            using (XmlReader reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    //return only when you have START tag
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            curDesc = reader.GetAttribute("desc");
                            break;
                        case XmlNodeType.Text:
                            if (!string.IsNullOrEmpty(curDesc))
                            {
                                samples.Add(new sample(curDesc, reader.Value));
                            }

                            break;

                    }
                }
            }
#endif
#if true

            Task.Run(async () =>
            {
                string targetUrl = "http://www.hypermediasystems.de/HWM/samples.xml";
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        using (HttpResponseMessage response = await client.GetAsync(targetUrl, HttpCompletionOption.ResponseHeadersRead))
                        using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                        {
                            stream2samples(streamToReadFrom, samples);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }



            });
#endif

            closeMenu();
        }
        public void stopServer()
        {
            hybridWebView?.stopServer();
        }
        private void stream2samples(Stream stream, List<Sample> samples)
        {
            samples.Clear();

            string curDesc = "";
            using (XmlReader reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    //return only when you have START tag
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            curDesc = reader.GetAttribute("desc");
                            break;
                        case XmlNodeType.Text:
                            if (!string.IsNullOrEmpty(curDesc))
                            {
                                samples.Add(new Sample(curDesc, reader.Value));
                            }

                            break;

                    }
                }
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                cmbSamples.Items.Clear();
                foreach (var s in samples)
                    cmbSamples.Items.Add(s.ToString());

            });
        }
#if false
        private async void DoPopup()
        {     
            try
            {
                await DoPopupShow();
            }
            catch (Exception e) // handle whatever exceptions you expect
            {
                //Handle exceptions
            }

        }
        private InputPage popupPage;
        private async Task DoPopupShow()
        {
            popupPage = new InputPage();
            await Navigation.PushPopupAsync(popupPage);
            popupPage.onClose = () => {
                loopSteps();
            };      
        }
        JQueryParse jqp = null;
        private Task<JQueryParseStepReturn> doNextStep()
        {
            return jqp.step(JQueryParseExecuteFlag.TRACE, (flags, s) =>
            {
                if (flags == JQueryParseExecuteFlag.WAIT)
                {
                    Device.BeginInvokeOnMainThread(DoPopup);
                }
                lblIDs.Text += flags.ToString() + " " + s + Environment.NewLine;
            });
        }
        private Boolean loopStepRunning = false;
        private async void loopSteps()
        {
            if (loopStepRunning == true)
                return;

            loopStepRunning = true;
            JQueryParseStepReturn stepReturn = JQueryParseStepReturn.NONE;
            while (!jqp.endOfList())
            {
                stepReturn = await doNextStep();
                if (stepReturn == JQueryParseStepReturn.CONTINUE)
                {
                    break;
                }
                if (stepReturn == JQueryParseStepReturn.ERROR)
                    break;
            }
            loopStepRunning = false;
        }
#endif
        private void btnGoto_Clicked(object sender, System.EventArgs e)
        {
            hybridWebView.Navigate(tbUrl.Text);
        }

        private Boolean menueExpanded = true;
        private void btnMenu_Clicked(object sender, EventArgs e)
        {
            toggleMenu();
        }

        private void toggleMenu()
        {
            if (menueExpanded == true)
            {
                closeMenu();
            }
            else
            {
                openMenu();
            }
        }

        public void openMenu()
        {
            if (menueExpanded == false)
            {
                gridLayout.ColumnDefinitions[0].Width = new GridLength(20, GridUnitType.Star);
                menueExpanded = true;
                extendedMenu.IsVisible = true;
                btnMenu.Text = "<<<";
            }
        }

        public void closeMenu()
        {
            if (menueExpanded == true)
            {
                gridLayout.ColumnDefinitions[0].Width = new GridLength(3, GridUnitType.Star);
                menueExpanded = false;
                extendedMenu.IsVisible = false;
                btnMenu.Text = ">";
            }
        }

        private void btnExecute_Clicked(object sender, EventArgs e)
        {
            HWM hwm = new HWM();
            hwm.trace = (s) => {
                lblIDs.Text += s + Environment.NewLine;
            };
            JSParse jsp = new JSParse(hybridWebView, hwm, code.Text);
            jsp.execute();

        }

        private async void explore_Clicked(object sender, EventArgs e)
        {
            JsUtil ju = new JsUtil();
            ju.setSelector("[id],[name]");
            ju.setFunction("node.title = node.name + '#' + node.id;");

            string jsCommand = ju.ToString();

            await hybridWebView.EvaluateJavascript(jsCommand);
        }

        private void btnRestore_Clicked(object sender, EventArgs e)
        {
            string msg = "Do you want to restore the local database?" + Environment.NewLine + "This will overwrite all local data.";
            ModalDialog.showQuestion(gridLayout, "HccPlayer", msg, ModalDialog.Buttons.YESNO,
                async () =>
                {
                    string serverUrl = tbRestoreUrl.Text.Trim();
                    if (cbNodeJS.IsToggled)
                    {
                        serverUrl = hcc.HccUtil.url_join(serverUrl, "download?url=" + HttpCachedClient._dbName + ".sqlite");
                    }
                    string user = tbRestoreUser.Text.Trim();
                    string pwd = tbRestorePWD.Text.Trim();

                    if( !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
                    {
                        var byteArray = new UTF8Encoding().GetBytes(user + ":" + pwd );
                        hybridWebView.hc.authenticationHeaderValue = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    }

                    try
                    {
                        Boolean ret = await hybridWebView.hc.RestoreAsync(serverUrl);
                    }
                    catch (Exception)
                    {
                        // ToDo log this error
                        throw;
                    }
                }, () => { });
        }
        private void btnBackup_Clicked(object sender, EventArgs e)
        {
                ModalDialog.showQuestion(gridLayout, "HccPlayer", "Do you want to backup the local database?", ModalDialog.Buttons.YESNO,
                    async () =>
                {
                    string serverUrl = tbRestoreUrl.Text.Trim();
                    if ( cbNodeJS.IsToggled )
                    {
                        serverUrl = hcc.HccUtil.url_join(serverUrl, "download?url=" + HttpCachedClient._dbName + ".sqlite");
                    }

                    string user = tbRestoreUser.Text.Trim();
                    string pwd = tbRestorePWD.Text.Trim();

                    if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
                    {
                        var byteArray = new UTF8Encoding().GetBytes(user + ":" + pwd);
                        hybridWebView.hc.authenticationHeaderValue = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    }

                    try
                    {
                        Boolean ret = await hybridWebView.hc.BackupAsync(serverUrl);
                    }
                    catch (Exception)
                    {
                        // ToDo log this error
                        throw;
                    }
                }, () => { });
        }
        private void btnAccordionTitle_Clicked(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string tag = HccXAML.GetTag(btn);

            Frame sl = btn.Parent.FindByName<Frame>(tag);
            if( sl != null )
                sl.IsVisible = !sl.IsVisible;
        }
       
        

    }
    class Sample
    {
        string desc;
        string code;
        public Sample(string desc, string code)
        {
            this.desc = desc;
            this.code = code;
        }
        public override string ToString()
        {
            return this.desc;
        }
        public string getCode()
        {
            return this.code;
        }
    }
}