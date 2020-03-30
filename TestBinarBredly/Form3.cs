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
        private Bitmap image;
        private CancellationTokenSource cancelTokenStatus = new CancellationTokenSource();
        private CancellationToken tokenStatus;
        private BinarBradly photoObj = new BinarBradly();
        private int lenght = 0;
        private Label[] labels;
        private PictureBox[] pictureBox;
        private Button[] buttons;
        private Parametrs[] parametr;
        private bool isWork = false;
        private BinarBradly[] beginWindow, oneWindow, twoWindow, thrWindow;// массивы с экранами фото
        private StatusAnaliz statusAnaliz = StatusAnaliz.isEmpty;// статус для проверки можно ли добавить новый лист
        private ListPhoto listOpen = ListPhoto.isEmpty;// оображенный лист фото на форме в данный момент
        private ListPhoto quantityScreen = ListPhoto.isEmpty;// кол-во готовых листов с фото которые можно отобразить

        // конст. для кнопки начать анализ
        private const int beginD_Analiz = 1;
        private const int beginProc_Analiz = 1;
        private const int shagD_Analiz = 7;
        private const int shagProc_Analiz = 6;

        // то с каким шагом будут обрабатываться будущие экраны. по верт. и гориз. шаги одинаковы
        private const int shagOne = 4;
        private const int shagTwo = 2;
        private const int shagThr = 1;

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

        private void SetStatusAsync(string Message, bool Hide = true)
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
            InitMassivWindow();
        }

        private void TestImageCoef_FormClosing(object sender, FormClosingEventArgs e)
        {
            InitMassivWindow();
        }

        /// <summary>
        /// Проинициализировать массивы харанящие экраны изображений.
        /// </summary>
        private void InitMassivWindow()
        {
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
                buttons[i].Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                buttons[i].Location = new Point(180, 230);
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
                    listOpen = ListPhoto.isEmpty;
                    quantityScreen = ListPhoto.isEmpty;
                    statusAnaliz = StatusAnaliz.isEmpty;
                    start.Enabled = true;
                    Analiz.Enabled = true;
                    nextAnaliz.Enabled = false;
                    backAnaliz.Enabled = false;
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
            InitMassivWindow();
            listOpen = ListPhoto.begin;
            quantityScreen = ListPhoto.begin;
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
                nextAnaliz.Enabled = false;
                backAnaliz.Enabled = false;
                await TestImageView(beginD, beginProc, shagD, shagProc);
                start.Enabled = true;
                Open.Enabled = true;
                Analiz.Enabled = true;
                CheckList();
                SetStatusAsync("Процесс обработки завершен.");
                statusAnaliz = StatusAnaliz.isEmpty;
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

                    switch (listOpen)
                    {
                        case ListPhoto.begin:
                            {
                                beginWindow[n] = photoObj;
                            }
                            break;
                        case ListPhoto.one:
                            {
                                oneWindow[n] = photoObj;
                            }
                            break;
                        case ListPhoto.two:
                            {
                                twoWindow[n] = photoObj;
                            }
                            break;
                        case ListPhoto.thr:
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
        /// Начать анализ без ввода параметров. (Если надо изменить грбый шаг с кнопки анализ, то делать это в const)
        /// </summary>
        private void Analiz_Click(object sender, EventArgs e)
        {
            InitMassivWindow();
            listOpen = ListPhoto.begin;
            quantityScreen = ListPhoto.begin;
            Start(beginD_Analiz, beginProc_Analiz, shagD_Analiz, shagProc_Analiz);
        }

        /// <summary>
        /// Анализировать с другим шагов вокрух выбранного фото.
        /// </summary>
        private void Button_Click(object sender, EventArgs e)
        {
            int nom = Convert.ToInt32((sender as Button).Name);

            if (!isWork && photoObj.GetStatus == StatusBinar.completed && parametr[nom].Area != 1 &&
                        statusAnaliz != StatusAnaliz.inProcess)
            {
                switch (listOpen)
                {
                    case ListPhoto.begin:
                        {
                            listOpen = ListPhoto.one;
                            quantityScreen = ListPhoto.one;
                            Start(parametr[nom].Area - shagOne, parametr[nom].Bright - shagOne * 2, shagOne, shagOne);
                        }
                        break;
                    case ListPhoto.one:
                        {
                            listOpen = ListPhoto.two;
                            quantityScreen = ListPhoto.two;
                            Start(parametr[nom].Area - shagTwo, parametr[nom].Bright - shagTwo * 2, shagTwo, shagTwo);
                        }
                        break;
                    case ListPhoto.two:
                        {
                            listOpen = ListPhoto.thr;
                            quantityScreen = ListPhoto.thr;
                            Start(parametr[nom].Area - shagThr, parametr[nom].Bright - shagThr * 2, shagThr, shagThr);
                        }
                        break;
                    case ListPhoto.thr:
                        {
                            Start(parametr[nom].Area - shagThr, parametr[nom].Bright - shagThr * 2, shagThr, shagThr);
                        }
                        break;
                }
            }

            if (!isWork && photoObj.GetStatus == StatusBinar.completed && parametr[nom].Area == 1 &&
                        statusAnaliz != StatusAnaliz.inProcess)
            {
                switch (listOpen)
                {
                    case ListPhoto.begin:
                        {
                            listOpen = ListPhoto.one;
                            quantityScreen = ListPhoto.one;
                            Start(parametr[nom].Area, parametr[nom].Bright - shagOne * 2, shagOne, shagOne);
                        }
                        break;
                    case ListPhoto.one:
                        {
                            listOpen = ListPhoto.two;
                            quantityScreen = ListPhoto.two;
                            Start(parametr[nom].Area, parametr[nom].Bright - shagTwo * 2, shagTwo, shagTwo);
                        }
                        break;
                    case ListPhoto.two:
                        {
                            listOpen = ListPhoto.thr;
                            quantityScreen = ListPhoto.thr;
                            Start(parametr[nom].Area, parametr[nom].Bright - shagThr * 2, shagThr, shagThr);
                        }
                        break;
                    case ListPhoto.thr:
                        {
                            Start(parametr[nom].Area, parametr[nom].Bright - shagThr * 2, shagThr, shagThr);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Показать лист.
        /// </summary>
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
            if (statusAnaliz != StatusAnaliz.inProcess)
            {
                switch (listOpen)
                {
                    case ListPhoto.one:
                        {
                            listOpen = ListPhoto.begin;
                            ViewList(beginWindow);
                        }
                        break;
                    case ListPhoto.two:
                        {
                            listOpen = ListPhoto.one;
                            ViewList(oneWindow);
                        }
                        break;
                    case ListPhoto.thr:
                        {
                            listOpen = ListPhoto.two;
                            ViewList(twoWindow);
                        }
                        break;
                }
            }
            CheckList();
        }

        /// <summary>
        /// Вперед на шаг если есть листы с фото.
        /// </summary>
        private void nextAnaliz_Click(object sender, EventArgs e)
        {
            if (quantityScreen >= (listOpen + 1) && statusAnaliz != StatusAnaliz.inProcess)
            {
                switch (listOpen)
                {
                    case ListPhoto.begin:
                        {
                            listOpen = ListPhoto.one;
                            ViewList(oneWindow);
                        }
                        break;
                    case ListPhoto.one:
                        {
                            listOpen = ListPhoto.two;
                            ViewList(twoWindow);
                        }
                        break;
                    case ListPhoto.two:
                        {
                            listOpen = ListPhoto.thr;
                            ViewList(thrWindow);
                        }
                        break;
                }
            }
            CheckList();
        }

        /// <summary>
        /// Для правильного отображения кнопок вперед назад.
        /// </summary>
        private void CheckList()
        {
            if (quantityScreen > listOpen)
            {
                nextAnaliz.Enabled = true;
            }
            else
            {
                nextAnaliz.Enabled = false;
            }

            if (listOpen != ListPhoto.begin)
            {
                backAnaliz.Enabled = true;
            }
            else
            {
                backAnaliz.Enabled = false;
            }
        }
    }

    /// <summary>
    /// Для хранения D и %.
    /// </summary>
    public class Parametrs
    {
        public int Area { get; set; }
        public int Bright { get; set; }
    }

    /// <summary>
    /// Статус работы анализа.
    /// </summary>
    [Flags]
    public enum StatusAnaliz
    {
        /// <summary>
        /// Не запущена обработка фото.
        /// </summary>
        isEmpty = 0,
        /// <summary>
        /// Запущена обработка фото.
        /// </summary>
        inProcess = 1,
    };

    /// <summary>
    /// Для статуса открытого листа.
    /// </summary>
    [Flags]
    public enum ListPhoto
    {
        /// <summary>
        /// Нет ни одного экрана с фото.
        /// </summary>
        isEmpty = -1,
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
