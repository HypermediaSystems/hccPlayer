using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace hccPlayer
{
    static class ModalDialog
    {
        public enum Buttons
        {
            YES,
            NO,
            YESNO,
            OK,
            CANCEL,
            OKCANCEL,
        }

        public static string strYes = "Yes";
        public static string strNo = "No";
        public static string strOK = "OK";
        public static string strCancel = "Cancel";

        public static Grid grid;

        public static void showMessage( string msg)
        {
            showQuestion( "", msg, ModalDialog.Buttons.OK, () => { }, () => { });
        }
        public static void showError(string msg)
        {
            showQuestion("", msg, ModalDialog.Buttons.OK, () => { }, () => { });
        }
        public static void showMessage(string title, string msg, Buttons buttons, Action onOk)
        {
            showQuestion(title, msg, buttons, onOk, () => { });
        }
        public static void showQuestion(string title, string msg, Buttons buttons, Action onOk, Action onCancel)
        {
            StackLayout mdFrame = new StackLayout
            {
                StyleId = "mdFrame",
                WidthRequest = grid.Width,
                HeightRequest = grid.Height,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent
            };

            StackLayout slTop = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent
            };
            StackLayout slBottom = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent
            };
            StackLayout slMiddle = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                // HorizontalOptions = LayoutOptions.FillAndExpand,
                // VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent
            };
            StackLayout slLeft = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                // VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent
            };
            StackLayout slRight = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                ///VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent
            };

            StackLayout slCenter = new StackLayout
            {
                StyleId = "slCenter",
                // Padding = pad,
                // Margin=pad,
                Padding = 20,
                Margin = 10,
                Orientation = StackOrientation.Vertical,
                Spacing = 0,
                BackgroundColor = Color.LightGray
            };
            StackLayout slButton = new StackLayout
            {
                StyleId = "slButton",   
                // Padding = pad,
                // Margin=pad,
                Padding = 5,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 0,
                BackgroundColor = Color.Transparent
            };
            
            {

                StackLayout slX = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.EndAndExpand,
                    ///VerticalOptions = LayoutOptions.FillAndExpand,
                    BackgroundColor = Color.Transparent
                };
                Button btnX = new Button {
                    Text = "X",
                    BackgroundColor = Color.Transparent
                };
                btnX.Clicked += (object sender, EventArgs e) => { grid.Children.Remove(mdFrame); onCancel(); };
                slX.Children.Add(btnX);
                slCenter.Children.Add(slX);

                Frame frame = new Frame
                {
                    Padding = 5
                };
                if( !string.IsNullOrEmpty(title))
                {
                    frame.Content = new Label
                    {
                        Text = title,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = 5
                    };
                }
                slCenter.Children.Add(frame);


                ScrollView sv = new ScrollView();
                sv.Content = new Label {
                    Text = msg,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = 10 };
                slCenter.Children.Add(sv);

                slCenter.Children.Add(slButton);
                {
                    if (buttons == Buttons.YES || buttons == Buttons.YESNO || buttons == Buttons.OK || buttons == Buttons.OKCANCEL)
                    {
                        string btnFirst = strYes;
                        if (buttons == Buttons.OK || buttons == Buttons.OKCANCEL)
                            btnFirst = strOK;
                        Button btnOk = new Button { Text = btnFirst };
                        btnOk.Clicked += (object sender, EventArgs e) => { grid.Children.Remove(mdFrame); onOk(); };
                        slButton.Children.Add(btnOk);
                    }
                    if (buttons == Buttons.NO || buttons == Buttons.YESNO || buttons == Buttons.CANCEL || buttons == Buttons.OKCANCEL)
                    {
                        string btnSecond = strNo;
                        if (buttons == Buttons.CANCEL || buttons == Buttons.OKCANCEL)
                            btnSecond = strCancel;
                        Button btnCancel = new Button { Text = btnSecond };
                        btnCancel.Clicked += (object sender, EventArgs e) => { grid.Children.Remove(mdFrame); onCancel(); };
                        slButton.Children.Add(btnCancel);
                    }
                }
            }
            mdFrame.Children.Add(slTop);
            mdFrame.Children.Add(slMiddle);
            {
                slMiddle.Children.Add(slLeft);
                Frame frame = new Frame { OutlineColor = Color.Black, Padding = 0 };
                slMiddle.Children.Add(frame);
                frame.Content = slCenter;
                slMiddle.Children.Add(slRight);
            }
            mdFrame.Children.Add(slBottom);

            grid.Children.Add(mdFrame, 0, grid.ColumnDefinitions.Count, 0, grid.RowDefinitions.Count);

        }
    }
}
