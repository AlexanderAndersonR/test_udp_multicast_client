using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data;

namespace test_udp_multicast
{
    public partial class Form1 : Form
    {
        public Form_terminal _form_Terminal { get; set; }
        bool flag = false;
        bool flag_ = false;
        private HashSet<string> processedIds = new HashSet<string>();
        private HashSet<string> processedAI = new HashSet<string>();
        public Form1()
        {
            InitializeComponent();
            textBox2.Text = Properties.Settings.Default.ip;
            textBox3.Text = Properties.Settings.Default.port.ToString();
            comboBox1.SelectedIndex = Properties.Settings.Default.ComboBoxIndex;
        }
        delegate void SetTextDeleg(string text);
        private void si_DataReceived(string data)
        {
            if (_form_Terminal != null) 
                _form_Terminal.GetTextBox().AppendText(data + "\r\n");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(ReceiveMessageAsync);
            flag = true;
        }

        async Task ReceiveMessageAsync()
        {
            try
            {
                UdpClient receiver = new UdpClient(Properties.Settings.Default.port);
                receiver.JoinMulticastGroup(IPAddress.Parse(Properties.Settings.Default.ip));
                receiver.MulticastLoopback = false;
                while (flag)
                {
                    var result = await receiver.ReceiveAsync();
                    var message_2 = Encoding.UTF8.GetChars(result.Buffer);
                    string message = new string(message_2);
                    message = message.Replace("\0", @"\0");
                    BeginInvoke(new SetTextDeleg(si_DataReceived), message);

                    ProcessReceivedMessage(message);
                }
                receiver.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                flag = false;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            flag = false;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            flag = false;
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.port = Convert.ToInt32(textBox3.Text);
            Properties.Settings.Default.Save();
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ip = textBox2.Text;
            Properties.Settings.Default.Save();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            SendMessageAsync();
        }
        async Task SendMessageAsync()
        {
            using var sender = new UdpClient();
            string? message = textBox4.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                // и отправляем в группу
                await sender.SendAsync(data, new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.ip), Properties.Settings.Default.port));
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ComboBoxIndex = comboBox1.SelectedIndex;
            Properties.Settings.Default.Save(); 
            switch (comboBox1.SelectedItem.ToString())
            {
                case "SATD":
                    textBox2.Text = "239.192.0.3";
                    textBox3.Text = "60003";
                    break;
                case "NAVD":
                    textBox2.Text = "239.192.0.4";
                    textBox3.Text = "60004";
                    break;
                case "BAM1":
                    textBox2.Text = "239.192.0.17";
                    textBox3.Text = "60017";
                    break;
                case "BAM2":
                    textBox2.Text = "239.192.0.18";
                    textBox3.Text = "60018";
                    break;
                default:
                    break;
            }
        }

