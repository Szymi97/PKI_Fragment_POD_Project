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
        int idTransfer = 1;
        private void DoTransfer()
        {
            string source = textBox_AccountNumber.Text + " " + textBox_Name.Text + " " +
                            textBox_Zł.Text + "." + textBox_Gr.Text + " " +
                            textBox_Address.Text + " " + textBox_PostCode.Text + " " +
                            textBox_City.Text + " " + textBox_Note.Text;

            MD5 md5Hash = MD5.Create();
            string hashClient = GetMd5Hash(md5Hash, source);

            string messageToServer = PrepareItForServer(hashClient + " " + source);

            if(messageToServer == null)
                return;

            string messageFromServer = ResponseFromServer(messageToServer);
            
            if (messageFromServer == null)
            {
                if (!IsItGoodServer(messageFromServer))
                {
                    MessageBox.Show("Wystąpił błąd", "Wystąpił błąd", MessageBoxButtons.OK);
                    return;
                }
                else
                {
                    File.AppendAllText(@"Wyniki.txt", idTransfer + " " + hashClient + " " + source + "\r\n");
                    File.AppendAllText(@"Logs.txt", "idTransfer + hash + source " + idTransfer + " " + hashClient + " " + source + "\r\n");
                    idTransfer++;

                    UpdateAllMoney();
                }    
            }
        }

        private bool IsItGoodServer(string messageFromServer)
        {

            try
            {
                //Create a UnicodeEncoder to convert between byte array and string.
                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                //Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] dataToDecrypt = ByteConverter.GetBytes(messageFromServer);
                byte[] decryptedData;

                //Create a new instance of RSACryptoServiceProvider to generate
                //public and private key data.
                using (RSA_Client)
                {
                    decryptedData = RSAEncrypt(dataToDecrypt, RSA_Client.ExportParameters(true), false); //RSA_Client.ExportParameters(false) to publiczny klucz
                    decryptedData = RSAEncrypt(decryptedData, RSA_Server.ExportParameters(false), false); //RSA_Client.ExportParameters(false) to publiczny klucz
                }

                messageFromServer = ByteConverter.GetString(decryptedData);
                string[] sourceServer = messageFromServer.Split(' ');
                string source = "";

                for (int i = 1; i < sourceServer.Length; i++)
                {
                    source += sourceServer[i];

                    if (i != sourceServer.Length)
                        source += " ";
                }

                MD5 md5Hash = MD5.Create();
                string hash = GetMd5Hash(md5Hash, source);

                if (hash == sourceServer[0])
                {
                    File.AppendAllText(@"Logs.txt", "Client: hash " + hash + "\r\n");
                    return true;
                }
                else
                    return false;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
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
        ///////////////////////////////////////////////////////////////////////////////////////// 
        static RSACryptoServiceProvider RSA_Client = new RSACryptoServiceProvider();
        static RSACryptoServiceProvider RSA_Server = new RSACryptoServiceProvider();
        private string PrepareItForServer(string stringIn)
        {
            try
            {
                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                byte[] dataToEncrypt = ByteConverter.GetBytes(stringIn);
                byte[] encryptedData;
                

                encryptedData = RSAEncrypt(dataToEncrypt, RSA_Client.ExportParameters(true), false); //RSA_Client.ExportParameters(false) to publiczny klucz
                encryptedData = RSAEncrypt(encryptedData, RSA_Server.ExportParameters(false), false); //RSA_Client.ExportParameters(false) to publiczny klucz


                return ByteConverter.GetString(encryptedData);
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        static public byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //to include the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                MessageBox.Show(e.Message);
                return null;
            }

        }

        static public byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////

        string serverName = "ServerSzymi";
        private string ResponseFromServer(string messageToServer)
        {
            try
            {
                //Create a UnicodeEncoder to convert between byte array and string.
                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                //Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] dataToDecrypt = ByteConverter.GetBytes(messageToServer);
                byte[] decryptedData;

                //Create a new instance of RSACryptoServiceProvider to generate
                //public and private key data.
                using (RSA_Server)
                {
                    decryptedData = RSAEncrypt(dataToDecrypt, RSA_Server.ExportParameters(true), false); //RSA_Client.ExportParameters(false) to publiczny klucz
                    decryptedData = RSAEncrypt(decryptedData, RSA_Client.ExportParameters(false), false); //RSA_Client.ExportParameters(false) to publiczny klucz
                }

                string messageFromClient = ByteConverter.GetString(decryptedData);
                string[] sourceClient = messageFromClient.Split(' ');
                string source = "";

                for(int i = 1; i < sourceClient.Length; i++)
                {
                    source += sourceClient[i];
                    
                    if(i != sourceClient.Length)
                        source += " ";
                }

                MD5 md5Hash = MD5.Create();
                string hash = GetMd5Hash(md5Hash, source);

                if (hash == sourceClient[0])
                {
                    string serverMD5 = GetMd5Hash(md5Hash, serverName + " " + sourceClient[0]);
                    File.AppendAllText(@"Logs.txt", "Server: serverMD5 " + serverMD5 + "\r\n");
                    return serverMD5 + ConfirmTransfer(serverMD5);
                }
                else
                    return null;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        private string ConfirmTransfer(string stringIn)
        {
            try
            {
                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                byte[] dataToEncrypt = ByteConverter.GetBytes(stringIn);
                byte[] encryptedData;

                using (RSA_Client)
                {
                    encryptedData = RSAEncrypt(dataToEncrypt, RSA_Server.ExportParameters(true), false); //RSA_Client.ExportParameters(false) to publiczny klucz
                    encryptedData = RSAEncrypt(encryptedData, RSA_Client.ExportParameters(false), false); //RSA_Client.ExportParameters(false) to publiczny klucz
                }

                File.AppendAllText(@"Logs.txt", "Server: ByteConverter.GetString(encryptedData) " + ByteConverter.GetString(encryptedData) + "\r\n");
                return ByteConverter.GetString(encryptedData);
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }
    }
}
