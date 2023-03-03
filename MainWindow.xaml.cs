using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;

namespace Music_player
{
    public partial class MainWindow : Window
    {
        string[] fullPaths = new string[0];
        string[] items = new string[0];
        string[] notShaffledFullPaths = new string[0];
        string[] notShuffledItems = new string[0];
        double? currentVolume = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddMusic_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog() { IsFolderPicker = true };
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                items = Directory.GetFiles(dialog.FileName).Where(x => x.EndsWith(".mp3") || x.EndsWith(".m4a") || x.EndsWith(".wav")).ToArray();
                fullPaths = items.ToArray();
                items = get_names(items).ToArray();
                Music_list.ItemsSource = items;
                mediaPlayer.Volume = 0.5;
                Volume_slider.Value = mediaPlayer.Volume;
                Music_list.SelectedIndex = 0;
                mediaPlayer.Source = new Uri(fullPaths[0]);
                Play_button_icon.Kind = PackIconKind.Pause;
                mediaPlayer.Play();
            }
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        static string[] get_names(string[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                FileInfo file = new FileInfo(array[i]);
                array[i] = file.Name;
            }
            return array;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mediaPlayer.Source != null) && (mediaPlayer.NaturalDuration.HasTimeSpan))
            {
                Time_slider.Minimum = 0;
                Time_slider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                Time_slider.Value = mediaPlayer.Position.TotalSeconds;
                Debug.WriteLine(Time_slider.Value);
            }
        }

        private void Music_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = fullPaths[Music_list.SelectedIndex].ToString();
            mediaPlayer.Source = new Uri(selected);
            mediaPlayer.Play();
        }

        private void Play_button_Click(object sender, RoutedEventArgs e)
        {
            if (Play_button_icon.Kind == PackIconKind.Play)
            {
                Play_button_icon.Kind = PackIconKind.Pause;
                mediaPlayer.Play();
            }
            else
            {
                Play_button_icon.Kind = PackIconKind.Play;
                mediaPlayer.Pause();
            }
        }

        private void Volume_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume_slider.Minimum = 0;
            Volume_slider.Maximum = 1;
            mediaPlayer.Volume = Volume_slider.Value;
            if (Volume_slider.Value > 0)
                Mute_button_icon.Kind = PackIconKind.VolumeHigh;
            if (Volume_slider.Value == 0)
                Mute_button_icon.Kind = PackIconKind.VolumeMute;

        }

        private void Time_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblCur.Content = mediaPlayer.Position.ToString(@"mm\:ss");
            lblLeast.Content = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            mediaPlayer.Position = TimeSpan.FromSeconds(Time_slider.Value);
        }

        private void mediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (Repeat_button_icon.Kind == PackIconKind.RepeatOff)
            {
                if (Music_list.SelectedIndex == (fullPaths.Length - 1))
                {
                    Time_slider.Value = 0;
                    Music_list.SelectedIndex = 0;
                }
                else
                {
                    Time_slider.Value = 0;
                    Music_list.SelectedIndex++;
                }
            }
            else if (Repeat_button_icon.Kind == PackIconKind.RepeatOne)
            {
                Time_slider.Value = 0;
                Music_list.SelectedIndex = Music_list.SelectedIndex;
            }
        }

        private void Prev_button_Click(object sender, RoutedEventArgs e)
        {
            if (Music_list.SelectedIndex == 0)
            {
                Music_list.SelectedIndex = fullPaths.Length - 1;
            }
            else
            {
                Music_list.SelectedIndex--; ;
            }
            mediaPlayer.Play();
            Play_button_icon.Kind = PackIconKind.Pause;
        }

        private void Next_button_Click(object sender, RoutedEventArgs e)
        {
            if (Music_list.SelectedIndex != fullPaths.Length - 1)
            {
                Music_list.SelectedIndex++;
            }
            else
            {
                Music_list.SelectedIndex = 0;
            }
            mediaPlayer.Play();
            Play_button_icon.Kind = PackIconKind.Pause;
        }

        private void mediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            Time_slider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.Ticks;
        }

        private void Repeat_button_Click(object sender, RoutedEventArgs e)
        {
            if (Repeat_button_icon.Kind == PackIconKind.RepeatOff)
            {
                Repeat_button_icon.Kind = PackIconKind.RepeatOne;
            }
            else
            {
                Repeat_button_icon.Kind = PackIconKind.RepeatOff;
            }
        }



        private void Shuffle_button_Click(object sender, RoutedEventArgs e)
        {
            if (Shuffle_button_icon.Kind == PackIconKind.ShuffleVariant)
            {
                Shuffle_button_icon.Kind = PackIconKind.ShuffleDisabled;
                notShaffledFullPaths = fullPaths.ToArray();
                notShuffledItems = items.ToArray();
                var rnd = new Random();
                string[] shuffledToList = fullPaths.OrderBy(x => rnd.Next()).ToArray();
                string[] shuffledFullPath = shuffledToList.ToArray();
                shuffledToList = get_names(shuffledToList).ToArray();
                Music_list.ItemsSource = shuffledToList;
                Music_list.SelectedIndex = 0;
                mediaPlayer.Source = new Uri(shuffledFullPath[0]);
                mediaPlayer.Play();
            }
            else
            {
                Shuffle_button_icon.Kind = PackIconKind.ShuffleVariant;
                fullPaths = notShaffledFullPaths.ToArray();
                items = notShuffledItems.ToArray();
                Music_list.ItemsSource = items;
                Music_list.SelectedIndex = 0;
                mediaPlayer.Source = new Uri(fullPaths[0]);
                mediaPlayer.Play();
            }
        }

        private void Mute_button_Click(object sender, RoutedEventArgs e)
        {
            if (Mute_button_icon.Kind == PackIconKind.VolumeHigh)
            {
                currentVolume = Volume_slider.Value;
                Mute_button_icon.Kind = PackIconKind.VolumeMute;
                Volume_slider.Value = 0;
            }
            else
            {
                Mute_button_icon.Kind = PackIconKind.VolumeHigh;
                Volume_slider.Value = (double)currentVolume;
                currentVolume = null;
            }
        }
    }
}
