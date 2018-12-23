using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace ScreenControl
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        int screenWidth;
        int screenHeight;
        float scale;

        delegate void Action();

        MouseHook mouseHook = new MouseHook();
        KeyboardHook keyboardHook = new KeyboardHook();

        Server server;
        Screen screen;

        Thread thread;

        DispatcherTimer waitConnectEffect;
        int waitConnectEffectCount = 0;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        public MainWindow()
        {
            InitializeComponent();

            var currentDPI = (int)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop\\WindowMetrics", "AppliedDPI", 96);
            scale = (float)currentDPI / 96;

            screenWidth = (int)(System.Windows.SystemParameters.PrimaryScreenWidth * Math.Round(scale));
            screenHeight = (int)(System.Windows.SystemParameters.PrimaryScreenHeight * Math.Round(scale));
            screen = new Screen();
            screen.Show();

            waitConnectEffect = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 500),
                IsEnabled = true
            };

            waitConnectEffect.Tick += (sender, e) =>
            {
                if (waitConnectEffectCount > 3) waitConnectEffectCount = 0;

                String dot = "";

                for (int i = 0; i < waitConnectEffectCount; i++)
                {
                    dot += ".";
                }

                txtStatus.Text = "接続待ち" + dot;

                waitConnectEffectCount++;
            };

            setupHook();

            setupNewServer();
        }

        void setupNewServer()
        {
            waitConnectEffect.IsEnabled = true;

            if (server != null)
            {
                server.close();
            }

            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
            }

            server = new Server(new Server.ActionAcceptConnect(
            (ip, data) =>
            {
                txtStatus.Dispatcher.Invoke(new Action(() =>
                {
                    txtStatus.Text = "接続済み";
                }));

                waitConnectEffect.IsEnabled = false;

                btnDisconnect.Dispatcher.Invoke(new Action(() =>
                {
                    btnDisconnect.IsEnabled = true;
                }));
            }),
                new Server.ActionReceive(data => { }));

            thread = new Thread(start);
            thread.IsBackground = true;
            thread.Start();
        }

        void setupHook()
        {
            mouseHook.MouseMove += new MouseHook.MouseHookCallback((p) =>
            {
                if (screen.isPrimaryScreen)
                {
                    if ((p.pt.x <= 2 && screen.screenDrirection == Screen.Direction.Left) ||
                    (p.pt.x >= screenWidth - 2 && screen.screenDrirection == Screen.Direction.Right) ||
                    (p.pt.y <= 2 && screen.screenDrirection == Screen.Direction.Top) ||
                    (p.pt.y >= screenHeight - 2 && screen.screenDrirection == Screen.Direction.Bottom))
                    {
                        screen.Opacity = 0.01;
                    }
                }

                if (!screen.isPrimaryScreen)
                {
                    server.SendData(0, (double)(p.pt.x) / screenWidth, (double)(p.pt.y) / screenHeight);
                }
            });

            mouseHook.LeftButtonDown += new MouseHook.MouseHookCallback((p) =>
            {
                if (!screen.isPrimaryScreen)
                    server.SendData(1, 0, 0);
            });

            mouseHook.LeftButtonUp += new MouseHook.MouseHookCallback((p) =>
            {
                if (!screen.isPrimaryScreen)
                    server.SendData(2, 0, 0);
            });

            mouseHook.RightButtonDown += new MouseHook.MouseHookCallback((p) =>
            {
                if (!screen.isPrimaryScreen)
                    server.SendData(3, 0, 0);
            });

            mouseHook.RightButtonUp += new MouseHook.MouseHookCallback((p) =>
            {
                if (!screen.isPrimaryScreen)
                    server.SendData(4, 0, 0);
            });

            mouseHook.MiddleButtonDown += new MouseHook.MouseHookCallback((p) =>
            {
                if (!screen.isPrimaryScreen)
                    server.SendData(5, 0, 0);
            });

            mouseHook.MiddleButtonUp += new MouseHook.MouseHookCallback((p) =>
            {
                if (!screen.isPrimaryScreen)
                    server.SendData(6, 0, 0);
            });

            mouseHook.MouseWheel += new MouseHook.MouseHookCallback((p) =>
            {
                if (!screen.isPrimaryScreen)
                {
                    if (p.mouseData == 7864320)
                    {
                        server.SendData(7, 0, 0);
                    }
                    else
                    {
                        server.SendData(8, 0, 0);
                    }
                }
            });

            keyboardHook.KeyDown += new KeyboardHook.KeyboardHookCallback((p) =>
            {
                //MessageBox.Show(p.ToString());
                if (!screen.isPrimaryScreen)
                {
                    byte[] data = new byte[2];
                    data[0] = 10;
                    data[1] = (byte)p;
                    //data[1] = 0x4A;
                    server.Send(data);
                }
            });

            mouseHook.Install();
            keyboardHook.Install();
        }

        byte[] createData(byte comm, int p)
        {
            byte[] str = BitConverter.GetBytes(p);

            byte[] result = new byte[2 + str.Length];

            result[0] = comm;
            result[1] = (byte)str.Length;

            for (int i = 0; i < str.Length; i++)
            {
                result[2 + i] = str[i];
            }

            return result;
        }

        void start()
        {
            server.StartServer();
        }

        void mouseHook_MouseMove(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            this.Title = mouseStruct.pt.x.ToString();
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            btnDisconnect.IsEnabled = false;
            server.SendData(9, 0, 0);

            setupNewServer();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            server?.close();
            Environment.Exit(0);
        }

        private void txtTop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            txtTop.Background = new SolidColorBrush(Colors.BlueViolet);
            txtLeft.Background = null;
            txtRight.Background = null;
            txtBottom.Background = null;

            screen.screenDrirection = Screen.Direction.Top;
        }

        private void txtLeft_MouseUp(object sender, MouseButtonEventArgs e)
        {
            txtTop.Background = null;
            txtLeft.Background = new SolidColorBrush(Colors.BlueViolet);
            txtRight.Background = null;
            txtBottom.Background = null;

            screen.screenDrirection = Screen.Direction.Left;
        }

        private void txtRight_MouseUp(object sender, MouseButtonEventArgs e)
        {
            txtTop.Background = null;
            txtLeft.Background = null;
            txtRight.Background = new SolidColorBrush(Colors.BlueViolet);
            txtBottom.Background = null;

            screen.screenDrirection = Screen.Direction.Right;
        }

        private void txtBottom_MouseUp(object sender, MouseButtonEventArgs e)
        {
            txtTop.Background = null;
            txtLeft.Background = null;
            txtRight.Background = null;
            txtBottom.Background = new SolidColorBrush(Colors.BlueViolet);

            screen.screenDrirection = Screen.Direction.Bottom;
        }
    }
}
