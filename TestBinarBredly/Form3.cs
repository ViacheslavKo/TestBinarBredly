using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace TestBinarBredly
{
    public partial class TestImageCoef : Form
    {
        public TestImageCoef()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            AddPictureBox();
            LabelText();
        }

        CancellationTokenSource cancelTokenStatus = new CancellationTokenSource();
        CancellationToken tokenStatus;
        BinarBradly photoObj = new BinarBradly();
        Label[] labels = new Label[12];
        PictureBox[] pictureBox = new PictureBox[12];

        void SetStatusAsync(string Message, bool Hide = true)
        {
            cancelTokenStatus.Cancel();
            Task.Run(() =>
            {
                Thread.Sleep(200);
                cancelTokenStatus = new CancellationTokenSource();
                tokenStatus = cancelTokenStatus.Token;

                Action action = () => toolStripStatusLabel1.Text = Message;
                try
                {
                    if (InvokeRequired)
                        Invoke(action);
                    else
                        action();

                    if (Hide)
                    {
                        DateTime DT = DateTime.Now.AddSeconds(5);

                        while (DT.Subtract(DateTime.Now).TotalMilliseconds > 0 && !cancelTokenStatus.IsCancellationRequested)
                            Thread.Sleep(100);

                        if (!cancelTokenStatus.IsCancellationRequested)
                        {
                            action = () => toolStripStatusLabel1.Text = string.Empty;
                            if (InvokeRequired)
                                Invoke(action);
                            else
                                action();
                        }
                    }
                }
                catch { }
            });
        }

        private void AddPictureBox()
        {
            for (int i = 0; i < labels.Length; i++)
            {
                pictureBox[i] = new PictureBox();
                pictureBox[i].Name = "pictureBox" + i;
                pictureBox[i].SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox[i].Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

                tableLayoutPanel2.Controls.Add(pictureBox[i]);
            }
        }

        private void LabelText()
        {
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = new Label();
                labels[i].Name = "Label" + i;
                labels[i].Font = new Font("Microsoft YaHei", 10);
                labels[i].BackColor = Color.White;
                labels[i].BorderStyle = BorderStyle.FixedSingle;
                labels[i].Text = "D = 0, % = 0.";
                labels[i].Location = new Point(0, 0);
                labels[i].AutoSize = true;

                pictureBox[i].Controls.Add(labels[i]);
            }
        }

        private void Open_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Image Files(*.BMP;)(*.JPG;)|*.BMP;*.JPG;|All files (*.*)|*.*";
            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //photoObj = new BinarBradly(new Bitmap(open_dialog.FileName));
                    photoObj.SetImageOrig(new Bitmap(open_dialog.FileName));
                    start.Enabled = true;
                }
                catch
                {
                    DialogResult rezult = MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void start_Click(object sender, EventArgs e)
        {
            SetStatusAsync("Процесс обработки запущен.", false);
            Open.Enabled = false;
            start.Enabled = false;
            await TestImageView();
            start.Enabled = true;
            Open.Enabled = true;
            SetStatusAsync("Процесс обработки завершен.");
        }

        private async Task TestImageView()
        {
            int otD = (int)numericUpDown1.Value;
            int otProc = (int)numericUpDown2.Value;
            int n = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    photoObj.SetOblastD(otD);
                    photoObj.SetProcent(otProc);
                    await Task.Run(() => photoObj.StartBradlyBinar());
                    labels[n].Text = $"D = {otD}, % = {otProc}.";
                    pictureBox[n++].Image = photoObj.GetImageBinariz;
                    otProc += (int)numericUpDown4.Value;
                }
                otD += (int)numericUpDown3.Value;
                otProc = (int)numericUpDown2.Value;
            }
        }
    }
}
