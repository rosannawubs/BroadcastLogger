using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Controllers
{
    /// <summary>
    /// This class is used to show the volume of a sound device.
    /// </summary>
    public partial class Meter : UserControl
    {
        #region privates
        /// <summary>
        /// Height of an in indvidual box
        /// </summary>
        private double      boxHeight;
        /// <summary>
        /// Height of the padding between the boxes
        /// </summary>
        private double      paddingHeight;
        /// <summary>
        /// When to change the color to yellow curently at 70 percent of the way
        /// </summary>
        private const int   YELLOW_ZONE = (int) (BAR_COUNT * .7);
        /// <summary>
        /// When to change the color to red currently at 90 percent of the way
        /// </summary>
        private const int   RED_ZONE    = (int) (BAR_COUNT * .9);
        /// <summary>
        /// The number of bars we want to show.
        /// </summary>
        private const int   BAR_COUNT   = 10;
        private Color GREEN       = (Color)ColorConverter.ConvertFromString("#00ee00");
        private Color YELLOW      = (Color)ColorConverter.ConvertFromString("#f5f600");
        private Color RED         = (Color)ColorConverter.ConvertFromString("#ff1800");
        #endregion
        public Meter()
        {
            InitializeComponent();
            paddingHeight = (Panel.Height * 0.1) / (BAR_COUNT);//Use 10 percent for padding
            boxHeight = (Panel.Height * 0.9) / BAR_COUNT;    //Use 90 perecent for boxes
        }
        /// <summary>
        /// Update the bar to the specified volume
        /// </summary>
        /// <param name="volume">Volume must be between 0 and 1</param>
        public void updateBar(float volume)
        {

            Panel.Children.Clear();
            double height = (Panel.Height * volume) * .95;//Height that the bar should be
            int boxCount = (int) (height / boxHeight);
            int i;
            for (i = 0; i < boxCount && i < YELLOW_ZONE; i++)
            {
                Panel.Children.Add(rectangleFactory(GREEN));
            }
            for (; i < boxCount && i < RED_ZONE; i++)
            {
                Panel.Children.Add(rectangleFactory(YELLOW));
            }
            for (; i < boxCount; i++)
            {
                Panel.Children.Add(rectangleFactory(RED));
            }

        }
        /// <summary>
        /// Used to generate rectangles for the boxes
        /// </summary>
        /// <param name="color">Color of the box</param>
        /// <returns></returns>
        private Rectangle rectangleFactory(Color color)
        {
            Rectangle rect      = new Rectangle();
            rect.Height         = boxHeight;
            rect.Width          = Panel.Width;
            rect.Margin         = new Thickness(0, 0, 0, paddingHeight);
            rect.Fill           = new SolidColorBrush(color);
            return rect;
        }
    }
}
