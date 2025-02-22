using Microsoft.Data.SqlClient;
using System.Windows;

namespace RecipeApp
{
    public partial class YeniMalzemeEkle : Window
    {
        public YeniMalzemeEkle()
        {
            InitializeComponent();
        }

        private void EkleButton_Click(object sender, RoutedEventArgs e)
        {
            // Malzeme adı, miktarı ve birimi kontrol ediyoruz
            if (!string.IsNullOrWhiteSpace(malzemeAdiTextBox.Text) &&
                !string.IsNullOrWhiteSpace(malzemeMiktariTextBox.Text) &&
                !string.IsNullOrWhiteSpace(malzemeBirimiTextBox.Text))
            {
                // Miktarın float olup olmadığını kontrol ediyoruz
                if (float.TryParse(malzemeMiktariTextBox.Text, out float malzemeMiktari))
                {
                    // Girilen değer float ise işlem yapılabilir
                    string malzemeAdi = malzemeAdiTextBox.Text;
                    string malzemeBirimi = malzemeBirimiTextBox.Text;

                    DialogResult = true; // Dialog'un başarılı bir şekilde kapanmasını sağlıyor
                }
                else
                {
                    
                    MessageBox.Show("Lütfen geçerli bir miktar girin (örn. 1.5).");
                }
            }
            else
            {
                MessageBox.Show("Lütfen hem malzeme adı, miktar hem de birim girin.");
            }
        }



    }
}