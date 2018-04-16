using ImageSelector.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ImageSelector
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window, INotifyPropertyChanged
    {
        double zoomSpeed;
        public double ZoomSpeed
        {
            get { return zoomSpeed; }
            set { zoomSpeed = value; NotifyPropertyChanged(); }
        }

        private string defaultFolder;
        public string DefaultFolder
        {
            get { return defaultFolder; }
            set { defaultFolder = value; NotifyPropertyChanged(); }
        }

        private string defaultSaveFolder;
        public string DefaultSaveFolder
        {
            get { return defaultSaveFolder; }
            set { defaultSaveFolder = value; NotifyPropertyChanged(); }
        }

        public OptionsWindow()
        {
            InitializeComponent();

            DefaultFolder = Config.DefaultFolder;
            DefaultSaveFolder = Config.DefaultSaveFolder;
            ZoomSpeed = Config.ZoomSpeed;

            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void deafultFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                Config.DefaultFolder = dialog.SelectedPath;
                DefaultFolder = dialog.SelectedPath;
            }
        }

        private void defaultSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                Config.DefaultSaveFolder = dialog.SelectedPath;
                DefaultFolder = dialog.SelectedPath;
            }
        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
