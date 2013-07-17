using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;
using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.Phone.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace TelerikHelper
{
    public partial class MainPage : PhoneApplicationPage
    {
        string[] post;
        string[] link;
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            WebClient webClient = new WebClient();

            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);

            webClient.DownloadStringAsync(new System.Uri("http://www.telerikhelper.net/feed"));
            progressbar.Visibility = Visibility.Visible;
        }

        private void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            progressbar.Visibility = Visibility.Collapsed;
            if (e.Error != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                   MessageBox.Show(e.Error.Message);
                });
            }
            else
            {
                this.State["feed"] = e.Result;

                UpdateFeedList(e.Result);
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.State.ContainsKey("feed"))
            {
                if (feedListBox.Items.Count == 0)
                {
                    UpdateFeedList(State["feed"] as string);
                }
            }
        }

        private void UpdateFeedList(string feedXML)
        {
            StringReader stringReader = new StringReader(feedXML);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            SyndicationFeed feed = SyndicationFeed.Load(xmlReader);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                feedListBox.ItemsSource = feed.Items;

            });
        }

        private void feedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;

            if (listBox != null && listBox.SelectedItem != null)
            {
                SyndicationItem sItem = (SyndicationItem)listBox.SelectedItem;

                if (sItem.Links.Count > 0)
                {
                    Uri uri = sItem.Links.FirstOrDefault().Uri;

                    WebBrowserTask webBrowserTask = new WebBrowserTask();
                    webBrowserTask.Uri = uri;
                    webBrowserTask.Show();
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WebClient webClient = new WebClient();
            webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_LoadStoriesCompleted);
            webClient.OpenReadAsync(new Uri("http://telerikhelper.net/?s=" + searchtext.Text));
            progressbar.Visibility = Visibility.Visible;
            SecondListBox.Items.Clear();
        }
        private void webClient_LoadStoriesCompleted(object sender, OpenReadCompletedEventArgs args)
        {
            progressbar.Visibility = Visibility.Collapsed;
            if (args.Error != null) return; // an error occurred - you can handle to show an alert about this

            HtmlDocument doc = new HtmlDocument();
            doc.Load(args.Result); // load Html source from IO Stream
            post = new string[20];
            link = new string[20];
            int i = 0;
            foreach (HtmlNode postNode in doc.DocumentNode.SelectNodes("//div[contains(@id,'post-')]"))
            {
                HtmlNodeCollection nc = postNode.ChildNodes;
                post[i] = nc[1].InnerText;
                post[i] = HTMLtoPlain(post[i]);
                link[i] = nc[1].Element("a").GetAttributeValue("href", "unknown").ToString();
                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.Wrap;
                tb.Text = post[i];
                tb.FontSize = 24;
                tb.TextDecorations = TextDecorations.Underline;
                Color currentAccentColorHex = (Color)Application.Current.Resources["PhoneAccentColor"];
                SolidColorBrush backColor = new SolidColorBrush(currentAccentColorHex);
                tb.Foreground = backColor;
                TextBlock tbdesc = new TextBlock();
                tbdesc.TextWrapping = TextWrapping.Wrap;
                tbdesc.Text = HTMLtoPlain(nc[6].InnerText);
                //tbdesc.TextTrimming = TextTrimming.WordEllipsis;
                StackPanel sp = new StackPanel();
                sp.Children.Add(tb);
                sp.Children.Add(tbdesc);
                SecondListBox.Items.Add(sp);
                i++;
            }
        }

        void sf_Tap(object sender, GestureEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.URL = link[SecondListBox.SelectedIndex];
            task.Show();
        }
        public string HTMLtoPlain(string html)
        {
            string text = Regex.Replace(html.ToString(), "<[^>]+>", string.Empty);

            // Remove newline characters.
            text = text.Replace("\r", "").Replace("\n", "");

            // Remove encoded HTML characters.
            text = HttpUtility.HtmlDecode(text);
            return text;
        }
        
        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            WebClient webClient = new WebClient();

            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);

            webClient.DownloadStringAsync(new System.Uri("http://www.telerikhelper.net/feed"));
            mainPivot.SelectedIndex = 0;
            progressbar.Visibility = Visibility.Visible;
        }

        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            mainPivot.SelectedIndex = 1;
        }

        private void ApplicationBarMenuItem_Click_1(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/about.xaml", UriKind.RelativeOrAbsolute));
        }

    }
}