using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using PInvoke;

namespace Fullscreenifier
{
    public class Fullscreenifier
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        private readonly string executablePath;
        private readonly int desktopWidth, desktopHeight;

        private BackgroundWindow backgroundWindow;

        public Fullscreenifier(string executablePath)
        {
            this.executablePath = executablePath;

            var desktopRect = GetDesktopRect();
            desktopWidth = desktopRect.right - desktopRect.left;
            desktopHeight = desktopRect.bottom - desktopRect.top;
        }

        public void Run()
        {
            Process process = null;
            try
            {
                process = Process.Start(executablePath);
                process.WaitForInputIdle();

                int width = 0, height = 0;
                User32.WINDOWINFO windowInfo = default;
                windowInfo = GetWindowInfo(process.MainWindowHandle);
                width = windowInfo.rcWindow.right - windowInfo.rcWindow.left;
                height = windowInfo.rcWindow.bottom - windowInfo.rcWindow.top;

#if DEBUG
                // If Notepad was started, test different window sizes.
                if (executablePath == "notepad")
                {
                    width = 300;
                    height = 600;
                }
#endif

                ResetWindowPosition(process.MainWindowHandle, width, height);
                InitializeMagnification();

                backgroundWindow = new BackgroundWindow(process.MainWindowHandle, desktopWidth, desktopHeight).Show();

                User32.ShowCursor(false);

                var refreshInterval = 200;
                var previousRect = new RECT();
                bool pause = false, forceUpdate = false;
                while (!process.HasExited)
                {
                    if ((User32.GetAsyncKeyState((int)User32.VirtualKey.VK_LWIN)) != 0 &&
                        (User32.GetAsyncKeyState((int)User32.VirtualKey.VK_ESCAPE)) != 0)
                    {
                        pause = !pause;
                        if (pause)
                        {
                            backgroundWindow.Hide();
                            ResetMagnification();
                        }
                        else
                        {
                            backgroundWindow.Show();
                            forceUpdate = true;
                        }
                    }

                    if (pause)
                    {
                        Thread.Sleep(refreshInterval);
                        continue;
                    }

                    if (User32.GetForegroundWindow() != process.MainWindowHandle)
                    {
                        User32.SetForegroundWindow(process.MainWindowHandle);
                    }

                    windowInfo = GetWindowInfo(process.MainWindowHandle);
                    if (windowInfo.rcWindow.left != 0 || windowInfo.rcWindow.top != 0)
                    {
                        ResetWindowPosition(process.MainWindowHandle, windowInfo);
                        windowInfo = GetWindowInfo(process.MainWindowHandle);
                    }

                    if (forceUpdate || !windowInfo.rcClient.Equals(previousRect))
                    {
                        SetMagnification(windowInfo);
                        previousRect = windowInfo.rcClient;
                    }

                    if (forceUpdate)
                    {
                        forceUpdate = false;
                    }

                    Thread.Sleep(refreshInterval);
                }
            }
            catch (Exception ex)
            {
                ex.Message.Log();

                if (process != null && !process.HasExited)
                {
                    process.Kill();
                }
            }
            finally
            {
                try { User32.ShowCursor(true); } catch { }
                try { Magnification.MagUninitialize(); } catch { }
                try { backgroundWindow?.Hide(); } catch { }
            }
        }

        private static void InitializeMagnification()
        {
            if (!Magnification.MagInitialize())
            {
                throw new Exception(Kernel32.GetLastError().ToString());
            }
        }

        private void ResetMagnification()
        {
            if (!Magnification.MagSetFullscreenTransform(1, 0, 0))
            {
                throw new Exception(Kernel32.GetLastError().ToString());
            }
        }

        private RECT GetDesktopRect()
        {
            if (!User32.GetWindowRect(User32.GetDesktopWindow(), out RECT desktopRect))
            {
                throw new Exception(Kernel32.GetLastError().ToString());
            }
            return desktopRect;
        }

        private void ResetWindowPosition(IntPtr windowHandle, User32.WINDOWINFO windowInfo)
        {
            if (!User32.SetWindowPos(windowHandle, default, 0, 0, windowInfo.rcWindow.GetWidth(), windowInfo.rcWindow.GetHeight(), 0))
            {
                throw new Exception(Kernel32.GetLastError().ToString());
            }
        }

        private void ResetWindowPosition(IntPtr windowHandle, int width, int height)
        {
            if (!User32.SetWindowPos(windowHandle, default, 0, 0, width, height, 0))
            {
                throw new Exception(Kernel32.GetLastError().ToString());
            }
        }

        private User32.WINDOWINFO GetWindowInfo(IntPtr windowHandle)
        {
            User32.WINDOWINFO windowInfo = default;
            if (!User32.GetWindowInfo(windowHandle, ref windowInfo))
            {
                throw new Exception(Kernel32.GetLastError().ToString());
            }
            return windowInfo;
        }

        private void SetMagnification(User32.WINDOWINFO windowInfo)
        {
            var width = windowInfo.rcClient.GetWidth();
            var height = windowInfo.rcClient.GetHeight();

            var isHigherThanWider = desktopHeight / height <= desktopWidth / width;
            var magnification = isHigherThanWider ?
                (float)desktopHeight / height :
                (float)desktopWidth / width;

            var xOffset = Math.Max(0, isHigherThanWider ?
                (int)Math.Ceiling((desktopWidth / magnification - width) / 2f) :
                (int)Math.Floor((desktopWidth / magnification - width) / 2f)
            );
            var yOffset = Math.Max(0, isHigherThanWider ?
                (int)Math.Floor((desktopHeight / magnification - height) / 2f) :
                (int)Math.Ceiling((desktopHeight / magnification - height) / 2f)
            );

            if (!Magnification.MagSetFullscreenTransform(magnification, windowInfo.rcClient.left - xOffset, windowInfo.rcClient.top - yOffset))
            {
                throw new Exception(Kernel32.GetLastError().ToString());
            }
        }
    }
}
