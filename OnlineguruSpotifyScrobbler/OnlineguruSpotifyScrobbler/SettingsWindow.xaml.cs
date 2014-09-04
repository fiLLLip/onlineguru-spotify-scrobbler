using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using JariZ;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OnlineguruSpotifyScrobbler.Properties;
using OnlineguruSpotifyScrobbler.Resources;
using Hardcodet.Wpf.TaskbarNotification;

namespace OnlineguruSpotifyScrobbler
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Resources.CustomWindow
    {
        private StatusModel _statusModel;
        private SpotifyAPI _spotifyApi;
        private DispatcherTimer _timer;
        private Resources.SpotifyStatus _spotifyStatus;

        public SettingsWindow()
        {
            InitializeComponent();
            
            try
            {
                _spotifyApi = new SpotifyAPI(SpotifyAPI.GetOAuth(), "127.0.0.1");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Feil oppstod ved initialisering av SpotifyAPI:" + Environment.NewLine + ex.Message, "En feil oppstod", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            _timer = new DispatcherTimer(TimeSpan.FromSeconds(5), DispatcherPriority.Background, OnTimerTick, Application.Current.Dispatcher);
            _timer.Stop();
            
            _statusModel = new StatusModel();
            _statusModel.TaskbarIcon = TaskbarIcon;

            DataContext = _statusModel;

            TxtAuth.Text = Settings.Default.Auth;
            TxtUrl.Text = Settings.Default.ScrobbleUrl;
            ChkDebug.IsChecked = Settings.Default.Debug;
            ChkUrlEncoded.IsChecked = Settings.Default.UrlEncoded;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            Responses.Status status = null;
            RunAsynchronously(()=> { status = _spotifyApi.Status; }, ()=> Update(status));
        }
        private void Update(Responses.Status status)
        {
            if (status == null || status.track == null || status.track.track_resource == null)
            {
                return;
            }
            if (_spotifyStatus == null)
            {
                _spotifyStatus = new SpotifyStatus();
            }
            if ((_spotifyStatus.Uri ?? "") == status.track.track_resource.uri)
            {
                return;
            }
            _spotifyStatus.Uri = status.track.track_resource.uri;
            _spotifyStatus.Artist = status.track.artist_resource.name;
            _spotifyStatus.Album = status.track.album_resource.name;
            _spotifyStatus.Track = status.track.track_resource.name;
            _statusModel.UpdateSongInfo(_spotifyStatus);

            //TODO: Send _spotifyStatus as json to webservice
            var data = JsonConvert.SerializeObject(_spotifyStatus);
            if (!Settings.Default.UrlEncoded)
            {
                WebClientHelper.DoAsyncJsonRequest(Settings.Default.ScrobbleUrl, data);
            }
            else
            {
                WebClientHelper.DoAsyncUrlEncodedRequest(Settings.Default.ScrobbleUrl, data);
            }
        }

        public static void RunAsynchronously(Action method, Action callback)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    method();
                }
                catch (ThreadAbortException) { /* dont report on this */ }
                catch (Exception ex)
                {
                }
                // note: this will not be called if the thread is aborted
                if (callback != null) callback();
            });
        }

        private void SettingsWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!UserClose)
            {
                Hide();
                e.Cancel = true;
                return;
            }
            TaskbarIcon.Dispose();
        }

        private void SettingsWindow_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                return;
            }
            var title = "Kjører fortsatt!";
            var message = "Onlineguru Spotify Scrobbler kjører fortsatt her nede. Høyreklikk og velg avslutt for å avslutte eller dobbelklikk for å åpne igjen.";
            TaskbarIcon.ShowBalloonTip(title, message, BalloonIcon.Info);
        }

        private void StartScrobbleClick(object sender, RoutedEventArgs e)
        {
            StartScrobble();
        }

        private void StopScrobbleClick(object sender, RoutedEventArgs e)
        {
            StopScrobble();
        }

        private void StartScrobble()
        {
            _spotifyStatus = null;
            _timer.Start();
            BtnStartScrobble.IsEnabled = false;
            BtnStopScrobble.IsEnabled = true;
            _statusModel.SongInfo = "Scrobbling: waiting for songinfo";
        }

        private void StopScrobble()
        {
            _timer.Stop();
            BtnStartScrobble.IsEnabled = true;
            BtnStopScrobble.IsEnabled = false;
            _statusModel.SongInfo = "Not scrobbling";
        }

        private void SaveSettingsClick(object sender, RoutedEventArgs e)
        {
            bool wasRunning = false;
            if (_timer != null && _timer.IsEnabled)
            {
                wasRunning = true;
                StopScrobble();
            }
            Settings.Default.Auth = TxtAuth.Text;
            Settings.Default.ScrobbleUrl = TxtUrl.Text;
            Settings.Default.Debug = ChkDebug.IsChecked ?? false;
            Settings.Default.UrlEncoded = ChkUrlEncoded.IsChecked ?? false;
            Settings.Default.Save();
            if (wasRunning)
            {
                StartScrobble();
            }
        }
    }
}
