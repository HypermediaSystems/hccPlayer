using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace hccPlayer
{
    class ModalDialog
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

        public static void showMessage(Grid grid, string title, string msg, Buttons buttons, Action onOk)
        {
            showQuestion(grid, title, msg, buttons, onOk, () => { });
        }
        public static void showQuestion(Grid grid, string title, string msg, Buttons buttons, Action onOk, Action onCancel)
        {
            StackLayout mdFrame = new StackLayout
            {
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
                // Padding = pad,
                // Margin=pad,
                Padding = 5,
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 0,
                BackgroundColor = Color.Transparent
            };

            {
                slCenter.Children.Add(new Frame {
                    Padding=5,
                    Content = new Label {
                        Text = title,
                        FontAttributes=FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = 5
                    }
                });
                slCenter.Children.Add(new Label { Text = msg, HorizontalOptions = LayoutOptions.Center, Margin = 10 });

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
