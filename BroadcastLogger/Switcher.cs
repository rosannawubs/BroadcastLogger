using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BroadcastLogger
{
    public class switcher
    {
        public static MainWindow mainWindow;
        public static void Switch(UserControl newPage)
        {
            mainWindow.Navigate(newPage);
        }
    }
}
