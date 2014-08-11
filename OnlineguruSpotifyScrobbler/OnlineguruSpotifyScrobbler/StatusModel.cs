using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;

namespace OnlineguruSpotifyScrobbler
{
    public class StatusModel : INotifyPropertyChanged
    {
        private string _songInfo;

        public string SongInfo
        {
            get { return _songInfo; }
            set
            {
                _songInfo = value;
                Application.Current.Dispatcher.BeginInvoke(new Action(() => OnPropertyChanged("SongInfo")));
            }
        }

        public TaskbarIcon TaskbarIcon { get; set; }

        public StatusModel()
        {
            SongInfo = "Not scrobbling";
        }

        public void UpdateSongInfo(Resources.SpotifyStatus spotifyStatus)
        {
            if (spotifyStatus == null)
            {
                return;
            }
            SongInfo = string.Format("Scrobbling: {0} - {1}", spotifyStatus.Artist, spotifyStatus.Track);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
