using AudioPlayer.SpotifyLikeApp;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Song> uploadedSongs = new ObservableCollection<Song>();
        private ObservableCollection<Song> personalPlaylist = new ObservableCollection<Song>();
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private int currentSongIndex = -1;
        private bool isPlaying = false;

        public MainWindow()
        {
            InitializeComponent();
            UploadedSongsBox.ItemsSource = uploadedSongs;
            PersonalPlaylistBox.ItemsSource = personalPlaylist;
            VolumeSlider.Value = 50;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                var song = new Song("Unknown Artist", System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName), new TimeSpan(0, 3, 30), openFileDialog.FileName);
                uploadedSongs.Add(song);
            }
        }

        private void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (UploadedSongsBox.SelectedItem is Song selectedSong)
            {
                personalPlaylist.Add(selectedSong);
            }
        }

        private void DeleteFromPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (PersonalPlaylistBox.SelectedItem is Song selectedSong)
            {
                personalPlaylist.Remove(selectedSong);
            }
        }

        private void PlayFromPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (PersonalPlaylistBox.SelectedItem is Song selectedSong)
            {
                currentSongIndex = personalPlaylist.IndexOf(selectedSong);
                PlaySong(selectedSong);
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                mediaPlayer.Pause();
                isPlaying = false;
            }
            else
            {
                if (currentSongIndex >= 0 && currentSongIndex < personalPlaylist.Count)
                {
                    PlaySong(personalPlaylist[currentSongIndex]);
                }
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            isPlaying = false;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            isPlaying = false;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSongIndex > 0)
            {
                currentSongIndex--;
                PlaySong(personalPlaylist[currentSongIndex]);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentSongIndex < personalPlaylist.Count - 1)
            {
                currentSongIndex++;
                PlaySong(personalPlaylist[currentSongIndex]);
            }
        }

        private void SavePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Playlist files (*.playlist)|*.playlist|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    foreach (var song in personalPlaylist)
                    {
                        writer.WriteLine($"{song.Artist}|{song.Title}|{song.Length}|{song.FilePath}");
                    }
                }
            }
        }

        private void LoadPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Playlist files (*.playlist)|*.playlist|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                personalPlaylist.Clear();
                using (StreamReader reader = new StreamReader(openFileDialog.FileName))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split('|');
                        if (parts.Length == 4)
                        {
                            var song = new Song(parts[0], parts[1], TimeSpan.Parse(parts[2]), parts[3]);
                            personalPlaylist.Add(song);
                        }
                    }
                }
            }
        }

        private void PlaySong(Song song)
        {
            if (File.Exists(song.FilePath))
            {
                mediaPlayer.Open(new Uri(song.FilePath));
                mediaPlayer.Play();
                isPlaying = true;
                SongInfo.Text = song.ToString();
            }
            else
            {
                MessageBox.Show("File not found: " + song.FilePath);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = VolumeSlider.Value / 100;
        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                TotalTime.Text = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                ProgressSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                ProgressSlider.Value = 0;
                mediaPlayer.Position = TimeSpan.Zero;
                CurrentTime.Text = "0:00";

                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                timer.Tick += (s, ev) =>
                {
                    if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan)
                    {
                        ProgressSlider.Value = mediaPlayer.Position.TotalSeconds;
                        CurrentTime.Text = mediaPlayer.Position.ToString(@"mm\:ss");
                    }
                };
                timer.Start();
            }
        }



        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            NextButton_Click(sender, null);
        }

    }
}
