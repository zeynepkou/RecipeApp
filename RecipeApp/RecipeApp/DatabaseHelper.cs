using Microsoft.Data.SqlClient;
using System;

namespace RecipeApp
{
    public class DatabaseHelper
    {
        private string _connectionString;


        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Method to establish connection to SQL Server
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }


        /*public void CreateTables()
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                // SQL query to create Kategoriler table
                string createKategorilerTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Kategoriler]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE Kategoriler (
                            KategoriID INT PRIMARY KEY IDENTITY(1,1),
                            KategoriAdi VARCHAR(50) NOT NULL
                        );
                    END;";


                string createTariflerTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tarifler]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE Tarifler (
                            TarifID INT PRIMARY KEY IDENTITY(1,1),
                            TarifAdi VARCHAR(100) NOT NULL,
                            KategoriID INT NOT NULL,
                            HazirlamaSuresi INT NOT NULL,
                            Talimatlar TEXT NOT NULL,
                            ResimYolu NVARCHAR(MAX) NOT NULL,
                            FOREIGN KEY (KategoriID) REFERENCES Kategoriler(KategoriID)
                        );
                    END;";

                string createMalzemelerTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Malzemeler]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE Malzemeler (
                            MalzemeID INT PRIMARY KEY IDENTITY(1,1),
                            MalzemeAdi VARCHAR(100) NOT NULL,
                            ToplamMiktar VARCHAR(50) NOT NULL,
                            MalzemeBirim VARCHAR(20) NOT NULL,
                            BirimFiyat DECIMAL(18, 2) NOT NULL
                        );
                    END;";


                string createTarifMalzemeTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TarifMalzeme]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE TarifMalzeme (
                            TarifID INT NOT NULL,
                            MalzemeID INT NOT NULL,
                            MalzemeMiktar FLOAT NOT NULL,
                            FOREIGN KEY (TarifID) REFERENCES Tarifler(TarifID),
                            FOREIGN KEY (MalzemeID) REFERENCES Malzemeler(MalzemeID),
                            PRIMARY KEY (TarifID, MalzemeID)
                        );
                    END;";


                ExecuteNonQuery(connection, createKategorilerTable); // Kategori tablosunu oluştur
                ExecuteNonQuery(connection, createTariflerTable);    // Tarif tablosunu oluştur
                ExecuteNonQuery(connection, createMalzemelerTable);  // Malzeme tablosunu oluştur
                ExecuteNonQuery(connection, createTarifMalzemeTable); // Tarif-Malzeme ilişki tablosunu oluştur

                connection.Close();
            }
        }
        /*
                    INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Kuzu Tandır', 1, '3 saat', 'Kuzu etini baharatlarla marine edip, yavaş yavaş pişirin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\kuzutandır.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Tavuk Şiş', 1, '1 saat', 'Tavuk parçalarını şişe dizip, mangalda pişirin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\tavukşiş.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('İskender Kebap', 1, '1.5 saat', 'Döner eti, yoğurt ve domates sosuyla servis edin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\iskender.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Karnıyarık', 1, '1 saat', 'Patlıcanları kızartıp, iç harcıyla doldurun.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\karnıyarık.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Manti', 1, '1.5 saat', 'Hamuru açıp, iç harcı koyup kapatın, kaynatarak pişirin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\manti.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Güveç', 1, '2 saat', 'Et ve sebzeleri güveçte yavaş pişirin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\guvec.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Döner Kebap', 1, '1.5 saat', 'Etleri döner kebap şeklinde kesip pişirin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\doner.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Beşamel Soslu Karnabahar', 1, '45 dakika', 'Karnabaharı haşlayıp, beşamel sosla fırınlayın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\karnabahar.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Etli Nohut Yemeği', 1, '1 saat', 'Nohut ve eti bir arada pişirin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\nohut.jpg');

                    INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Sigara Böreği', 2, '30 dakika', 'Yufkayı sarıp, kızartın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\sigara_boregi.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Mücver', 2, '30 dakika', 'Rendelenmiş kabakları un ve yumurta ile karıştırıp kızartın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\mucver.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Acılı Ezme', 2, '15 dakika', 'Domates, biber ve baharatları karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\acili_ezme.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Kısır', 2, '20 dakika', 'Bulguru yoğurt, nar ekşisi ve sebzelerle karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\kisir.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Patates Kızartması', 2, '15 dakika', 'Patatesleri doğrayıp kızartın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\patates_kizartmasi.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Kumpir', 2, '1 saat', 'Patatesi fırında pişirip, iç harçlarla doldurun.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\kumpir.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Zeytin Ezmesi', 2, '10 dakika', 'Zeytinleri ezip baharatlarla karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\zeytin_ezmesi.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Fıstıklı Baklava', 2, '2 saat', 'Yufkaları açıp, fıstıkla doldurup fırınlayın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\fistikli_baklava.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Taze Fasulye Kızartması', 2, '30 dakika', 'Fasulyeleri kızartın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\taze_fasulye_kizartmasi.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Tarator', 2, '15 dakika', 'Yoğurt, ceviz ve salatalık ile karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\tarator.jpg');

                    INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Çoban Salata', 3, '15 dakika', 'Domates, salatalık ve soğanı doğrayıp karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\coban_salata.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Mevsim Salatası', 3, '10 dakika', 'Mevsim sebzelerini doğrayıp karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\mevsim_salatasi.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Taze Nohut Salatası', 3, '20 dakika', 'Nohutları haşlayıp sebzelerle karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\taze_nohut_salatasi.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Patates Salatası', 3, '30 dakika', 'Haşlanmış patatesleri sebzelerle karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\patates_salatasi.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Havuç Salatası', 3, '15 dakika', 'Rendelenmiş havuçları yoğurtla karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\havuc_salatasi.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Rokoko Salatası', 3, '10 dakika', 'Rokayı zeytinyağı ile karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\rokoko_salatasi.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Kısır', 3, '20 dakika', 'Bulguru yoğurt ve nar ekşisi ile karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\kisir.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Cacık', 3, '10 dakika', 'Yoğurt ve salatalığı karıştırın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\cacik.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Şakşuka', 3, '30 dakika', 'Patlıcan ve domates ile yapılan bir mezedir.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\saksuka.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Zeytinyağlı Yaprak Sarma', 3, '1 saat', 'Asma yapraklarına iç harcı sarın, zeytinyağı ile pişirin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\yaprak_sarma.jpg');

                    INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Baklava', 4, '2 saat', 'Yufkaları açıp, fıstıkla kat kat yerleştirip şerbetleyin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\baklava.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Künefe', 4, '1 saat', 'Künefe hamurunu peynirle doldurup şerbetleyin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\kuneffe.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Sütlaç', 4, '45 dakika', 'Pirinç ve sütü kaynatıp fırınlayın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\sutlac.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Revani', 4, '1 saat', 'Semolina ile yapılan bir tatlıdır, şerbetle ıslatılır.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\revani.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Aşure', 4, '2 saat', 'Tahıllar ve kuru meyvelerle pişirilir.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\asure.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Kadayıf Dolması', 4, '1.5 saat', 'Kadayıfı şerbetle ıslatıp dolma şeklinde sarın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\kadayıf_dolmasi.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Mohito Dondurma', 4, '30 dakika', 'Meyve ve yoğurt ile karıştırıp dondurun.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\dondurma.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Lokma', 4, '1 saat', 'Hamuru kızartıp şerbetle ıslatın.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\lokma.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Fırın Sütlaç', 4, '1 saat', 'Pirinç ve süt ile fırında pişirin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\firin_sutlac.jpg');

            INSERT INTO Tarifler (TarifAdi, KategoriID, HazirlamaSuresi, Talimatlar, ResimYolu)
            VALUES ('Tavuk Göğsü', 4, '45 dakika', 'Tavuk etini haşlayıp süzgeçten geçirin.', 'C:\\Users\\HpNtb\\OneDrive\\Masaüstü\\YAZLABLAR\\tavuk_gogsu.jpg');
        

        public void CreateTable2()
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                // Create Favoriler table
                string createFavorilerTable = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Favoriler]') AND type in (N'U'))
BEGIN
    CREATE TABLE Favoriler (
        FavoriID INT PRIMARY KEY IDENTITY(1,1),  -- Auto-incrementing primary key
        TarifID INT NOT NULL,  -- Foreign key referencing the Tarifler table
        CONSTRAINT FK_Favoriler_TarifID FOREIGN KEY (TarifID) REFERENCES Tarifler(TarifID)
    );
END;";

                // Execute the query to create the table
                ExecuteNonQuery(connection, createFavorilerTable);
            }
        }*/



        private void ExecuteNonQuery(SqlConnection connection, string query)
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
