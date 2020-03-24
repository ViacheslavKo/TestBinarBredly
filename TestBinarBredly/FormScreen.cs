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
        Bitmap original = null;
        Bitmap source;
        object LockSource = new object();
        Label label1;

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

        private void LabelText()
        {
            label1 = new Label();
            label1.Name = "Label1";
            label1.Font = new Font("Microsoft YaHei", 14);
            label1.BackColor = Color.White;
            label1.BorderStyle = BorderStyle.FixedSingle;
            label1.Text = "Обработанное";
            label1.Location = new Point(15, 15);
            label1.AutoSize = true;

            pictureBox1.Controls.Add(label1);
        }

        private void FormScreen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Space)
                this.Close();

            if (e.KeyCode == Keys.Enter)
            {
                if (label1.Text == "Оригинал")
                {
                    label1.Text = "Обработанное";
                    pictureBox1.Image = Source;
                }
                else
                {
                    label1.Text = "Оригинал";
                    pictureBox1.Image = original;
                }
            }
        }

        public void SetImage(Bitmap bmp, Bitmap original)
        {
            LabelText();
            Source = bmp;
            this.original = original;
            Action action = () => pictureBox1.Image = bmp;
            if (InvokeRequired)
                Invoke(action);
            else
                action();
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

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (label1.Text == "Оригинал")
            {
                label1.Text = "Обработанное";
                pictureBox1.Image = Source;
            }
            else
            {
                label1.Text = "Оригинал";
                pictureBox1.Image = original;
            }
        }
    }
}
