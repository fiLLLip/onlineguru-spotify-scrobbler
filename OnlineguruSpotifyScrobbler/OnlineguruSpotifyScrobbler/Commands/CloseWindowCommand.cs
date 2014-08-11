using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace OnlineguruSpotifyScrobbler.Commands
{
    public class CloseWindowCommand : CommandBase<CloseWindowCommand>
    {
        public override void Execute(object parameter)
        {
            var window = GetTaskbarWindow(parameter);
            window.UserClose = true;
            window.Close();
            CommandManager.InvalidateRequerySuggested();
        }


        public override bool CanExecute(object parameter)
        {
            Window win = GetTaskbarWindow(parameter);
            return win != null;
        }
    }
}