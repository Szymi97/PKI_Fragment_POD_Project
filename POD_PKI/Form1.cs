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
            DateTime thisDay = DateTime.Today;
            File.AppendAllText(@"Przelewy.txt", thisDay.ToString("d") + "\r\n" + "IdPrzelewu SkrótMD5 NumerKontaOdbiorcy NazwaOdbiorcy " +
                                 "WartośćPrzelewu AdresOdbiorcy KodPocztowy Miasto Tytuł\r\n");
            File.AppendAllText(@"Logs.txt", "");
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
                File.AppendAllText(@"Logs.txt", "Client: Tranzakcja przebiegła pomyślnie\r\n");
                MessageBox.Show("Tranzakcja przebiegła pomyślnie", "Przelew", MessageBoxButtons.OK);

            }
            else 
            {
                File.AppendAllText(@"Logs.txt", "Client: Sprawdź dane i spróbuj ponownie\r\n");
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
                File.AppendAllText(@"Logs.txt", "Client: Brak wystarczających środków na koncie\r\n");
                return false;
            }

            if(textBox_AccountNumber.Text.Length != 26)
            {
                textBox_AccountNumber.Clear();
                File.AppendAllText(@"Logs.txt", "Client: Za krótki numer konta\r\n");
                return false;
            }

            if (textBox_Name.Text.Length < 1 || textBox_Name.Text == "Podaj dane odbiorcy")
            {
                textBox_Name.Text = "Podaj dane odbiorcy";
                File.AppendAllText(@"Logs.txt", "Client: Zła nazwa/imie i nazwisko odbiorcy\r\n");
                return false;
            }

            if (textBox_Address.Text.Length < 1 || textBox_Address.Text == "Podaj dane odbiorcy")
            {
                textBox_Address.Text = "Podaj dane odbiorcy";
                File.AppendAllText(@"Logs.txt", "Client: Zły adres odbiorcy\r\n");
                return false;
            }

            if (textBox_PostCode.Text.Length != 5 || textBox_PostCode.Text == "00000")
            {
                textBox_PostCode.Text = "00000";
                File.AppendAllText(@"Logs.txt", "Client: Zły kod pocztowy odbiorcy\r\n");
                return false;
            }

            if (textBox_City.Text.Length < 1 || textBox_City.Text == "Podaj dane odbiorcy")
            {
                textBox_City.Text = "Podaj dane odbiorcy";
                File.AppendAllText(@"Logs.txt", "Client: Zła nazwa miasta odbiorcy\r\n");
                return false;
            }

            if (textBox_Note.Text.Length < 1 || textBox_Note.Text == "Dodaj tytuł przelewu")
            {
                textBox_Note.Text = "Dodaj tytuł przelewu";
                File.AppendAllText(@"Logs.txt", "Client: Zły tytuł przelewu\r\n");
                return false;
            }

            return true;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Main Function
        /// </summary>
        static int idTransfer = 1;
        static SymmetricAlgorithm sa;
        private void DoTransfer()
        {
            string source = textBox_AccountNumber.Text + " " + textBox_Name.Text + " " +
                            textBox_Zł.Text + "." + textBox_Gr.Text + " " +
                            textBox_Address.Text + " " + textBox_PostCode.Text + " " +
                            textBox_City.Text + " " + textBox_Note.Text;

            File.AppendAllText(@"Logs.txt", "Client: source - " + source + "\r\n");

            MD5 md5Hash = MD5.Create();
            string hashClient = GetMd5Hash(md5Hash, source);

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            sa = new TripleDESCryptoServiceProvider();

            var msgFromClientToServer = hashClient + " " + source; //tworzymy wiadomosc dla serwera

            var encryptMsg = Encrypt(msgFromClientToServer); //szyfrujemy ją

            var responseFromServer = Server(encryptMsg); //wysyłamy i czekamy na odpowiedz

            var msgFromServer = Decrypt(responseFromServer);
            string[] sourceSplit = msgFromServer.Split(' ');

            string hashServerName = GetMd5Hash(md5Hash, sourceSplit[1]);

            if (hashServerName == sourceSplit[0])
            {
                File.AppendAllText(@"Przelewy.txt", idTransfer + " " + hashClient + " " + source + "\r\n");
                File.AppendAllText(@"Logs.txt", "Client: idTransfer - " + idTransfer + " | hash - " + hashClient + " | source - " + source + "\r\n");
                idTransfer++;

                UpdateAllMoney();
            }
            else
            {
                MessageBox.Show("Wystąpił błąd", "Wystąpił błąd", MessageBoxButtons.OK);
                File.AppendAllText(@"Logs.txt", "Client: MessageBoxError \r\n");
                return;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private void UpdateAllMoney()
        {
            AllMoneyD -= Money;
            File.AppendAllText(@"Logs.txt", "Client: AllMoney - " + AllMoneyD + "\r\n");
            label_AllMoney.Text = AllMoneyD.ToString();
        }


        /////////////////////////////////////SERVER///////////////////////////////////////////////////////////////
        private byte[] Server(byte[] messageFromClient)
        {
            var msg2 = Decrypt(messageFromClient);
            string[] sourceSplit = msg2.Split(' ');
            string source = "";

            for(int i = 1; i < sourceSplit.Length; i++)
            {
                source += sourceSplit[i];
            }

            MD5 md5Hash = MD5.Create();
            string hashServer = GetMd5Hash(md5Hash, source);

            string hashServerName = GetMd5Hash(md5Hash,"ServerSzymi");

            if (hashServer == sourceSplit[0])
            {
                var response = Encrypt(hashServerName + " ServerSzymi " + hashServer + " YES");
                File.AppendAllText(@"Logs.txt", "Server: hashServerName - " + hashServerName + " | hashServer - " + hashServer + " | response - YES\r\n");
                return response;
            }
            else
            {
                var response = Encrypt(hashServerName + " ServerSzymi " + hashServer + " NO");
                File.AppendAllText(@"Logs.txt", "Server: hashServerName - " + hashServerName + " | hashServer - " + hashServer + " | response - NO\r\n");
                return response;
            }
                
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        static private byte[] Encrypt(string text)
        {
            ICryptoTransform encryptor = sa.CreateEncryptor(sa.Key, sa.IV);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(text);
                    }
                    return msEncrypt.ToArray();
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        static private string Decrypt(byte[] msg)
        {
            ICryptoTransform decryptor = sa.CreateDecryptor(sa.Key, sa.IV);
            using (var msDecrypt = new MemoryStream(msg))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
