using System.Windows;

namespace ImageSelector
{
    /// <summary>
    /// Contains event handlers for MainWindow when the user clicks 
    /// on any of the available buttons
    /// </summary>
    public partial class MainWindow
    {
        private void ButtonNextImage_Click(object sender, RoutedEventArgs e)
        {
            ChangeImage(Direction.Next);
        }
        private void ButtonPreviousImage_Click(object sender, RoutedEventArgs e)
        {
            ChangeImage(Direction.Previous);
        }
        private void ButtonSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var currentFolder = buttonSelectFolder.Content;

            var selectedFile = Helpers.BrowserDialog.SelectFile(this);
            InitialiseFolder(selectedFile);
        }
        private void ButtonSelectSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            var currentFolder = buttonSelectSaveFolder.Content;

            SaveFolderPath = Helpers.BrowserDialog.SelectFolder(this);
            buttonSelectSaveFolder.Content = SaveFolderPath;
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var optionsWindow = new OptionsWindow();
            optionsWindow.ShowDialog();
        }
    }
}
