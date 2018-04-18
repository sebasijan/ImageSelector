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
            InitializeImage();
            InitializeConfigurationOptions();

            this.DataContext = this;

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);
        }

        private void InitializeConfigurationOptions()
        {
            InitialiseFolder();
            ZoomSpeed = Config.ZoomSpeed;
            SaveFolderPath = Config.DefaultSaveFolder;
        }

        private void InitializeImage()
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

        private void InitialiseFolder()
        {
            imageSources = new List<ImageSource>();

            if (Directory.Exists(ImageFolderPath))
            {
                files = Directory.EnumerateFiles(ImageFolderPath)
                    .Where(f => imageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (files.Count > 0)
                {
                    SetCurrentImage(files.First());
                }
                else
                {
                    SetCurrentImage(string.Empty);
                }
                currentIndex = 0;
                lastIndex = files.Count - 1;
            }
            else
            {
                ImageFolderPath = "PATH NOT FOUND";
            }
        }

        private void SetCurrentImage(string filePath)
        {
            DisplayedImage = filePath;
            Picture.ResetView(image);
        }

        private void SetCurrentImage(int fileIndex)
        {
            image.Source = imageSources[fileIndex];
            Picture.ResetView(image);
        }

        // Buttons
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

            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                buttonSelectFolder.Content = dialog.SelectedPath;
                InitialiseFolder();
            }
        }
        private void ButtonSelectSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            var currentFolder = buttonSelectSaveFolder.Content;

            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                buttonSelectSaveFolder.Content = dialog.SelectedPath;
                SaveFolderPath = dialog.SelectedPath;
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var optionsWindow = new OptionsWindow();
            optionsWindow.ShowDialog();
        }

        // Image Pan and zoom events
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            image.ReleaseMouseCapture();
        }
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!image.IsMouseCaptured) return;

            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            Vector v = _start - e.GetPosition(border);
            tt.X = _origin.X - v.X;
            tt.Y = _origin.Y - v.Y;
        }
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            image.CaptureMouse();
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            _start = e.GetPosition(border);
            _origin = new Point(tt.X, tt.Y);
        }
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var transformGroup = (TransformGroup)image.RenderTransform;
            var transform = (ScaleTransform)transformGroup.Children[0];
            var pos1 = e.GetPosition(image);
            double zoom = e.Delta > 0 ? ZoomSpeed : -ZoomSpeed;

            if (!(transform.ScaleX < minScale) || zoom > 0) 
            {
                transform.ScaleX += zoom;
                transform.ScaleY += zoom;

                transform.CenterX = pos1.X;
                transform.CenterY = pos1.Y;
            }
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

            SetCurrentImage(files[currentIndex]);
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