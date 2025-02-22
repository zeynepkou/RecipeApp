using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Data.SqlClient;
using System.Windows.Media.Imaging;
using System.Formats.Tar;
using System.Diagnostics;

namespace RecipeApp
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";
        private List<int> selectedIngredientIds = new List<int>();
        private bool isIngredientSearch = false; // İlk başta arama yapılmamış
        private List<Tuple<int, string, string, double>> allRecipes; 
        private List<Tuple<int, string, string, double>> filteredRecipes; // Filtrelenmiş tarifler listesi
        private bool card = false;

        public MainWindow()
        {
            InitializeComponent();

            // Günün yemeğini göster
            List<Tuple<int, string, string, double>> tarifler = GetTarifAdlari(); // Tuple olarak alıyoruz
            ShowRandomTarifOfTheDay(tarifler);
            AdjustSidebar(true); // Sidebar başlangıçta geniş olarak ayarlanıyor
            //DatabaseHelper databaseHelper = new DatabaseHelper(connectionString);
            //databaseHelper.CreateTable2();
            // Uygulama başlatıldığında tüm tarifleri göster
            LoadAllTarifler();

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

        // Tüm tarifleri veritabanından getir ve ekrana yerleştir
        public void LoadAllTarifler()
        {
            allRecipes = GetTarifAdlari(); // Veritabanından tarifleri al
            filteredRecipes = new List<Tuple<int, string, string, double>>(allRecipes); // Başlangıçta tüm tarifler filtrelenmiş gibi davranır

            DisplayRecipes(filteredRecipes); // Tarifi ekrana yazdır
        }


        // Tarif kartlarını ekrana yerleştir
        public void DisplayRecipes(List<Tuple<int, string, string, double>> recipes)
        {
            wrapPanel.Children.Clear(); // Paneli temizle
            foreach (var recipe in recipes)
            {
                Border recipeCard = CreateTarifCard(recipe.Item1, recipe.Item2, recipe.Item3, recipe.Item4); // Eşleşme yüzdesini de gönderiyoruz
                wrapPanel.Children.Add(recipeCard);
            }
        }


        // Tarifleri veritabanından al
        public List<Tuple<int, string, string, double>> GetTarifAdlari()
        {
            List<Tuple<int, string, string, double>> tarifler = new List<Tuple<int, string, string, double>>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT TarifID, TarifAdi, ResimYolu FROM Tarifler";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int tarifID = reader.GetInt32(0);
                        string tarifAdi = reader.GetString(1);
                        string resimYolu = reader.IsDBNull(2) ? "" : reader.GetString(2);

                        // İlk başta eşleşme yüzdesi olmadığı için 0.0 olarak ekliyoruz
                        tarifler.Add(Tuple.Create(tarifID, tarifAdi, resimYolu, 0.0));
                    }
                }
            }

            return tarifler;
        }

        private Border CreateTarifCard(int tarifID, string tarifAdi, string resimYolu, double? eslesmeYuzdesi = null, double width = 230)
        {
            bool isFavorite = IsRecipeFavorite(tarifID);

            Border border = new Border
            {
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(15), // Kartın tüm köşeleri 15 derece oval
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4F4F4")),
                Width = width,
                Height = 400,
                Margin = new Thickness(10)
            };

            StackPanel stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Resim ekleme
            Image image = new Image
            {
                Height = 175, // Kartın üst kısmına tam oturacak şekilde ayarlayın
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Stretch = Stretch.Fill // Resmin alanı tamamen doldurmasını sağlar
            };

            // Eğer genişlik 210 ise, sağ ve sol üst köşeleri 15 derece oval yap
            double clipWidth = (width == 210) ? 210 : 230;
            image.Clip = new RectangleGeometry(new Rect(0, 0, clipWidth, 175), 15, 15);


            // Resim dosyasını kontrol et ve ekle
            try
            {
                if (!string.IsNullOrEmpty(resimYolu) && System.IO.File.Exists(resimYolu))
                {
                    image.Source = new BitmapImage(new Uri(resimYolu, UriKind.Absolute));
                }
                else
                {
                    image.Source = new BitmapImage(new Uri("C:\\Lab\\resim\\elma.jpg", UriKind.Absolute));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Resim yüklenirken hata oluştu: {ex.Message}");
                image.Source = new BitmapImage(new Uri("C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\nohut.jpg", UriKind.Absolute));
            }

            double toplamMaliyet = EksikMalzeme(tarifID, out bool isEksikMalzemeVar);
            border.BorderBrush = isEksikMalzemeVar ? Brushes.Red : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3A5F49"));
            border.BorderThickness = new Thickness(2); // Çerçeve kalınlığını 2 yapar

            // Tarif adı ve kalp ikonu ortalanmış şekilde bir Grid kullanarak düzenleyin
            Grid titleGrid = new Grid
            {
                Margin = new Thickness(10, 5, 10, 0)
            };
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock tarifAdBlock = new TextBlock
            {
                Text = tarifAdi,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(tarifAdBlock, 0);

            TextBlock favoriteIcon = new TextBlock
            {
                Text = isFavorite ? "♥" : "♡",
                FontSize = 40,
                Foreground = isFavorite ? Brushes.Red : Brushes.Green,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, -16, 0, 0)
            };
            Grid.SetColumn(favoriteIcon, 1);

            titleGrid.Children.Add(tarifAdBlock);
            titleGrid.Children.Add(favoriteIcon);

            stackPanel.Children.Add(image);
            stackPanel.Children.Add(titleGrid);

            // Hazırlanma süresi gösterimi
            int preparationTime = GetRecipeTime(tarifID);
            Border prepTimeBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3A5F49")),
                CornerRadius = new CornerRadius(20),
                Width = 100,
                Height = 35,
                Margin = new Thickness(10, 5, 10, 0),
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            StackPanel prepTimePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // MaterialDesign PackIcon ile saat ikonu ekleme
            var timeIcon = new MaterialDesignThemes.Wpf.PackIcon
            {
                Kind = MaterialDesignThemes.Wpf.PackIconKind.Clock,
                Foreground = Brushes.White,
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock prepTimeText = new TextBlock
            {
                Text = $"{preparationTime} dk",
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };

            prepTimePanel.Children.Add(timeIcon);
            prepTimePanel.Children.Add(prepTimeText);
            prepTimeBorder.Child = prepTimePanel;

            stackPanel.Children.Add(prepTimeBorder);

            StackPanel bottomPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(10, 5, 10, 5)
            };

            Button viewRecipeButton = new Button
            {
                Content = "Tarifi Görüntüle",
                Width = 150,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A5F49")),
                Foreground = Brushes.White,
                Margin = new Thickness(0, 5, 0, 0),
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            viewRecipeButton.Click += (sender, args) =>
            {
                RecipeDetailPage recipeDetailPage = new RecipeDetailPage(tarifID);
                this.Content = recipeDetailPage;
            };

            TextBlock maliyetBlock = new TextBlock
            {
                Text = isSortedByCost
                    ? $"Toplam Maliyet: {GetRecipeCost(tarifID)} TL"
                    : isEksikMalzemeVar
                        ? $"Eksik Malzeme Maliyeti: {toplamMaliyet} TL"
                        : "Tüm Malzemeler Yeterli",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = isEksikMalzemeVar ? Brushes.Red : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3A5F49")),
                Margin = new Thickness(10, 5, 10, 5),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };

            bottomPanel.Children.Add(viewRecipeButton);
            bottomPanel.Children.Add(maliyetBlock);

            stackPanel.Children.Add(bottomPanel);

            // Eşleşme yüzdesi en alta ekleniyor
            if (isIngredientSearch && eslesmeYuzdesi > 0)
            {
                TextBlock eslesmeYuzdesiBlock = new TextBlock
                {
                    Text = $"Eşleşme: %{eslesmeYuzdesi.Value:F2}",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(10, 5, 10, 5),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                stackPanel.Children.Add(eslesmeYuzdesiBlock);
            }

            border.Child = stackPanel;

            return border;
        }





        public double EksikMalzeme(int tarifId, out bool isEksikMalzemeVar)
        {
            double toplamMaliyet = 0;
            isEksikMalzemeVar = false; // Varsayılan olarak eksik malzeme yok

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
SELECT tm.MalzemeID, m.MalzemeAdi, tm.MalzemeMiktar, m.ToplamMiktar, m.BirimFiyat,
       CASE 
           WHEN tm.MalzemeMiktar > m.ToplamMiktar THEN 'Eksik'
           ELSE 'Yeterli'
       END AS Durum
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
                        double birimFiyat = Convert.ToDouble(reader["BirimFiyat"]);
                        string durum = reader["Durum"].ToString();

                        // Eğer malzeme eksikse maliyeti hesapla
                        if (durum == "Eksik")
                        {
                            double eksikMiktar = gerekenMiktar - mevcutMiktar;
                            double maliyet = eksikMiktar * birimFiyat;
                            toplamMaliyet += maliyet;

                            isEksikMalzemeVar = true; // En az bir eksik malzeme var
                            Debug.WriteLine($"Eksik Malzeme: {malzemeAdi} | Eksik Miktar: {eksikMiktar} | Maliyet: {maliyet} TL");
                        }

                        // Her tarif kartı için UI oluşturma (örnek olarak konsolda yazdırılıyor)
                        Debug.WriteLine($"Malzeme: {malzemeAdi} | Gereken: {gerekenMiktar} | Mevcut: {mevcutMiktar}");
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

            return toplamMaliyet;
        }

        // Tarif Ekle butonuna tıklandığında çalışan event
        private void BtnTarifEkle_Click(object sender, RoutedEventArgs e)
        {

            RecipeAddPage recipeAddPage = new RecipeAddPage();

            MainFrame.Navigate(recipeAddPage);
        }

        private void BtnStokIslemleri_Click(object sender, RoutedEventArgs e)
        {
            StokPage stokPage = new StokPage();
            this.Content = stokPage;
        }

        // Favoriler butonuna tıklandığında
        private void BtnFavoriler_Click(object sender, RoutedEventArgs e)
        {
            FavoritesPage favoritesPage = new FavoritesPage(); // Favoriler sayfasına geçiş
            this.Content = favoritesPage;
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

        // Arama TextBox'ına tıklandığında içeriği temizleme
        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            txtMainSearch.Text = string.Empty;
        }

        // Arama butonuna tıklandığında

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtMainSearch.Text.Trim().ToLower();

            // Sadece tarif adı üzerinde arama yap
            List<Tuple<int, string, string, double>> aramaSonuclari = AramaYap(searchText);

            // Arama sonuçlarını filteredRecipes listesine kaydet
            filteredRecipes = aramaSonuclari;

            // Arama sonuçlarını ekrana yazdır
            DisplayRecipes(filteredRecipes);
        }

        public List<Tuple<int, string, string, double>> AramaYap(string searchText)
        {
            List<Tuple<int, string, string, double>> sonucTarifler = new List<Tuple<int, string, string, double>>();
            searchText = "%" + searchText.ToLower() + "%"; // Arama metnini LIKE için hazırlıyoruz

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT TarifID, TarifAdi, ResimYolu
            FROM Tarifler
            WHERE LOWER(TarifAdi) LIKE @searchText";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Arama terimini parametre olarak ekleyelim
                    command.Parameters.AddWithValue("@searchText", searchText);

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int tarifID = reader.GetInt32(0);
                        string tarifAdi = reader.GetString(1);
                        string resimYolu = reader.IsDBNull(2) ? "" : reader.GetString(2);

                        sonucTarifler.Add(Tuple.Create(tarifID, tarifAdi, resimYolu, 0.0)); // Başlangıçta eşleşme yüzdesi yok, 0.0
                    }
                }
            }

            return sonucTarifler;
        }



        // En hızlıdan en yavaşa sıralama
        private void SortByTimeAsc_Click(object sender, RoutedEventArgs e)
        {
            filteredRecipes = filteredRecipes.OrderBy(recipe => GetRecipeTime(recipe.Item1)).ToList();
            DisplayRecipes(filteredRecipes);
        }

        // En yavaştan en hızlıya sıralama
        private void SortByTimeDesc_Click(object sender, RoutedEventArgs e)
        {
            filteredRecipes = filteredRecipes.OrderByDescending(recipe => GetRecipeTime(recipe.Item1)).ToList();
            DisplayRecipes(filteredRecipes);
        }

        // Tarifin süresini veritabanından alan fonksiyon
        private int GetRecipeTime(int recipeId)
        {
            int time = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT HazirlamaSuresi FROM Tarifler WHERE TarifID = @TarifID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", recipeId);
                    time = (int)command.ExecuteScalar();
                }
            }

            return time;
        }


        // Maliyeti hesaplayan metod

        private double GetRecipeCost(int recipeId)
        {
            // Bu örnekte SQL ile maliyeti hesapladığımızı varsayıyoruz
            double toplamMaliyet = 0;

            string query = @"
        SELECT SUM(tm.MalzemeMiktar * m.BirimFiyat) AS ToplamMaliyet
        FROM TarifMalzeme tm
        JOIN Malzemeler m ON tm.MalzemeID = m.MalzemeID
        WHERE tm.TarifID = @TarifID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TarifID", recipeId);
                connection.Open();

                object result = command.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    toplamMaliyet = Convert.ToDouble(result);
                }
            }

            return toplamMaliyet;
        }

        private bool isSortedByCost = false;

        // En ucuzdan en pahalıya sıralama
        private void SortByCostAsc_Click(object sender, RoutedEventArgs e)
        {
            isSortedByCost = true;  // Bayrağı güncelle
            filteredRecipes = filteredRecipes.OrderBy(recipe => GetRecipeCost(recipe.Item1)).ToList();
            DisplayRecipes(filteredRecipes);
        }

        // En pahalıdan en ucuza sıralama
        private void SortByCostDesc_Click(object sender, RoutedEventArgs e)
        {
            isSortedByCost = true;  // Bayrağı güncelle
            filteredRecipes = filteredRecipes.OrderByDescending(recipe => GetRecipeCost(recipe.Item1)).ToList();
            DisplayRecipes(filteredRecipes);
        }
       

        private void BtnSort_Click(object sender, RoutedEventArgs e)
        {
            sortPopup.IsOpen = true; // Popup'ı aç
        }

        
        

        // Arama yapıldığında sonuçları filtrele
        private void BtnIngredientSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtIngredientSearch.Text.ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                // Eğer arama kutusu boşsa, tüm malzemeleri getir
                LoadIngredients();
            }
            else
            {
                // Arama metnine göre malzemeleri filtrele
                FilterIngredients(searchText);
            }
            ingredientPopup.IsOpen = true;
        }

        // Tüm malzemeleri yükle
        public void LoadIngredients()
        {
            ingredientCheckList.Children.Clear(); // Mevcut listeyi temizle

            List<Tuple<int, string>> ingredients = GetIngredientsFromDatabase();
            foreach (var ingredient in ingredients)
            {
                CheckBox checkBox = new CheckBox
                {
                    Content = ingredient.Item2,
                    Tag = ingredient.Item1, // Tag to store the MalzemeID
                    FontSize = 16,
                    Margin = new Thickness(20, 5, 0, 5)
                };
                checkBox.Checked += IngredientCheckBox_Checked;
                checkBox.Unchecked += IngredientCheckBox_Unchecked;
                ingredientCheckList.Children.Add(checkBox);
            }
        }

        // Filtreleme işlemi
        public void FilterIngredients(string searchText)
        {
            ingredientCheckList.Children.Clear(); // Mevcut listeyi temizle

            List<Tuple<int, string>> ingredients = GetIngredientsFromDatabase();
            foreach (var ingredient in ingredients)
            {
                if (ingredient.Item2.ToLower().Contains(searchText))
                {
                    CheckBox checkBox = new CheckBox
                    {
                        Content = ingredient.Item2,
                        Tag = ingredient.Item1, // Tag to store the MalzemeID
                        Margin = new Thickness(10, 5, 0, 5)
                    };
                    checkBox.Checked += IngredientCheckBox_Checked;
                    checkBox.Unchecked += IngredientCheckBox_Unchecked;
                    ingredientCheckList.Children.Add(checkBox);
                }
            }
        }

        // Veritabanından malzemeleri al
        public List<Tuple<int, string>> GetIngredientsFromDatabase()
        {
            List<Tuple<int, string>> ingredients = new List<Tuple<int, string>>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT MalzemeID, MalzemeAdi FROM Malzemeler";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        ingredients.Add(Tuple.Create(id, name));
                    }
                }
            }
            return ingredients;
        }

        // Malzeme Checkbox'ı seçildiğinde
        private void IngredientCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int ingredientId = (int)checkBox.Tag;
            string ingredientName = checkBox.Content.ToString();

            // Kullanıcı tarafından seçilen malzemeyi listeye ekle
            if (!selectedIngredientIds.Contains(ingredientId))
            {
                selectedIngredientIds.Add(ingredientId);

                TextBlock selectedIngredient = new TextBlock
                {
                    Text = ingredientName,
                    FontSize = 16,
                    Margin = new Thickness(10, 5, 0, 5)
                };

                selectedIngredientList.Children.Add(selectedIngredient);
            }
        }

        // Malzeme Checkbox'ı seçimi kaldırıldığında
        private void IngredientCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            int ingredientId = (int)checkBox.Tag;
            string ingredientName = checkBox.Content.ToString();

            // Seçilen malzemeyi listeden çıkar
            if (selectedIngredientIds.Contains(ingredientId))
            {
                selectedIngredientIds.Remove(ingredientId);

                // Seçilenler panelinden ilgili malzemeyi kaldır
                foreach (TextBlock textBlock in selectedIngredientList.Children)
                {
                    if (textBlock.Text == ingredientName)
                    {
                        selectedIngredientList.Children.Remove(textBlock);
                        break;
                    }
                }
            }
        }

        private void BtnSelectedSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchText = txtSelectedSearch.Text.ToLower();
            foreach (TextBlock textBlock in selectedIngredientList.Children)
            {
                textBlock.Visibility = textBlock.Text.ToLower().Contains(searchText) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void TxtIngredientSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            txtIngredientSearch.Text = string.Empty;
        }

        private void TxtSelectedSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSelectedSearch.Text = string.Empty;
        }

        // "Ara" butonuna tıklandığında yapılacak işlemler
        private void BtnPopupAra_Click(object sender, RoutedEventArgs e)
        {
            isIngredientSearch = true;

            // Eşleşme yüzdesine göre arama yap
            List<Tuple<int, string, string, double>> matchedRecipes = AramaSonuclariVeEslesmeHesapla(filteredRecipes);

            // filteredRecipes listesi eşleşen tarifler ve eşleşme yüzdeleri ile güncelleniyor
            filteredRecipes = matchedRecipes;

            wrapPanel.Children.Clear();

            foreach (var recipe in matchedRecipes)
            {
                Border recipeCard = CreateTarifCard(recipe.Item1, recipe.Item2, recipe.Item3, recipe.Item4);
                wrapPanel.Children.Add(recipeCard);
            }
        }




        // "Seçimi Temizle" butonuna tıklandığında yapılacak işlemler
        private void BtnClearSelection_Click(object sender, RoutedEventArgs e)
        {
            // Seçili malzeme ID'lerini temizle
            selectedIngredientIds.Clear();

            // Seçilen malzemeleri ekrandan temizle
            selectedIngredientList.Children.Clear();

            // Arama kutularını temizle ve placeholder yazılarını yeniden yaz
            txtIngredientSearch.Text = "Malzeme Ara...";
            txtSelectedSearch.Text = "Seçilen Malzeme Ara...";

            // IngredientCheckList'teki tüm CheckBox'ların işaretini kaldır
            foreach (var item in ingredientCheckList.Children)
            {
                if (item is CheckBox checkBox)
                {
                    checkBox.IsChecked = false; // İşareti kaldır
                }
            }

            // Tüm malzemeleri yeniden yükle
            LoadIngredients();
        }


        // Eşleşme yüzdesini hesaplayan fonksiyon
        public List<Tuple<int, string, string, double>> AramaSonuclariVeEslesmeHesapla(List<Tuple<int, string, string, double>> recipeList)
        {
            List<Tuple<int, string, string, double>> results = new List<Tuple<int, string, string, double>>();

            foreach (var recipe in recipeList)
            {
                int eslesmeSayisi = GetMatchingIngredientCount(recipe.Item1);
                double eslesmeYuzdesi = (double)eslesmeSayisi / selectedIngredientIds.Count * 100;

                // Eşleşme yüzdesi 0'dan büyükse listeye ekle
                if (eslesmeYuzdesi > 0)
                {
                    results.Add(Tuple.Create(recipe.Item1, recipe.Item2, recipe.Item3, eslesmeYuzdesi));
                }
            }

            return results.OrderByDescending(r => r.Item4).ToList(); // Eşleşme yüzdesine göre sıralı şekilde döner
        }



        // Tarif ile kullanıcı seçimlerindeki malzemelerin eşleşme sayısını al
        private int GetMatchingIngredientCount(int recipeId)
        {
            int count = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT COUNT(*) FROM TarifMalzeme WHERE TarifID = @TarifID AND MalzemeID IN ({string.Join(",", selectedIngredientIds)})";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", recipeId);
                    count = (int)command.ExecuteScalar();
                }
            }

            return count;
        }



      
      
        private void ShowRandomTarifOfTheDay(List<Tuple<int, string, string, double>> tarifler)
        {
            // Eksik malzemesi olmayan tarifleri filtrele
            List<Tuple<int, string, string, double>> tariflerEksiksiz = tarifler
                .Where(t =>
                {
                    // Eksik malzeme olup olmadığını kontrol et
                    EksikMalzeme(t.Item1, out bool isEksikMalzemeVar);
                    return !isEksikMalzemeVar;  
                })
                .ToList();

            if (tariflerEksiksiz.Count > 0)
            {
                Random random = new Random();
                int randomIndex = random.Next(tariflerEksiksiz.Count); // Rastgele eksiksiz tarif seç

                Tuple<int, string, string, double> randomTarif = tariflerEksiksiz[randomIndex];
                Border randomTarifCard = CreateTarifCard(randomTarif.Item1, randomTarif.Item2, randomTarif.Item3, randomTarif.Item4, 210); // Günün menüsü için genişliği 210 olarak ayarlıyoruz

                TextBlock gununYemegiBaslik = new TextBlock
                {
                    Text = "Günün Yemeği",
                    FontSize = 23,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White, 
                    TextAlignment = TextAlignment.Center, 
                    HorizontalAlignment = HorizontalAlignment.Center, 
                    Margin = new Thickness(0, 45, 0, 5) 
                };


                // Yeni bir StackPanel oluştur
                StackPanel stackPanel = new StackPanel();
                stackPanel.Children.Add(gununYemegiBaslik); 
                stackPanel.Children.Add(randomTarifCard);

                // Sidebar'a ekle
                sidebarContent.Children.Insert(sidebarContent.Children.Count, stackPanel); 
            }
            else
            {
                MessageBox.Show("Tüm malzemeleri yeterli olan bir tarif bulunamadı.");
            }
        }





        // Filtreleme butonuna tıklandığında popup'ı açar
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            // Kategorileri veritabanından çek ve ComboBox'a doldur
            LoadCategories();

            filterPopup.IsOpen = true; // Popup'ı aç
        }

        // Yer tutucu fonksiyonları
        private void TxtPlaceholder_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "En az" || textBox.Text == "En çok")
            {
                textBox.Text = string.Empty;  // Yer tutucu metin varsa, temizle
            }
        }

        private void TxtPlaceholder_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                // Yer tutucu metni geri yükle
                if (textBox.Name.Contains("Malzeme"))
                {
                    textBox.Text = textBox.Name.Contains("EnAz") ? "En az" : "En çok";
                }
                else if (textBox.Name.Contains("Maliyet"))
                {
                    textBox.Text = textBox.Name.Contains("EnAz") ? "En az" : "En çok";
                }
            }
        }

        // Veritabanından kategorileri yükleyip ComboBox'a ekleyen fonksiyon
        public void LoadCategories()
        {
            List<Tuple<int, string>> kategoriler = new List<Tuple<int, string>>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT KategoriID, KategoriAdi FROM Kategoriler";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int kategoriID = reader.GetInt32(0);
                        string kategoriAdi = reader.GetString(1);
                        kategoriler.Add(Tuple.Create(kategoriID, kategoriAdi));
                    }
                }
            }

            // ComboBox'a kategorileri ekle
            cmbKategori.ItemsSource = kategoriler;
            cmbKategori.DisplayMemberPath = "Item2";  // Kategori adını göster
            cmbKategori.SelectedValuePath = "Item1";  // Kategori ID'sini tut
        }

        // Filtre temizleme butonu işleyicisi
        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            cmbKategori.SelectedIndex = -1;  
            txtMalzemeEnAz.Text = "En az";   
            txtMalzemeEnCok.Text = "En çok";
            txtMaliyetEnAz.Text = "En az";
            txtMaliyetEnCok.Text = "En çok";
        }

        // Filtrele butonuna tıklandığında filtreleme işlemi yapar
        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            // Kategori seçimi (isteğe bağlı)
            int? selectedKategori = cmbKategori.SelectedValue as int?;

            // Malzeme sayısı aralıkları (isteğe bağlı)
            int? malzemeEnAz = null;
            int? malzemeEnCok = null;

            if (!string.IsNullOrWhiteSpace(txtMalzemeEnAz.Text) && txtMalzemeEnAz.Text != "En az")
            {
                malzemeEnAz = int.Parse(txtMalzemeEnAz.Text);
            }

            if (!string.IsNullOrWhiteSpace(txtMalzemeEnCok.Text) && txtMalzemeEnCok.Text != "En çok")
            {
                malzemeEnCok = int.Parse(txtMalzemeEnCok.Text);
            }

            // Maliyet aralıkları (isteğe bağlı)
            double? maliyetEnAz = null;
            double? maliyetEnCok = null;

            if (!string.IsNullOrWhiteSpace(txtMaliyetEnAz.Text) && txtMaliyetEnAz.Text != "En az")
            {
                maliyetEnAz = double.Parse(txtMaliyetEnAz.Text);

            }

            if (!string.IsNullOrWhiteSpace(txtMaliyetEnCok.Text) && txtMaliyetEnCok.Text != "En çok")
            {
                maliyetEnCok = double.Parse(txtMaliyetEnCok.Text);
            }

            

            // Kategori filtresi uygulanacaksa
            if (selectedKategori.HasValue)
            {
                filteredRecipes = filteredRecipes
                    .Where(recipe => GetRecipeCategory(recipe.Item1) == selectedKategori)
                    .ToList();
            }

            // Malzeme sayısı filtresi uygulanacaksa
            if (malzemeEnAz.HasValue || malzemeEnCok.HasValue)
            {
                filteredRecipes = filteredRecipes
                    .Where(recipe =>
                    {
                        int ingredientCount = GetRecipeIngredientCount(recipe.Item1);
                        bool passesMin = !malzemeEnAz.HasValue || ingredientCount >= malzemeEnAz.Value;
                        bool passesMax = !malzemeEnCok.HasValue || ingredientCount <= malzemeEnCok.Value;
                        return passesMin && passesMax;
                    })
                    .ToList();
            }

            // Maliyet filtresi uygulanacaksa
            if (maliyetEnAz.HasValue || maliyetEnCok.HasValue)
            {

                isSortedByCost = true;
                filteredRecipes = filteredRecipes
                    .Where(recipe =>
                    {
                        double cost = GetRecipeCost(recipe.Item1);
                        bool passesMin = !maliyetEnAz.HasValue || cost >= maliyetEnAz.Value;
                        bool passesMax = !maliyetEnCok.HasValue || cost <= maliyetEnCok.Value;
                        return passesMin && passesMax;
                    })
                    .ToList();
            }

            // Sonuçları ekrana yazdır
            DisplayRecipes(filteredRecipes);
        }



        // Tarifin kategorisini veritabanından alan fonksiyon
        private int? GetRecipeCategory(int recipeId)
        {
            int? category = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT KategoriID FROM Tarifler WHERE TarifID = @TarifID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", recipeId);
                    category = (int?)command.ExecuteScalar();
                }
            }

            return category;
        }

        // Tarifin malzeme sayısını veritabanından alan fonksiyon
        private int GetRecipeIngredientCount(int recipeId)
        {
            int ingredientCount = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM TarifMalzeme WHERE TarifID = @TarifID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TarifID", recipeId);
                    ingredientCount = (int)command.ExecuteScalar();
                }
            }

            return ingredientCount;
        }



        // "Seçimi Temizle" butonuna tıklandığında yapılacak işlemler
        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            // 1. Arama kutularını sıfırlama
            txtMainSearch.Text = "Tarif Adına Göre Arama...";
            txtIngredientSearch.Text = "Malzeme Ara...";
            txtSelectedSearch.Text = "Seçilen Malzeme Ara...";

            // 2. Seçilen malzemeler listesini temizleme
            selectedIngredientList.Children.Clear();
            selectedIngredientIds.Clear();

            // 3. Filtreleme alanlarını sıfırlama (kategori, malzeme sayısı ve maliyet aralıkları)
            cmbKategori.SelectedIndex = -1;
            txtMalzemeEnAz.Text = "En az";
            txtMalzemeEnCok.Text = "En çok";
            txtMaliyetEnAz.Text = "En az";
            txtMaliyetEnCok.Text = "En çok";

            // 4. WrapPanel'deki tarif kartlarını temizleme
            wrapPanel.Children.Clear();

            // 5. filteredRecipes listesini tüm tariflerle doldurma ve ekrana yazdırma
            LoadAllTarifler();  // Veritabanından tüm tarifleri tekrar yükleyip filteredRecipes listesini doldurur
        }


        private void AnaSayfaButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // Filtreleme Popup açıldığında buton arka planını değiştir
        private void FilterPopup_Opened(object sender, EventArgs e)
        {
            btnFilter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#294739"));
        }

        // Filtreleme Popup kapandığında buton arka planını eski haline getir
        private void FilterPopup_Closed(object sender, EventArgs e)
        {
            btnFilter.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A5F49"));
        }

        // Malzeme Pop-up açıldığında tüm malzemeleri göster
        private void IngredientPopup_Opened(object sender, EventArgs e)
        {
            LoadIngredients(); // Tüm malzemeleri yükle
            ingredientPopup.IsOpen = true;
            btnIngredientSearch.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#294739"));

        }
        

        // Malzemeye Göre Arama Popup kapandığında buton arka planını eski haline getir
        private void IngredientPopup_Closed(object sender, EventArgs e)
        {
            btnIngredientSearch.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A5F49"));
        }

        private bool IsRecipeFavorite(int tarifID)
        {
            // Favori durumunu belirlemek için Favoriler tablosunu kontrol ediyoruz
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Favoriler WHERE TarifID = @tarifID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@tarifID", tarifID);
                    int count = (int)command.ExecuteScalar();
                    return count > 0; // Eğer sonuç 0'dan büyükse favori olarak kabul edilir
                }
            }
        }


    }
}