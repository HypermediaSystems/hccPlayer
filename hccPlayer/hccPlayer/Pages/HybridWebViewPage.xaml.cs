using Newtonsoft.Json.Linq;
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
using PCLStorage;

namespace hccPlayer
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HybridWebViewPage : ContentPage
    {
        private ISql SQL;
        public HybridWebViewPage(ISql SQL)
        {
            InitializeComponent();

            ModalDialog.grid = gridLayout;

            this.SQL = SQL;
            startServer(SQL);

            loadFiles();

            closeMenu();
        }
        public void stopServer()
        {
            hybridWebView?.stopServer();
        }
        
        private async Task<int> loadFiles()
        {
            // list aall SqLite files from localStorage
            IFolder rootFolder = FileSystem.Current.LocalStorage;

            var files = await rootFolder.GetFilesAsync();

            cmbSQLFiles.Items.Clear();
            int nr = 0;
            foreach(var f in files)
            {
                if (f.Name.EndsWith(".sqlite", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (f.Name == HttpCachedClient._dbName)
                        nr = cmbSQLFiles.Items.Count;
                    cmbSQLFiles.Items.Add(f.Name);
                }
            }

            cmbSQLFiles.SelectedIndex = nr;

            return files.Count;
        }
        private void startServer(ISql SQL)
        {
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
                ModalDialog.showMessage( "HccPlayer", msg, ModalDialog.Buttons.OK, () => { });

                // ToDo log the error
                
            }

        }
        private void reStartServer(string newName)
        {
            try
            {
                Task.Run(async () =>
                   await hybridWebView.reStartServer(this.SQL, newName)
                ).Wait();

                hybridWebView.Uri = hcc.HccUtil.url_join(hybridWebView.getServer(), hybridWebView.getDefaultHTML());
            }
            catch (Exception ex)
            {
                string msg = "ERROR: " + ex.ToString();
                ModalDialog.showMessage( "HccPlayer", msg, ModalDialog.Buttons.OK, () => { });

                // ToDo log the error
            }
            hybridWebView.Navigate(hybridWebView.Uri);

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

        private async void explore_Clicked(object sender, EventArgs e)
        {
            JsUtil ju = new JsUtil();
            ju.setSelector("[id],[name]");
            ju.setFunction("node.title = node.name + '#' + node.id;");

            string jsCommand = ju.ToString();

            await hybridWebView.EvaluateJavascript(jsCommand);
        }
        private async void btnFillDownload_Clicked(object sender, EventArgs e)
        {
            string serverUrl = tbDownloadUrl.Text.Trim();
            string user = tbDownloadUser.Text.Trim();
            string pwd = tbDownloadPWD.Text.Trim();

            using (HttpClient httpCient = new HttpClient())
            {
                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
                {
                    var byteArray = new UTF8Encoding().GetBytes(user + ":" + pwd);
                    httpCient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }
                try
                {
                    using (HttpResponseMessage response = await httpCient.GetAsync(serverUrl, HttpCompletionOption.ResponseContentRead).ConfigureAwait(true))
                    {
                        cmbDownloadSQLFiles.Items.Clear();

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            using (HttpContent content = response.Content)
                            {

                                var lst = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonSqLite[]>(await content.ReadAsStringAsync());
                                foreach(var s in lst)
                                {
                                    cmbDownloadSQLFiles.Items.Add(s.file);
                                }
                            }
                        }
                        else
                        {
                            cmbDownloadSQLFiles.Items.Add("Error " + response.StatusCode);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModalDialog.showError("Error: " + ex.ToString());
                    // ToDo log this error
                }

            }
        }
        private void btnDownload_Clicked(object sender, EventArgs e)
        {

            if (cmbDownloadSQLFiles.SelectedItem == null)
            {
                return;
            }
            string DBName = cmbDownloadSQLFiles.SelectedItem.ToString();
            string msg = "Do you want to download the database " + DBName  + "? " + Environment.NewLine;
            ModalDialog.showQuestion( "HccPlayer", msg, ModalDialog.Buttons.YESNO,
                async () =>
                {
                    string serverUrl = tbDownloadUrl.Text.Trim();
                    string user = tbDownloadUser.Text.Trim();
                    string pwd = tbDownloadPWD.Text.Trim();

                    if( !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
                    {
                        var byteArray = new UTF8Encoding().GetBytes(user + ":" + pwd );
                        hybridWebView.hc.authenticationHeaderValue = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    }
                    string dbUrl = serverUrl;
                    int pos = dbUrl.LastIndexOf("/",StringComparison.CurrentCultureIgnoreCase);
                    if (pos > 0)
                        dbUrl = dbUrl.Substring(0, pos+1);
                    dbUrl += DBName;
                    try
                    {
                        Boolean ret = await hybridWebView.hc.RestoreAsync(dbUrl, DBName);

                        // fill the list
                        await loadFiles();
                    }
                    catch (Exception ex)
                    {
                        // ToDo log this error
                        ModalDialog.showError("Error: " + ex.ToString());
                    }

                }, () => { });
        }
        private void btnUpload_Clicked(object sender, EventArgs e)
        {
                ModalDialog.showQuestion( "HccPlayer", "Do you want to backup the local database?", ModalDialog.Buttons.YESNO,
                    async () =>
                {
                    string serverUrl = tbUploadUrl.Text.Trim();

                    string user = tbUploadUser.Text.Trim();
                    string pwd = tbUploadPWD.Text.Trim();

                    if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
                    {
                        var byteArray = new UTF8Encoding().GetBytes(user + ":" + pwd);
                        hybridWebView.hc.authenticationHeaderValue = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    }

                    try
                    {
                        Boolean ret = await hybridWebView.hc.BackupAsync(serverUrl);
                    }
                    catch (Exception ex)
                    {
                        // ToDo log this error
                        ModalDialog.showError("Error: " + ex.ToString());
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

        private void btnSelect_Clicked(object sender, EventArgs e)
        {
            if (cmbSQLFiles.SelectedItem != null)
            {
                string fName = cmbSQLFiles.SelectedItem.ToString();

                ModalDialog.showQuestion( "", "Do you want to set " + fName + " as startup DB?", ModalDialog.Buttons.YESNO,
                    () =>
                    {
                        // slHybridWebView.Children.Clear();
                        // hybridWebView = new HybridWebView {
                        //     HorizontalOptions = LayoutOptions.FillAndExpand,
                        //     VerticalOptions = LayoutOptions.FillAndExpand
                        // };
                        // slHybridWebView.Children.Add(this.hybridWebView);
                        // 
                        // reStartServer(fName);
                        hccPlayer.Helpers.Settings.LastSqLite = fName;
                        ModalDialog.showMessage( "", "You have to restart the app in order to apply the changed settings.", ModalDialog.Buttons.OK, () => { });
                    },
                    () =>
                    {
                    });
            }
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
    class JsonSqLite
    {
        public string name {  get;set;}
        public string file { get; set; }


    }
}