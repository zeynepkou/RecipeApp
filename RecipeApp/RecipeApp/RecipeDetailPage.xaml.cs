using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
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

namespace RecipeApp
{
    public partial class RecipeDetailPage : Page
    {
        private string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";
        private int tarifID;
        private string currentImagePath = "";
        private bool isImageChanged = false;
        private List<int> silinenMalzemeler = new List<int>(); // Silinecek malzemeleri kaydetmek için
        private List<Tuple<int, double>> yeniEklenenMalzemeler = new List<Tuple<int, double>>();
        private bool isFavori = false;


        public RecipeDetailPage(int tarifID)
        {
            InitializeComponent();
            this.tarifID = tarifID; // Yapıcı metodda tarifID'yi alıp saklıyoruz
            LoadRecipeDetails(tarifID); // Tarif detaylarını yükle
        }

        private void LoadRecipeDetails(int tarifID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Tarif ve kategori bilgilerini yükle
                string query = @"
            SELECT t.TarifAdi, t.ResimYolu, t.Talimatlar, k.KategoriAdi, t.HazirlamaSuresi
            FROM Tarifler t
            INNER JOIN Kategoriler k ON t.KategoriID = k.KategoriID
            WHERE t.TarifID = @TarifID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", tarifID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Tarif ve kategori bilgilerini oku
                            string tarifAdi = reader.GetString(0);
                            string resimYolu = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            string talimatlar = reader.GetString(2);
                            string kategoriAdi = reader.GetString(3); // Kategori Adı
                            int hazırlanmaSuresi = reader.GetInt32(4); // Hazırlanma Süresi

                            // UI'ya tarif bilgilerini yükle
                            yemekAdiTextBlock.Text = tarifAdi; 
                            tarifTextBlock.Text = talimatlar; 
                            kategoriTextBlock.Text = kategoriAdi; 
                            sureTextBlock.Text = hazırlanmaSuresi.ToString(); 

                            if (!string.IsNullOrEmpty(resimYolu) && System.IO.File.Exists(resimYolu))
                            {
                                yemekImage.Source = new BitmapImage(new Uri(resimYolu, UriKind.Absolute));
                            }
                            else
                            {
                                yemekImage.Source = new BitmapImage(new Uri("C:\\Lab\\resim\\elma.jpg", UriKind.Absolute));
                            }
                        }
                    }
                }

                // Favori olup olmadığını kontrol et
                string favoriQuery = "SELECT COUNT(*) FROM Favoriler WHERE TarifID = @TarifID";
                using (SqlCommand favoriCommand = new SqlCommand(favoriQuery, connection))
                {
                    favoriCommand.Parameters.AddWithValue("@TarifID", tarifID);
                    int isFavoriCount = (int)favoriCommand.ExecuteScalar();

                    // Kalp simgesini güncelle
                    if (isFavoriCount > 0)
                    {
                        kalpPath.Fill = Brushes.Red; // İçi dolu kırmızı kalp
                        kalpPath.Stroke = Brushes.Red; // Kırmızı çerçeve
                        isFavori = true;
                    }
                    else
                    {
                        kalpPath.Fill = Brushes.Transparent; // İçi boş kalp
                        kalpPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3A5F49")); // Yeşil çerçeve
                        isFavori = false;
                    }
                }

                // Kategorileri yükle
                LoadKategoriler();
                // Kategoriyi seçili hale getir
                foreach (ComboBoxItem item in cmbKategori.Items)
                {
                    if (item.Content.ToString() == kategoriTextBlock.Text)
                    {
                        cmbKategori.SelectedItem = item;
                        break;
                    }
                }

                // Malzemeleri yükle
                LoadRecipeIngredients(tarifID);
            }
        }



        private void LoadRecipeIngredients(int tarifID)
        {
            malzemeStackPanel.Children.Clear();

            // Eksik malzemeler listesini dolduracak bir metod çağırıyoruz
            bool isEksikMalzemeVar;
            List<string> eksikMalzemeler;
            EksikMalzeme(tarifID, out isEksikMalzemeVar, out eksikMalzemeler);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
        SELECT m.MalzemeID, m.MalzemeAdi, tm.MalzemeMiktar, m.MalzemeBirim
        FROM TarifMalzeme tm
        JOIN Malzemeler m ON tm.MalzemeID = m.MalzemeID
        WHERE tm.TarifID = @TarifID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", tarifID);

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int malzemeID = reader.GetInt32(0);
                        string malzemeAdi = reader.GetString(1);
                        double malzemeMiktar = reader.GetDouble(2);
                        string malzemeBirim = reader.GetString(3);

                        StackPanel malzemePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

                        TextBlock malzemeTextBlock = new TextBlock
                        {
                            Text = $"• {malzemeMiktar} {malzemeBirim} {malzemeAdi}",  // Updated format with bullet point
                            FontSize = 16,
                            Foreground = Brushes.White,
                            Margin = new Thickness(5)
                        };


                        // Eğer malzeme eksikse kırmızı ünlem ekliyoruz
                        if (eksikMalzemeler.Contains(malzemeAdi))
                        {
                            malzemeTextBlock.Text += " ⚠️"; // Kırmızı ünlem simgesi
                            malzemeTextBlock.Foreground = Brushes.Red; // Eksik olan malzemenin rengi kırmızı
                        }

                        // Gereksiz görünmeyen bileşenler
                        TextBox malzemeTextBox = new TextBox
                        {
                            Text = malzemeMiktar.ToString(),
                            Width = 50,
                            Visibility = Visibility.Collapsed
                        };

                        Button btnRemove = new Button
                        {
                            Content = "Sil",
                            Width = 50,
                            Tag = malzemeID,
                            Visibility = Visibility.Collapsed,
                            Background = Brushes.Red,
                            Foreground = Brushes.White
                        };
                        btnRemove.Click += (s, e) => ConfirmRemoveIngredient(malzemeID);

                        malzemePanel.Children.Add(malzemeTextBlock);
                        malzemePanel.Children.Add(malzemeTextBox);
                        malzemePanel.Children.Add(btnRemove);

                        malzemeStackPanel.Children.Add(malzemePanel);
                    }
                }
            }
        }

        public void EksikMalzeme(int tarifId, out bool isEksikMalzemeVar, out List<string> eksikMalzemeler)
        {
            isEksikMalzemeVar = false; // Varsayılan olarak eksik malzeme yok
            eksikMalzemeler = new List<string>(); // Eksik malzeme listesini başlatıyoruz

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                    SELECT tm.MalzemeID, m.MalzemeAdi, tm.MalzemeMiktar, m.ToplamMiktar
                    FROM TarifMalzeme tm
                    JOIN Malzemeler m ON tm.MalzemeID = m.MalzemeID
                    WHERE tm.TarifID = @tarifId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@tarifId", tarifId);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string malzemeAdi = reader["MalzemeAdi"].ToString();
                        double gerekenMiktar = Convert.ToDouble(reader["MalzemeMiktar"]);
                        double mevcutMiktar = Convert.ToDouble(reader["ToplamMiktar"]);

                        // Eğer malzeme eksikse eksik malzemeler listesine ekliyoruz
                        if (gerekenMiktar > mevcutMiktar)
                        {
                            isEksikMalzemeVar = true;
                            eksikMalzemeler.Add(malzemeAdi);
                        }
                    }

                    reader.Close();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SQL hatası: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Genel hata: " + ex.Message);
            }
        }



        private void LoadMalzemeler()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT MalzemeID, MalzemeAdi, MalzemeBirim FROM Malzemeler";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int malzemeID = reader.GetInt32(0);
                        string malzemeAdi = reader.GetString(1);
                        string malzemeBirim = reader.GetString(2);

                        ComboBoxItem item = new ComboBoxItem
                        {
                            Content = malzemeAdi,
                            Tag = new Tuple<int, string>(malzemeID, malzemeBirim) // MalzemeID ve Birim bilgisi
                        };
                        cmbMalzemeler.Items.Add(item);
                    }
                }
            }
        }

        private void cmbMalzemeler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMalzemeler.SelectedItem is ComboBoxItem selectedItem)
            {
                var malzemeInfo = (Tuple<int, string>)selectedItem.Tag;
                txtMalzemeBirim.Text = malzemeInfo.Item2; 
                txtMalzemeMiktar.Visibility = Visibility.Visible;
                txtMalzemeBirim.Visibility = Visibility.Visible;
                btnAddIngredient.Visibility = Visibility.Visible;
            }
        }


        private void BtnAddIngredient_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMalzemeler.SelectedItem is ComboBoxItem selectedItem && !string.IsNullOrWhiteSpace(txtMalzemeMiktar.Text))
            {
                var malzemeInfo = (Tuple<int, string>)selectedItem.Tag;
                int malzemeID = malzemeInfo.Item1;
                double miktar = double.Parse(txtMalzemeMiktar.Text);

                var yeniMalzeme = new Tuple<int, double>(malzemeID, miktar);
                yeniEklenenMalzemeler.Add(yeniMalzeme);

                StackPanel malzemePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

                TextBlock malzemeBlock = new TextBlock
                {
                    Text = $"{selectedItem.Content} ({miktar} {txtMalzemeBirim.Text})",
                    FontSize = 16,
                    Foreground = Brushes.White,
                    Margin = new Thickness(5)
                };

                // Sil butonunu ekleyin ve görünür yapın
                Button silButton = new Button
                {
                    Content = "Sil",
                    Width = 50,
                    Background = Brushes.Red,
                    Foreground = Brushes.White,
                    Tag = yeniMalzeme,
                    Visibility = Visibility.Visible // "Sil" butonunu görünür yapıyoruz
                };

                silButton.Click += (s, ev) =>
                {
                    // Paneli arayüzden kaldır
                    addedIngredientsPanel.Children.Remove(malzemePanel);
                    // Listeyi güncelle
                    yeniEklenenMalzemeler.Remove(yeniMalzeme);
                };

                malzemePanel.Children.Add(malzemeBlock);
                malzemePanel.Children.Add(silButton);

                addedIngredientsPanel.Children.Add(malzemePanel);

                // Giriş alanlarını temizle ve görünürlüğü kapat
                cmbMalzemeler.SelectedIndex = -1;
                txtMalzemeMiktar.Clear();
                txtMalzemeMiktar.Visibility = Visibility.Collapsed;
                txtMalzemeBirim.Visibility = Visibility.Collapsed;
                btnAddIngredient.Visibility = Visibility.Collapsed;
            }
        }






        List<Tuple<string, float, string>> malzemeVeMiktarListesi = new List<Tuple<string, float, string>>();
        // Yeni malzeme ekleme butonu
        private void YeniMalzemeEkleButton_Click(object sender, RoutedEventArgs e)
        {
            YeniMalzemeEkle yeniMalzemeEkle = new YeniMalzemeEkle();
            if (yeniMalzemeEkle.ShowDialog() == true)
            {
                string yeniMalzemeAdi = yeniMalzemeEkle.malzemeAdiTextBox.Text;  
                float yeniMalzemeMiktari = float.Parse(yeniMalzemeEkle.malzemeMiktariTextBox.Text);  
                string yeniMalzemeBirimi = yeniMalzemeEkle.malzemeBirimiTextBox.Text; 

                if (!string.IsNullOrWhiteSpace(yeniMalzemeAdi) && !string.IsNullOrWhiteSpace(yeniMalzemeBirimi))
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection("Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True"))
                        {
                            connection.Open();

                            // Aynı malzemenin olup olmadığını kontrol eden duplicate kontrolü yapıyoruz
                            string checkQuery = "SELECT COUNT(*) FROM Malzemeler WHERE MalzemeAdi = @MalzemeAdi";

                            using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                            {
                                checkCommand.Parameters.AddWithValue("@MalzemeAdi", yeniMalzemeAdi);

                                int count = (int)checkCommand.ExecuteScalar();

                                if (count > 0)
                                {
                                    MessageBox.Show("Bu isimde bir malzeme zaten mevcut. Lütfen farklı bir malzeme adı girin.");
                                    return; // Kayıt eklenmeden işlem durdurulur
                                }
                            }

                            
                            string insertQuery = "INSERT INTO Malzemeler (MalzemeAdi, ToplamMiktar, MalzemeBirim, BirimFiyat) " +
                                                  "OUTPUT INSERTED.MalzemeID " +  
                                                  "VALUES (@MalzemeAdi, 0, @MalzemeBirimi, 0)";

                            using (SqlCommand command = new SqlCommand(insertQuery, connection))
                            {
                                command.Parameters.AddWithValue("@MalzemeAdi", yeniMalzemeAdi);
                                command.Parameters.AddWithValue("@MalzemeBirimi", yeniMalzemeBirimi);

                                
                                int yeniMalzemeID = (int)command.ExecuteScalar();

                                if (yeniMalzemeID > 0)
                                {
                                    MessageBox.Show("Malzeme başarıyla eklendi.");

                                    
                                    malzemeVeMiktarListesi.Add(new Tuple<string, float, string>(yeniMalzemeAdi, yeniMalzemeMiktari, yeniMalzemeBirimi));

                                    // Yeni malzemeyi ComboBox'a ekleme kısmı
                                    ComboBoxItem yeniComboBoxItem = new ComboBoxItem
                                    {
                                        Content = yeniMalzemeAdi,
                                        Tag = new Tuple<int, string>(yeniMalzemeID, yeniMalzemeBirimi) 
                                    };
                                    cmbMalzemeler.Items.Add(yeniComboBoxItem); 

                                    // Yeni eklenen malzemeyi tarif malzeme tablosuna eklediğimiz sorgu
                                    string insertTarifMalzemeQuery = "INSERT INTO TarifMalzeme (TarifID, MalzemeID, MalzemeMiktar) " +
                                                                      "VALUES (@TarifID, @MalzemeID, @MalzemeMiktar)";

                                    using (SqlCommand insertTarifMalzemeCommand = new SqlCommand(insertTarifMalzemeQuery, connection))
                                    {
                                        insertTarifMalzemeCommand.Parameters.AddWithValue("@TarifID", tarifID);
                                        insertTarifMalzemeCommand.Parameters.AddWithValue("@MalzemeID", yeniMalzemeID);
                                        insertTarifMalzemeCommand.Parameters.AddWithValue("@MalzemeMiktar", yeniMalzemeMiktari); 

                                        insertTarifMalzemeCommand.ExecuteNonQuery();
                                    }

                                    // Ekranda listeye eklenen malzemeyi göster
                                    TextBlock malzemeBlock = new TextBlock
                                    {
                                        Text = $"{yeniMalzemeAdi} ({yeniMalzemeMiktari} {yeniMalzemeBirimi})",
                                        FontSize = 16,
                                        Foreground = Brushes.White,
                                        Margin = new Thickness(5)
                                    };
                                    addedIngredientsPanel.Children.Add(malzemeBlock);
                                }
                                else
                                {
                                    MessageBox.Show("Malzeme eklenirken bir hata oluştu.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Bir hata oluştu: {ex.Message}");
                    }
                }
            }
        }



        private void ConfirmRemoveIngredient(int malzemeID)
        {
            MessageBoxResult result = MessageBox.Show("Bu malzemeyi silmek istediğinize emin misiniz?", "Malzemeyi Sil", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                silinenMalzemeler.Add(malzemeID); 

                // Malzeme sayfadan anlık olarak siliniyor
                foreach (var child in malzemeStackPanel.Children.OfType<StackPanel>())
                {
                    Button silButton = child.Children.OfType<Button>().FirstOrDefault(b => b.Tag != null && (int)b.Tag == malzemeID);
                    if (silButton != null)
                    {
                        malzemeStackPanel.Children.Remove(child); // Paneli kaldır
                        break;
                    }
                }
            }
        }




        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Bu tarifi silmek istediğinize emin misiniz?", "Tarifi Sil", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Adım: TarifMalzeme tablosundaki ilişkili malzemeleri sil
                    string deleteFromTarifMalzemeQuery = "DELETE FROM TarifMalzeme WHERE TarifID = @TarifID";
                    using (SqlCommand command1 = new SqlCommand(deleteFromTarifMalzemeQuery, connection))
                    {
                        command1.Parameters.AddWithValue("@TarifID", this.tarifID);  // Mevcut tarif ID'yi kullanıyoruz
                        command1.ExecuteNonQuery();
                    }

                    // 2. Adım: Tarifler tablosundan tarif kaydını sil
                    string deleteFromTariflerQuery = "DELETE FROM Tarifler WHERE TarifID = @TarifID";
                    using (SqlCommand command2 = new SqlCommand(deleteFromTariflerQuery, connection))
                    {
                        command2.Parameters.AddWithValue("@TarifID", this.tarifID);  // Mevcut tarif ID'yi kullanıyoruz
                        command2.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                // Silme işlemi tamamlandıktan sonra ana sayfaya geri dön
                MessageBox.Show("Tarif başarıyla silindi.");
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Window.GetWindow(this)?.Close();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Halihazırdaki düzenleme ayarları
            txtYemekAdi.Text = yemekAdiTextBlock.Text;
            txtTarif.Text = tarifTextBlock.Text;
            txtSure.Text = sureTextBlock.Text;

            yemekAdiTextBlock.Visibility = Visibility.Collapsed;
            tarifTextBlock.Visibility = Visibility.Collapsed;

            txtYemekAdi.Visibility = Visibility.Visible;
            txtTarif.Visibility = Visibility.Visible;

            sureTextBlock.Visibility = Visibility.Collapsed;
            txtSure.Visibility = Visibility.Visible;

            kategoriTextBlock.Visibility = Visibility.Collapsed;
            cmbKategori.Visibility = Visibility.Visible;

            ChangeImageButton.Visibility = Visibility.Visible;
            btnEdit.Visibility = Visibility.Collapsed;
            btnDelete.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;

            newIngredientPanel.Visibility = Visibility.Visible;
            addedIngredientsPanel.Visibility = Visibility.Visible;

            LoadMalzemeler(); // Mevcut malzeme listesini yükle

            
            foreach (var child in malzemeStackPanel.Children)
            {
                if (child is StackPanel panel)
                {
                    var malzemeTextBlock = panel.Children.OfType<TextBlock>().FirstOrDefault();
                    var malzemeTextBox = panel.Children.OfType<TextBox>().FirstOrDefault();
                    var btnRemove = panel.Children.OfType<Button>().FirstOrDefault();

                    if (malzemeTextBlock != null && malzemeTextBox != null && btnRemove != null)
                    {
                        malzemeTextBlock.Visibility = Visibility.Visible;
                        malzemeTextBox.Visibility = Visibility.Visible;
                        btnRemove.Visibility = Visibility.Visible; // "Sil" butonunu görünür yap
                    }
                }
            }

           
            foreach (var child in addedIngredientsPanel.Children)
            {
                if (child is StackPanel panel)
                {
                    var btnRemove = panel.Children.OfType<Button>().FirstOrDefault();
                    if (btnRemove != null)
                    {
                        btnRemove.Visibility = Visibility.Visible; // "Sil" butonunu görünür yap
                    }
                }
            }
        }





        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Kategori adına göre KategoriID'yi al
                    int kategoriID = 0;
                    if (cmbKategori.SelectedItem is ComboBoxItem selectedCategory)
                    {
                        kategoriID = (int)selectedCategory.Tag; // Tag özelliğinden ID'yi alıyoruz
                    }
                    else
                    {
                        MessageBox.Show("Geçersiz kategori seçimi.");
                        return;
                    }

                    // Tarif güncelleme sorgusu
                    string updateTarifQuery = @"
                UPDATE Tarifler
                SET TarifAdi = @TarifAdi, ResimYolu = @ResimYolu, Talimatlar = @Talimatlar, HazirlamaSuresi = @HazirlamaSuresi, KategoriID = @KategoriID
                WHERE TarifID = @TarifID";

                    using (SqlCommand command = new SqlCommand(updateTarifQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@TarifAdi", txtYemekAdi.Text);
                        command.Parameters.AddWithValue("@ResimYolu", isImageChanged ? currentImagePath : GetCurrentImagePath());
                        command.Parameters.AddWithValue("@Talimatlar", txtTarif.Text);
                        command.Parameters.AddWithValue("@HazirlamaSuresi", int.Parse(txtSure.Text));
                        command.Parameters.AddWithValue("@KategoriID", kategoriID);  // KategoriID burada kullanılacak
                        command.Parameters.AddWithValue("@TarifID", tarifID);
                        command.ExecuteNonQuery();
                    }

                    // Malzeme miktarlarını güncelle
                    foreach (var child in malzemeStackPanel.Children)
                    {
                        if (child is StackPanel panel)
                        {
                            var malzemeTextBox = panel.Children.OfType<TextBox>().FirstOrDefault();
                            var malzemeTextBlock = panel.Children.OfType<TextBlock>().FirstOrDefault();

                            if (malzemeTextBox != null && malzemeTextBlock != null)
                            {
                                string malzemeAdi = malzemeTextBlock.Text.Split('(')[0].Trim();
                                double newMiktar = double.Parse(malzemeTextBox.Text);

                                string updateIngredientQuery = @"
UPDATE TarifMalzeme
SET MalzemeMiktar = @MalzemeMiktar
WHERE TarifID = @TarifID AND MalzemeID = (SELECT TOP 1 MalzemeID FROM Malzemeler WHERE MalzemeAdi = @MalzemeAdi)";

                                using (SqlCommand command = new SqlCommand(updateIngredientQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@MalzemeMiktar", newMiktar);
                                    command.Parameters.AddWithValue("@TarifID", tarifID);
                                    command.Parameters.AddWithValue("@MalzemeAdi", malzemeAdi);
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    // Silinen malzemeleri veritabanından kaldırdığımız sorgumuz 
                    foreach (int malzemeID in silinenMalzemeler)
                    {
                        string deleteMalzemeQuery = "DELETE FROM TarifMalzeme WHERE TarifID = @TarifID AND MalzemeID = @MalzemeID";
                        using (SqlCommand command = new SqlCommand(deleteMalzemeQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@TarifID", tarifID);
                            command.Parameters.AddWithValue("@MalzemeID", malzemeID);
                            command.ExecuteNonQuery();
                        }
                    }

                    // Yeni eklenen malzemeleri eklediğimiz eklerkende duplicate kontrolü yaptığımız kısım
                    foreach (var malzeme in yeniEklenenMalzemeler)
                    {
                        string checkQuery = @"
SELECT COUNT(*) FROM TarifMalzeme 
WHERE TarifID = @TarifID AND MalzemeID = @MalzemeID";

                        using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection, transaction))
                        {
                            checkCommand.Parameters.AddWithValue("@TarifID", tarifID);
                            checkCommand.Parameters.AddWithValue("@MalzemeID", malzeme.Item1);

                            int count = (int)checkCommand.ExecuteScalar();

                            if (count > 0)
                            {
                                MessageBox.Show($"Bu tarifte zaten {malzeme.Item1} ID'li malzeme mevcut.");
                                continue;
                            }
                        }

                        // Yeni malzeme ekleme sorgusu
                        string insertQuery = "INSERT INTO TarifMalzeme (TarifID, MalzemeID, MalzemeMiktar) VALUES (@TarifID, @MalzemeID, @MalzemeMiktar)";
                        using (SqlCommand command = new SqlCommand(insertQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@TarifID", tarifID);
                            command.Parameters.AddWithValue("@MalzemeID", malzeme.Item1);
                            command.Parameters.AddWithValue("@MalzemeMiktar", malzeme.Item2);
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    MessageBox.Show("Tarif başarıyla güncellendi.");
                    LoadRecipeDetails(tarifID);

                    newIngredientPanel.Visibility = Visibility.Collapsed;
                    addedIngredientsPanel.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Bir hata oluştu: " + ex.Message);
                }
            }

            SwitchToViewMode(); // Düzenleme modundan görüntüleme moduna geç
        }



        private string GetCurrentImagePath()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ResimYolu FROM Tarifler WHERE TarifID = @TarifID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", tarifID);
                    return command.ExecuteScalar().ToString();
                }
            }
        }

        // Resmi değiştirmek için dosya seçme işlemi
        private void ChangeImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Image Files (*.jpg; *.png)|*.jpg;*.png";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                currentImagePath = dlg.FileName;  // Yeni resim dosya yolunu kaydet
                yemekImage.Source = new BitmapImage(new Uri(currentImagePath));  // Yeni resmi göster
                isImageChanged = true;  // Resmin değiştiğini belirle
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Değişiklikleri geri al
            silinenMalzemeler.Clear(); // Silinecek malzeme listesini temizle
            yeniEklenenMalzemeler.Clear(); // Eklenen malzemeleri temizle

            // Geçici eklenen malzemeleri arayüzden kaldır
            addedIngredientsPanel.Children.Clear();

            // Yeni malzeme ekleme panelini gizle
            newIngredientPanel.Visibility = Visibility.Collapsed;

            kategoriTextBlock.Visibility = Visibility.Visible;
            cmbKategori.Visibility = Visibility.Collapsed;

            // Sayfayı ilk haline geri döndür
            LoadRecipeDetails(tarifID); // Tarifi yeniden yükle ve tüm bilgileri sıfırla

            SwitchToViewMode(); // Görüntüleme moduna geri dön
        }





        private void SwitchToViewMode()
        {
            // Görüntüleme moduna geri dön
            yemekAdiTextBlock.Visibility = Visibility.Visible;
            tarifTextBlock.Visibility = Visibility.Visible;

            txtYemekAdi.Visibility = Visibility.Collapsed;
            txtTarif.Visibility = Visibility.Collapsed;

            sureTextBlock.Visibility = Visibility.Visible;
            txtSure.Visibility = Visibility.Collapsed;

            kategoriTextBlock.Visibility = Visibility.Visible;
            cmbKategori.Visibility = Visibility.Collapsed;
            // Resim değiştirme butonunu gizle
            ChangeImageButton.Visibility = Visibility.Collapsed;

            // Düğme görünürlüğünü ayarla
            btnEdit.Visibility = Visibility.Visible;
            btnDelete.Visibility = Visibility.Visible; // Sil butonunu görünür yapın
            btnSave.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;

            // Malzemeler görünüm moduna geçsin
            foreach (var child in malzemeStackPanel.Children)
            {
                if (child is StackPanel panel)
                {
                    var malzemeTextBlock = panel.Children.OfType<TextBlock>().FirstOrDefault();
                    var malzemeTextBox = panel.Children.OfType<TextBox>().FirstOrDefault();
                    var btnRemove = panel.Children.OfType<Button>().FirstOrDefault();

                    if (malzemeTextBlock != null && malzemeTextBox != null && btnRemove != null)
                    {
                        malzemeTextBlock.Visibility = Visibility.Visible; // TextBlock'u göster
                        malzemeTextBox.Visibility = Visibility.Collapsed; // TextBox'ı gizle
                        btnRemove.Visibility = Visibility.Collapsed; // Silme butonunu gizle
                    }
                }
            }

            // Resim değişikliği durumu sıfırlanır
            isImageChanged = false;
        }





      
        private void FavoriButton_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                if (isFavori)
                {
                    // Favorilerden kaldırma sorgumuz 
                    string deleteQuery = "DELETE FROM Favoriler WHERE TarifID = @TarifID";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@TarifID", tarifID);
                        command.ExecuteNonQuery();
                    }
                    kalpPath.Fill = Brushes.Transparent; // İçi boş kalp
                    kalpPath.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3A5F49")); // Yeşil çerçeve
                }
                else
                {
                    // Favorilere ekle
                    string insertQuery = "INSERT INTO Favoriler (TarifID) VALUES (@TarifID)";
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@TarifID", tarifID);
                        command.ExecuteNonQuery();
                    }
                    kalpPath.Fill = Brushes.Red; // İçi dolu kırmızı kalp
                    kalpPath.Stroke = Brushes.Red; // Kırmızı çerçeve
                }

                // Favori durumunu güncelle
                isFavori = !isFavori;
            }
        }



        // Favoriler tablosuna ekleme işlemi
        private void AddToFavoriler(string favoriAdi)
        {
            string insertQuery = "INSERT INTO Favoriler ( FavoriAdi) VALUES ( @FavoriAdi)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        // Parametreleri ekle

                        command.Parameters.AddWithValue("@FavoriAdi", favoriAdi); 

                        // Veritabanına kaydet
                        int result = command.ExecuteNonQuery();

                        
                        if (result > 0)
                        {
                            MessageBox.Show("Favori başarıyla kaydedildi!");
                        }
                        else
                        {
                            MessageBox.Show("Favori eklenemedi.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }

        // Favoriler tablosundan silme işlemi
        private void RemoveFromFavoriler(string favoriAdi)
        {
            string deleteQuery = "DELETE FROM Favoriler WHERE FavoriAdi = @FavoriAdi";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FavoriAdi", favoriAdi);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }

        private void BtnTarifEkle_Click(object sender, RoutedEventArgs e)
        {
            Frame navigationFrame = new Frame();
            RecipeAddPage recipeAddPage = new RecipeAddPage();
            navigationFrame.Content = recipeAddPage;
            this.Content = navigationFrame; 
        }
        private void ToggleSidebar_Checked(object sender, RoutedEventArgs e)
        {
            sidebarColumn.Width = new GridLength(230); // Sidebar genişliğini büyüt
            sidebarContent.Visibility = Visibility.Visible;
        }

        private void ToggleSidebar_Unchecked(object sender, RoutedEventArgs e)
        {
            sidebarColumn.Width = new GridLength(60); // Sidebar daralt
            sidebarContent.Visibility = Visibility.Collapsed;
        }

        private void AnaSayfaButton_Click(object sender, RoutedEventArgs e)
        {

            MainWindow mainWindow = new MainWindow();

            mainWindow.Show();

            Window.GetWindow(this)?.Close();
        }

        
        private void BtnFavoriler_Click(object sender, RoutedEventArgs e)
        {
            Frame navigationFrame = new Frame();
            FavoritesPage favoritesPage = new FavoritesPage();
            navigationFrame.Content = favoritesPage;
            this.Content = navigationFrame;

        }

        private void BtnStokIslemleri_Click(object sender, RoutedEventArgs e)
        {
            Frame navigationFrame = new Frame();
            StokPage stokPage = new StokPage();
            navigationFrame.Content = stokPage;
            this.Content = navigationFrame; 
        }

        private void LoadKategoriler()
        {
            cmbKategori.Items.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT KategoriID, KategoriAdi FROM Kategoriler";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        cmbKategori.Items.Add(new ComboBoxItem
                        {
                            Content = reader["KategoriAdi"].ToString(),
                            Tag = reader["KategoriID"]
                        });
                    }
                }
            }
        }




    }
}
