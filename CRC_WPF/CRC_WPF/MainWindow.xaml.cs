using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace CRC_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MaxHeight = SystemParameters.WorkArea.Height;
            MaxWidth = SystemParameters.WorkArea.Width;
            InitializeComponent();

            // 初始化页面为控制台
            Function_Dock.Navigate(new Uri("Pages/Guide.xaml", UriKind.Relative));
        }

        private static bool Locked = true;

        /* 窗口基础操作 */

        // 可拖动
        private void Window_MouseLeftButtonDrag(object sender, MouseButtonEventArgs e)
        {
            Point dragArea = e.GetPosition(FuncDock);
            if (dragArea.Y < 0)
            {
                DragMove();
            }
        }

        // 固定窗口大小
        private void Window_MouseClickLocke(object sender, RoutedEventArgs e)
        {
            if (Locked)
            {
                Locked = false;
                Btn_Locke.Content = "🔓";
            }
            else
            {
                Locked = true;
                Btn_Locke.Content = "🔒";
            }
        }

        // 最小化
        private void Window_MouseClickMinim(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // 窗口大小
        private void Window_MouseClickResiz(object sender, RoutedEventArgs e)
        {
            if (!Locked && WindowState ==WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                Btn_Resiz.Content = "□";
                Btn_Resiz.FontSize = 16;
            }
            else if (!Locked && WindowState != WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
                Btn_Resiz.Content = "⚪";
                Btn_Resiz.FontSize = 12;
            }
        }

        // 关闭窗口
        private void Window_MouseClickClose(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
            Process.GetCurrentProcess().Kill();
        }


        /* 页面切换操作 */

        // 打开控制台主页
        private void Console_PageClick(object sender, RoutedEventArgs e)
        {
            Function_Dock.Navigate(new Uri("Pages/Console.xaml", UriKind.Relative));
        }

        // 打开作者主页
        private void Author_PageClick(object sender, RoutedEventArgs e)
        {
            Function_Dock.Navigate(new Uri("Pages/Author.xaml", UriKind.Relative));
        }

        // 打开报告主页
        private void Guide_PageClick(object sender, RoutedEventArgs e)
        {
            Function_Dock.Navigate(new Uri("Pages/Guide.xaml", UriKind.Relative));
        }
    }
}
