using System.Linq;
using System.Windows.Media;

namespace ImageSelector.Helpers
{
    class Picture
    {
        internal static void ResetView(System.Windows.Controls.Image image)
        {
            // reset zoom
            var transformGroup = (TransformGroup)image.RenderTransform;
            var transform = (ScaleTransform)transformGroup.Children[0];
            transform.ScaleX = 1.0;
            transform.ScaleY = 1.0;

            // reset pan
            var translateTransform = 
                (TranslateTransform)((TransformGroup)image.RenderTransform)
                    .Children.First(tr => tr is TranslateTransform);
            translateTransform.X = 0;
            translateTransform.Y = 0;
        }
    }
}
