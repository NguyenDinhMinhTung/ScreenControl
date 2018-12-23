using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;

namespace ScreenControl
{
    class Clipboard
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        IntPtr _ClipboardViewerNext;
        System.Windows.Window window;

        public Clipboard(System.Windows.Window window)
        {
        }

        public void RegisterClipboardViewer()
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
                _ClipboardViewerNext = User32.SetClipboardViewer(GetModuleHandle(module.ModuleName));
        }

        public void UnregisterClipboardViewer()
        {
            using (ProcessModule module = Process.GetCurrentProcess().MainModule)
                User32.ChangeClipboardChain(GetModuleHandle(module.ModuleName), _ClipboardViewerNext);
        }

        ~Clipboard()
        {
            UnregisterClipboardViewer();
        }

      
    }
}
