using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace projekt_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Zdroj zdroj = new Zdroj();
		DispatcherTimer actual_values_timer = new DispatcherTimer();
        private bool zdrojConnected = false;
        private DateTime scriptStartTime;

		public MainWindow()
        {
            InitializeComponent();
			actual_values_timer.Interval = TimeSpan.FromMilliseconds(300);
			actual_values_timer.Tick += Actual_values_interval_expired;
            base.Closing += MainWindow_Closing;                         //call MainWindow_Closing when window is about to be closed
		}

        //method called when the main window is about to be closed
        private void MainWindow_Closing(object sender,  System.ComponentModel.CancelEventArgs e)
        {
            zdroj.Disconnect();
        }


		private void Connect_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (zdrojConnected)
                {
                    zdroj.Disconnect();
                    zdrojConnected = false;
                }
                else
                {
                    zdrojConnected = zdroj.Connect();
                }

                if (zdrojConnected)
                {
					connect_btn.Background = new SolidColorBrush(Colors.Green);
					user_input_radiobtn.IsEnabled = true;
					script_input_radiobtn.IsEnabled = true;
					sys_log.Content = "Connected";
					actual_values_timer.Start();
				}
				else
                {
                    connect_btn.Background = new SolidColorBrush(Colors.Red);
                    sys_log.Content = "Disconnected";
                    actual_values_timer.Stop();
                }
            }
            catch (Exception ex)
            {
                sys_log.Content = ex.Message.ToString();
            }
        }

        private void User_input_radiobtn_checked(object sender, RoutedEventArgs e)
        {
            if (ZdrojSkript.ScriptRunning)
            {
                user_input_radiobtn.IsChecked = false;
                script_input_radiobtn.IsChecked = true;
                return;
            }

            actual_values.IsEnabled = true;
            set_values.IsEnabled = true;
            protections.IsEnabled = true;
            output_btn.IsEnabled = true;
            set_values_btn.IsEnabled = true;

            protection_off.IsChecked = true;

            script.IsEnabled = false;
            abort_script_btn.IsEnabled = false;
        }

        private void Script_input_radiobtn_checked(object sender, RoutedEventArgs e)
        {
            /*
            if (ZdrojSkript.ScriptRunning)
            {
                return;
            }
            */

            actual_values.IsEnabled = false;
            set_values.IsEnabled = false;
            protections.IsEnabled = false;
            output_btn.IsEnabled = false;
            set_values_btn.IsEnabled = false;

            script.IsEnabled = true;
            abort_script_btn.IsEnabled = true;
        }

        private void Output_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool flag_output = !zdroj.Output;
                zdroj.Output = flag_output;

                if (flag_output)
                {
                    output_btn.Background = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    output_btn.Background = new SolidColorBrush(Colors.LightGray);
                }
                sys_log.Content = "Output: " + flag_output;
            }
            catch (Exception ex)
            {
                sys_log.Content = ex.Message.ToString();
			}
        }

        
        private void Protection_check(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)protection_off.IsChecked)
                {
                        zdroj.OverLimProt = Zdroj.OverLimitProtection.Disabled;
                        sys_log.Content = "Protection: Disabled";
            }
                else if ((bool)protection_OVP.IsChecked)
                {
                    zdroj.OverLimProt = Zdroj.OverLimitProtection.OVP;
                    sys_log.Content = "Protection: OVP";
                }
                else
                {
                    zdroj.OverLimProt = Zdroj.OverLimitProtection.OCP;
                    sys_log.Content = "Protection: OCP";                
                }
            }
            catch (Exception ex)
            {
                sys_log.Content = ex.Message.ToString();
			}  
        }

        private double VoltageLimit = 0.0;
        private double CurrentLimit = 0.0;
        private int VoltSlew = 0;

		private double ReadVoltageLimit;
		private double ReadCurrentLimit;
		private int ReadVoltSlew;

		private void Actual_values_interval_expired(object sender, EventArgs e)
        {
            try
            {
                actual_voltage.Content = zdroj.MeasVoltage + " V";
                actual_current.Content = zdroj.MeasCurrent + " mA";
                actual_power.Content = zdroj.MeasPower + " W";

				ReadCurrentLimit = zdroj.CurrentLimit;
                ReadVoltageLimit = zdroj.VoltageLimit;
                ReadVoltSlew = zdroj.VoltSlew;

                if(ReadVoltageLimit != VoltageLimit)
                {
                    if(set_voltage.IsFocused == false)
                    {
						set_voltage.Text = ReadVoltageLimit.ToString();
                        VoltageLimit = ReadVoltageLimit;
					}
				}
				if (ReadCurrentLimit != CurrentLimit)
				{
                    if(set_current.IsFocused == false)
                    {
						set_current.Text = ReadCurrentLimit.ToString();
					}
					CurrentLimit = ReadCurrentLimit;
				}
				if (ReadVoltSlew != VoltSlew)
				{
                    if(set_slew.IsFocused == false)
                    {
						set_slew.Text = ReadVoltSlew.ToString();
					}
					VoltSlew = ReadVoltSlew;
				}
            }
            catch (Exception ex)
            {
                actual_values_timer.Stop();
                sys_log.Content = ex.Message.ToString();
			}

            if (ZdrojSkript.ScriptRunning)
            {
                script_progress_bar.Value = (DateTime.Now - scriptStartTime).TotalSeconds / ZdrojSkript.TotalTime * 100;
            }
            else
            {
                script_progress_bar.Value = 0;
                script_progress_bar_lbl.Content = "Status: script stopped";
			}
        }

        private void Set_values_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double number_double;
                if (double.TryParse(set_voltage.Text, out number_double))
                {
                    if (number_double != VoltageLimit)
                    {
                        zdroj.VoltageLimit = number_double;
                        VoltageLimit = number_double;
                    }
                }
                else
                {
                    sys_log.Content = "Incorrect voltage limit value";
                    return;
                }

                if (double.TryParse(set_current.Text, out number_double))
				{
                    if (number_double != CurrentLimit)
                    {
                        zdroj.CurrentLimit = number_double;
                        CurrentLimit = number_double;
                    }
				}
				else
				{
					sys_log.Content = "Incorrect current limit value";
					return;
				}

				int number_int;                
                if (int.TryParse(set_slew.Text, out number_int))
				{
                    if(number_int != VoltSlew)
                    {
                        zdroj.VoltSlew = number_int;
                        VoltSlew = number_int;
                    }
				}
				else
				{
					sys_log.Content = "Incorrect voltage slew value";
					return;
				}

				sys_log.Content = "Set values: V = " + VoltageLimit + "; I = " + CurrentLimit + "; Slew rate = " + VoltSlew;

            }
            catch (Exception ex)
            {
                sys_log.Content = ex.Message.ToString();
			}
        }

		private void Open_script_btn_Click(object sender, RoutedEventArgs e)
        {
			// Configure open file dialog box
			var dialog = new Microsoft.Win32.OpenFileDialog();
			dialog.FileName = "Script"; // Default file name
			dialog.DefaultExt = ".csv"; // Default file extension
			dialog.Filter = "power supply scripts (.csv)|*.csv|All files (*.*)|*.*"; // Filter files by extension

			// Show open file dialog box
			bool? result = dialog.ShowDialog();

			// Process open file dialog box results
			if (result == true)
			{
				// Open document
				script_path.Text = dialog.FileName;
			}
		}


		private void Start_script_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(script_path.Text))
                {
                    ZdrojSkript.Init(script_path.Text);
                    ZdrojSkript.Start(zdroj);
                    scriptStartTime = DateTime.Now;
					sys_log.Content = "Script started. Total commands to be executed: " +
                        ZdrojSkript.TotalScriptCommands + ". Total time: " +
                        string.Format("{0:f2}", ZdrojSkript.TotalTime/60) +
                        " minutes.";
					script_progress_bar.Value = 0;
                    script_progress_bar_lbl.Content = "Status: script running";
                }
                else
                {
                    sys_log.Content = "Script file does not exist. Check file path.";
                }
            }
            catch (Exception ex)
            {
                sys_log.Content = ex.Message.ToString();
			}

        }

        private void Abort_script_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ZdrojSkript.Abort();
                sys_log.Content = "Script is aborted";
                script_progress_bar.Value = 0;
				script_progress_bar_lbl.Content = "Status: script stopped";
			}
            catch (Exception ex)
            {
                sys_log.Content = ex.Message.ToString();
			}
        }
    }
}
