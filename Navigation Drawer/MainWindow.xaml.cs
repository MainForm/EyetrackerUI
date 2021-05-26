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

using System.Diagnostics;

namespace Navigation_Drawer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            // Set tooltip visibility
            if(Tg_Btn.IsChecked == true)
            {
                tt_analysis.Visibility = Visibility.Collapsed;
                tt_replay.Visibility = Visibility.Collapsed;
                tt_setting.Visibility = Visibility.Collapsed;
            }
            else
            {
                tt_analysis.Visibility = Visibility.Visible;
                tt_replay.Visibility = Visibility.Visible;
                tt_setting.Visibility = Visibility.Visible;
            }
        }

        private void Tg_Btn_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void Tg_Btn_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void nav_pnl_MouseLeave(object sender, MouseEventArgs e)
        {
            Tg_Btn.IsChecked = false;
        }

        private void Analysis_Selected(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content.GetType().Name == "Anlaysis")
                return;

            MainFrame.Navigate(new Analysis());
        }

        private void Replay_Selected(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content.GetType().Name == "Replay")
                return;
            MainFrame.Navigate(new Replay());
        }
    }
}
