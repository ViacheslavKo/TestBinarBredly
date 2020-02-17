using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace TestBinarBredly
{
    public partial class Form1 : Form
    {
        Bitmap imageOriginal, imageBinariz;
        int width, height, d;
        double[,] imageIntegral;

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
                    imageOriginal = new Bitmap(open_dialog.FileName);

                    width = imageOriginal.Width;
                    height = imageOriginal.Height;
                    imageIntegral = new double[imageOriginal.Width + 1, imageOriginal.Height + 1];

                    pictureBox1.Image = imageOriginal;

                    Binarization.Enabled = true;
                    saveBinariz.Enabled = true;
					
					label4.Text = "Ширина: " + width + " px";
                    label5.Text = "Высота: " + height + " px";
					progressBar1.Maximum = width;
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
                imageBinariz.Save(save_dialog.FileName);
            }
        }

        private async void Binarization_Click(object sender, EventArgs e)
        {
            SetStatusAsync("Процесс бинарицации запущен. Ждите...", false);
            imageBinariz = new Bitmap(width, height);
            d = width / (int)numericUpDown1.Value;//4 - большое изображение, 8 - маленькое
            saveBinariz.Enabled = false;
            Open.Enabled = false;
            Binarization.Enabled = false;
            await Task.Run(() => GetIntegralImage());
            await Task.Run(() => BradlyBinarization());
            pictureBox1.Image = imageBinariz;
            Binarization.Enabled = true;
            Open.Enabled = true;
            saveBinariz.Enabled = true;
            SetStatusAsync("Процесс бинарицации завершен.");
        }

        public void GetIntegralImage()
        {
            for (int i = 1; i <= width; i++)
            {
                for (int j = 1; j <= height; j++)
                {
                    double pixel = imageOriginal.GetPixel(i - 1, j - 1).GetBrightness();
                    imageIntegral[i, j] = pixel + imageIntegral[i - 1, j] + imageIntegral[i, j - 1] - imageIntegral[i - 1, j - 1];
                }
            }
        }

        public double GetSrRectangleSum(int x, int y)
        {
            double sumLmObl = imageIntegral[x, y] + imageIntegral[x - d, y - d] - imageIntegral[x, y - d] - imageIntegral[x - d, y];
            double srObl = sumLmObl / (d * d);
            double procentOt_srObl = srObl * ((double)numericUpDown2.Value / 100);
            return srObl - procentOt_srObl;
        }

        public void BradlyBinarization()
        {
            for (int i = 1; i <= width; i++)
            {
				progressBar1.Invoke(new Action(() => { progressBar1.Value = i; }));
                for (int j = 1; j <= height; j++)
                {
                    if (i % d == 0 && j % d == 0)
                    {
                        OblastBinarization(i, j);
                    }
                }
            }

            if (height % d != 0 && d != width)
            {
                for (int i = d; i <= width; i += d)
                {
                    OblastBinarization(i, height);
                }
            }
        }

        public void OblastBinarization(int x, int y)
        {
            double porogVelich = GetSrRectangleSum(x, y);
            for (int i = x - d + 1; i <= x; i++)
            {
                for (int j = y - d + 1; j <= y; j++)
                {
                    double pixel = imageOriginal.GetPixel(i - 1, j - 1).GetBrightness();
                    if (pixel < porogVelich)
                        imageBinariz.SetPixel(i - 1, j - 1, Color.Black);
                    else
                        imageBinariz.SetPixel(i - 1, j - 1, Color.White);
                }
            }
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

    }
}

