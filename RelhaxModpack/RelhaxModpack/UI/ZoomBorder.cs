using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RelhaxModpack.UI
{
    //modified from https://stackoverflow.com/a/6782715/3128017

    /// <summary>
    /// Represents a border that allows for panning and zooming of the UIElement object inside
    /// </summary>
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        /// <summary>
        /// Cancel the mouse down event for panning
        /// </summary>
        public bool CancelMouseDown = false;

        /// <summary>
        /// Returns the TranslateTransform part of the UIElement's RenderTransform
        /// </summary>
        /// <param name="element">The element to get the transformation parameter from</param>
        /// <returns>The TranslateTransformpart of the UIElement's RenderTransform</returns>
        public static TranslateTransform GetTranslateTransform(UIElement element)
        {
            TransformGroup transformGroup = (TransformGroup)element.RenderTransform;
            return (TranslateTransform)transformGroup.Children.First(tr => tr is TranslateTransform);
        }

        /// <summary>
        /// Returns the ScaleTransform part of the UIElement's RenderTransform
        /// </summary>
        /// <param name="element">The element to get the transformation parameter from</param>
        /// <returns>The ScaleTransform of the UIElement's RenderTransform</returns>
        public static ScaleTransform GetScaleTransform(UIElement element)
        {
            TransformGroup transformGroup = (TransformGroup)element.RenderTransform;
            return (ScaleTransform)transformGroup.Children.First(tr => tr is ScaleTransform);
        }

        /// <summary>
        /// Gets or Sets the child UIElement inside this border
        /// </summary>
        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                //don't initialize the value if it's null or is currently the child
                if (value != null && value != this.Child)
                {
                    //set it first
                    this.child = value;

                    //create scale and translate transforms for pan and zoom
                    TransformGroup group = new TransformGroup();
                    ScaleTransform st = new ScaleTransform();
                    group.Children.Add(st);
                    TranslateTransform tt = new TranslateTransform();
                    group.Children.Add(tt);

                    //set them to the child
                    child.RenderTransform = group;
                    child.RenderTransformOrigin = new Point(0.0, 0.0);

                    //init the events
                    MouseWheel += OnMouseWheelChange;
                    MouseLeftButtonDown += OnMouseLeftButtonDown;
                    MouseLeftButtonUp += OnMouseLeftButtonUp;
                    MouseMove += OnMouseMove;
                    PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
                }

                //always set value for the base if it's not already set
                base.Child = value;
            }
        }

        /// <summary>
        /// Reset the scale and translation (zoom and pan) values
        /// </summary>
        public void Reset()
        {
            if (child != null)
            {
                // reset zoom
                ScaleTransform scaleTransform = GetScaleTransform(child);
                scaleTransform.ScaleX = 1.0;
                scaleTransform.ScaleY = 1.0;

                // reset pan
                TranslateTransform translateTransform = GetTranslateTransform(child);
                translateTransform.X = 0.0;
                translateTransform.Y = 0.0;
            }
        }

        #region Child Events

        private void OnMouseWheelChange(object sender, MouseWheelEventArgs e)
        {
            if (child == null)
                return;

            //get the Scale and Translate transform objects
            ScaleTransform scaleTransform = GetScaleTransform(child);
            TranslateTransform translateTransform = GetTranslateTransform(child);

            //create a zoom factor
            //a negative delta represents a zoom out
            double zoomFactor = e.Delta > 0 ? .2 : -.2;

            //if we're requesting to zoom out, but the value is already small, stop
            if ((e.Delta < 0) && (scaleTransform.ScaleX < .4 || scaleTransform.ScaleY < .4))
                return;

            //same for zooming in
            if ((e.Delta > 0) && (scaleTransform.ScaleX > 4 || scaleTransform.ScaleY > 4))
                return;

            Point relative = e.GetPosition(child);

            //zoom on the position of the mouse
            double abosuluteX = relative.X * scaleTransform.ScaleX + translateTransform.X;
            double abosuluteY = relative.Y * scaleTransform.ScaleY + translateTransform.Y;

            //apply the zoom scaling
            scaleTransform.ScaleX += zoomFactor;
            scaleTransform.ScaleY += zoomFactor;

            //zoom on the position of the mouse
            translateTransform.X = abosuluteX - relative.X * scaleTransform.ScaleX;
            translateTransform.Y = abosuluteY - relative.Y * scaleTransform.ScaleY;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child == null)
                return;

            //check for a cancel from preview double click
            if(CancelMouseDown)
            {
                CancelMouseDown = false;
                Reset();
                return;
            }

            TranslateTransform translateTransform = GetTranslateTransform(child);
            //set where the mouse is starting the operation from, treat it as "0" (new starting point)
            start = e.GetPosition(this);
            //set the current translated value as starting point for option (for UI updating and final value)
            origin = new Point(translateTransform.X, translateTransform.Y);

            //capture the mouse and change to hand (common UI practices)
            Cursor = Cursors.Hand;
            child.CaptureMouse();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child == null)
                return;

            //change the courser back and release the mouse
            child.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //reset the transform and scale factors
            Reset();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (child == null)
                return;

            //move the object based on mouse location
            if (child.IsMouseCaptured)
            {
                TranslateTransform translateTransform = GetTranslateTransform(child);

                //get the difference of change from when the mouse was clicked down (at that first point)
                Vector vector = start - e.GetPosition(this);

                //transform the object by that difference offset
                translateTransform.X = origin.X - vector.X;
                translateTransform.Y = origin.Y - vector.Y;
            }
        }

        #endregion
    }
}
