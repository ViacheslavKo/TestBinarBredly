using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestBinarBredly
{
    public partial class FormScreen : Form
    {
        Bitmap source;
        object LockSource = new object();

        Bitmap Source
        {
            get
            {
                lock (LockSource)
                {
                    return source;
                }
            }
            set
            {
                lock (LockSource)
                {
                    source = value;
                }
            }
        }

        public FormScreen()
        {
            InitializeComponent();
        }

        private void FormScreen_Load(object sender, EventArgs e)
        {
            this.Width = Screen.PrimaryScreen.WorkingArea.Width;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        private void FormScreen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Space)
                this.Close();
        }

        public void SetImage(Bitmap bmp)
        {
            Source = bmp;

            Action action = () => pictureBox1.Image = bmp;
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }

        private void FormScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
