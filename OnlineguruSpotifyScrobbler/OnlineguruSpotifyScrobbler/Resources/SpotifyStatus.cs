using System.Windows;
using OnlineguruSpotifyScrobbler.Properties;

namespace OnlineguruSpotifyScrobbler.Resources
{
    public class SpotifyStatus
    {
        public string Uri { get; set; }
        public string Track { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }

        public string Auth
        {
            get { return Settings.Default.Auth; }
        }
    }
}
