﻿using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TestBinarBredly
{
    public partial class Form1 : Form
    {
        Bitmap imageOriginal, bitmapImageBinariz;
        int width, height, d;
        double[,] imageIntegral;
        byte[] imageBinarizByte;

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

                    imageIntegral = new double[height + 1, width + 1];
                    imageBinarizByte = new byte[height * width];
                    pictureBox1.Image = imageOriginal;

                    Binarization.Enabled = true;
                    saveBinariz.Enabled = true;
                    label4.Text = "Ширина: " + width + " px";
                    label5.Text = "Высота: " + height + " px";
                    progressBar1.Value = 0;
                    progressBar1.Maximum = height;
                }
                catch
                {
                    MessageBox.Show("Невозможно открыть выбранный файл", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void saveBinariz_Click(object sender, EventArgs e)
        {
            SaveFileDialog save_dialog = new SaveFileDialog();
            save_dialog.Filter = "Image Files(*.Bmp;)(*.Jpg;)|*.Bmp;*.Jpg;|All files (*.*)|*.*";
            if (save_dialog.ShowDialog() == DialogResult.OK)
            {
                bitmapImageBinariz.Save(save_dialog.FileName);
            }
        }

        private async void Binarization_Click(object sender, EventArgs e)
        {
            SetStatusAsync("Процесс бинарицации запущен. Ждите...", false);
            progressBar1.Value = 0;
            d = width / (int)numericUpDown1.Value;//4 - большое изображение, 8 - маленькое
            saveBinariz.Enabled = false;
            Open.Enabled = false;
            Binarization.Enabled = false;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            await Task.Run(() => GetIntegralImage());
            await Task.Run(() => BradlyBinarization());
            bitmapImageBinariz = ByteArrayToBitmap(imageBinarizByte, width, height);
            pictureBox1.Image = bitmapImageBinariz;

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            label7.Text = ts.Seconds + ":" + ts.Milliseconds;

            Binarization.Enabled = true;
            Open.Enabled = true;
            saveBinariz.Enabled = true;
            SetStatusAsync("Процесс бинарицации завершен.");
        }

        public Bitmap ByteArrayToBitmap(byte[] byteIn, int imwidth, int imheight)
        {
            Bitmap picOut = new Bitmap(imwidth, imheight, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            System.Drawing.Imaging.BitmapData bmpData = picOut.LockBits(new Rectangle(0, 0, imwidth, imheight), 
                System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            IntPtr ptr = bmpData.Scan0;
            Int32 psize = bmpData.Stride * imheight;
            System.Runtime.InteropServices.Marshal.Copy(byteIn, 0, ptr, psize);
            picOut.UnlockBits(bmpData);
            return picOut;
        }

        public double GetSrRectangleSum(int x1, int y1, int x2, int y2)
        {
            double sumLmObl = imageIntegral[x2, y2] + imageIntegral[x1, y1] - imageIntegral[x2, y1] - imageIntegral[x1, y2];
            double srObl = sumLmObl / (d * d);
            double procentOt_srObl = srObl * ((double)numericUpDown2.Value / 100);
            return srObl - procentOt_srObl;
        }

        public void GetIntegralImage()
        {
            System.Drawing.Imaging.BitmapData bdImageOrig = imageOriginal.LockBits(new Rectangle(0, 0, width, height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);//imageOriginal.PixelFormat
            unsafe
            {
                byte* curpos = ((byte*)bdImageOrig.Scan0);
                for (int i = 1; i <= height; i++)
                {
                    for (int j = 1; j <= width; j++)
                    {
                        double ree = *curpos++;
                        imageIntegral[i, j] = ree + imageIntegral[i - 1, j] + imageIntegral[i, j - 1] - imageIntegral[i - 1, j - 1];
                    }
                }
                imageOriginal.UnlockBits(bdImageOrig);
            }
        }

        public void BradlyBinarization()
        {
            System.Drawing.Imaging.BitmapData bdImageOrig = imageOriginal.LockBits(new Rectangle(0, 0, width, height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            unsafe
            {
                int scht = 0;
                byte* curpos = ((byte*) bdImageOrig.Scan0);
                int d2 = d / 2;
                for (int i = 0; i < height; i++)
                {
                    //progressBar1.Invoke(new Action(() => { progressBar1.Value = i; }));

                    int x1 = i - d2;
                    int x2 = i + d2;
                    if (x1 < 0)
                        x1 = 0;
                    if (x2 >= height)
                        x2 = height - 1;

                    for (int j = 0; j < width; j++)
                    {
                        int y1 = j - d2;
                        int y2 = j + d2;
                        if (y1 < 0)
                            y1 = 0;
                        if (y2 >= width)
                            y2 = width - 1;

                        if (*curpos++ < GetSrRectangleSum(x1, y1, x2, y2))
                        {
                            imageBinarizByte[scht] = 0x00;
                            //imageBinariz0or1[i, j] = 0;//0x00 Black
                        }
                        else
                        {
                            imageBinarizByte[scht] = 0xFF;
                            //imageBinariz0or1[i, j] = 1;//0xFF White
                        }
                        scht++;
                    }
                }
                imageOriginal.UnlockBits(bdImageOrig);
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

