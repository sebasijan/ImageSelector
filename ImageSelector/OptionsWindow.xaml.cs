using ImageSelector.Helpers;
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
        public event PropertyChangedEventHandler PropertyChanged;

        public double ZoomSpeed
        {
            get { return Config.ZoomSpeed; }
            set { Config.ZoomSpeed = value; NotifyPropertyChanged(); }
        }
        public string DefaultFolder
        {
            get { return Config.DefaultFolder; }
            set { Config.DefaultFolder = value; NotifyPropertyChanged(); }
        }
        public string DefaultSaveFolder
        {
            get { return Config.DefaultSaveFolder; }
            set { Config.DefaultSaveFolder = value; NotifyPropertyChanged(); }
        }

        public OptionsWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void DefaultFolder_Click(object sender, RoutedEventArgs e)
        {
            DefaultFolder = Folder.Select(this);
        }

        private void DefaultSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            DefaultSaveFolder = Folder.Select(this);
        }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
