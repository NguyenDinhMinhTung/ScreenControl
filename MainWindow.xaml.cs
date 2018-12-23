using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Interop;
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

        IntPtr _ClipboardViewerNext;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public MainWindow()
        {
            InitializeComponent();

            //RegisterClipboardViewer();

            //HwndSource src = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            //src.AddHook(new HwndSourceHook(WndProc));

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

        public void RegisterClipboardViewer()
        {
                _ClipboardViewerNext = new WindowInteropHelper(this).Handle;
        }

        public void UnregisterClipboardViewer()
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
                User32.ChangeClipboardChain(GetModuleHandle(module.ModuleName), _ClipboardViewerNext);
        }

        private void GetClipboardData()
        {
            System.Windows.Forms.IDataObject iData = new System.Windows.Forms.DataObject();

            try
            {
                iData = System.Windows.Forms.Clipboard.GetDataObject();
            }
            catch (System.Runtime.InteropServices.ExternalException externEx)
            {
                // Copying a field definition in Access 2002 causes this sometimes?
                Debug.WriteLine("InteropServices.ExternalException: {0}", externEx.Message);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            if (iData.GetDataPresent(DataFormats.Text))
            {
                MessageBox.Show((string)iData.GetData(DataFormats.Text));
            }
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
                if (Screen.isPrimaryScreen)
                {
                    if ((p.pt.x <= 2 && screen.screenDrirection == Screen.Direction.Left) ||
                    (p.pt.x >= screenWidth - 2 && screen.screenDrirection == Screen.Direction.Right) ||
                    (p.pt.y <= 2 && screen.screenDrirection == Screen.Direction.Top) ||
                    (p.pt.y >= screenHeight - 2 && screen.screenDrirection == Screen.Direction.Bottom))
                    {
                        screen.Opacity = 0.01;
                    }
                }

                if (!Screen.isPrimaryScreen)
                {
                    server.SendData(0, (double)(p.pt.x) / screenWidth, (double)(p.pt.y) / screenHeight);
                }
            });

            mouseHook.LeftButtonDown += new MouseHook.MouseHookCallback((p) =>
            {
                if (!Screen.isPrimaryScreen)
                    server.SendData(1, 0, 0);
            });

            mouseHook.LeftButtonUp += new MouseHook.MouseHookCallback((p) =>
            {
                if (!Screen.isPrimaryScreen)
                    server.SendData(2, 0, 0);
            });

            mouseHook.RightButtonDown += new MouseHook.MouseHookCallback((p) =>
            {
                if (!Screen.isPrimaryScreen)
                    server.SendData(3, 0, 0);
            });

            mouseHook.RightButtonUp += new MouseHook.MouseHookCallback((p) =>
            {
                if (!Screen.isPrimaryScreen)
                    server.SendData(4, 0, 0);
            });

            mouseHook.MiddleButtonDown += new MouseHook.MouseHookCallback((p) =>
            {
                if (!Screen.isPrimaryScreen)
                    server.SendData(5, 0, 0);
            });

            mouseHook.MiddleButtonUp += new MouseHook.MouseHookCallback((p) =>
            {
                if (!Screen.isPrimaryScreen)
                    server.SendData(6, 0, 0);
            });

            mouseHook.MouseWheel += new MouseHook.MouseHookCallback((p) =>
            {
                if (!Screen.isPrimaryScreen)
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
                if (!Screen.isPrimaryScreen)
                {
                    byte[] data = new byte[2];
                    data[0] = 10;
                    data[1] = (byte)p;
                    //data[1] = 0x4A;
                    server.Send(data);
                }
            });

            keyboardHook.KeyUp += new KeyboardHook.KeyboardHookCallback((p) =>
              {
                  if (!Screen.isPrimaryScreen)
                  {
                      byte[] data = new byte[2];
                      data[0] = 11;
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

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg!=160&&msg!=132&&msg!=522&&msg!=512&&msg!=32)
            text.AppendText(msg.ToString()+" ");
            switch ((Msgs)msg)
            {
                //
                // The WM_DRAWCLIPBOARD message is sent to the first window 
                // in the clipboard viewer chain when the content of the 
                // clipboard changes. This enables a clipboard viewer 
                // window to display the new content of the clipboard. 
                //
                case Msgs.WM_DRAWCLIPBOARD:

                    Debug.WriteLine("WindowProc DRAWCLIPBOARD: " + msg, "WndProc");

                    GetClipboardData();

                    //
                    // Each window that receives the WM_DRAWCLIPBOARD message 
                    // must call the SendMessage function to pass the message 
                    // on to the next window in the clipboard viewer chain.
                    //
                    User32.SendMessage(_ClipboardViewerNext, msg, wParam, lParam);
                    break;


                //
                // The WM_CHANGECBCHAIN message is sent to the first window 
                // in the clipboard viewer chain when a window is being 
                // removed from the chain. 
                //
                case Msgs.WM_CHANGECBCHAIN:
                    Debug.WriteLine("WM_CHANGECBCHAIN: lParam: " + lParam, "WndProc");

                    // When a clipboard viewer window receives the WM_CHANGECBCHAIN message, 
                    // it should call the SendMessage function to pass the message to the 
                    // next window in the chain, unless the next window is the window 
                    // being removed. In this case, the clipboard viewer should save 
                    // the handle specified by the lParam parameter as the next window in the chain. 

                    //
                    // wParam is the Handle to the window being removed from 
                    // the clipboard viewer chain 
                    // lParam is the Handle to the next window in the chain 
                    // following the window being removed. 
                    if (wParam == _ClipboardViewerNext)
                    {
                        //
                        // If wParam is the next clipboard viewer then it
                        // is being removed so update pointer to the next
                        // window in the clipboard chain
                        //
                        _ClipboardViewerNext = lParam;
                    }
                    else
                    {
                        User32.SendMessage(_ClipboardViewerNext, msg, wParam, lParam);
                    }
                    break;

                default:
                    //
                    // Let the form process the messages that we are
                    // not interested in
                    //
                    break;

            }

            return IntPtr.Zero;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            RegisterClipboardViewer();
        }
    }
}
