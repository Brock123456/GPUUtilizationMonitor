﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;

namespace ScreenShotService
{
    public class ScreenShotClass
    {
        public static string errorProcName = "";
        public void CaptureApplication(string processes, string path, string rig)
        {
            if (processes != "" && path != "") {
                String[] spiltNames = processes.Split(',');
                foreach (string procName in spiltNames)
                {
                    try
                    {
                        errorProcName = procName;
                        var proc = Process.GetProcessesByName(procName)[0];
                        var rect = new User32.Rect();
                        User32.GetWindowRect(proc.MainWindowHandle, ref rect);

                        User32.MoveWindow(proc.MainWindowHandle, 0, 0, 1000, 1000, true);
                        int width = rect.right - rect.left;
                        int height = rect.bottom - rect.top;
                        System.Threading.Thread.Sleep(1000);
                        bool b = User32.SetForegroundWindow(proc.MainWindowHandle);
                        System.Threading.Thread.Sleep(1000);
                        User32.GetWindowRect(proc.MainWindowHandle, ref rect);
                        width = rect.right - rect.left;
                        height = rect.bottom - rect.top;                        

                        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                        Graphics graphics = Graphics.FromImage(bmp);
                        graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
                        string filename = path + rig + procName + ".png";
                        bmp.Save(filename, ImageFormat.Png);
                        System.Threading.Thread.Sleep(2000);
                        Console.WriteLine(errorProcName + " is open. Screen shot taken.");
                    }
                    catch 
                    {
                        //Not logging assuming extra windows are there to catch the diffrent miners.
                        Console.WriteLine(errorProcName + " is not open.");
                    }
                }
            }
        }

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

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    }
}

