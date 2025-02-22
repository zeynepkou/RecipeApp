using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RecipeApp
{
    public partial class RecipeAddPage : Page
    {
        public RecipeAddPage()
        {
            InitializeComponent();
            LoadMalzemeler();
            LoadKategoriler();
        }
        private void AnaSayfaButton_Click(object sender, RoutedEventArgs e)
        {

            MainWindow mainWindow = new MainWindow();

            mainWindow.Show();

            Window.GetWindow(this)?.Close();
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

        /*  private void EkleKategoriler()
          {
              // Veritabanı bağlantı cümlesi
              string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";

              // Kategori ekleme sorgusu
              string insertQuery = "INSERT INTO Kategoriler (KategoriAdi) VALUES (@KategoriAdi)";

              // Eklemek istediğiniz kategoriler
              string[] kategoriler = { "Tatlı", "Ana Yemek", "Fast Food" };

              using (SqlConnection connection = new SqlConnection(connectionString))
              {
                  try
                  {
                      connection.Open();

                      foreach (string kategori in kategoriler)
                      {
                          using (SqlCommand command = new SqlCommand(insertQuery, connection))
                          {
                              // Parametreyi ekle
                              command.Parameters.AddWithValue("@KategoriAdi", kategori);

                              // Veritabanına kaydet
                              int result = command.ExecuteNonQuery();

                              // Başarı mesajı
                              if (result > 0)
                              {
                                  MessageBox.Show($"{kategori} başarıyla kaydedildi!");
                              }
                              else
                              {
                                  MessageBox.Show($"{kategori} eklenemedi.");
                              }
                          }
                      }
                  }
                  catch (Exception ex)
                  {
                      // Hata mesajı
                      MessageBox.Show("Hata: " + ex.Message);
                  }
              }
          }
        */
        // Seçilen resmin yolunu global bir değişkende tutalım
        private string selectedImagePath = "";

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {

            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|.jpg;.jpeg;.png;.bmp;*.gif";

            // Kullanıcının bir dosya seçmesini sağlıyoruz
            if (openFileDialog.ShowDialog() == true)
            {
                
                selectedImagePath = openFileDialog.FileName;

                
                selectedImage.Source = new BitmapImage(new Uri(selectedImagePath));
            }
        }
        List<string> malzemeAdListe = new List<string>();
        List<float> malzemeMiktarListe = new List<float>();
        private void KaydetButton_Click(object sender, RoutedEventArgs e)
        {
            // Tarif bilgilerini al
            string tarifTalimatlari = tarifTextBox.Text;
            string tarifAd = yemekAdiTextBox.Text;
            string kategoriAd = kategoriComboBox.Text;

            if (!int.TryParse(hazirlamaSuresiTextBox.Text, out int hazirlamaSuresi))
            {
                MessageBox.Show("Hazırlama süresini girin.");
                return;
            }

            if (string.IsNullOrEmpty(selectedImagePath))
            {
                MessageBox.Show("Lütfen bir resim seçin.");
                return;
            }

            // Resmi dosya sistemine belirli bir dizine kaydediyoruz
            string saveDirectory = @"C:\Users\HpNtb\OneDrive\Masaüstü\YAZLABLAR"; 
            string fileName = System.IO.Path.GetFileName(selectedImagePath); 
            string savePath = System.IO.Path.Combine(saveDirectory, fileName); 

            
            try
            {
                File.Copy(selectedImagePath, savePath, true);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Resim dosyası kopyalanamadı: " + ex.Message);
                return;
            }

            int kategoriID = GetKategoriIdByAd(kategoriAd);

            if (kategoriID == -1)
            {
                MessageBox.Show("Kategori bulunamadı.");
                return;
            }

            // Tarif ve resim yolunu kaydet
            KaydetTarifVeResim(tarifAd, kategoriID, hazirlamaSuresi, tarifTalimatlari, savePath);

            // Tarifi kaydettikten sonra TarifID'yi al
            int tarifID = GetTarifIdByAd(tarifAd); // TarifID'yi geri dönen fonksiyon yazmanız gerekecek

            // Tarif malzemelerini kaydet
            KaydetTarifMalzemeleri(tarifID, GetMalzemeIdsByAdList(malzemeVeMiktarListesi));

            
            foreach (var malzeme in malzemeAdListe)
            {
                Debug.WriteLine(malzeme);
            }
            foreach (var malzeme1 in malzemeVeMiktarListesi)
            {
                Debug.WriteLine(malzeme1);
            }

            
            gifPopup.IsOpen = true;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(6);
            timer.Tick += (s, args) =>
            {
                
                gifPopup.IsOpen = false;
                timer.Stop();

                MessageBox.Show("Tarif Başarıyla Kaydedildi");

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Window.GetWindow(this)?.Close();
            };
            timer.Start(); 
        }

        private void CloseGifPopup_Click(object sender, RoutedEventArgs e)
        {
            
            gifPopup.IsOpen = false;
        }

        List<string> malzemeListesi2 = new List<string>();
        // Kategori adından kategori ID'sini dönen fonksiyon
        private int GetKategoriIdByAd(string kategoriAd)
        {
            // Veritabanı bağlantı cümlesi
            string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";

            // Kategori adından kategori ID'sini çeken SQL sorgusu
            string query = "SELECT KategoriID FROM Kategoriler WHERE KategoriAdi = @KategoriAd";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                       
                        command.Parameters.AddWithValue("@KategoriAd", kategoriAd);

                       
                        object result = command.ExecuteScalar();

                        
                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                        else
                        {
                            
                            return -1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                    MessageBox.Show("Hata: " + ex.Message);
                    return -1;
                }
            }
        }
        private int GetTarifIdByAd(string tarifAd)
        {
            // Veritabanı bağlantı cümlesi
            string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";

            // Kategori adından kategori ID'sini çeken SQL sorgusu
            string query = "SELECT TarifID FROM Tarifler WHERE TarifAdi = @TarifAdi";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        
                        command.Parameters.AddWithValue("@TarifAdi", tarifAd);

                        
                        object result = command.ExecuteScalar();

                        
                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                        else
                        {
                            
                            return -1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                    MessageBox.Show("Hata: " + ex.Message);
                    return -1;
                }
            }
        }
        List<int> malzemeIds = new List<int>(); 
        private List<Tuple<int, float>> GetMalzemeIdsByAdList(List<Tuple<string, float, string>> malzemeVeMiktarListesi)
        {
            List<Tuple<int, float>> malzemeIdVeMiktarListesi = new List<Tuple<int, float>>();
            string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";
            string query = "SELECT MalzemeID FROM Malzemeler WHERE MalzemeAdi = @MalzemeAdi";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    foreach (var malzeme in malzemeVeMiktarListesi)
                    {
                        string malzemeAd = malzeme.Item1; 
                        float malzemeMiktari = malzeme.Item2;

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@MalzemeAdi", malzemeAd);
                            object result = command.ExecuteScalar();

                            if (result != null)
                            {
                                int malzemeId = Convert.ToInt32(result);
                                malzemeIdVeMiktarListesi.Add(new Tuple<int, float>(malzemeId, malzemeMiktari));
                            }
                            else
                            {
                                
                                malzemeIdVeMiktarListesi.Add(new Tuple<int, float>(-1, malzemeMiktari));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }

            return malzemeIdVeMiktarListesi;
        }


        // Tarif bilgilerini ve resim yolunu veritabanına kaydeden fonksiyon
        private void KaydetTarifVeResim(string tarifAd, int kategoriID, int hazirlamaSuresi, string tarifTalimatlari, string resimYolu)
        {
            // Veritabanı bağlantı cümlesi
            string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";

            // Aynı tarifin olup olmadığını kontrol eden SQL sorgusu
            string checkQuery = "SELECT COUNT(*) FROM Tarifler WHERE TarifAdi = @TarifAdi";

            // Tarif bilgilerini ve resim yolunu ekleyen SQL sorgusu
            string insertQuery = "INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu) VALUES (@TarifAdi, @KategoriID, @HazirlamaSuresi, @Talimatlar, @ResimYolu)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Aynı tarifin olup olmadığını kontrol et
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@TarifAdi", tarifAd);

                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Bu isimde bir tarif zaten mevcut. Lütfen farklı bir isim girin.");
                            return; // Kayıt eklenmeden işlem durdurulur
                        }
                    }

                    
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        
                        insertCommand.Parameters.AddWithValue("@TarifAdi", tarifAd);
                        insertCommand.Parameters.AddWithValue("@KategoriID", kategoriID);
                        insertCommand.Parameters.AddWithValue("@HazirlamaSuresi", hazirlamaSuresi);
                        insertCommand.Parameters.AddWithValue("@Talimatlar", tarifTalimatlari);
                        insertCommand.Parameters.AddWithValue("@ResimYolu", resimYolu);

                        
                        int result = insertCommand.ExecuteNonQuery();

                        
                        if (result > 0)
                        {
                            
                        }
                        else
                        {
                            MessageBox.Show("Kayıt eklenemedi.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }




        // Sınıf düzeyinde bir liste tanımladık
        private List<string> malzemeListesi = new List<string>();
        private List<string> kategoriListesi = new List<string>();
        
        // Malzeme adı, miktarı ve birimini tutacak liste
        List<Tuple<string, float, string>> malzemeVeMiktarListesi = new List<Tuple<string, float, string>>();


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

                            string checkQuery = "SELECT COUNT(*) FROM Malzemeler WHERE MalzemeAdi = @MalzemeAdi";
                            using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                            {
                                checkCommand.Parameters.AddWithValue("@MalzemeAdi", yeniMalzemeAdi);
                                int count = (int)checkCommand.ExecuteScalar();

                                if (count > 0)
                                {
                                    MessageBox.Show("Bu isimde bir malzeme zaten mevcut. Lütfen farklı bir malzeme adı girin.");
                                    return;
                                }
                            }

                            string insertQuery = "INSERT INTO Malzemeler (MalzemeAdi, ToplamMiktar, MalzemeBirim, BirimFiyat) " +
                                                 "VALUES (@MalzemeAdi, 0, @MalzemeBirimi, 0)";
                            using (SqlCommand command = new SqlCommand(insertQuery, connection))
                            {
                                command.Parameters.AddWithValue("@MalzemeAdi", yeniMalzemeAdi);
                                command.Parameters.AddWithValue("@MalzemeBirimi", yeniMalzemeBirimi);

                                int result = command.ExecuteNonQuery();

                                if (result > 0)
                                {
                                    MessageBox.Show("Malzeme başarıyla eklendi.");
                                    malzemeVeMiktarListesi.Add(new Tuple<string, float, string>(yeniMalzemeAdi, yeniMalzemeMiktari, yeniMalzemeBirimi));
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

                    malzemeListesi2.Add($"{yeniMalzemeAdi} - {yeniMalzemeMiktari} {yeniMalzemeBirimi}");
                    malzemeComboBox.ItemsSource = null;
                    malzemeComboBox.ItemsSource = malzemeListesi2;

                   
                    StackPanel malzemePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

                    TextBlock malzemeTextBlock = new TextBlock
                    {
                        Text = $"•{yeniMalzemeAdi} - {yeniMalzemeMiktari} {yeniMalzemeBirimi}",
                        FontSize = 16,
                        Foreground = Brushes.White,
                        Margin = new Thickness(5)
                    };

                    // Sil butonunu oluştur
                    Button silButton = new Button
                    {
                        Content = "Sil",
                        FontSize = 12,
                        Width = 60, // Genişliği artırıyoruz
                        Margin = new Thickness(10, 0, 0, 0),
                        Background = Brushes.Red, // Arka plan rengi
                        Foreground = Brushes.White, // Yazı rengi
                        Padding = new Thickness(5, 3, 5, 3), // İç boşluk
                        Cursor = Cursors.Hand, // Fareyle üzerine gelindiğinde el imleci
                        Tag = yeniMalzemeAdi // Malzeme adını butona ek olarak kaydediyoruz
                    };

                    // Mouse üzerinde butona gelindiğinde değişiklik yapma
                    silButton.MouseEnter += (s, ev) =>
                    {
                        silButton.Background = Brushes.DarkRed; 
                    };

                    silButton.MouseLeave += (s, ev) =>
                    {
                        silButton.Background = Brushes.Red; 
                    };

                    
                    silButton.Click += (s, ev) =>
                    {
                        string malzemeAdi = (string)((Button)s).Tag;

                        
                        var itemToRemove = malzemeVeMiktarListesi.FirstOrDefault(m => m.Item1 == malzemeAdi);
                        if (itemToRemove != null)
                        {
                            malzemeVeMiktarListesi.Remove(itemToRemove);
                        }

                        // StackPanel'den öğeyi kaldır
                        malzemeListesiStackPanel.Children.Remove(malzemePanel);

                        // ComboBox'tan öğeyi kaldır
                        malzemeListesi2.Remove($"{malzemeAdi} - {yeniMalzemeMiktari} {yeniMalzemeBirimi}");
                        malzemeComboBox.ItemsSource = null;
                        malzemeComboBox.ItemsSource = malzemeListesi2;
                    };

                   
                    malzemePanel.Children.Add(malzemeTextBlock);
                    malzemePanel.Children.Add(silButton);

                   
                    malzemeListesiStackPanel.Children.Add(malzemePanel);

                }
            }
        }




        private void malzemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (malzemeComboBox.SelectedItem != null)
            {
                string selectedMalzeme = malzemeComboBox.SelectedItem.ToString();
                string malzemeBirimi = "";

                // Veritabanından malzeme birimini çekme
                try
                {
                    using (SqlConnection connection = new SqlConnection("Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True"))
                    {
                        connection.Open();
                        string query = "SELECT MalzemeBirim FROM Malzemeler WHERE MalzemeAdi = @MalzemeAdi";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@MalzemeAdi", selectedMalzeme);
                            var result = command.ExecuteScalar();
                            malzemeBirimi = result != null ? result.ToString() : "birim";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Bir hata oluştu: {ex.Message}");
                    return;
                }

               
                TextBox miktarTextBox = new TextBox
                {
                    Width = 100,
                    FontSize = 16,
                    Margin = new Thickness(5),
                    Name = "MiktarTextBox_" + selectedMalzeme.Replace(" ", "_").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ı", "i").Replace("ö", "o").Replace("ç", "c")
                };


               
                TextBlock malzemeTextBlock = new TextBlock
                {
                    Text = $"•{selectedMalzeme} - {malzemeBirimi}",
                    FontSize = 16,
                    Foreground = Brushes.White,
                    Margin = new Thickness(5)
                };

                
                StackPanel malzemeEntryStackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5)
                };

                
                Button silButton = new Button
                {
                    Content = "Sil",
                    FontSize = 12,
                    Width = 60, 
                    Margin = new Thickness(10, 0, 0, 0),
                    Background = Brushes.Red, 
                    Foreground = Brushes.White, 
                    Padding = new Thickness(5, 3, 5, 3), 
                    Cursor = Cursors.Hand 
                };

                
                silButton.MouseEnter += (s, ev) =>
                {
                    silButton.Background = Brushes.DarkRed; 
                };

                silButton.MouseLeave += (s, ev) =>
                {
                    silButton.Background = Brushes.Red; 
                };

               
                silButton.Click += (s, ev) =>
                {
                    // Silinecek StackPanel
                    StackPanel stackPanelToRemove = (StackPanel)((Button)s).Tag;

                    // StackPanel'i sil
                    malzemeListesiStackPanel.Children.Remove(stackPanelToRemove);

                    // ComboBox'tan öğeyi kaldır
                    malzemeListesi2.Remove($"{selectedMalzeme} - {miktarTextBox.Text} {malzemeBirimi}");
                    malzemeComboBox.ItemsSource = null;
                    malzemeComboBox.ItemsSource = malzemeListesi2;

                    // Listeyi güncelle
                    var itemToRemove = malzemeVeMiktarListesi.FirstOrDefault(m => m.Item1 == selectedMalzeme);
                    if (itemToRemove != null)
                    {
                        malzemeVeMiktarListesi.Remove(itemToRemove);
                    }
                };

                
                malzemeEntryStackPanel.Children.Add(malzemeTextBlock);
                malzemeEntryStackPanel.Children.Add(miktarTextBox);
                malzemeEntryStackPanel.Children.Add(silButton);

                // Sil butonunun Tag'ini ayarla
                silButton.Tag = malzemeEntryStackPanel; // Malzeme giriş panelini Tag olarak kaydediyoruz

                // StackPanel'i malzeme listesine ekle
                malzemeListesiStackPanel.Children.Add(malzemeEntryStackPanel);

                // Miktar değişiminde listeyi güncelle
                miktarTextBox.TextChanged += (s, ev) =>
                {
                    if (float.TryParse(miktarTextBox.Text, out float miktar))
                    {
                        var existingMalzeme = malzemeVeMiktarListesi.FirstOrDefault(m => m.Item1 == selectedMalzeme);
                        if (existingMalzeme != null)
                        {
                            malzemeVeMiktarListesi.Remove(existingMalzeme);
                            malzemeVeMiktarListesi.Add(new Tuple<string, float, string>(selectedMalzeme, miktar, malzemeBirimi));
                        }
                        else
                        {
                            malzemeVeMiktarListesi.Add(new Tuple<string, float, string>(selectedMalzeme, miktar, malzemeBirimi));
                        }
                    }
                    else
                    {
                        MessageBox.Show("Lütfen geçerli bir miktar girin.");
                    }
                };
            }
        }





        // + butonuna tıklanınca TextBox'ı görünür yapar
        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            newCategoryTextBox.Visibility = Visibility.Visible; // TextBox'ı görünür yapar
            newCategoryTextBox.Focus(); // Odağı TextBox'a alır
        }

        

        private void newCategoryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string newCategory = newCategoryTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(newCategory))
                {
                    // Veritabanına kategori eklemek için fonksiyon çağrısı
                    AddCategoryToDatabase(newCategory);

                    // Eğer başarıyla veritabanına eklendiyse kategori listesine ekle
                    if (!kategoriListesi.Contains(newCategory))
                    {
                        kategoriListesi.Add(newCategory); 
                        kategoriComboBox.ItemsSource = null;
                        kategoriComboBox.ItemsSource = kategoriListesi; 
                        kategoriComboBox.SelectedItem = newCategory;
                    }

                    newCategoryTextBox.Text = string.Empty; 
                    newCategoryTextBox.Visibility = Visibility.Collapsed; 
                }
            }
        }

        private void AddCategoryToDatabase(string category)
        {
            using (SqlConnection connection = new SqlConnection("Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True"))
            {
                try
                {
                    connection.Open(); 

                    // Aynı isimde bir kategori olup olmadığını kontrol et
                    string checkQuery = "SELECT COUNT(*) FROM Kategoriler WHERE KategoriAdi = @KategoriAdi";

                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@KategoriAdi", category);

                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Bu isimde bir kategori zaten mevcut. Lütfen farklı bir isim girin.");
                            return; 
                        }
                    }

                    // Eğer kategori mevcut değilse ekleme işlemini gerçekleştir
                    string query = "INSERT INTO Kategoriler (KategoriAdi) VALUES (@KategoriAdi)";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@KategoriAdi", category);
                        cmd.ExecuteNonQuery(); // Sorguyu çalıştır
                    }

                    // Başarıyla veritabanına eklendikten sonra kategoriyi listeye ekle
                    LoadCategoriesIntoComboBox(); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veritabanına kategori eklerken hata oluştu: " + ex.Message);
                }
                finally
                {
                    connection.Close(); 
                }
            }
        }


        // Kategorileri ComboBox'a yükleyen fonksiyon
        private void LoadCategoriesIntoComboBox()
        {
            kategoriListesi.Clear(); // Eski listeyi temizle

            using (SqlConnection connection = new SqlConnection("Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True"))
            {
                try
                {
                    connection.Open(); // Bağlantıyı aç

                    string query = "SELECT KategoriAdi FROM Kategoriler"; // Kategoriler tablosundan verileri al

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            kategoriListesi.Add(reader["KategoriAdi"].ToString()); // Her bir kategoriyi listeye ekle
                        }
                    }

                    kategoriComboBox.ItemsSource = null; 
                    kategoriComboBox.ItemsSource = kategoriListesi; 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kategorileri yüklerken hata oluştu: " + ex.Message);
                }
                finally
                {
                    connection.Close(); 
                }
            }
        }


        private void LoadMalzemeler()
        {
            string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";
            string query = "SELECT MalzemeAdi FROM Malzemeler";

            malzemeListesi2.Clear(); // Öncelikle listeyi temizleyin

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    malzemeListesi2.Add(reader["MalzemeAdi"].ToString());

                }

                reader.Close();
            }

            // Malzemeleri ComboBox'a ekleyin
            malzemeComboBox.ItemsSource = malzemeListesi2;
        }
        private void LoadKategoriler()
        {
            string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";
            string query = "SELECT KategoriID, KategoriAdi FROM Kategoriler"; // Kategori ID ve Ad'ını seçin



            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    kategoriListesi.Add(reader["KategoriAdi"].ToString());
                }

                reader.Close();
            }

            // Kategorileri ComboBox'a ekleyin
            kategoriComboBox.ItemsSource = kategoriListesi;
        }

        private void kategoriComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            if (kategoriComboBox.SelectedItem != null)
            {
                
                string selectedKategoriAdi = kategoriComboBox.SelectedItem.ToString();

                // Kategori adıyla ilgili işlemler yapabilirsiniz
                MessageBox.Show($"Seçilen Kategori: {selectedKategoriAdi}"); 
            }
        }
        private void KaydetTarifMalzemeleri(int tarifID, List<Tuple<int, float>> malzemeListesi)
        {
            // Veritabanı bağlantı cümlesi
            string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";

            // Tarif malzemelerini ekleyecek SQL sorgusu
            string insertQuery = "INSERT INTO TarifMalzeme (TarifID, MalzemeID, MalzemeMiktar) VALUES (@TarifID, @MalzemeID, @MalzemeMiktar)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    
                    foreach (var malzeme in malzemeListesi)
                    {
                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@TarifID", tarifID);
                            command.Parameters.AddWithValue("@MalzemeID", malzeme.Item1); 
                            command.Parameters.AddWithValue("@MalzemeMiktar", malzeme.Item2); 

                           
                            command.ExecuteNonQuery();
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
            Frame navigationFrame = new Frame();
            StokPage stokPage = new StokPage();
            navigationFrame.Content = stokPage;
            this.Content = navigationFrame;
        }

        private void BtnFavoriler_Click(object sender, RoutedEventArgs e)
        {
            Frame navigationFrame = new Frame();
            FavoritesPage favoritesPage = new FavoritesPage();
            navigationFrame.Content = favoritesPage;
            this.Content = navigationFrame; // Ana pencere içeriğini Frame ile değiştiriyoruz

        }

        private void hazirlamaSuresiTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void BtnTarifEkle_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}