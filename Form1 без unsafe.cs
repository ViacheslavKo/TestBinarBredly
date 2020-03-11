using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TestBinarBredly
{
    public partial class Form1 : Form
    {
        Bitmap imageOriginal, imageBinariz;
        int width, height, d;
        double[,] imageIntegral;
        byte[,] imageBinariz0or1, massByteImageOriginal;

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

                    massByteImageOriginal = new byte[width, height];
                    imageIntegral = new double[width + 1, height + 1];
                    imageBinariz0or1 = new byte[width, height];

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

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            await Task.Run(() => BitmapToByteArray());
            await Task.Run(() => CreateIntegralImage());
            await Task.Run(() => StartBradlyBinarization());
            pictureBox1.Image = GetByteArrayToBitmap();

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            label7.Text = ts.Seconds + ":" + ts.Milliseconds;

            Binarization.Enabled = true;
            Open.Enabled = true;
            saveBinariz.Enabled = true;
            SetStatusAsync("Процесс бинарицации завершен.");
        }

        public void BitmapToByteArray()
        {
            BitmapData bmpData = imageOriginal.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);// PixelFormat.Format8bppIndexed
            IntPtr ptr = bmpData.Scan0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    massByteImageOriginal[j, i] = Marshal.ReadByte(ptr);
                    ptr += 0x01;
                }
            }
            imageOriginal.UnlockBits(bmpData);
        }

        public Bitmap GetByteArrayToBitmap()
        {
            Bitmap picOut = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = picOut.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            IntPtr ptr = bmpData.Scan0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Marshal.WriteByte(ptr, imageBinariz0or1[j, i]);
                    ptr += 0x01;
                }
            }
            picOut.UnlockBits(bmpData);
            return picOut;
        }

        public void CreateIntegralImage()
        {
            for (int i = 1; i <= width; i++)
            {
                for (int j = 1; j <= height; j++)
                {
                    imageIntegral[i, j] = massByteImageOriginal[i - 1, j - 1] + imageIntegral[i - 1, j] + imageIntegral[i, j - 1] - imageIntegral[i - 1, j - 1];
                }
            }
        }

        public double GetSrRectangleSum(int x1, int y1, int x2, int y2)
        {
            double sumLmObl = imageIntegral[x2, y2] + imageIntegral[x1, y1] - imageIntegral[x2, y1] - imageIntegral[x1, y2];
            double srObl = sumLmObl / (d * d);
            double procentOt_srObl = srObl * ((double)numericUpDown2.Value / 100);
            return srObl - procentOt_srObl;
        }

        public void StartBradlyBinarization()
        {
            int d2 = d / 2;
            for (int i = 0; i < width; i++)
            {
                //progressBar1.Invoke(new Action(() => { progressBar1.Value = i; }));

                int x1 = i - d2;
                int x2 = i + d2;
                if (x1 < 0)
                    x1 = 0;
                if (x2 >= width)
                    x2 = width - 1;

                for (int j = 0; j < height; j++)
                {
                    int y1 = j - d2;
                    int y2 = j + d2;
                    if (y1 < 0)
                        y1 = 0;
                    if (y2 >= height)
                        y2 = height - 1;

                    if (massByteImageOriginal[i, j] < GetSrRectangleSum(x1, y1, x2, y2))
                    {
                        imageBinariz0or1[i, j] = 0x00;// 0x00 Black
                    }
                    else
                    {
                        imageBinariz0or1[i, j] = 0xFF;// 0xFF White
                    }
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

