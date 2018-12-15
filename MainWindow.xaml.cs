using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenControl
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {


            InitializeComponent();
        }


        void mouseHook_MouseMove(MouseHooker.MSLLHOOKSTRUCT mouseStruct)
        {
            this.Title = mouseStruct.pt.x.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MouseHooker mouseHoocker = new MouseHooker();
            MouseHooker.MouseMove += new MouseHooker.MouseHookCallback(mouseHook_MouseMove);

        }
    }
}
