using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;



namespace ScreenShotService
{
    public class ScreenShotClass
    {
        public void CaptureApplication(string processes, string path, string rig)
        {
            String[] spiltNames = processes.Split(',');
            foreach (string procName in spiltNames)
            {

                var proc = Process.GetProcessesByName(procName)[0];
                var rect = new User32.Rect();
                User32.GetWindowRect(proc.MainWindowHandle, ref rect);

                int width = rect.right - rect.left;
                int height = rect.bottom - rect.top;
                ScreenShotClass.MoveWindow(proc.MainWindowHandle, 0, 0, width, height, true);
                bool b = SetForegroundWindow(proc.MainWindowHandle);
                User32.GetWindowRect(proc.MainWindowHandle, ref rect);
                width = rect.right - rect.left;
                height = rect.bottom - rect.top;

                var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                Graphics graphics = Graphics.FromImage(bmp);
                graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

                bmp.Save(path + rig + "_" + procName + ".png", ImageFormat.Png);
            }
        }
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    }
    class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

    }
}

