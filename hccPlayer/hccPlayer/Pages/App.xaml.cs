using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HMS.Net.Http;
using Xamarin.Forms;

namespace hccPlayer
{
    public partial class App : Application
    {
        public App(ISql SQL)
        {
            InitializeComponent();

            MainPage = new hccPlayer.HybridWebViewPage(SQL);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