        private void ProcessReceivedMessage(string message)
        {
            int startIndex = message.IndexOf("UdPbC");
            int endIndex = message.LastIndexOf('*');

            if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
            {
                string mainPart = message.Substring(startIndex, endIndex - startIndex);
                string[] parameters = mainPart.Split(',');

                //UdPbC\0\s:GP0001*6E\$GPALF,1,1,0,123325.00,B,W,V,,3115,1,1,0,Antenna error or failure*65
                // Убедимся, что строка имеет правильный формат
                if (mainPart.StartsWith("UdPbC") && mainPart.Substring(20, 1) == "$" && mainPart.Substring(23, 3) == "ALF")
                {
                    if (!processedIds.Contains(parameters[9]) || !processedAI.Contains(parameters[10]))
                    {
                        //string fileName = $"Архив тревог.txt";
                        //int indexOfDollar = mainPart.IndexOf('$');
                        //if (indexOfDollar != -1)
                        //{
                        //    string result = mainPart[indexOfDollar..];
                        //    try
                        //    {
                        //        using (StreamWriter sw = new StreamWriter(fileName, true))
                        //        {
                        //            sw.WriteLine(result);
                        //        }
                        //    }
                        //    catch { }
                        //}
                        AddToDataGridView1(parameters);
                        AddToDataGridView2(parameters);
                        processedIds.Add(parameters[9]); // Отмечаем ID как обработанный
                        processedAI.Add(parameters[10]); // Отмечаем AI как обработанный
                    }
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells["ID_"].Value?.ToString() == parameters[9] && row.Cells["Column6"].Value?.ToString() == parameters[10])
                        {
                            UpdateCellValueIfDifferent1(row, "Column1", parameters[3], "Column2", parameters[4],
                                "Column3", parameters[6], "Column4", parameters[7],
                                "Column7", parameters[11], "Column8", parameters[12],
                                "Column9", parameters[13],
                                parameters);
                            UpdateCellValueIfDifferent(row, "Column1", parameters[3], parameters[7], parameters[9], parameters[10]);
                            UpdateCellValueIfDifferent(row, "Column2", parameters[4], parameters[7], parameters[9], parameters[10]);
                            UpdateCellValueIfDifferent(row, "Column3", parameters[6], parameters[7], parameters[9], parameters[10]);
                            UpdateCellValueIfDifferent(row, "Column4", parameters[7], parameters[7], parameters[9], parameters[10]);
                            UpdateCellValueIfDifferent(row, "Column7", parameters[11], parameters[7], parameters[9], parameters[10]);
                            UpdateCellValueIfDifferent(row, "Column8", parameters[12], parameters[7], parameters[9], parameters[10]);
                            UpdateCellValueIfDifferent(row, "Column9", parameters[13], parameters[7], parameters[9], parameters[10]);
                            break;
                        }
                    }
                }
            }
        }

        private void AddToDataGridView1(string[] parameters)
        {
            // Проверяем, что обращение к DataGridView происходит в UI-потоке
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new Action(() => AddToDataGridView1(parameters)));
                return;
            }
            if (parameters[7] != "N")
            {
                // Добавляем параметры как новую строку в DataGridView
                dataGridView1.Rows.Add(
                parameters[3],          // ПИС
                parameters[4],          // Время
                parameters[6],          // Приоритет
                parameters[7],          // Состояние
                parameters[9],          // ID
                parameters[10],         // Экземпляр оповещения
                parameters[11],         // Счетчик изменений
                parameters[12],         // Счетчик эскалации
                parameters[13]          // Текст (до символа '*')
                );
            }
        }
        private void AddToDataGridView2(string[] parameters)
        {
            // Проверяем, что обращение к DataGridView происходит в UI-потоке
            if (dataGridView2.InvokeRequired)
            {
                dataGridView2.Invoke(new Action(() => AddToDataGridView2(parameters)));
                return;
            }
            // Добавляем параметры как новую строку в DataGridView
            dataGridView2.Rows.Add(
            parameters[3],          // ПИС
            parameters[4],          // Время
            parameters[6],          // Приоритет
            parameters[7],          // Состояние
            parameters[9],          // ID
            parameters[10],         // Экземпляр оповещения
            parameters[11],         // Счетчик изменений
            parameters[12],         // Счетчик эскалации
            parameters[13]          // Текст (до символа '*')
            );
        }
        private void UpdateCellValueIfDifferent(DataGridViewRow row, string columnName, string newValue, string stateParameter, string par9, string par10)
        {
            // Если обращение происходит не из UI-потока, используем Invoke
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new Action(() => UpdateCellValueIfDifferent(row, columnName, newValue, stateParameter, par9, par10)));
                return;
            }

            // Проверяем, нужно ли удалить строку
            if (IsRowDeletionRequired(stateParameter))
            {
                DeleteRow(row.Index);
                return; // Прерываем выполнение после удаления строки
            }

            // Обновляем значение ячейки, если оно отличается от текущего
            if (!string.Equals(row.Cells[columnName]?.Value?.ToString(), newValue, StringComparison.Ordinal))
            {
                row.Cells[columnName].Value = newValue;
            }
        }

        // Метод для проверки необходимости удаления строки
        private bool IsRowDeletionRequired(string stateParameter)
        {
            return !string.IsNullOrEmpty(stateParameter) && stateParameter.Equals("N", StringComparison.OrdinalIgnoreCase);
        }

        // Метод для удаления строки
        private void DeleteRow(int rowIndex)
        {
            if (dataGridView1.Rows.Count > 0 && rowIndex >= 0 && rowIndex < dataGridView1.Rows.Count)
            {
                dataGridView1.Rows.RemoveAt(rowIndex);
            }
        }
        private void UpdateCellValueIfDifferent1(DataGridViewRow row,
            string columnName1, string newValue1,
            string columnName2, string newValue2,
            string columnName3, string newValue3,
            string columnName4, string newValue4,
            string columnName7, string newValue7,
            string columnName8, string newValue8,
            string columnName9, string newValue9, string[] parameters)
        {
            if (row.Cells[columnName1].Value?.ToString() != newValue1 || row.Cells[columnName2].Value?.ToString() != newValue2 ||
                row.Cells[columnName3].Value?.ToString() != newValue3 || row.Cells[columnName4].Value?.ToString() != newValue4 ||
                row.Cells[columnName7].Value?.ToString() != newValue7 || row.Cells[columnName8].Value?.ToString() != newValue8 ||
                row.Cells[columnName9].Value?.ToString() != newValue9)
            {
                AddToDataGridView2(parameters);
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            SendMessageAsync_();
        }
        async Task SendMessageAsync_()
        {
            // Получаем текущее время в формате hhmmss.ss
            string currentTime = DateTime.Now.ToString("HHmmss.ff");

            using var sender = new UdpClient();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["ID_"].Value != null && row.Cells["Column6"].Value != null)
                {
                    string id = row.Cells["ID_"].Value.ToString();
                    string ai = row.Cells["Column6"].Value.ToString();

                    // Формируем строку сообщения
                    string message = GenerateMessage(currentTime, id, ai);

                    //string fileName = $"Архив тревог.txt";
                    //try
                    //{
                    //    using (StreamWriter sw = new StreamWriter(fileName, true))
                    //    {
                    //        sw.WriteLine(message);
                    //    }
                    //}
                    //catch { }

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        //MessageBox.Show($"Отправлено: {message}");
                        if(_form_Terminal!=null)
                            _form_Terminal.GetTextBox().Text += message + "\r\n";
                        byte[] data = Encoding.UTF8.GetBytes(message);
                        // и отправляем в группу
                        await sender.SendAsync(data, new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.ip), Properties.Settings.Default.port));
                    }
                }
            }
        }
        private string GenerateMessage(string time, string id, string ai)
        {
            // Используем StringBuilder для эффективного формирования строки
            StringBuilder message = new StringBuilder();
            StringBuilder basemessage = new StringBuilder();
            // Добавляем начальную часть сообщения
            basemessage.Append("UdPbC");
            basemessage.Append('\0');
            basemessage.Append("\\$");
            message.Append("BMACN,");
            message.Append(time);
            message.Append(",,");
            message.Append(id);
            message.Append(",");
            message.Append(ai);
            message.Append(",S,C");

            // Рассчитываем контрольную сумму от текущего содержимого StringBuilder
            string checksum = CalculateChecksum(message.ToString());

            // Добавляем контрольную сумму к сообщению
            message.Append("*");
            message.Append(checksum);

            basemessage.Append(message);
            // Возвращаем готовое сообщение
            return basemessage.ToString();
        }
        private string CalculateChecksum(string input)
        {
            byte checksum = 0;

            //foreach (char c in input)
            //{
            //    checksum ^= (byte)c; // Применяем XOR к ASCII-коду символа
            //}

            return checksum.ToString("X2"); // Возвращаем результат в виде шестнадцатеричного числа с двумя цифрами
        }
        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что была нажата кнопка
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView1.Columns["Column5"].Index)
            {
                // Получаем строку, где была нажата кнопка
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Извлекаем данные из строки
                string id = row.Cells["ID_"].Value?.ToString() ?? "";
                string ai = row.Cells["Column6"].Value?.ToString() ?? "";

                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(ai))
                {
                    // Отправляем сообщение
                    SendMessageAsync__(id, ai);
                }
            }
        }
        async Task SendMessageAsync__(string id, string ai)
        {
            // Получаем текущее время в формате hhmmss.ss
            string currentTime = DateTime.Now.ToString("HHmmss.ff");

            using var sender = new UdpClient();

            // Формируем строку сообщения
            string message = GenerateMessage_(currentTime, id, ai);

            //string fileName = $"Архив тревог.txt";
            //try
            //{
            //    using (StreamWriter sw = new StreamWriter(fileName, true))
            //    {
            //        sw.WriteLine(message);
            //    }
            //}

            //catch { }

            if (!string.IsNullOrWhiteSpace(message))
            {
                //MessageBox.Show($"Отправлено: {message}");
                byte[] data = Encoding.UTF8.GetBytes(message);
                if(_form_Terminal != null)
                    _form_Terminal.GetTextBox().Text += message + "\r\n";
                // и отправляем в группу
                await sender.SendAsync(data, new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.ip), Properties.Settings.Default.port));
            }
        }
        private string GenerateMessage_(string time, string id, string ai)
        {
            // Используем StringBuilder для эффективного формирования строки
            StringBuilder message = new StringBuilder();
            StringBuilder basemessage = new StringBuilder();
            // Добавляем начальную часть сообщения
            basemessage.Append("UdPbC");
            basemessage.Append('\0');
            basemessage.Append("\\$");
            message.Append("BMACN,");
            message.Append(time);
            message.Append(",,");
            message.Append(id);
            message.Append(",");
            message.Append(ai);
            message.Append(",A,C");

            // Рассчитываем контрольную сумму от текущего содержимого StringBuilder
            string checksum = CalculateChecksum(message.ToString());

            // Добавляем контрольную сумму к сообщению
            message.Append("*");
            message.Append(checksum);

            basemessage.Append(message);
            // Возвращаем готовое сообщение
            return basemessage.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (_form_Terminal == null)
                {
                    _form_Terminal = new Form_terminal(this);
                    _form_Terminal.Show();
                }
                else  
                {
                    _form_Terminal.Focus();
                }
            }
            catch (Exception)
            {
                _form_Terminal = new Form_terminal(this);
                _form_Terminal.Show();
            }
        }
    }
}
