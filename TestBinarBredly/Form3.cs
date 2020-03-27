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

        Bitmap image;
        CancellationTokenSource cancelTokenStatus = new CancellationTokenSource();
        CancellationToken tokenStatus;
        BinarBradly photoObj = new BinarBradly();
        int lenght = 0;
        Label[] labels;
        PictureBox[] pictureBox;
        Button[] buttons;
        Parametrs[] parametr;
        bool isWork = false;
        StatusAnaliz statusAnaliz = StatusAnaliz.isEmpty;
        BinarBradly[] beginWindow, oneWindow, twoWindow, thrWindow;

        const int beginD_Analiz = 1;
        const int beginProc_Analiz = 1;
        const int shagD_Analiz = 7;
        const int shagProc_Analiz = 6;

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

        /// <summary>
        /// Создать массивы объектов длиной кол-ва окон в tableLayoutPanel2.
        /// </summary>
        private void InitFormVisual()
        {
            lenght = tableLayoutPanel2.RowCount * tableLayoutPanel2.ColumnCount;

            parametr = new Parametrs[lenght];
            labels = new Label[lenght];
            pictureBox = new PictureBox[lenght];
            buttons = new Button[lenght];
            beginWindow = new BinarBradly[lenght];
            oneWindow = new BinarBradly[lenght];
            twoWindow = new BinarBradly[lenght];
            thrWindow = new BinarBradly[lenght];
        }

        /// <summary>
        /// Вставка PictureBox.
        /// </summary>
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

        /// <summary>
        /// Вставка Label.
        /// </summary>
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

        /// <summary>
        /// Вставка Button.
        /// </summary>
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

        /// <summary>
        /// Открыть фото.
        /// </summary>
        private void Open_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Image Files(*.BMP;)(*.JPG;)|*.BMP;*.JPG;|All files (*.*)|*.*";
            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    image = new Bitmap(open_dialog.FileName);
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

        /// <summary>
        /// Открыть фото на весь экран.
        /// </summary>
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

        /// <summary>
        /// Старт с параметрами заданными вручную.
        /// </summary>
        private void start_Click(object sender, EventArgs e)
        {
            Start((int)numericUpDown1.Value, (int)numericUpDown2.Value, (int)numericUpDown3.Value, (int)numericUpDown4.Value);
        }

        /// <summary>
        /// Ф-я запуска обработки.
        /// </summary>
        private async void Start(int beginD, int beginProc, int shagD, int shagProc)
        {
            if (!isWork)
            {
                isWork = true;
                statusAnaliz = StatusAnaliz.inProcess;
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

        /// <summary>
        /// Запускает обработку вариантов фото.
        /// </summary>
        private async Task TestImageView(int beginD, int beginProc, int shagD, int shagProc)
        {
            int otProc2 = beginProc;
            int n = 0;

            for (int i = 0; i < tableLayoutPanel2.RowCount; i++)
            {
                for (int j = 0; j < tableLayoutPanel2.ColumnCount; j++)
                {
                    photoObj = new BinarBradly(image);
                    parametr[n] = new Parametrs { Area = beginD, Bright = otProc2 };
                    photoObj.SetOblastD(parametr[n].Area);
                    photoObj.SetProcent(parametr[n].Bright);
                    await Task.Run(() => photoObj.StartBradlyBinar());
                    labels[n].Text = $"D = {parametr[n].Area}, % = {parametr[n].Bright}.";

                    switch (statusAnaliz)
                    {
                        case StatusAnaliz.begin:
                            {
                                beginWindow[n] = photoObj;
                            }
                            break;
                        case StatusAnaliz.one:
                            {
                                oneWindow[n] = photoObj;
                            }
                            break;
                        case StatusAnaliz.two:
                            {
                                twoWindow[n] = photoObj;
                            }
                            break;
                        case StatusAnaliz.thr:
                            {
                                thrWindow[n] = photoObj;
                            }
                            break;
                    }

                    pictureBox[n++].Image = photoObj.GetImageBinariz;
                    otProc2 += shagProc;
                }
                beginD += shagD;
                otProc2 = beginProc;
            }
        }

        /// <summary>
        /// Начать анализ без ввода параметров. (Если надо изменить грбый шаг с кнопки анализ, то делать это тут)
        /// </summary>
        private void Analiz_Click(object sender, EventArgs e)
        {
            beginWindow = new BinarBradly[lenght];
            oneWindow = new BinarBradly[lenght];
            twoWindow = new BinarBradly[lenght];
            thrWindow = new BinarBradly[lenght];
            Start(beginD_Analiz, beginProc_Analiz, shagD_Analiz, shagProc_Analiz);
            statusAnaliz = StatusAnaliz.begin;
        }

        private void TestImageCoef_FormClosing(object sender, FormClosingEventArgs e)
        {
            beginWindow = new BinarBradly[lenght];
            oneWindow = new BinarBradly[lenght];
            twoWindow = new BinarBradly[lenght];
            thrWindow = new BinarBradly[lenght];
        }

        /// <summary>
        /// Анализировать с другим шагов вокрух выбранного фото.
        /// </summary>
        private void Button_Click(object sender, EventArgs e)
        {
            int nom = Convert.ToInt32((sender as Button).Name);
            if (!isWork && photoObj.GetStatus == StatusBinar.completed && parametr[nom].Area != 1 &&
                        statusAnaliz != StatusAnaliz.isEmpty && statusAnaliz != StatusAnaliz.inProcess)
            {
                switch (statusAnaliz)
                {
                    case StatusAnaliz.begin:
                        {
                            Start(parametr[nom].Area - 4, parametr[nom].Bright - 8, 4, 4);
                            statusAnaliz = StatusAnaliz.one;
                        }
                        break;
                    case StatusAnaliz.one:
                        {
                            Start(parametr[nom].Area - 2, parametr[nom].Bright - 4, 2, 2);
                            statusAnaliz = StatusAnaliz.two;
                        }
                        break;
                    case StatusAnaliz.two:
                        {
                            Start(parametr[nom].Area - 1, parametr[nom].Bright - 2, 1, 1);
                            statusAnaliz = StatusAnaliz.thr;
                        }
                        break;
                    case StatusAnaliz.thr:
                        {
                            Start(parametr[nom].Area - 1, parametr[nom].Bright - 2, 1, 1);
                            statusAnaliz = StatusAnaliz.thr;
                        }
                        break;
                }
            }
            else
                return;
        }

        private void ViewList(BinarBradly[] mass)
        {
            for (int i = 0; i < lenght; i++)
            {
                pictureBox[i].Image = mass[i].GetImageBinariz;
                parametr[i] = new Parametrs { Area = mass[i].GetArea, Bright = (int)mass[i].GetBright };
                labels[i].Text = $"D = {parametr[i].Area}, % = {parametr[i].Bright}.";

            }
        }

        /// <summary>
        /// Вернуться на шаг назад в анализе фото (показать прошлые варианты).
        /// </summary>
        private void backAnaliz_Click(object sender, EventArgs e)
        {
            if (statusAnaliz != StatusAnaliz.isEmpty && statusAnaliz != StatusAnaliz.inProcess)
            {
                switch (statusAnaliz)
                {
                    case StatusAnaliz.one:
                        {
                            ViewList(beginWindow);
                            statusAnaliz = StatusAnaliz.begin;
                        }
                        break;
                    case StatusAnaliz.two:
                        {
                            ViewList(oneWindow);
                            statusAnaliz = StatusAnaliz.one;
                        }
                        break;
                    case StatusAnaliz.thr:
                        {
                            ViewList(twoWindow);
                            statusAnaliz = StatusAnaliz.two;
                        }
                        break;
                }
            }
        }

        private void nextAnaliz_Click(object sender, EventArgs e)
        {
            if (statusAnaliz != StatusAnaliz.isEmpty && statusAnaliz != StatusAnaliz.inProcess)
            {
                switch (statusAnaliz)
                {
                    case StatusAnaliz.begin:
                        {
                            ViewList(oneWindow);
                            statusAnaliz = StatusAnaliz.one;
                        }
                        break;
                    case StatusAnaliz.one:
                        {
                            ViewList(twoWindow);
                            statusAnaliz = StatusAnaliz.two;
                        }
                        break;
                    case StatusAnaliz.two:
                        {
                            ViewList(thrWindow);
                            statusAnaliz = StatusAnaliz.thr;
                        }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Для хранения D и %.
    /// </summary>
    public class Parametrs
    {
        //public int Name { get; set; }
        public int Area { get; set; }
        public int Bright { get; set; }
    }

    /// <summary>
    /// Статус этапа анализа фото.
    /// </summary>
    [Flags]
    public enum StatusAnaliz
    {
        /// <summary>
        /// Не запущен анализ.
        /// </summary>
        isEmpty = -1,
        /// <summary>
        /// Идет обработка фото.
        /// </summary>
        inProcess = 10,

        /// <summary>
        /// Показаны изображения с грубым шагом.
        /// </summary>
        begin = 0,
        /// <summary>
        /// Первый этап.
        /// </summary>
        one = 1,
        /// <summary>
        /// Второй этап.
        /// </summary>
        two = 2,
        /// <summary>
        /// Третий этап.
        /// </summary>
        thr = 3,
    };
}
