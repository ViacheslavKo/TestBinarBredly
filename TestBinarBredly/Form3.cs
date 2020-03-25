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
            InitFormVisual();
            AddPictureBox();
            AddLabel();
            AddButton();
        }

        CancellationTokenSource cancelTokenStatus = new CancellationTokenSource();
        CancellationToken tokenStatus;
        BinarBradly photoObj = new BinarBradly();
        int lenght = 0;
        Label[] labels;
        PictureBox[] pictureBox;
        Button[] buttons;
        Parametrs[] parametr;
        bool isWork = false;

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

        private void InitFormVisual()
        {
            lenght = tableLayoutPanel2.RowCount * tableLayoutPanel2.ColumnCount;

            parametr = new Parametrs[lenght];
            labels = new Label[lenght];
            pictureBox = new PictureBox[lenght];
            buttons = new Button[lenght];
        }

        private void AddPictureBox()
        {
            for (int i = 0; i < lenght; i++)
            {
                pictureBox[i] = new PictureBox();
                pictureBox[i].Name = "pictureBox" + i;
                pictureBox[i].SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox[i].Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

                tableLayoutPanel2.Controls.Add(pictureBox[i]);
                pictureBox[i].Click += new EventHandler(PictureBox_Click);
            }
        }

        private void AddLabel()
        {
            for (int i = 0; i < lenght; i++)
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

        private void AddButton()
        {
            for (int i = 0; i < lenght; i++)
            {
                buttons[i] = new Button();
                buttons[i].Size = new Size(105, 27);
                //buttons[i].Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                //buttons[i].Dock = DockStyle.Bottom;
                buttons[i].Location = new Point(0, 220);
                buttons[i].UseVisualStyleBackColor = true;
                buttons[i].Font = new Font("Microsoft YaHei", 10);
                buttons[i].Name = Convert.ToString(i);
                buttons[i].Text = "Исследовать";

                pictureBox[i].Controls.Add(buttons[i]);
                buttons[i].Click += new EventHandler(Button_Click);
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
                    Analiz.Enabled = true;
                }
                catch
                {
                    DialogResult rezult = MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void start_Click(object sender, EventArgs e)
        {
            Start((int)numericUpDown1.Value, (int)numericUpDown2.Value, (int)numericUpDown3.Value, (int)numericUpDown4.Value);
        }

        //BinarBradly[] testPhoto = new BinarBradly[12];
        //private void TestImage()
        //{
        //    int otD = (int)numericUpDown1.Value;
        //    int otProc = (int)numericUpDown2.Value;
        //    int n = 0;

        //    for (int i = 0; i < 3; i++)
        //    {
        //        for (int j = 0; j < 4; j++)
        //        {
        //            testPhoto[n] = new BinarBradly(photoObj.GetImageOrig);
        //            testPhoto[n].SetOblastD(otD);
        //            testPhoto[n].SetProcent(otProc);
        //            Task.Run(() => testPhoto[n].StartBradlyBinar());
        //            otProc += (int)numericUpDown4.Value;
        //            n++;
        //        }
        //        otD += (int)numericUpDown3.Value;
        //        otProc = (int)numericUpDown2.Value;
        //    }
        //}

        //private void ImageViev(object sender, EventArgs e)
        //{
        //    //for (int i = 0; i < 12; i++)
        //    //{
        //    //    pictureBox[i].Image = testPhoto[i].GetImageBinariz;
        //    //}
        //}

        private async void Start(int beginD, int beginProc, int shagD, int shagProc)
        {
            if (!isWork)
            {
                isWork = true;
                SetStatusAsync("Процесс обработки запущен.", false);
                Open.Enabled = false;
                start.Enabled = false;
                Analiz.Enabled = false;
                await TestImageView(beginD, beginProc, shagD, shagProc);
                start.Enabled = true;
                Open.Enabled = true;
                Analiz.Enabled = true;
                SetStatusAsync("Процесс обработки завершен.");
                isWork = false;
            }
            else
                MessageBox.Show("Уже запущена обработка.\nПодождите завершения обработки фото.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private async Task TestImageView(int beginD, int beginProc, int shagD, int shagProc)
        {
            int otProc2 = beginProc;
            int n = 0;

            for (int i = 0; i < tableLayoutPanel2.RowCount; i++)
            {
                for (int j = 0; j < tableLayoutPanel2.ColumnCount; j++)
                {
                    parametr[n] = new Parametrs { Area = beginD, Bright = otProc2 };
                    photoObj.SetOblastD(parametr[n].Area);
                    photoObj.SetProcent(parametr[n].Bright);
                    await Task.Run(() => photoObj.StartBradlyBinar());
                    labels[n].Text = $"D = {parametr[n].Area}, % = {parametr[n].Bright}.";
                    pictureBox[n++].Image = photoObj.GetImageBinariz;
                    otProc2 += shagProc;
                }
                beginD += shagD;
                otProc2 = beginProc;
            }
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            PictureBox picBox = sender as PictureBox;
            FormScreen Screen = new FormScreen();
            if (!isWork && photoObj.GetStatus == StatusBinar.completed)
            {
                Screen.SetImage((Bitmap)picBox.Image, photoObj.GetImageOrig);
                Screen.ShowDialog();
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            int nom = Convert.ToInt32((sender as Button).Name);
            if (!isWork && photoObj.GetStatus == StatusBinar.completed && parametr[nom].Area != 1)
            {
                Start(parametr[nom].Area - 1, parametr[nom].Bright - 2, 1, 1);
            }
            else
                return;
        }

        private void Analiz_Click(object sender, EventArgs e)
        {
            Start(2, 1, 5, 5);
        }
    }

    public class Parametrs
    {
        //public int Name { get; set; }
        public int Area { get; set; }
        public int Bright { get; set; }
    }
}
