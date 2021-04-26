using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;

namespace CGI_Uppgift
{
    #region Skapa tabeller med Entity Framework
    public class AppDbContext : DbContext
    {
        public DbSet<Order> Order { get; set; }
        public DbSet<Orderrow> Orderrow { get; set; }
        public DbSet<Article> Article { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(@"Data Source=(local)\SQLEXPRESS;Initial Catalog=CGI_Uppgift;Integrated Security=True");
        }
    }
    public class Order
    {
        [Key]
        public int ID { get; set; }
        public string CustomerName { get; set; }
        public List<Orderrow> Orderrow { get; set; }
    }
    public class Orderrow
    {
        [Key]
        public int RowNumber { get; set; }
        public int Amount { get; set; }
        public int ArticleID { get; set; }
        public Article Article { get; set; }
    }
    public class Article
    {
        [Key]
        public int ID { get; set; }
        public int ArticleNumber { get; set; }
        public string ArticleName { get; set; }
        public double PricePerUnit { get; set; }
    }
    #endregion

    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Vill du mata in en ny artikel (a) eller lägga en ny order (o)?");
                string keypress = Console.ReadKey().KeyChar.ToString();
                if (keypress == "a")
                {
                    AddNewArticle();
                }
                else if (keypress == "o")
                {
                    AddNewOrder();
                }
                else
                {
                    Console.Clear();
                }
            }
        }
        static void AddNewArticle()
        {
            Console.Clear();
            Console.WriteLine("MATA IN EN NY ARTIKEL");
            Console.WriteLine("Artiklar som redan finns:");

            ReadArticlesFromDatabase();

            Console.WriteLine("Ange nytt artikelnummer");
            int articleNumber;
            while (!int.TryParse(Console.ReadLine(), out articleNumber))
            {
                Console.WriteLine("Artikelnumret får bara innehålla siffror! Skriv in artikelnumret igen.");
            }

            Console.WriteLine("Ange artikelnamn");
            string articleName = Console.ReadLine();

            Console.WriteLine("Ange styckpris");
            double pricePerUnit;
            while (!double.TryParse(Console.ReadLine(), out pricePerUnit))
            {
                Console.WriteLine("Styckpris får bara innehålla siffror och kommatecken! Skriv in styckpriset igen.");
            }
            pricePerUnit = Math.Round(pricePerUnit, 2);
            AddArticlesToDatabase(articleNumber, articleName, pricePerUnit);
        }
        static void AddNewOrder()
        {
            Console.Clear();
            Console.WriteLine("LÄGG EN NY ORDER");
            Console.WriteLine("Ange kundnamn");
            string customerName = Console.ReadLine();
            Console.WriteLine();

            Console.WriteLine("Varor som går att beställa:");
            ReadArticlesFromDatabase();

            int j = 0;
            int k = 0;
            while (k == 0)
            {
                Console.WriteLine("Mata in en ny order. Ange artikel nummer.");
                while (j == 0)
                {
                    int articleNumber;
                    while (!int.TryParse(Console.ReadLine(), out articleNumber))
                    {
                        Console.WriteLine("Artikelnumret får bara innehålla siffror! Skriv in artikelnumret igen.");
                    }
                    using (SqlConnection connection = new SqlConnection(@"Data Source=(local)\SQLEXPRESS;Initial Catalog=CGI_Uppgift;Integrated Security=True"))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("SELECT ArticleNumber FROM Article", connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        int value = int.Parse(reader.GetValue(i).ToString());
                                        if (articleNumber == value)
                                        {
                                            Console.WriteLine("Ange antal");
                                            int amount;
                                            while (!int.TryParse(Console.ReadLine(), out amount))
                                            {
                                                Console.WriteLine("Antal får bara innehålla siffror! Skriv in antalet igen.");
                                            }
                                            AddOrderToDatabase(customerName, amount, articleNumber);
                                            j = 1;
                                        }
                                    }
                                }
                                if (j == 0)
                                {
                                    Console.WriteLine("Artikeln hittades inte! Skriv in artikelnumret igen.");
                                }
                            }
                        }
                        connection.Close();
                    }
                }
                int m = 0;
                while (m == 0)
                {
                    Console.WriteLine("Vill du fortsätta att lägga till nya artiklar till ordern? j/n");
                    string keypress = Console.ReadKey().KeyChar.ToString();
                    Console.WriteLine();
                    if (keypress != "j" && keypress != "n")
                    {
                        Console.WriteLine("Tryck 'j' för ja eller 'n' för nej");
                    }
                    else if (keypress == "n")
                    {
                        k = 1;
                        m = 1;
                    }
                    else if (keypress == "j")
                    {
                        j = 0;
                        m = 1;
                    }
                }
            }
        }
        static void ReadArticlesFromDatabase()
        {
            using (SqlConnection connection = new SqlConnection(@"Data Source=(local)\SQLEXPRESS;Initial Catalog=CGI_Uppgift;Integrated Security=True"))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT * FROM Article", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 1; i < reader.FieldCount; i++)
                            {
                                Console.WriteLine(reader.GetValue(i));
                            }
                            Console.WriteLine();
                        }
                    }
                }
                connection.Close();
            }
        }
        static void AddArticlesToDatabase(int articleNumber, string articleName, double pricePerUnit)
        {
            var database = new AppDbContext();
            database.Article.Add(new Article
            {
                ArticleNumber = articleNumber,
                ArticleName = articleName,
                PricePerUnit = pricePerUnit
            });
            database.SaveChanges();
            database.Dispose();
        }
        static void AddOrderToDatabase(string customerName, int amount, int articleNumber)
        {
            var database = new AppDbContext();
            int articleID = database.Article.First(a => a.ArticleNumber == articleNumber).ID;
            database.Order.Add(new Order
            {
                CustomerName = customerName,
                Orderrow = new List<Orderrow>
                { new Orderrow
                    {
                        Amount = amount,
                        ArticleID = articleID                    
                    }
                }
            });
            database.SaveChanges();
            database.Dispose();
        }
    }    
}
