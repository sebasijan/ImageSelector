using System.Windows;

namespace ImageSelector.Helpers
{
    class Folder
    {
        internal static string Select(Window window)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            return (dialog.ShowDialog(window).GetValueOrDefault())
                ? dialog.SelectedPath
                : string.Empty;
        }
    }
}
