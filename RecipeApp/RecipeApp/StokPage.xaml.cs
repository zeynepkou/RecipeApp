using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace RecipeApp
{
    /// <summary>
    /// Interaction logic for StokPage.xaml
    /// </summary>
    public partial class StokPage : Page
    {
        public StokPage()
        {
            InitializeComponent();
            LoadMalzemeler();
        }


        string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            string malzemeAdi = txtMalzemeAdi.Text; // Malzeme Adı
            decimal toplamMiktar;
            decimal birimFiyat;

            
            if (!decimal.TryParse(txtToplamMiktar.Text, out toplamMiktar))
            {
                MessageBox.Show("Toplam Miktar geçersiz bir sayı.");
                return;
            }

            // BirimFiyat'ı decimal olarak dönüştür
            if (!decimal.TryParse(txtBirimFiyat.Text, out birimFiyat))
            {
                MessageBox.Show("Birim Fiyat geçersiz bir sayı.");
                return;
            }

            string malzemeBirim = txtMalzemeBirim.Text; // Malzeme Birimi alıyoruz

            // Veritabanı sorgusu için bağlantıyı açıyoruz
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // malzeme eklemek için sorgumuz
                    string query = "INSERT INTO Malzemeler (MalzemeAdi, ToplamMiktar, MalzemeBirim, BirimFiyat) VALUES (@MalzemeAdi, @ToplamMiktar, @MalzemeBirim, @BirimFiyat)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Parametreleri ekle
                        command.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);
                        command.Parameters.AddWithValue("@ToplamMiktar", toplamMiktar);
                        command.Parameters.AddWithValue("@MalzemeBirim", malzemeBirim);
                        command.Parameters.AddWithValue("@BirimFiyat", birimFiyat);

                        // Sorguyu çalıştır
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Malzeme başarıyla eklendi.");

                    
                    txtMalzemeAdi.Text = string.Empty;
                    txtToplamMiktar.Text = string.Empty;
                    txtMalzemeBirim.Text = string.Empty;
                    txtBirimFiyat.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }

                // Verileri yeniden yükle
                LoadMalzemeler();
            }
        }




        private void LoadMalzemeler()
        {
            // DataGrid'i temizle
            dataGrid.ItemsSource = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT MalzemeAdi, ToplamMiktar, MalzemeBirim, BirimFiyat FROM Malzemeler";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            // DataTable nesnesi oluştur
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            
                            dataGrid.ItemsSource = dataTable.DefaultView;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }



        private void BtnStokIslemleri_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnFavoriler_Click(object sender, RoutedEventArgs e)
        {
            Frame navigationFrame = new Frame();
            FavoritesPage favoritesPage = new FavoritesPage();
            navigationFrame.Content = favoritesPage;
            this.Content = navigationFrame; // Ana pencere içeriğini Frame ile değiştiriyoruz

        }

        private void BtnTarifEkle_Click(object sender, RoutedEventArgs e)
        {
            Frame navigationFrame = new Frame();
            RecipeAddPage recipeAddPage = new RecipeAddPage();
            navigationFrame.Content = recipeAddPage;
            this.Content = navigationFrame; // Ana pencere içeriğini Frame ile değiştiriyoruz
        }



        private void btnDuzenle_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AnaSayfaButton_Click(object sender, RoutedEventArgs e)
        {

            MainWindow mainWindow = new MainWindow();

            mainWindow.Show();

            Window.GetWindow(this)?.Close();
        }


        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // malzeme aramak için sorgumuz
                string query = "SELECT * FROM Malzemeler WHERE MalzemeAdi LIKE @searchText";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@searchText", "%" + searchText + "%");

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        
                        dataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
        }

        private void SilButton_Click(object sender, RoutedEventArgs e)
        {
            // Butonun Click olayından CommandParameter ile malzeme adını alıyoruz
            Button button = sender as Button;

            if (button == null || button.CommandParameter == null)
            {
                MessageBox.Show("Malzeme adı alınamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return; 
            }

            string malzemeAd = button.CommandParameter.ToString(); // Malzeme adını burada alıyoruz

            // Kullanıcıdan onay iste
            MessageBoxResult result = MessageBox.Show($"{malzemeAd} adlı malzemeyi silmek istediğinize emin misiniz?",
                                                     "Onay",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Malzemeyi silmek için sorgumuz
                    string deleteQuery = "DELETE FROM Malzemeler WHERE MalzemeAdi = @malzemeAd";
                    using (var command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@malzemeAd", malzemeAd);

                        try
                        {
                            // Silme işlemini gerçekleştirdiğimiz yer
                            int rowsAffected = command.ExecuteNonQuery();

                            // Silinen kayıt sayısını kontrol et
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show($"{malzemeAd} adlı malzeme başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show($"{malzemeAd} adlı malzeme bulunamadı veya silinemedi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (SqlException ex)
                        {
                            // Hata mesajı içeriğini kontrol et
                            if (ex.Number == 547) // SQL Server'da FOREIGN KEY hatası için hata numarası
                            {
                                MessageBox.Show("Silmek istediğiniz malzeme tariflerden birinde olduğu için silinemiyor.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                // Diğer SQL hataları için genel bir mesaj gösteriyoruz
                                MessageBox.Show("Bir hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }

                // Silme işleminden sonra DataGrid'i güncelliyoruz
                LoadMalzemeler();
            }
        }
        private void DuzenleButton_Click(object sender, RoutedEventArgs e)
        {
            // DataGrid'den seçili öğeyi alıyoruz
            if (dataGrid.SelectedItem is DataRowView selectedRow)
            {
                
                string malzemeAdi = selectedRow["MalzemeAdi"].ToString();
                string toplamMiktar = selectedRow["ToplamMiktar"].ToString();
                string malzemeBirim = selectedRow["MalzemeBirim"].ToString();
                string birimFiyat = selectedRow["BirimFiyat"].ToString();

                
                txtGuncelleMalzemeAdi.Text = malzemeAdi;
                txtGuncelleToplamMiktar.Text = toplamMiktar;
                txtGuncelleMalzemeBirim.Text = malzemeBirim;
                txtGuncelleBirimFiyat.Text = birimFiyat;
            }
            else
            {
                MessageBox.Show("Güncellenecek bir malzeme seçin.");
            }
        }


        private void btnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            
            string malzemeAdi = txtGuncelleMalzemeAdi.Text; // Malzeme adı TextBox'undan alınıyor
            string toplamMiktar = txtGuncelleToplamMiktar.Text; // Toplam Miktar TextBox'undan alınıyor
            string malzemeBirim = txtGuncelleMalzemeBirim.Text; // Malzeme Birim TextBox'undan alınıyor
            decimal birimFiyatDecimal;

            // Birim Fiyat değerini kontrol ediyoruz
            if (!decimal.TryParse(txtGuncelleBirimFiyat.Text, out birimFiyatDecimal))
            {
                MessageBox.Show("Geçersiz Birim Fiyat. Lütfen ondalık ayracı doğru kullanın.");
                return;
            }

            // Veritabanında güncelleme yapan sorgumuz
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE Malzemeler SET ToplamMiktar = @ToplamMiktar, MalzemeBirim = @MalzemeBirim, BirimFiyat = @BirimFiyat WHERE MalzemeAdi = @MalzemeAdi";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Güncelleme için parametreleri ekle
                    command.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);
                    command.Parameters.AddWithValue("@ToplamMiktar", toplamMiktar);
                    command.Parameters.AddWithValue("@MalzemeBirim", malzemeBirim);
                    command.Parameters.AddWithValue("@BirimFiyat", birimFiyatDecimal);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery(); // Etkilenen satır sayısını al

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Malzeme başarıyla güncellendi.");
                    }
                    else
                    {
                        MessageBox.Show("Güncelleme yapılamadı, lütfen verilerinizi kontrol edin.");
                    }
                }
            }

            // DataGrid'deki güncellemeleri yeniden yükle
            LoadMalzemeler();
        }

        // Sidebar genişletme ve daraltma ayarlamaları
        private void AdjustSidebar(bool isExpanded)
        {
            if (isExpanded)
            {
                sidebarBorder.Width = 230; // Sidebar genişken
                sidebarContent.Visibility = Visibility.Visible; // İçerik görünür
            }
            else
            {
                sidebarBorder.Width = 60; // Sidebar daraltıldığında
                sidebarContent.Visibility = Visibility.Collapsed; // İçerik gizlenir
            }
        }


        // Sidebar genişletme event handler
        private void ToggleSidebar_Checked(object sender, RoutedEventArgs e)
        {
            AdjustSidebar(true);
        }

        // Sidebar daraltma event handler
        private void ToggleSidebar_Unchecked(object sender, RoutedEventArgs e)
        {
            AdjustSidebar(false);
        }

    }
}