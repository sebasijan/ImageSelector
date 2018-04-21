using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageSelector
{
    /// <summary>
    /// Contains event handlers for MainWindow when the user either scrolls
    /// the mouse wheel to zoom in/out of the image, or uses the mouse to 
    /// pan around the image
    /// </summary>
    public partial class MainWindow
    {
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
    }
}
