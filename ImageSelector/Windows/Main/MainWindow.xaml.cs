using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImageSelector.Models;
using ImageSelector.Helpers;

namespace ImageSelector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static RoutedCommand MyCommand = new RoutedCommand();

        // panning
        private Point _origin;
        private Point _start;

        // min max zoom
        private const double minScale = 0.4;
        private const double zoomSpeed = 0.1;
        public double ZoomSpeed
        {
            get { return Config.ZoomSpeed; }
            set { Config.ZoomSpeed = value; NotifyPropertyChanged(); }
        }

        public string ImageFolderPath
        {
            get { return Config.DefaultFolder; }
            private set { Config.DefaultFolder = value; NotifyPropertyChanged(); }
        }

        public string SaveFolderPath
        {
            get { return Config.DefaultSaveFolder; }
            private set { Config.DefaultSaveFolder = value; NotifyPropertyChanged(); }
        }

        private string currentImagePath;
        public string DisplayedImage
        {
            get { return currentImagePath; }
            private set { currentImagePath = value; NotifyPropertyChanged(); }
        }

        private List<string> files;
        private static readonly List<string> imageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".TIFF", ".PNG" };
        private int currentIndex;
        private int lastIndex;

        private List<ImageSource> imageSources = new List<ImageSource>();
        private BitmapImage bitmapImage = new BitmapImage();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            InitialiseImage();
            InitialiseConfigurationOptions();

            this.DataContext = this;

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
        }
        
        private void InitialiseConfigurationOptions()
        {
            InitialiseFolder();
            ZoomSpeed = Config.ZoomSpeed;
            SaveFolderPath = Config.DefaultSaveFolder;
        }
        private void InitialiseFolder(string selectedFile ="")
        {
            if (!string.IsNullOrEmpty(selectedFile))
            {
                ImageFolderPath = new FileInfo(selectedFile).DirectoryName;
            }

            if (Directory.Exists(ImageFolderPath))
            {
                files = Directory.EnumerateFiles(ImageFolderPath)
                    .Where(f => imageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (string.IsNullOrEmpty(selectedFile))
                {
                    if (files.Count > 0)
                        SetCurrentImage(files.First());

                    currentIndex = 0;
                    lastIndex = files.Count - 1;
                }
                else
                {
                    SetCurrentImage(files.FirstOrDefault(f => f == selectedFile));
                    currentIndex = files.IndexOf(selectedFile);
                    lastIndex = files.Count - 1;
                }
            }
            else
            {
                ImageFolderPath = string.Empty;
            }
        }
        private void InitialiseImage()
        {
            // Apply transforms
            var groupTransform = new TransformGroup()
            {
                Children = new TransformCollection(new Transform[] {
                    new ScaleTransform(),
                    new TranslateTransform() })
            };
            // Register events
            image.RenderTransform = groupTransform;
            image.MouseWheel += Image_MouseWheel;
            image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
            image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
            image.MouseMove += Image_MouseMove;
        }

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            MessageBox.Show(args.ExceptionObject.ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void SetCurrentImage(string filePath)
        {
            DisplayedImage = filePath;
            Picture.ResetView(image);
        }

        // keyboad shortcuts
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                    ChangeImage(Direction.Next);
                    break;
                case Key.Left:
                    ChangeImage(Direction.Previous);
                    break;
                case Key.S:
                    SaveImage();
                    break;
                default:
                    break;
            }
        }
        private void SaveImage()
        {
            var fileName = Path.GetFileName(currentImagePath);
            var source = currentImagePath;
            var destination = Path.Combine(SaveFolderPath, fileName);

            try
            {
                File.Copy(source, destination);
                labelSaved.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ERROR\n\n{ex.Message}");
            }
        }

        private void ChangeImage(Direction direction)
        {
            labelSaved.Visibility = Visibility.Hidden;

            if (files != null)
            {
                switch (direction)
                {
                    case Direction.Previous:
                        PreviousImage();
                        break;
                    case Direction.Next:
                        NextImage();
                        break;
                }
            }

            if (files.Count > 0)
            {
                SetCurrentImage(files[currentIndex]);
            }
        }

        private void NextImage()
        {
            currentIndex = (currentIndex == lastIndex) ? 0 : ++currentIndex;
        }
        private void PreviousImage()
        {
            currentIndex = (currentIndex == 0) ? lastIndex : --currentIndex;
        }

        enum Direction { Previous, Next }

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}