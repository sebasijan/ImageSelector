using System.Windows;

namespace ImageSelector.Helpers
{
    class BrowserDialog
    {
        internal static string SelectFolder(Window window)
        {
             var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            return (dialog.ShowDialog(window).GetValueOrDefault())
                ? dialog.SelectedPath
                : string.Empty;
        }

        internal static string SelectFile(Window window)
        {
            // var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var dialog = new Microsoft.Win32.OpenFileDialog();

            return (dialog.ShowDialog(window).GetValueOrDefault())
                // ? dialog.FileName.SelectedPath
                ? dialog.FileName
                : string.Empty;
        }
    }
}
