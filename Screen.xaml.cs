using Microsoft.Win32;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScreenControl
{
    /// <summary>
    /// Screen.xaml の相互作用ロジック
    /// </summary>
    public partial class Screen : Window
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public enum Direction
        {
            Top, Left, Right, Bottom
        }

        public Direction screenDrirection = Direction.Left;

        float scale;
        public static bool isPrimaryScreen = true;

        public Screen()
        {
            InitializeComponent();

            var currentDPI = (int)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop\\WindowMetrics", "AppliedDPI", 96);
            scale = (float)currentDPI / 96;
        }

        public void setPos(int x, int y)
        {
            this.Left = x / scale - 30;
            this.Top = y / scale - 30;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(this);
            bool flag = false;

            switch (screenDrirection)
            {
                case Direction.Bottom:
                    if (point.Y >= Height - 2 && isPrimaryScreen)
                    {
                        point.Y = 15;
                        isPrimaryScreen = false;
                        flag = true;
                        Opacity = 0.01;
                    }
                    else if (point.Y <= 2)
                    {
                        point.Y = Height - 15;
                        isPrimaryScreen = true;
                        flag = true;
                        Opacity = 0;
                    }
                    break;

                case Direction.Top:
                    if (point.Y <= 2 && isPrimaryScreen)
                    {
                        point.Y = Height - 15;
                        isPrimaryScreen = false;
                        flag = true;
                        Opacity = 0.01;
                    }
                    else if (point.Y >= Height - 2)
                    {
                        point.Y = 15;
                        isPrimaryScreen = true;
                        flag = true;
                        Opacity = 0;
                    }
                    break;

                case Direction.Left:
                    if (point.X <= 2 && isPrimaryScreen)
                    {
                        point.X = Width - 15;
                        isPrimaryScreen = false;
                        flag = true;
                        Opacity = 0.01;
                    }
                    else if (point.X >= Width - 2)
                    {
                        point.X = 15;
                        isPrimaryScreen = true;
                        flag = true;
                        Opacity = 0;
                    }
                    break;

                case Direction.Right:
                    if (point.X >= Width - 2 && isPrimaryScreen)
                    {
                        point.X = 15;
                        isPrimaryScreen = false;
                        flag = true;
                        Opacity = 0.01;
                    }
                    else if (point.X <= 3)
                    {
                        point.X = Width - 15;
                        isPrimaryScreen = true;
                        flag = true;
                        Opacity = 0;
                    }
                    break;
            }

            if (flag) SetCursorPos((int)(point.X * scale), (int)(point.Y * scale));
        }
    }
}
