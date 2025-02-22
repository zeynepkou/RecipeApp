using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
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
    public partial class FavoritesPage : Page
    {
        private string connectionString = "Server=.;Database=RecipeApp;Trusted_Connection=True;TrustServerCertificate=True;";

        public FavoritesPage()
        {
            InitializeComponent();
            LoadFavoriteRecipes(); // Favori tarifleri yükle
        }

        // Favori tarifleri veritabanından alıp kartlar halinde gösteren metot
        private void LoadFavoriteRecipes()
        {
            wrapPanelFavoriler.Children.Clear(); // Önce wrapPanel'i temizleyelim

            List<Tuple<int, string, string>> favoriteRecipes = GetFavoriteRecipes(); // Favori tariflerin listesi

            foreach (var recipe in favoriteRecipes)
            {
                Border recipeCard = CreateRecipeCard(recipe.Item1, recipe.Item2, recipe.Item3); // Favori tarif kartı oluştur
                wrapPanelFavoriler.Children.Add(recipeCard); // Kartları panelde göster
            }
        }


        // Favoriler tablosundan tarifleri alma sorgusu 
        private List<Tuple<int, string, string>> GetFavoriteRecipes()
        {
            List<Tuple<int, string, string>> favorites = new List<Tuple<int, string, string>>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT f.TarifID, t.TarifAdi, t.ResimYolu 
                                 FROM Favoriler f 
                                 JOIN Tarifler t ON f.TarifID = t.TarifID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int tarifID = reader.GetInt32(0);
                        string tarifAdi = reader.GetString(1);
                        string resimYolu = reader.IsDBNull(2) ? "" : reader.GetString(2);

                        favorites.Add(Tuple.Create(tarifID, tarifAdi, resimYolu));
                    }
                }
            }

            return favorites;
        }

        // Favori tarif kartı oluşturma
        private Border CreateRecipeCard(int tarifID, string tarifAdi, string resimYolu)
        {
            bool isFavorite = IsRecipeFavorite(tarifID);

            Border border = new Border
            {
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(15),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4F4F4")),
                Width = 230,
                Height = 350,
                Margin = new Thickness(10)
            };

            StackPanel stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Resim ekleme
            Image image = new Image
            {
                Height = 175, 
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Stretch = Stretch.Fill 
            };

            image.Clip = new RectangleGeometry(new Rect(0, 0, 230, 175), 15, 15);

           
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
                image.Source = new BitmapImage(new Uri("C:\\Lab\\resim\\elma.jpg", UriKind.Absolute));
            }

            
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

            // Butona tıklandığında RecipeDetailPage'e yönlendirme ve TarifID'yi gönderme
            viewRecipeButton.Click += (sender, args) =>
            {
                Frame navigationFrame = new Frame();
                RecipeDetailPage recipeDetailPage = new RecipeDetailPage(tarifID); 
                navigationFrame.Content = recipeDetailPage;
                this.Content = navigationFrame;
            };

            bottomPanel.Children.Add(viewRecipeButton);
            stackPanel.Children.Add(bottomPanel);
            border.Child = stackPanel;
            border.BorderBrush = Brushes.Black;
            border.BorderThickness = new Thickness(2);


            return border;
        }


        // Sidebar genişletme event handler
        private void ToggleSidebar_Checked(object sender, RoutedEventArgs e)
        {
            AdjustSidebar(true); // Sidebar genişletilir
        }

        // Sidebar daraltma event handler
        private void ToggleSidebar_Unchecked(object sender, RoutedEventArgs e)
        {
            AdjustSidebar(false); // Sidebar daraltılır
        }

        // Tarif Ekle butonuna tıklandığında çalışan event
        private void BtnTarifEkle_Click(object sender, RoutedEventArgs e)
        {
            Frame navigationFrame = new Frame();
            RecipeAddPage recipeAddPage = new RecipeAddPage();
            navigationFrame.Content = recipeAddPage;
            this.Content = navigationFrame; 
        }

        // Favoriler butonuna tıklandığında çalışan event
        private void BtnFavoriler_Click(object sender, RoutedEventArgs e)
        {
            Frame navigationFrame = new Frame();
            FavoritesPage favoritesPage = new FavoritesPage();
            navigationFrame.Content = favoritesPage;
            this.Content = navigationFrame; 

        }

        // Sidebar genişlik ayarlama fonksiyonu
        private void AdjustSidebar(bool isExpanded)
        {
            if (isExpanded)
            {
                sidebarBorder.Width = 230; // Sidebar genişken genişlik
                sidebarContent.Visibility = Visibility.Visible; // Sidebar içerik görünsün
            }
            else
            {
                sidebarBorder.Width = 60; // Sidebar daraltıldığında genişlik
                sidebarContent.Visibility = Visibility.Collapsed; // Sidebar içerik gizlensin
            }
        }

        private void AnaSayfaButton_Click(object sender, RoutedEventArgs e)
        {

            MainWindow mainWindow = new MainWindow();

            mainWindow.Show();

            Window.GetWindow(this)?.Close();
        }

        private void BtnStokIslemleri_Click(object sender, RoutedEventArgs e)
        {
            Frame navigationFrame = new Frame();
            StokPage stokPage = new StokPage();
            navigationFrame.Content = stokPage;
            this.Content = navigationFrame;
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

    }
}