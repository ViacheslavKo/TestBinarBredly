using System;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace TestBinarBredly
{
    public partial class ProfSetting : Form
    {
        private UserProfil profil;
        private CancellationTokenSource cancelTokenStatus = new CancellationTokenSource();
        private CancellationToken tokenStatus;

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

        public ProfSetting()
        {
            InitializeComponent();
        }

        private void ProfSetting_Load(object sender, EventArgs e)
        {
            //if (UserProfil.LoadProfils())
            //{
            //    SetStatusAsync("Найдены профили настроек.");
            VisualProfils();
            RefreshComboBox();
            //}
            //else
            //    SetStatusAsync("Не найдено ни одного профиля с настройками.");

            //VisualProfils();
            //if (textBox4.Text != String.Empty)
            //{
            //    SetStatusAsync("Найдены профили с настройками.");
            //    RefreshComboBox();
            //}
            //else
            //    SetStatusAsync("Не найдено ни одного профиля с настройками.");
        }


        private void RefreshComboBox()
        {
            Task.Run(() =>
            {
                comboBox1.Invoke(new Action(() => { comboBox1.Items.Clear(); }));
                if (BinarBradly.GetSettingList.Count > 0)
                {
                    foreach (UserProfil profil in BinarBradly.GetSettingList)
                    {
                        comboBox1.Invoke(new Action(() => { comboBox1.Items.Add(profil.Name); }));
                    }
                }
            });
        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (CheckNullOfSpace())
            {
                if (BinarBradly.AddSetting(textBox1.Text, Convert.ToInt32(textBox2.Text), Convert.ToDouble(textBox3.Text)))
                {
                    SetStatusAsync($"Профиль №{textBox1.Text} добавлен.");
                    Task.Run(() => VisualProfils());
                    RefreshComboBox();
                }
                else
                    SetStatusAsync($"Профиль №{textBox1.Text} уже существует.");
            }
            else
                SetStatusAsync($"Заполнены не все поля.");
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            if (CheckNullOfSpace() && BinarBradly.EditSetting(textBox1.Text, Convert.ToInt32(textBox2.Text), Convert.ToDouble(textBox3.Text)))
            {
                SetStatusAsync($"Профиль №{textBox1.Text} изменен.");
                Task.Run(() => VisualProfils());
                RefreshComboBox();
            }
            else
                SetStatusAsync($"Профиль №{textBox1.Text} не найден.");
        }

        private void Dell_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(textBox1.Text) && BinarBradly.DelSetting(textBox1.Text))
            {
                SetStatusAsync($"Профиль №{textBox1.Text} удален.");
                Task.Run(() => VisualProfils());
                RefreshComboBox();
            }
            else
                SetStatusAsync($"Профиль №{textBox1.Text} не найден.");
        }

        private void VisualProfils()
        {
            Action action = () => textBox4.Clear();
            if (InvokeRequired)
                Invoke(action);
            else
                action();

            BinarBradly.GetSettingList.Sort(UserProfil.CompareProfils);

            foreach (UserProfil profil in BinarBradly.GetSettingList)
            {
                Action act = () => textBox4.Text += $"Профиль № {profil.Name}: область D = {profil.Area}, порог яркости = {profil.ThresholdBright}%." + Environment.NewLine;
                if (InvokeRequired)
                    Invoke(act);
                else
                    act();
            }
        }

        private bool CheckNullOfSpace()
        {
            if (String.IsNullOrWhiteSpace(textBox1.Text) || String.IsNullOrWhiteSpace(textBox2.Text) ||
                String.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Заполнены не все поля", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(comboBox1.Text))
            {
                profil = BinarBradly.GetSettingList.FirstOrDefault(x => x.Name == comboBox1.Text);

                textBox1.Text = profil.Name;
                textBox2.Text = Convert.ToString(profil.Area);
                textBox3.Text = Convert.ToString(profil.ThresholdBright);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (UserProfil.SaveProfils(BinarBradly.GetSettingList))
            {
                MessageBox.Show("Настройки сохранены.", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении.", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
