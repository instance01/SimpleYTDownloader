using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro;
using YoutubeExtractor;
using System.Threading;

namespace SimpleYTDownloader
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void menuitem1_click(object sender, RoutedEventArgs e)
        {
            if (listbox1.SelectedIndex != -1)
            {
                listbox1.Items.RemoveAt(listbox1.SelectedIndex);
            }
        }

        private void listbox1_dragdrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string s = (string)e.Data.GetData(DataFormats.Text, false);
                if (!listbox1.Items.Contains(s))
                {
                    listbox1.Items.Add(s);
                }
            }
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                string s = (string)e.Data.GetData(DataFormats.UnicodeText, false);
                if (!listbox1.Items.Contains(s))
                {
                    listbox1.Items.Add(s);
                }
            }
            if (e.Data.GetDataPresent(DataFormats.SymbolicLink))
            {
                string s = (string)e.Data.GetData(DataFormats.SymbolicLink, false);
                if (!listbox1.Items.Contains(s))
                {
                    listbox1.Items.Add(s);
                }
            }
            if (e.Data.GetDataPresent(DataFormats.Html))
            {
                string s = (string)e.Data.GetData(DataFormats.Html, false);
                if (!listbox1.Items.Contains(s))
                {
                    listbox1.Items.Add(s);
                }
            }
            if (e.Data.GetDataPresent(DataFormats.OemText))
            {
                string s = (string)e.Data.GetData(DataFormats.OemText, false);
                if (!listbox1.Items.Contains(s))
                {
                    listbox1.Items.Add(s);
                }
            }
        }

        private void Button_Click(object sender_, RoutedEventArgs e)
        {
            progressbar1.Maximum = 100;
            progressbar1.Minimum = 0;
            int count = listbox1.Items.Count;
            foreach (string i in listbox1.Items)
            {
                string link = i;

                Thread t = new Thread(() => downloadVideo(link, count));
                t.Start();
            }
        }


        public void downloadVideo(string link, int count)
        {
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            VideoInfo video = videoInfos
            .Where(info => info.CanExtractAudio)
            .OrderByDescending(info => info.AudioBitrate)
            .First();

            var audioDownloader = new AudioDownloader(video, Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/" + video.Title + video.AudioExtension);

            audioDownloader.DownloadProgressChanged += (sender, args) => this.Dispatcher.BeginInvoke(new Action(() => progressbar1.Value = args.ProgressPercentage / count)); // * 0.85
            audioDownloader.AudioExtractionProgressChanged += (sender, args) => this.Dispatcher.BeginInvoke(new Action(() => progressbar1.Value = args.ProgressPercentage * 0.15));
            audioDownloader.DownloadFinished += (sender, args) => this.Dispatcher.BeginInvoke(new Action(() =>
            {

                progressbar1.Value = 0;
                listbox1.Items.Remove(link);

            }));


            audioDownloader.Execute();
        }
    }
}
