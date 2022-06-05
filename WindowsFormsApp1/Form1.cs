using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Models;


namespace WindowsFormsApp1
{
    public partial class ClearButton : Form
    {
        List<Category> categories = new List<Category>();
        List<Subcategory> subcategories = new List<Subcategory>();
        List<Author> authors = new List<Author>();
        List<Illustrator> illustrators = new List<Illustrator>();
        List<Book> books = new List<Book>();
        List<string> selectedCategories = new List<string>();
        List<string> selectedSubCategories = new List<string>();
        List<string> selectableCallNumbers = new List<string>();
        List<int> reusableNumbers = new List<int>();
        int reusedNumber;
        int nextNumber;
        bool usedNextNumber;
        public ClearButton()
        {
            InitializeComponent();
            SetConstantOptions();
            FillCategories();
            FillSubCategories();
            grabNextNumber();
            FillAuthors();
            FillIllustrators();
            SetBookDisplay();
            SetReusedNumber();
            
           // dataGridView1.ReadOnly = false;

            SetupDataGridView1Permissions();

            dataGridView2.ReadOnly = true;
            dataGridView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dataGridView1_MouseClick);
            //dataGridView3.ReadOnly = true;
            
        }

        private void UpdateRowsInDatabase(List<Book> bookList)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();

                   
                    foreach(Book book in bookList)
                    {
                        int authorId = findAuthorId(book.Author);
                        int illustratorId = findIllustratorId(book.Illustrator);
                        int fOrNF = 0;

                        if (book.Format.Equals("Fiction"))
                        {
                            fOrNF = 1;
                        }

                        sb.Append("UPDATE Books SET Title = '" + book.Title + "', A_ID = " + authorId +
                            ", Format = '" + book.Format + "', UIN = " + book.Unique_Id + ", Fiction = " + fOrNF + 
                            ", CO_Total = " + book.Check_Outs + ", I_Id = " + illustratorId + ", Call_Num = '" + book.Call_Number + "' WHERE Id = " + book.Database_Id + ";");
                    }
                    
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SetupDataGridView1Permissions()
        {
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                if (col.Name.Equals("Unique_Id"))
                {
                    col.ReadOnly = true;
                }
                else
                {
                    col.ReadOnly = false;
                }
            }
        }

        private void AddNumberToReusable(int value)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO ReusedNumber (Value) VALUES (" + value + "); DELETE FROM ReusedNumber WHERE Value = -1");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu m = new ContextMenu();
                m.MenuItems.Add(new MenuItem("Delete", DeleteRows_Click));

                int currentMouseOverRow = dataGridView1.HitTest(e.X, e.Y).RowIndex;

                //if (currentMouseOverRow >= 0)
                //{
                //    m.MenuItems.Add(new MenuItem(string.Format("Do something to row {0}", currentMouseOverRow.ToString())));
                //}

                m.Show(dataGridView1, new Point(e.X, e.Y));
                
            }
        }

        private void DeleteRows_Click(Object sender, System.EventArgs e)
        {
            reusableNumbers = new List<int>();
            DataGridViewSelectedRowCollection selectedRows =  dataGridView1.SelectedRows;
            string queryFinisher = "(";
            foreach (DataGridViewRow row in selectedRows)
            {
                int uniqueId = int.Parse(row.Cells["Unique_Id"].Value.ToString());
                queryFinisher = queryFinisher + (uniqueId + ",");
                reusableNumbers.Add(uniqueId);
            }
            queryFinisher = queryFinisher.Substring(0, queryFinisher.Length - 1) + ")";

            DeleteRows(queryFinisher);
        }

        private void DeleteRows(string idList)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("DELETE FROM Books WHERE UIN IN " + idList + "; " + "DELETE FROM BookCategory WHERE B_Id IN " + idList + "; " +
                        "DELETE FROM BookSubcategory WHERE B_Id IN " + idList);
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            foreach (int number in reusableNumbers)
            {
                AddNumberToReusable(number);
            }
            grabNextNumber();
            SetBookDisplay();
            
        }


        private void SetReusedNumber()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM ReusedNumber;");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reusedNumber = int.Parse(reader[0].ToString());
                            }
                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SetBookDisplay()
        {
            books = new List<Book>();
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM Books ORDER BY UIN ASC;");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Book book = new Book();
                                book.Database_Id = int.Parse(reader[0].ToString());
                                book.Title = reader[1].ToString();
                                book.Author = findAuthorName(int.Parse(reader[2].ToString()));
                                book.Format = reader[3].ToString();
                                book.Unique_Id = int.Parse(reader[4].ToString());
                                if (reader[5].ToString().Equals("1"))
                                {
                                    book.Classification = "Fiction";
                                }
                                else
                                {
                                    book.Classification = "Non-fiction";
                                }
                                book.Check_Outs = int.Parse(reader[6].ToString());
                                book.Illustrator = findIllustratorName(int.Parse(reader[7].ToString()));
                                book.Call_Number = reader[8].ToString();
                                books.Add(book);
                            }
                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            BindingSource Source = new BindingSource();

            for (int i = 0; i < books.Count; i++)
            {
                Source.Add(books[i]);
            };

            dataGridView1.DataSource = Source;

            if (dataGridView1.Columns.Count > 0)
            dataGridView1.Columns["Database_Id"].Visible = false;

        }

        private void IllustratorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void IncrementNextNumber()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("UPDATE NextNumber SET Value = Value + 1");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                        }
                    }

                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {

            if (!doesAuthorExistInDatabase())
            {
                int authorId = AddAuthorToDatabase();
                Author newAuthor = new Author();
                newAuthor.Name = AuthorComboBox.Text;
                newAuthor.Id = authorId;
                authors.Add(newAuthor);
            }

            if (!doesIllustratorExistInDatabase())
            {
                int illustratorId = AddIllustratorToDatabase();
                Illustrator newIllustrator = new Illustrator();
                newIllustrator.Name = IllustratorComboBox.Text;
                newIllustrator.Id = illustratorId;
                illustrators.Add(newIllustrator);
            }
            
            int bookId = AddBookToDatabase();
            foreach (string category in selectedCategories)
            {
                AddBookCategoryConnection(category, bookId);
            }

            foreach (string subcat in selectedSubCategories)
            {
                AddBookSubCategoryConnection(subcat, bookId);
            }

            FillAuthors();
            FillIllustrators();

            if (UIDBox.Text.Equals(reusedNumber.ToString()))
            {
                RemoveLastNumber();
            }
            resetForm();
        }

        private void AddBookCategoryConnection(string categoryName, int bookId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO BookCategory(Cat, B_Id)" +
                        " VALUES('" + categoryName + "', " + bookId + ");");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        command.ExecuteReader();
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void AddBookSubCategoryConnection(string subcatName, int bookId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO BookSubcategory(SC, B_Id)" +
                        " VALUES('" + subcatName + "', " + bookId + ");");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        command.ExecuteReader();
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private int AddAuthorToDatabase()
        {
            int authorId = -1;
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO Authors(Name)" +
                        " VALUES('" + AuthorComboBox.Text + "'); SELECT SCOPE_IDENTITY();");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                authorId = int.Parse(reader[0].ToString());
                            }
                        }
                    }

                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            
            return authorId;
        }

        private int AddIllustratorToDatabase()
        {
            int illustratorId = -1;
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO Illustrators(Name)" +
                        " VALUES('" + IllustratorComboBox.Text + "'); SELECT SCOPE_IDENTITY()");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                illustratorId = int.Parse(reader[0].ToString());
                            }
                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            return illustratorId;
        }

        private int AddBookToDatabase()
        {
            string title = TitleBox.Text;
            int authorId = findAuthorId(AuthorComboBox.Text);
            string format = FormatBox.Text;
            int uniqueId = int.Parse(UIDBox.Text);
            int fiction = -1;
            int checkOutTotal = 0;
            int illustratorId = findIllustratorId(IllustratorComboBox.Text);
            string callNumber = CallNumberBox.Text;

            if (usedNextNumber)
            IncrementNextNumber();

            if (FictionBox.Text.Equals("Fiction"))
            {
               fiction = 1;
            }
            else
            {
                fiction = 0;
            }
            int bookId = -1;

            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT INTO Books(Title, A_Id, Format, UIN, Fiction, CO_Total, I_Id, Call_Num)" +
                        " VALUES('" + title + "', " + authorId + ", '" + format + "', '" + uniqueId + "', " + fiction +
                        ", " + checkOutTotal + ", " + illustratorId + ", '" + callNumber + "'); SELECT SCOPE_IDENTITY()");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                bookId = int.Parse(reader[0].ToString());
                            }
                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            return bookId;
        }

        private void grabNextNumber()
        {
            nextNumber = -1;
            reusedNumber = -1;

            UIDBox.ReadOnly = true;
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM NextNumber");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                nextNumber = int.Parse(reader[0].ToString());
                            }
                        }
                    }
                    connection.Close();
                }

                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT TOP 1 * FROM ReusedNumber ORDER BY Value ASC;");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reusedNumber = int.Parse(reader[0].ToString());
                            }
                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            if (reusedNumber != -1)
            {
                UIDBox.Text = reusedNumber.ToString();
                usedNextNumber = false;
            }
            else
            {
                UIDBox.Text = nextNumber.ToString();
                usedNextNumber = true;
            }

        }

        private void RemoveLastNumber()
        {
            using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
            {

                StringBuilder sb = new StringBuilder();
                sb.Append("DELETE TOP (1) FROM ReusedNumber;");
                String sql = sb.ToString();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteReader();
                }
                connection.Close();
            }

            using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
            {

                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT COUNT(*) FROM ReusedNumber;");
                String sql = sb.ToString();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (int.Parse(reader[0].ToString()) == 0)
                            {
                                AddInitialValueToReusedNumber();
                            }
                        }
                    }
                }
                connection.Close();
            }
        }

        private void AddInitialValueToReusedNumber()
        {
            using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
            {

                StringBuilder sb = new StringBuilder();
                sb.Append("INSERT INTO ReusedNumber(Value) VALUES (-1);");
                String sql = sb.ToString();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.ExecuteReader();
                }
                connection.Close();
            }
        }

        private void SetConstantOptions()
        {
            List<string> formatValues = new List<string>();
            formatValues.Add("Hard Cover");
            formatValues.Add("Board");
            formatValues.Sort();
            FormatBox.DataSource = formatValues;

            List<string> fictionValues = new List<string>();
            fictionValues.Add("Fiction");
            fictionValues.Add("Non-fiction");
            fictionValues.Sort();
            FictionBox.DataSource = fictionValues;
        }

        private void FillAuthors()
        {
            List<string> authorValues = new List<string>();
            authors = new List<Author>();
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM Authors;");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Author author = new Author();
                                author.Id = int.Parse(reader[0].ToString());
                                author.Name = reader[1].ToString();
                                authors.Add(author);
                                authorValues.Add(reader[1].ToString());
                            }
                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            authorValues.Sort();
            AuthorComboBox.DataSource = authorValues;
            AuthorComboBox.SelectedIndex = -1;
        }

        private void FillIllustrators()
        {
            List<string> illustratorValues = new List<string>();
            illustrators = new List<Illustrator>();
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM Illustrators;");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Illustrator illustrator = new Illustrator();
                                illustrator.Id = int.Parse(reader[0].ToString());
                                illustrator.Name = reader[1].ToString();
                                illustrators.Add(illustrator);
                                illustratorValues.Add(reader[1].ToString());
                            }
                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            illustratorValues.Sort();
            IllustratorComboBox.DataSource = illustratorValues;
            IllustratorComboBox.SelectedIndex = -1;
        }

        private void FillSubCategories()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM Subcategories;");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Subcategory category = new Subcategory();
                                category.Id = int.Parse(reader[0].ToString());
                                category.Name = reader[1].ToString();
                                category.Code = reader[2].ToString();
                                category.C_ID = int.Parse(reader[3].ToString());
                                subcategories.Add(category);
                            }
                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    

        private void FillCategories()
        {
            List<string> categoryValues = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=.\\SQLExpress;Initial Catalog=LibraryData;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Trusted_Connection=true;Connection Timeout=30;"))
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM Categories;");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Category category = new Category();
                                category.Id = int.Parse(reader[0].ToString());
                                category.Name = reader[1].ToString();
                                category.Code = reader[2].ToString();
                                categoryValues.Add(reader[1].ToString());
                                categories.Add(category);
                            }
                        }
                    }
                    connection.Close();
                }

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            categoryValues.Sort();
            CategoryBox.DataSource = categoryValues;
            CategoryBox.SelectedIndex = -1;
        }

        private void CategoryBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SubCatBox.SelectedIndex = -1;
            string currentCategory = (string)CategoryBox.SelectedItem;
            int categoryIndex = findCategoryId(currentCategory);

            List<string> newSubcatValues = getSubcatList(categoryIndex);
            newSubcatValues.Sort();
            SubCatBox.DataSource = newSubcatValues;
        }

        private int findCategoryId(string categoryName)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i].Name == categoryName)
                {
                    return categories[i].Id;
                }
            }
            return -1;
        }

        private int findAuthorId(string authorName)
        {
            for (int i = 0; i < authors.Count; i++)
            {
                if (authors[i].Name == authorName)
                {
                    return authors[i].Id;
                }
            }
            return -1;
        }

        private string findAuthorName(int authorId)
        {
            for (int i = 0; i < authors.Count; i++)
            {
                if (authors[i].Id == authorId)
                {
                    return authors[i].Name;
                }
            }
            return "Error";
        }

        private string findIllustratorName(int illustratorId)
        {
            for (int i = 0; i < illustrators.Count; i++)
            {
                if (illustrators[i].Id == illustratorId)
                {
                    return illustrators[i].Name;
                }
            }
            return "Error";
        }

        private int findIllustratorId(string illustratorName)
        {
            for (int i = 0; i < illustrators.Count; i++)
            {
                if (illustrators[i].Name == illustratorName)
                {
                    return illustrators[i].Id;
                }
            }
            return -1;
        }

        private List<string> getSubcatList(int categoryId)
        {
            List<string> returnValues = new List<string>();

            for (int i = 0; i < subcategories.Count; i++)
            {
                if (subcategories[i].C_ID == categoryId)
                {
                    returnValues.Add(subcategories[i].Name);
                }
            }

            return returnValues;
        }

        private void AddCatButton_Click(object sender, EventArgs e)
        {
            var source = new BindingSource();

            if (CategoryBox.SelectedItem != null)
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    if (categories[i].Name == CategoryBox.SelectedItem.ToString())
                    {
                        selectedCategories.Add(categories[i].Name);
                        selectableCallNumbers.Add(categories[i].Code);
                        break;
                    }
                }
            }
            source.DataSource = selectedCategories.Select(s => new { value = s }).ToList();
            dataGridView2.DataSource = source;

            selectableCallNumbers.Sort();
            List<string> currentSelectableCallNumbers = selectableCallNumbers.ToList();
            CallNumberBox.DataSource = currentSelectableCallNumbers;
        }

        private void AddSubcatButton_Click(object sender, EventArgs e)
        {
            var source = new BindingSource();

            if (SubCatBox.SelectedItem != null)
            {
                for (int i = 0; i < subcategories.Count; i++)
                {
                    if (subcategories[i].Name == SubCatBox.SelectedItem.ToString())
                    {
                        selectedSubCategories.Add(subcategories[i].Name);
                        selectableCallNumbers.Add(subcategories[i].Code);
                        break;
                    }
                }
            }

            source.DataSource = selectedSubCategories.Select(s => new { value = s }).ToList();
            dataGridView3.DataSource = source;

            selectableCallNumbers.Sort();
            List<string> currentSelectableCallNumbers = selectableCallNumbers.ToList();
            CallNumberBox.DataSource = currentSelectableCallNumbers;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clearSelectedCategories();
        }

        private void clearSelectedCategories()
        {
            selectedCategories.Clear();
            selectedSubCategories.Clear();
            selectableCallNumbers.Clear();
            dataGridView2.DataSource = selectedCategories.Select(s => new { value = s }).ToList();
            dataGridView3.DataSource = selectedSubCategories.Select(s => new { value = s }).ToList();
            CallNumberBox.DataSource = selectableCallNumbers;
            CallNumberBox.SelectedIndex = -1;
        }

        private void resetForm()
        {
            TitleBox.Clear();
            AuthorComboBox.SelectedIndex = -1;
            IllustratorComboBox.SelectedIndex = -1;
            FormatBox.SelectedIndex = -1;
            FictionBox.SelectedIndex = -1;
            grabNextNumber();
            CategoryBox.SelectedIndex = -1;
            SubCatBox.SelectedIndex = -1;
            CallNumberBox.SelectedIndex = -1;
            SetBookDisplay();
            clearSelectedCategories();
        }

        private bool doesAuthorExistInDatabase()
        {
            for (int i = 0; i < authors.Count; i++)
            {
                if (authors[i].Name == AuthorComboBox.Text)
                {
                    return true;
                }
            }

            return false;
        }

        private bool doesIllustratorExistInDatabase()
        {
            for (int i = 0; i < illustrators.Count; i++)
            {
                if (illustrators[i].Name == IllustratorComboBox.Text)
                {
                    return true;
                }
            }

            return false;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            BindingSource s = (BindingSource) dataGridView1.DataSource;

            List<Book> updatedBookList = new List<Book>();

            foreach (Book book in s.List)
            {
                updatedBookList.Add(book);
            }

            UpdateRowsInDatabase(updatedBookList);
            resetForm();
        }
    }
}
