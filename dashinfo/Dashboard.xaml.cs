using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace dashinfo
{
    public enum TemperatureUnit
    {
        C = 0,
        F = 1
    }
    public enum SpeedUnit
    {
        KPH = 0,
        MPH = 1
    }
    public enum CapacityUnit
    {
        Liters = 0,
        Gallons = 1
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        /// <summary>
        /// UDP client which receives messages from the game
        /// </summary>
        private UdpClient client;

        /// <summary>
        /// The max rpms of the car
        /// </summary>
        private int MaxRpm;

        /// <summary>
        /// Array of ellipses which represent the current engine RPM
        /// </summary>
        private Ellipse[] rpm;

        /// <summary>
        /// Are we showing tire temps or brake temps?
        /// </summary>
        private bool showingTireTemps = true;

        /// <summary>
        /// Port for listening for incoming information
        /// </summary>
        private const int PORT = 1337;

        /// <summary>
        /// Message types are identified using characters 
        /// </summary>
        const char RPM = '1';
        const char OILTEMP = '2';
        const char WATERTEMP = '3';
        const char FUEL = '4';
        const char MAXRPM = '5';
        const char GEAR = '6';
        const char LAPNUMBER = '7';
        const char MAXLAPS = '8';
        const char LAPTIME = '9';
        const char LF_TIRE_TEMP = 'a';
        const char LR_TIRE_TEMP = 'b';
        const char RF_TIRE_TEMP = 'c';
        const char RR_TIRE_TEMP = 'd';
        const char LF_BRAKE_TEMP = 'e';
        const char LR_BRAKE_TEMP = 'f';
        const char RF_BRAKE_TEMP = 'g';
        const char RR_BRAKE_TEMP = 'h';

        public Dashboard()
        {
            InitializeComponent();
            Gear = "0";
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Add all of the ellipses in the user interface to an array
            rpm = new Ellipse[16];
            int index = 0;
            foreach (Ellipse el in FindVisualChildren<Ellipse>(this))
            {
                rpm[index++] = el;
            }

            // Restore user settings from the registry
            RestoreSettings();

            // Start receiving messages
            client = new UdpClient(PORT);
            client.BeginReceive(new AsyncCallback(MessageReceived), null);
        }

        /// <summary>
        /// Gets or sets the unit to use for temperature
        /// </summary>
        private TemperatureUnit Temperature
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the unit to use for speed
        /// </summary>
        private SpeedUnit Speed
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the unit to use for capacities
        /// </summary>
        private CapacityUnit Capacity
        {
            get; set;
        }

        #region Engine Properties

        /// <summary>
        /// Oil temperature
        /// </summary>
        private static DependencyProperty oilProperty = DependencyProperty.Register("OilTemp", typeof(int), typeof(Dashboard));
        private int OilTemp
        {
            get { return (int)GetValue(oilProperty); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    SetValue(oilProperty, ProcessTemperature(value));
                });
            }
        }

        /// <summary>
        /// Water temperature
        /// </summary>
        public static DependencyProperty waterProperty = DependencyProperty.Register("WaterTemp", typeof(int), typeof(Dashboard));
        public int WaterTemp
        {
            get { return (int)GetValue(waterProperty); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    SetValue(waterProperty, ProcessTemperature(value));
                }); 
            }
        }

        /// <summary>
        /// Fuel
        /// </summary>
        public static DependencyProperty fuelProperty = DependencyProperty.Register("Fuel", typeof(int), typeof(Dashboard));
        public int Fuel
        {
            get { return (int)GetValue(fuelProperty); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    SetValue(fuelProperty, ProcessCapacity(value));
                });
            }
        }

        /// <summary>
        /// Gear
        /// </summary>
        public static DependencyProperty gearProperty = DependencyProperty.Register("Gear", typeof(string), typeof(Dashboard));
        public string Gear
        {
            get { return (string)GetValue(gearProperty); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    if (value == "0")
                    {
                        SetValue(gearProperty, "n");
                    }
                    else if (value == "-1")
                    {
                        SetValue(gearProperty, "r");
                    }
                    else
                    {
                        SetValue(gearProperty, value);
                    }
                });
            }
        }

        /// <summary>
        /// Lap time
        /// </summary>
        public static DependencyProperty lapTimeProperty = DependencyProperty.Register("LapTime", typeof(float), typeof(Dashboard));
        public float LapTime
        {
            get { return (float)GetValue(lapTimeProperty); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    SetValue(lapTimeProperty, value);
                });
            }
        }

        /// <summary>
        /// Lap number
        /// </summary>
        public static DependencyProperty lapNumberProperty = DependencyProperty.Register("LapNumber", typeof(int), typeof(Dashboard));
        public int LapNumber
        {
            get { return (int)GetValue(lapNumberProperty); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    SetValue(lapNumberProperty, value);
                });
            }
        }

        /// <summary>
        /// Max laps
        /// </summary>
        public static DependencyProperty maxLapsProperty = DependencyProperty.Register("MaxLaps", typeof(int), typeof(Dashboard));
        public int MaxLaps
        {
            get { return (int)GetValue(maxLapsProperty); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    if (value == 2147483647)
                    {
                        SetValue(maxLapsProperty, 0);
                    }
                    else
                    {
                        SetValue(maxLapsProperty, value);
                    }
                });
            }
        }

        public static DependencyProperty lfTireTempProperty = DependencyProperty.Register("LFTireTemp", typeof(int), typeof(Dashboard));
        public int LFTireTemp
        {
            get { return (int)GetValue(lfTireTempProperty); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    SetValue(lfTireTempProperty, ProcessTemperature(value));
                });
            }
        }

        public static DependencyProperty lrTireTempProperty = DependencyProperty.Register("LRTireTemp", typeof(int), typeof(Dashboard));
        public int LRTireTemp
        {
            get { return (int)GetValue(lrTireTempProperty); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    SetValue(lrTireTempProperty, ProcessTemperature(value));
                });
            }
        }

        public static DependencyProperty rfTireTempProperty = DependencyProperty.Register("RFTireTemp", typeof(int), typeof(Dashboard));
        public int RFTireTemp
        {
            get { return (int)GetValue(rfTireTempProperty); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    SetValue(rfTireTempProperty, ProcessTemperature(value));
                });
            }
        }

        public static DependencyProperty rrTireTempProperty = DependencyProperty.Register("RRTireTemp", typeof(int), typeof(Dashboard));
        public int RRTireTemp
        {
            get { return (int)GetValue(rrTireTempProperty); }
            set
            {
                Dispatcher.Invoke(() =>
                {
                    SetValue(rrTireTempProperty, ProcessTemperature(value));
                });
            }
        }

        #endregion
        /// <summary>
        /// Callback to receive UDP packet
        /// </summary>
        /// <param name="res"></param>
        private void MessageReceived(IAsyncResult res)
        {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 1337);
            byte[] received = client.EndReceive(res, ref remote);

            HandleMessage(Encoding.ASCII.GetString(received));
            
            client.BeginReceive(new AsyncCallback(MessageReceived), null);
        }

        
        /// <summary>
        /// Handle an incoming packet
        /// </summary>
        /// <param name="message">Incoming packet</param>
        private void HandleMessage(string message)
        {
            string data = message.Substring(1);
             
            // The first character of the message is the message type. 
            switch (message[0])
            {
                case RPM: 
                    UpdateRPMBubbles(int.Parse(data));
                    break;
                case OILTEMP: 
                    OilTemp = int.Parse(data);
                    break;
                case WATERTEMP: 
                    WaterTemp = int.Parse(data);
                    break;
                case FUEL: 
                    Fuel = int.Parse(data);
                    break;
                case MAXRPM: 
                    MaxRpm = int.Parse(data);
                    break;
                case GEAR: 
                    Gear = data;
                    break;
                case LAPNUMBER: 
                    LapNumber = int.Parse(data);
                    break;
                case MAXLAPS: 
                    MaxLaps = int.Parse(data);
                    break;
                case LF_TIRE_TEMP:
                    if (showingTireTemps) LFTireTemp = int.Parse(data) - 273; // convert from K -> C
                    break;
                case LR_TIRE_TEMP:
                    if (showingTireTemps) LRTireTemp = int.Parse(data) - 273;
                    break;
                case RF_TIRE_TEMP:
                    if (showingTireTemps) RFTireTemp = int.Parse(data) - 273;
                    break;
                case RR_TIRE_TEMP:
                    if (showingTireTemps) RRTireTemp = int.Parse(data) - 273;
                    break;
                case LF_BRAKE_TEMP:
                    if (!showingTireTemps) LFTireTemp = int.Parse(data) - 273;
                    break;
                case LR_BRAKE_TEMP:
                    if (!showingTireTemps) LRTireTemp = int.Parse(data) - 273;
                    break;
                case RF_BRAKE_TEMP:
                    if (!showingTireTemps) RFTireTemp = int.Parse(data) - 273;
                    break;
                case RR_BRAKE_TEMP:
                    if (!showingTireTemps) RRTireTemp = int.Parse(data) - 273;
                    break;
                    //case LAPTIME:
                    //    LapTime = float.Parse(data);
                    //    break;
            }
        }

        /// <summary>
        /// Updates the visual representation of the current RPMs
        /// </summary>
        /// <param name="currentRpm">Current RPMs</param>
        private void UpdateRPMBubbles(int currentRpm)
        {
            int i;
            float percentFilled = (float)currentRpm / MaxRpm;
            
            if (percentFilled >= 0.5)
            {
                int halfmax = MaxRpm / 2;
                float adjusted = (currentRpm - halfmax) / (float)halfmax;

                for (i = 0; ((float)i / 16) < adjusted && adjusted <= 1; i++)
                {
                    if (i < 5)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            rpm[i].Fill = new SolidColorBrush(Colors.Green);
                        });
                    }
                    else if (i >= 5 && i < 11)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            rpm[i].Fill = new SolidColorBrush(Colors.Red);
                        });
                    }
                   else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            rpm[i].Fill = new SolidColorBrush(Colors.Aqua);
                        });
                    }
                }

                for (; i < 16; i++)
                {
                    Dispatcher.Invoke(() =>
                    {
                        rpm[i].Fill = new SolidColorBrush(Colors.Black);
                    });

                }
            }
        }

        // From http://stackoverflow.com/questions/974598/find-all-controls-in-wpf-window-by-type
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            client.Close();
        }

        /// <summary>
        /// Event handler for the settings item in the window's context menu. 
        /// Opens the settings dialog and updates any modified settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            settings.Temperature = (int)Temperature;
            settings.Speed = (int)Speed;
            settings.Capacity = (int)Capacity;
            settings.ShowDialog();

            if (settings.DialogResult.HasValue && settings.DialogResult.Value)
            {
                Temperature = (TemperatureUnit)settings.Temperature;
                Speed = (SpeedUnit)settings.Speed;
                Capacity = (CapacityUnit)settings.Capacity;
                SaveSettings();
            }
        }

        /// <summary>
        /// Restores user settings from the registry
        /// </summary>
        private void RestoreSettings()
        {
            RegistryKey hcu = Registry.CurrentUser.OpenSubKey("Software", true);

            RegistryKey settings;
            if ((settings = hcu.OpenSubKey("dashinfo", true)) == null)
            {
                settings = hcu.CreateSubKey("dashinfo");
            }

            object tunit;
            if ((tunit = settings.GetValue("TemperatureUnit")) == null)
            {
                settings.SetValue("TemperatureUnit", 0);
            }
            else
            {
                Temperature = (TemperatureUnit)tunit;
            }

            object sunit;
            if ((sunit = settings.GetValue("SpeedUnit")) == null)
            {
                settings.SetValue("SpeedUnit", 0);
            }
            else
            {
                Speed = (SpeedUnit)sunit;
            }

            object gunit;
            if ((gunit = settings.GetValue("CapacityUnit")) == null)
            {
                settings.SetValue("CapacityUnit", 0);
            }
            else
            {
                Capacity = (CapacityUnit)gunit;
            }
        }

        /// <summary>
        /// Saves the current user settings to the registry
        /// </summary>
        private void SaveSettings()
        {
            RegistryKey hcu = Registry.CurrentUser.OpenSubKey("Software", true);

            RegistryKey settings;
            if ((settings = hcu.OpenSubKey("dashinfo", true)) == null)
            {
                settings = hcu.CreateSubKey("dashinfo");
            }

            settings.SetValue("TemperatureUnit", (int)Temperature);
            settings.SetValue("SpeedUnit", (int)Speed);
            settings.SetValue("CapacityUnit", (int)Capacity);
        }

        /// <summary>
        /// Converts the given temperature if necessary
        /// </summary>
        /// <param name="temp">The temperature to process</param>
        /// <returns>The processed temperature</returns>
        private int ProcessTemperature(int temp)
        {
            if (Temperature == TemperatureUnit.F)
            {
                return (int)(temp * 1.8) + 32;
            }
            else
            {
                return temp;
            }
        }

        /// <summary>
        /// Converts the given capacity if necessary
        /// </summary>
        /// <param name="cap">The capacity to process</param>
        /// <returns>The processed capacity</returns>
        private int ProcessCapacity(int cap)
        {
            if (Capacity == CapacityUnit.Gallons)
            {
                return (int)(cap * 0.26417);
            }
            else
            {
                return cap;
            }
        }

        private void mainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            showingTireTemps = showingTireTemps? false : true;
        }
    }
}
