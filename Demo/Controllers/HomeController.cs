using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Controllers
{
    public class HomeController : Controller
    {

        private readonly IConfiguration Configuration;

        public HomeController(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public string TestVerisiOlustur(int musteriAdet, int sepetAdet)
        {
            List<string> lstSehir = new List<string> { "Ankara", "İstanbul", "İzmir", "Bursa", "Edirne", "Konya", "Antalya", "Diyarbakır", "Van", "Rize" };
            List<Musteri> lstMusteri = new List<Musteri> { };
            int adLength = 4;
            int soyadLength = 4;
            Random random = new Random();

            for (int i = 1; i <= musteriAdet; i++)
            {
                Musteri musteri = new Musteri
                {
                    Ad = rastgeleStringGetir(adLength),
                    Soyad = rastgeleStringGetir(soyadLength),
                    Sehir = lstSehir[random.Next(0, lstSehir.Count)],
                };
                lstMusteri.Add(musteri);
            }

            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command;
                connection.Open();
                // Musteriler insert
                string query = "";
                for (int i = 1; i <= musteriAdet; i++)
                {
                    query += "Insert Into Musteri (Ad, Soyad, Sehir) Values (@Ad" + i + ", @Soyad" + i + ", @Sehir" + i + ");";
                }
                try
                {
                    using (command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        for (int i = 1; i <= musteriAdet; i++)
                        {
                            command.Parameters.AddWithValue("@Ad" + i, lstMusteri[i - 1].Ad);
                            command.Parameters.AddWithValue("@Soyad" + i, lstMusteri[i - 1].Soyad);
                            command.Parameters.AddWithValue("@Sehir" + i, lstMusteri[i - 1].Sehir);
                        }
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();

                }
                //Select yeni eklenen müsteri id
                List<int> lstMusteriId = new List<int> { };
                query = "Select top " + musteriAdet + " MusteriId From Musteri order by MusteriId desc"; command = new SqlCommand(query, connection);
                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        lstMusteriId.Add(Convert.ToInt32(dataReader["MusteriId"]));
                    }
                }
                //Sepetleri insert
                query = "";
                for (int i = 1; i <= sepetAdet; i++)
                {
                    query += "Insert Into Sepet (MusteriId) Values (@MusteriId" + i + ");";
                }
                try
                {
                    using (command = new SqlCommand(query, connection))
                    {
                        for (int i = 1; i <= musteriAdet; i++)
                        {
                            command.Parameters.AddWithValue("@MusteriId" + i, lstMusteriId[random.Next(0, musteriAdet)]);
                        }
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }

                //Select yeni eklenen sepet id
                List<int> lstSepetId = new List<int> { };
                query = "Select top " + sepetAdet + " Id From Sepet order by Id desc";
                command = new SqlCommand(query, connection);
                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        lstSepetId.Add(Convert.ToInt32(dataReader["Id"]));
                    }
                }

                //Sepet urun insert
                int urunTutar;
                string urunAciklama;
                int urunAdet;
                List<int> lstUrunAdet = new List<int> { };
                query = "";
                int degiskenCount = 0;
                for (int i = 1; i <= sepetAdet; i++)
                {
                    urunAdet = random.Next(1, 6);
                    lstUrunAdet.Add(urunAdet);
                    for (int j = 1; j <= urunAdet; j++)
                    {
                        query += "Insert Into SepetUrun (SepetId, Tutar, Aciklama) Values (@SepetId" + degiskenCount + ", @Tutar" + degiskenCount + ", @Aciklama" + degiskenCount + "); ";
                        degiskenCount++;
                    }
                }

                try
                {
                    using (command = new SqlCommand(query, connection))
                    {
                        degiskenCount = 0;
                        for (int i = 1; i <= sepetAdet; i++)
                        {
                            for (int j = 1; j <= lstUrunAdet[i - 1]; j++)
                            {
                                urunTutar = random.Next(100, 1000);
                                urunAciklama = rastgeleStringGetir(5) + " " + rastgeleStringGetir(5);
                                command.Parameters.AddWithValue("@SepetId" + degiskenCount, lstSepetId[i - 1]);
                                command.Parameters.AddWithValue("@Tutar" + degiskenCount, urunTutar);
                                command.Parameters.AddWithValue("@Aciklama" + degiskenCount, urunAciklama);
                                degiskenCount++;
                            }
                        }

                        command.ExecuteNonQuery();
                    }

                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
                connection.Close();

            }
            return "basarili?";

        }
        private string rastgeleStringGetir(int uzunluk)
        {
            StringBuilder str_build = new StringBuilder();
            Random random = new Random();
            char randomLetter;
            double flt;
            int shift;
            for (int j = 1; j < uzunluk; j++)
            {
                flt = random.NextDouble();
                shift = Convert.ToInt32(Math.Floor(25 * flt));
                randomLetter = Convert.ToChar(shift + 65);
                str_build.Append(randomLetter);
            }
            return str_build.ToString();
        }
    }
    public class Musteri
    {
        public int MusteriId { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Sehir { get; set; }
    }

    public class Sepet
    {
        public int Id { get; set; }
        public int MusteriId { get; set; }
    }
    public class SepetUrun
    {
        public int Id { get; set; }
        public int SepetId { get; set; }
        public float Tutar { get; set; }
        public string Aciklama { get; set; }

    }

}
