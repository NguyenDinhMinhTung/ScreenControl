using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenControl
{
    internal static class WinCursors
    {
        [DllImport("user32.dll")]
        private static extern int ShowCursor(bool bShow);
        [DllImport("user32.dll")]
        static extern bool SetSystemCursor(IntPtr hcur, uint id);
        [DllImport("user32.dll")]
        static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        //Normal cursor
        private static uint OCR_NORMAL = 32512;
        //The text selection (I-beam) cursor.
        private static uint OCR_IBEAM = 32513;
        //The cross-shaped cursor.
        private static uint OCR_CROSS = 32515;

        internal static void SetCursor()
        {
            //SetSystemCursor(LoadCursor(IntPtr.Zero, (int)OCR_NORMAL), OCR_CROSS);
        }

        internal static void ShowCursor()
        {
            while (ShowCursor(true) < 0)
            {

            }
        }

        internal static void HideCursor()
        {
            while (ShowCursor(false) >= 0)
            {

            }
        }
    }
}
