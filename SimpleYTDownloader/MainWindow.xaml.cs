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
using System.ComponentModel;

namespace SimpleYTDownloader
{
    public partial class MainWindow : MetroWindow
    {

        public IList<Video> Videos { get; set; }

        public class Video : INotifyPropertyChanged
        {
            public int percent;

            public string Name { get; set; }
            public int Progress
            {
                get { return this.percent; }
                set
                {
                    percent = value;
                    NotifyPropertyChanged("Progress");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public void NotifyPropertyChanged(String info)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
            }
        }

        public MainWindow()
        {
            Videos = new List<Video>();
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
                    addItem(s);
                }
            }
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                string s = (string)e.Data.GetData(DataFormats.UnicodeText, false);
                if (!listbox1.Items.Contains(s))
                {
                    addItem(s);
                }
            }
            if (e.Data.GetDataPresent(DataFormats.SymbolicLink))
            {
                string s = (string)e.Data.GetData(DataFormats.SymbolicLink, false);
                if (!listbox1.Items.Contains(s))
                {
                    addItem(s);
                }
            }
            if (e.Data.GetDataPresent(DataFormats.Html))
            {
                string s = (string)e.Data.GetData(DataFormats.Html, false);
                if (!listbox1.Items.Contains(s))
                {
                    addItem(s);
                }
            }
            if (e.Data.GetDataPresent(DataFormats.OemText))
            {
                string s = (string)e.Data.GetData(DataFormats.OemText, false);
                if (!listbox1.Items.Contains(s))
                {
                    addItem(s);
                }
            }
        }

        //Dictionary<int, ProgressBar> links_ = new Dictionary<int, ProgressBar>();

        public void addItem(String s)
        {
            //listbox1.Items.Add(new { Name=s, Progress=b });
            Video v = new Video() {Name=s, Progress=0};
            Videos.Add(v);
            listbox1.Items.Add(v);
            //links_.Add(listbox1.Items.Count - 1, b);
        }

        private void Button_Click(object sender_, RoutedEventArgs e)
        {
            //progressbar1.Maximum = 100;
            //progressbar1.Minimum = 0;
            int count = listbox1.Items.Count;

            for (int i = 0; i < Videos.Count; i++)
            {
                progressring1.IsActive = true;
                label1.Visibility = System.Windows.Visibility.Visible;
                MessageBox.Show(i.ToString());
                Video v = null;
                if (i != -1 && i < Videos.Count)
                {
                    v = Videos[i];
                }
                Thread t = new Thread(() => downloadVideo(v, count, i));
                t.Start();
            }
               /* foreach (Video i in listbox1.Items)
                {
                    //string link = i;

                    progressring1.IsActive = true;
                    label1.Visibility = System.Windows.Visibility.Visible;

                    Thread t = new Thread(() => downloadVideo(i, count, c));
                    t.Start();

                    c++; // C++ ô.ô
                }*/
        }


        public void downloadVideo(Video link, int count, int c)
        {
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link.Name);

            VideoInfo video = videoInfos
            .Where(info => info.CanExtractAudio)
            .OrderByDescending(info => info.AudioBitrate)
            .First();

            var audioDownloader = new AudioDownloader(video, Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/" + video.Title + video.AudioExtension);

            //audioDownloader.DownloadProgressChanged += (sender, args) => this.Dispatcher.BeginInvoke(new Action(() => progressbar1.Value = args.ProgressPercentage / count));
            //audioDownloader.AudioExtractionProgressChanged += (sender, args) => this.Dispatcher.BeginInvoke(new Action(() => progressbar1.Value = args.ProgressPercentage * 0.15));
            audioDownloader.DownloadProgressChanged += (sender, args) => this.Dispatcher.BeginInvoke(new Action(() => {
                Videos[c - 1].Progress = (int)args.ProgressPercentage;
                label1.Content = Convert.ToString(args.ProgressPercentage) + "%";
            }));
            audioDownloader.AudioExtractionProgressChanged += (sender, args) => this.Dispatcher.BeginInvoke(new Action(() => {
                Videos[c - 1].Progress = (int)args.ProgressPercentage;
                label1.Content = Convert.ToString(args.ProgressPercentage) + "%";
            }));
            audioDownloader.DownloadFinished += (sender, args) => this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (listbox1.Items.Count < 2)
                {
                    label1.Visibility = System.Windows.Visibility.Hidden;
                    label1.Content = "0%";
                    progressring1.IsActive = false;
                    listbox1.Items.Remove(link);
                    Videos.Remove(link);
                    Videos.Clear();
                }
                else
                {
                    listbox1.Items.Remove(link);
                    Videos.Remove(link);
                }
            }));


            audioDownloader.Execute();
        }

        private void base_keydown(object sender, KeyEventArgs e)
        {
            // ctrl + v
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (Clipboard.ContainsData(DataFormats.Text) || Clipboard.ContainsData(DataFormats.UnicodeText) || Clipboard.ContainsData(DataFormats.OemText))
                {
                    addItem(Clipboard.GetText());
                    //listbox1.Items.Add(Clipboard.GetText());
                }

                e.Handled = true;
            }
        }
    }
}
