using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POD_PKI
{
    public partial class PINIONDZE : Form
    {
        double AllMoneyD = 9123456.89;
        public PINIONDZE()
        {
            InitializeComponent();
            label_AllMoney.Text = AllMoneyD.ToString();
            File.AppendAllText(@"Wyniki.txt", "Skrót MD5 Numer Konta Odbiorcy Nazwa "+
                                 "Wartość Przelewu Adres Kod Pocztowy Miasto Tytuł\r\n");
        }

        private void textBox_Zł_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox_Gr_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox_AccountNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox_PostCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void textBox_Name_KeyPress(object sender, KeyPressEventArgs e)
        {
            /////////////////////////////////////////////
        }

        private void textBox_City_KeyPress(object sender, KeyPressEventArgs e)
        {
            /////////////////////////////////////////////
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            textBox_AccountNumber.Clear();
            textBox_Address.Clear();
            textBox_City.Clear();
            textBox_Gr.Clear();
            textBox_Name.Clear();
            textBox_Note.Clear();
            textBox_PostCode.Clear();
            textBox_Zł.Clear();
        }

        private void button_DoIt_Click(object sender, EventArgs e)
        {
            if(IsAllOk())
            {
                DoTransfer();
                MessageBox.Show("Tranzakcja przebiegła pomyślnie", "Przelew", MessageBoxButtons.OK);
            }
            else 
            {
                MessageBox.Show("Sprawdź dane i spróbuj ponownie", "Nieprawidłowe dane", MessageBoxButtons.OK);
            }
        }

        double Money; //wartosc przelewu
        private bool IsAllOk()
        {
            if (textBox_Gr.Text.Length < 1)
                textBox_Gr.Text = "00";

            if(textBox_Zł.Text.Length < 1)
            {
                textBox_Zł.Clear();
                return false;
            }

            double Zł = double.Parse(textBox_Zł.Text);
            double Gr = double.Parse(textBox_Gr.Text);
            Money = Zł + (Gr / 100.00);

            if(Money > AllMoneyD)
            {
                textBox_Zł.Clear();
                textBox_Gr.Clear();
                return false;
            }

            if(textBox_AccountNumber.Text.Length != 26)
            {
                textBox_AccountNumber.Clear();
                return false;
            }

            if (textBox_Name.Text.Length < 1 || textBox_Name.Text == "Podaj dane odbiorcy")
            {
                textBox_Name.Text = "Podaj dane odbiorcy";
                return false;
            }

            if (textBox_Address.Text.Length < 1 || textBox_Address.Text == "Podaj dane odbiorcy")
            {
                textBox_Address.Text = "Podaj dane odbiorcy";
                return false;
            }

            if (textBox_PostCode.Text.Length < 1 || textBox_PostCode.Text == "00000")
            {
                textBox_PostCode.Text = "00000";
                return false;
            }

            if (textBox_City.Text.Length < 1 || textBox_City.Text == "Podaj dane odbiorcy")
            {
                textBox_City.Text = "Podaj dane odbiorcy";
                return false;
            }

            return true;
        }

        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Main Function
        /// </summary>
        private void DoTransfer()
        {
            string source = textBox_AccountNumber.Text + " " + textBox_Name.Text + " " +
                            textBox_Zł.Text + "." + textBox_Gr.Text + " " +
                            textBox_Address.Text + " " + textBox_PostCode.Text + " " +
                            textBox_City.Text + " " + textBox_Note.Text;

            MD5 md5Hash = MD5.Create();
            string hash = GetMd5Hash(md5Hash, source);

            File.AppendAllText(@"Wyniki.txt", hash + " " + source + "\r\n");

            UpdateAllMoney();
        }

        private void UpdateAllMoney()
        {
            AllMoneyD -= Money;
            label_AllMoney.Text = AllMoneyD.ToString();
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
