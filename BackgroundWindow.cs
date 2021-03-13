using System;
using System.Drawing;
using System.Windows.Forms;

namespace Fullscreenifier
{
    public class BackgroundWindow
    {
        public IntPtr Handle => form.Handle;

        private readonly Control owner;
        private readonly int desktopWidth, desktopHeight;

        private Form form;

        public BackgroundWindow(IntPtr ownerHandle, int desktopWidth, int desktopHeight)
        {
            owner = Control.FromHandle(ownerHandle);
            this.desktopWidth = desktopWidth;
            this.desktopHeight = desktopHeight;
        }

        public BackgroundWindow Show()
        {
            InitializeForm().Show();
            return this;
        }

        public BackgroundWindow Hide()
        {
            form.Close();
            return this;
        }

        private Form InitializeForm()
        {
            form = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                SizeGripStyle = SizeGripStyle.Hide,
                BackColor = Color.Black,
                Size = new Size(desktopWidth, desktopHeight),
                StartPosition = FormStartPosition.Manual,
                Left = 0,
                Top = 0,
                Parent = owner,
                ShowInTaskbar = false
            };
            return form;
        }
    }
}
