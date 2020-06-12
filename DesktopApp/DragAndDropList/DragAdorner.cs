using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DesktopApp.DragAndDropList
{
    public class DragAdorner : Adorner
    {

        private Rectangle child = null;
        private double offsetLeft = 0;
        private double offsetTop = 0;

        public DragAdorner(UIElement adornedElement, Size size, Brush brush)
            : base(adornedElement)
        {
            Rectangle rect = new Rectangle();
            rect.Fill = brush;
            rect.Width = size.Width;
            rect.Height = size.Height;
            rect.IsHitTestVisible = false;
            child = rect;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(this.offsetLeft, this.offsetTop));
            return result;
        }

        public double OffsetLeft
        {
            get { return offsetLeft; }
            set
            {
                offsetLeft = value;
                UpdateLocation();
            }
        }

        public void SetOffsets(double left, double top)
        {
            offsetLeft = left;
            offsetTop = top;
            UpdateLocation();
        }

        public double OffsetTop
        {
            get { return offsetTop; }
            set
            {
                offsetTop = value;
                UpdateLocation();
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            child.Measure(constraint);
            return child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return child;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        private void UpdateLocation()
        {
            AdornerLayer adornerLayer = Parent as AdornerLayer;
            if (adornerLayer != null)
                adornerLayer.Update(AdornedElement);
        }
    }
}