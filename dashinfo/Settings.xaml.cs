using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace dashinfo
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_MINIMIZEBOX = 0x20000;

        private int _temperature;
        public int Temperature
        {
            get
            {
                return _temperature;
            }
            set
            {
                _temperature = value;
                tempUnit.SelectedIndex = _temperature;
            }
        }

        private int _speed;
        public int Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value;
                speedUnit.SelectedIndex = _speed;
            }
        }

        private int _capacity;
        public int Capacity
        {
            get
            {
                return _capacity;
            }
            set
            {
                _capacity = value;
                capacityUnit.SelectedIndex = _capacity;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Temperature = tempUnit.SelectedIndex;
            Speed = speedUnit.SelectedIndex;
            Capacity = capacityUnit.SelectedIndex;
            DialogResult = true;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // This removes the maximize and minimize boxes from the window.
            // For some reason, there is no way to do this using WPF APIs, so we must call the Windows API.
            var hwnd = new WindowInteropHelper((Window)sender).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tempUnit.SelectedIndex = Temperature;
            speedUnit.SelectedIndex = Speed;
            capacityUnit.SelectedIndex = Capacity;
        }
    }
}
