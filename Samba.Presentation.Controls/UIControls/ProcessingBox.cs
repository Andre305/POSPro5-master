using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Samba.Presentation.Controls.UIControls
{
    public static class ProcessingBox
    {
        static Form f;

        public static void show_Processing(string message)
        {
            f = new Form();
            f.Width = 330;
            f.Height = 120;
            f.ControlBox = false;
            f.StartPosition = FormStartPosition.CenterScreen;

            Label l = new Label();
            l.Text = message;
            l.Left = 15;
            l.Top = 15;
            l.Font = new Font(l.Font.FontFamily, 20);
            l.AutoSize = true;
            l.Visible = true;

            f.Controls.Add(l);

            f.Show();
        }

        public static void hide_Processing()
        {
            f.Hide();
        }
    }
}
