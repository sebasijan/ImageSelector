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

namespace ImageSelector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static RoutedCommand MyCommand = new RoutedCommand();

        // panning
        private Point origin;
        private Point start;

        // min max zoom
        double minScale = 0.4;
        double zoomSpeed = 0.1;
        public double ZoomSpeed
        {
            get { return zoomSpeed; }
            set { zoomSpeed = value; NotifyPropertyChanged(); }
        }

        private string imageFolderPath;
        public string ImageFolderPath
        {
            get { return imageFolderPath; }
            private set { imageFolderPath = value; NotifyPropertyChanged(); }
        }

        private string saveFolderPath;
        public string SaveFolderPath
        {
            get { return saveFolderPath; }
            private set { saveFolderPath = value; NotifyPropertyChanged(); }
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
            InitialiseFolder(Config.DefaultFolder);
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
            image.MouseWheel += image_MouseWheel;
            image.MouseLeftButtonDown += image_MouseLeftButtonDown;
            image.MouseLeftButtonUp += image_MouseLeftButtonUp;
            image.MouseMove += image_MouseMove;
        }

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            MessageBox.Show(args.ExceptionObject.ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void InitialiseFolder(string folderPath)
        {
            imageFolderPath = folderPath;
            imageSources = new List<ImageSource>();

            if (Directory.Exists(ImageFolderPath))
            {
                files = Directory.EnumerateFiles(ImageFolderPath)
                    .Where(f => imageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                    .ToList();

                SetCurrentImage(files.First());
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
            ResetImageView();
        }

        private void SetCurrentImage(int fileIndex)
        {
            image.Source = imageSources[fileIndex];

            ResetImageView();
        }

        private void ResetImageView()
        {
            // reset zoom
            var transformGroup = (TransformGroup)image.RenderTransform;
            var transform = (ScaleTransform)transformGroup.Children[0];
            transform.ScaleX = 1.0;
            transform.ScaleY = 1.0;
            // reset pan
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            tt.X = 0;
            tt.Y = 0;
        }

        // Buttons
        private void buttonNextImage_Click(object sender, RoutedEventArgs e)
        {
            ChangeImage(Direction.Next);
        }
        private void buttonPreviousImage_Click(object sender, RoutedEventArgs e)
        {
            ChangeImage(Direction.Previous);
        }
        private void buttonSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var currentFolder = buttonSelectFolder.Content;

            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                buttonSelectFolder.Content = dialog.SelectedPath;
                InitialiseFolder(dialog.SelectedPath);
            }
        }
        private void buttonSelectSaveFolder_Click(object sender, RoutedEventArgs e)
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
        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            image.ReleaseMouseCapture();
        }
        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!image.IsMouseCaptured) return;

            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            Vector v = start - e.GetPosition(border);
            tt.X = origin.X - v.X;
            tt.Y = origin.Y - v.Y;
        }
        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            image.CaptureMouse();
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            start = e.GetPosition(border);
            origin = new Point(tt.X, tt.Y);
        }
        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
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

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}