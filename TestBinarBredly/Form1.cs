using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;

using System.Diagnostics;

namespace TestBinarBredly
{
    public partial class Form1 : Form
    {
        BinarBradly photoObj = new BinarBradly();

        CancellationTokenSource cancelTokenStatus = new CancellationTokenSource();
        CancellationToken tokenStatus;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Open_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Image Files(*.BMP;)(*.JPG;)|*.BMP;*.JPG;|All files (*.*)|*.*";
            if (open_dialog.ShowDialog() == DialogResult.OK) //если в окне была нажата кнопка "ОК"
            {
                try
                {
                    photoObj.SetImageOrig(new Bitmap(open_dialog.FileName));
                    pictureBox1.Image = photoObj.GetImageOrig;

                    label4.Text = "Ширина: " + photoObj.Width + " px";
                    label5.Text = "Высота: " + photoObj.Height + " px";
                    Binarization.Enabled = true;
                    saveBinariz.Enabled = false;
                    button1.Enabled = false;
                }
                catch
                {
                    DialogResult rezult = MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void saveBinariz_Click(object sender, EventArgs e)
        {
            SaveFileDialog save_dialog = new SaveFileDialog();
            save_dialog.Filter = "Image Files(*.Bmp;)(*.Jpg;)|*.Bmp;*.Jpg;|All files (*.*)|*.*";
            if (save_dialog.ShowDialog() == DialogResult.OK)
            {
                photoObj.GetImageBinariz.Save(save_dialog.FileName);
            }
        }

        private async void Binarization_Click(object sender, EventArgs e)
        {
            SetStatusAsync("Процесс бинарицации запущен. Ждите...", false);
            saveBinariz.Enabled = false;
            Binarization.Enabled = false;
            Open.Enabled = false;
            button1.Enabled = false;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            photoObj.SetOblastD((int)numericUpDown1.Value);
            photoObj.SetProcent((double)numericUpDown2.Value);
            await photoObj.StartBradlyBinar();
            pictureBox1.Image = photoObj.GetImageBinariz;

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            label7.Text = ts.Seconds + ":" + ts.Milliseconds;

            button1.Text = "Оригинал";
            button1.Enabled = true;
            Binarization.Enabled = true;
            saveBinariz.Enabled = true;
            Open.Enabled = true;
            SetStatusAsync("Процесс бинарицации завершен.");
        }

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

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Оригинал")
            {
                pictureBox1.Image = photoObj.GetImageOrig;
                button1.Text = "Обработанное";
            }
            else
            {
                pictureBox1.Image = photoObj.GetImageBinariz;
                button1.Text = "Оригинал";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BinarBradly.AddSetting(textBox1.Text, Convert.ToInt32(textBox2.Text), Convert.ToDouble(textBox3.Text));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BinarBradly.DelSetting(textBox1.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            photoObj.GetSetting(textBox1.Text);
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            SetStatusAsync("Процесс бинарицации запущен. Ждите...", false);
            saveBinariz.Enabled = false;
            Binarization.Enabled = false;
            Open.Enabled = false;
            button1.Enabled = false;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            await photoObj.StartBradlyBinar();
            pictureBox1.Image = photoObj.GetImageBinariz;

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            label7.Text = ts.Seconds + ":" + ts.Milliseconds;

            button1.Text = "Оригинал";
            button1.Enabled = true;
            Binarization.Enabled = true;
            saveBinariz.Enabled = true;
            Open.Enabled = true;
            SetStatusAsync("Процесс бинарицации завершен.");
        }
    }
}

