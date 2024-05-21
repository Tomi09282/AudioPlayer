using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioPlayer
{
    using System;
    using System.Windows.Media;

    namespace SpotifyLikeApp
    {
        public class Song
        {
            public string Artist { get; set; }
            public string Title { get; set; }
            public TimeSpan Length { get; set; }
            public string FilePath { get; set; }

            public Song(string artist, string title, TimeSpan timeSpan, string filePath)
            {
                Artist = artist;
                Title = title;
                FilePath = filePath;
                Length = GetMediaDuration(filePath);
            }

            private TimeSpan GetMediaDuration(string filePath)
            {
                MediaPlayer mediaPlayer = new MediaPlayer();
                mediaPlayer.Open(new Uri(filePath));
                mediaPlayer.MediaOpened += (sender, e) =>
                {
                    Length = mediaPlayer.NaturalDuration.TimeSpan;
                    mediaPlayer.Close();
                };
                mediaPlayer.Play();
                mediaPlayer.Stop();
                return Length;
            }

            public override string ToString()
            {
                return $"{Artist} - {Title} ({Length:mm\\:ss})";
            }


        }
    }

}
