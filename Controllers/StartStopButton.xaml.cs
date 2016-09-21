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

public enum status { Start, Stop}; 
/// <summary>
/// The class is used with the event ButtonClicked to 
/// notify the user what the status of the button is. 
/// </summary>
public class StartStopArgs : EventArgs 
{
    public status Status{get;set;}
    public StartStopArgs(status status) {Status = status;}    
}
namespace Controllers
{
    /// <summary>
    /// Interaction logic for StartStopButton.xaml
    /// </summary>
    public partial class StartStopButton : UserControl
    {
        public event EventHandler<StartStopArgs> ButtonClicked;
        public status Status
        {
            get;
            set;
        }
        private Color startColor = (Color)ColorConverter.ConvertFromString("#FF5AC64C");
        private Color stopColor = (Color)ColorConverter.ConvertFromString("#c74f50");
        private SolidColorBrush startBrush = new SolidColorBrush();
        private SolidColorBrush stopBrush = new SolidColorBrush();
        public StartStopButton()
        {
            InitializeComponent();
            startBrush.Color = startColor;
            stopBrush.Color = stopColor;
            //Set the default to a start button. 
            Button.Fill = startBrush;
            Text.Text = "Start";
            Status = status.Start;
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Status == status.Start)
            {
                Status = status.Stop;
                Button.Fill = stopBrush;
                Text.Text = "Stop";
            }
            else
            {
                Status = status.Start;
                Button.Fill = startBrush;
                Text.Text = "Start";
            }
            if (ButtonClicked != null)
            {
                ButtonClicked(this, new StartStopArgs(Status));
            }
        }
        public void SetStart()
        {
            Status = status.Start;
            Button.Fill = startBrush;
            Text.Text = "Start";
        }
        public void SetStop()
        {
            Status = status.Stop;
            Button.Fill = stopBrush;
            Text.Text = "Stop";
        }
    }
}
