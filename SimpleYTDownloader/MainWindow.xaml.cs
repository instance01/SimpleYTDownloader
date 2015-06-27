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
using System.Timers;

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
                Video video = listbox1.Items.GetItemAt(listbox1.SelectedIndex) as Video;
                Videos.Remove(video);
                listbox1.Items.RemoveAt(listbox1.SelectedIndex);
            }
        }

        private void listbox1_dragdrop(object sender, DragEventArgs e)
        {
            string[] formats = new string[5] { DataFormats.Text, DataFormats.UnicodeText, DataFormats.SymbolicLink, DataFormats.Html, DataFormats.OemText};
            foreach (string format in formats)
            {
                if (e.Data.GetDataPresent(format))
                {
                    string s = (string) e.Data.GetData(format, false);
                    if (!listbox1.Items.Contains(s))
                    {
                        addItem(s);
                    }
                }
            }
        }

        public void addItem(String s)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(s, "^(?:https?://)?(?:www\\.)?(?:youtube\\.com|youtu\\.be)/watch\\?v=([^&]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                Video v = new Video() { Name = s, Progress = 0 };
                foreach (Object obj in listbox1.Items)
                {
                    string name = (obj as Video).Name;
                    if (v.Name == name)
                    {
                        return;
                    }
                }
                Videos.Add(v);
                listbox1.Items.Add(v);
            }
            else
            {
                MessageBox.Show(s + " doesn't seem to be a valid youtube link.");
            }
        }

        System.Timers.Timer timer;

        private void Button_Click(object sender_, RoutedEventArgs e)
        {
            if (Videos.Count > 0)
            {
                progressring1.IsActive = true;
                label1.Visibility = System.Windows.Visibility.Visible;
                if(timer == null){
                    timer = new System.Timers.Timer(1000);
                    timer.Elapsed += OnTick;
                }
                timer.Start();
            }

            for (int i = 0; i < Videos.Count; i++)
            {
                Video v = Videos[i];
                int c = i;
                Thread t = new Thread(() => downloadVideo(v, Videos.Count, c));
                t.Start();
            }
        }

        private void OnTick(object source, ElapsedEventArgs e)
        {
            int g = 0;
            foreach (Video v in Videos)
            {
                g += v.Progress;
            }
            if (Videos.Count < 1)
            {
                timer.Stop();
                return;
            }
            double percentage = g / Videos.Count;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                label1.Content = Math.Round(percentage) + "%";
            }));
        }

        public void downloadVideo(Video link, int count, int c)
        {
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link.Name);
            
            VideoInfo video = videoInfos.Where(info => info.CanExtractAudio).OrderByDescending(info => info.AudioBitrate).First();

            var audioDownloader = new AudioDownloader(video, Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "/" + video.Title + video.AudioExtension);

            audioDownloader.DownloadProgressChanged += (sender, args) => this.Dispatcher.BeginInvoke(new Action(() => {
                if (c < Videos.Count)
                {
                    Videos[c].Progress = (int)Math.Round(args.ProgressPercentage);
                }
            }));
            audioDownloader.AudioExtractionProgressChanged += (sender, args) => this.Dispatcher.BeginInvoke(new Action(() => {
                if (c < Videos.Count)
                {
                    Videos[c].Progress = (int)Math.Round(args.ProgressPercentage);
                }
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
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (Clipboard.ContainsData(DataFormats.Text) || Clipboard.ContainsData(DataFormats.UnicodeText) || Clipboard.ContainsData(DataFormats.OemText))
                {
                    addItem(Clipboard.GetText());
                }
                e.Handled = true;
            }
        }
    }
}
