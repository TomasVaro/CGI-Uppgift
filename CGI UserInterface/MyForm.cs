using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Data.SqlClient;
using System.Data;

namespace CGI_UserInterface
{
    public class MyForm : Form
    {
        public class Article
        {
            public int ArticleNumber;
            public string ArticleName;
            public int PricePerUnit;
        };

        public TableLayoutPanel Table; 
        public ComboBox Articles;
        public DataGridView OrderList;
        public Label TotalPriceLabel;
        public Label TotalPriceValue;
        public string ComboBoxClickItem;

        public MyForm()
        {
            #region UI Layout
            InitLayout();
            WindowState = FormWindowState.Maximized;
            Text = "CGI Search User Interface";
            Table = new TableLayoutPanel
            {
                RowCount = 13,
                ColumnCount = 2,
                Dock = DockStyle.Fill
            };
            Controls.Add(Table);
            Table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            Table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 9));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 5));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 8));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 9));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 8));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 9));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 8));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 9));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 9));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 9));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 8));
            Table.RowStyles.Add(new RowStyle(SizeType.Percent, 8));

            Label searchLabel = new Label
            {
                Text = "Sök på en artikel",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Arial Black", 30),
            };
            Table.Controls.Add(searchLabel);
            Table.SetColumnSpan(searchLabel, 2);

            Label articlesToSearch = new Label
            {
                Text = "Artiklar att söka på",
                Font = new Font("Times New Roman", 18),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Table.Controls.Add(articlesToSearch, 0, 2);

            Articles = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Font = new Font("Times New Roman", 14)                
            };
            Table.Controls.Add(Articles, 0, 3);
            Articles.SelectedIndexChanged += ComboboxChanged;

            Label orderLabel = new Label
            {
                Text = "Beställda artiklar",
                Font = new Font("Times New Roman", 18),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Table.Controls.Add(orderLabel, 2, 2);

            OrderList = new DataGridView
            {
                RowHeadersVisible = false,
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                BackgroundColor = Color.WhiteSmoke,
                Font = new Font("Times New Roman", 12),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Table.Controls.Add(OrderList, 2, 3);
            Table.SetRowSpan(OrderList, 5);

            TotalPriceLabel = new Label
            {
                Text = "Summan av ordervärdet:",
                Font = new Font("Times New Roman", 18),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            Table.Controls.Add(TotalPriceLabel, 3, 7);

            TotalPriceValue = new Label
            {
                Font = new Font("Times New Roman", 18),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            Table.Controls.Add(TotalPriceValue, 2, 8);
            #endregion

            #region Produktinläsning
            // Lägger till artiklarna i dropdowlistan.
            using (SqlConnection connection = new SqlConnection(@"Data Source=(local)\SQLEXPRESS;Initial Catalog=CGI_Uppgift;Integrated Security=True"))
            {
                connection.Open();
                using (SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM Article", connection))
                {
                    DataTable dt = new DataTable();
                    sda.Fill(dt);
                    Articles.ValueMember = "ArticleName";
                    Articles.DataSource = dt;
                    Articles.SelectedIndex = -1;
                }
            }
            #endregion
        }

        #region Beräkningar
        // Visa data i DataGridView och Summan av ordervärdet
        private void ComboboxChanged(object sender, EventArgs e)
        {
            ComboBox c = (ComboBox)sender;
            ComboBoxClickItem = Convert.ToString(c.SelectedValue);
            OrderList.DataSource = ComboBoxClickItem;

            if (c.SelectedIndex != -1)
            {
                SqlConnection con = new SqlConnection(@"Data Source=(local)\SQLEXPRESS;Initial Catalog=CGI_Uppgift;Integrated Security=True");
                con.Open();

                SqlDataAdapter adapt;
                DataTable dt = new DataTable();
                adapt = new SqlDataAdapter($@"SELECT
                    [Order].ID as [Ordernummer],
                    [Order].CustomerName as [Kundnamn],
                    Orderrow.RowNumber as [Orderrad],
                    Article.ArticleName as [Artikel],
                    Article.PricePerUnit as [Pris/st],
                    Orderrow.Amount as [Antal],
                    round (Orderrow.Amount * Article.PricePerUnit, 2) as [Summa för ordern]
                FROM [Order]
                JOIN Orderrow Orderrow on [Order].ID = Orderrow.OrderID
                JOIN Article on Orderrow.ArticleID = Article.ID
                WHERE Article.ArticleName = '{ComboBoxClickItem}'", con);
                adapt.Fill(dt);
                OrderList.DataSource = dt;
                                
                SqlCommand SelectCommand = new SqlCommand($@"SELECT
                        SUM(subtotals.subtotal)
                    FROM (
                        SELECT round (SUM (Orderrow.Amount * Article.PricePerUnit), 2) as subtotal
                            FROM [Order]
                            JOIN Orderrow on [Order].Id = Orderrow.OrderId
                            JOIN Article on Orderrow.ArticleID = Article.ID	
                            WHERE Article.ArticleName = '{ComboBoxClickItem}'
                    ) as subtotals", con);
                SqlDataReader myreader;
                myreader = SelectCommand.ExecuteReader();               
                while (myreader.Read())
                {
                    TotalPriceValue.Text = myreader[0].ToString();
                }
                con.Close();
            }
        }
        #endregion
    }
}
