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
            foreach(var f in files)
            {
                if (f.Name.EndsWith(".sqlite", StringComparison.CurrentCultureIgnoreCase))
                {
                    cmbSQLFiles.Items.Add(f.Name);
                }
            }
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
                ModalDialog.showMessage(gridLayout, "HccPlayer", msg, ModalDialog.Buttons.OK, () => { });

                // ToDo log the error
                // throw;
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
                ModalDialog.showMessage(gridLayout, "HccPlayer", msg, ModalDialog.Buttons.OK, () => { });

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

        private void btnDownload_Clicked(object sender, EventArgs e)
        {
            string msg = "Do you want to Download a database?" + Environment.NewLine;
            ModalDialog.showQuestion(gridLayout, "HccPlayer", msg, ModalDialog.Buttons.YESNO,
                async () =>
                {
                    string serverUrl = tbDownloadUrl.Text.Trim();
                    string DBName = tbDownloadDBName.Text.Trim();
                    string user = tbDownloadUser.Text.Trim();
                    string pwd = tbDownloadPWD.Text.Trim();

                    if( !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
                    {
                        var byteArray = new UTF8Encoding().GetBytes(user + ":" + pwd );
                        hybridWebView.hc.authenticationHeaderValue = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    }

                    try
                    {
                        HttpCachedClient._dbName = DBName;
                        Boolean ret = await hybridWebView.hc.RestoreAsync(serverUrl);

                        // start the server
                        this.reStartServer(DBName);
                    }
                    catch (Exception)
                    {
                        // ToDo log this error
                        throw;
                    }
                }, () => { });
        }
        private void btnUpload_Clicked(object sender, EventArgs e)
        {
                ModalDialog.showQuestion(gridLayout, "HccPlayer", "Do you want to backup the local database?", ModalDialog.Buttons.YESNO,
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

        private void btnSelect_Clicked(object sender, EventArgs e)
        {
            if (cmbSQLFiles.SelectedItem != null)
            {
                string fName = cmbSQLFiles.SelectedItem.ToString();

                ModalDialog.showQuestion(gridLayout, "", "Do you want to start " + fName, ModalDialog.Buttons.YESNO,
                    () =>
                    {
                        slHybridWebView.Children.Clear();
                        hybridWebView = new HybridWebView {
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            VerticalOptions = LayoutOptions.FillAndExpand
                        };
                        slHybridWebView.Children.Add(this.hybridWebView);

                        reStartServer(fName);
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
}