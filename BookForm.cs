using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace MyLibrary
{
    public partial class BookForm : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\MyLibraryDB.mdf;Integrated Security=True";
        private int bookId = -1; // -1 indicates a new book

        public BookForm()
        {
            InitializeComponent();
            this.Text = "Add New Book";
        }

        public BookForm(int bookId) : this()
        {
            this.bookId = bookId;
            this.Text = "Edit Book";
        }

        private void BookForm_Load(object sender, EventArgs e)
        {
            if (bookId != -1) // Editing existing book
            {
                LoadBookData();
            }
            else // Adding new book
            {
                txtCopies.Text = "1"; // Default value for new books
            }
        }

        private void LoadBookData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "SELECT Title, Author, Year, AvailableCopies FROM Books WHERE BookID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", bookId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtTitle.Text = reader["Title"].ToString();
                            txtAuthor.Text = reader["Author"].ToString();
                            txtYear.Text = reader["Year"].ToString();
                            txtCopies.Text = reader["AvailableCopies"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading book data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateChildren())
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        if (bookId == -1) // Insert new book
                        {
                            SqlCommand cmd = new SqlCommand(
                                "INSERT INTO Books (Title, Author, Year, AvailableCopies) " +
                                "VALUES (@title, @author, @year, @copies)", conn);

                            cmd.Parameters.AddWithValue("@title", txtTitle.Text);
                            cmd.Parameters.AddWithValue("@author", txtAuthor.Text);
                            cmd.Parameters.AddWithValue("@year", Convert.ToInt32(txtYear.Text));
                            cmd.Parameters.AddWithValue("@copies", Convert.ToInt32(txtCopies.Text));

                            cmd.ExecuteNonQuery();
                        }
                        else // Update existing book
                        {
                            SqlCommand cmd = new SqlCommand(
                                "UPDATE Books SET Title = @title, Author = @author, " +
                                "Year = @year, AvailableCopies = @copies WHERE BookID = @id", conn);

                            cmd.Parameters.AddWithValue("@title", txtTitle.Text);
                            cmd.Parameters.AddWithValue("@author", txtAuthor.Text);
                            cmd.Parameters.AddWithValue("@year", Convert.ToInt32(txtYear.Text));
                            cmd.Parameters.AddWithValue("@copies", Convert.ToInt32(txtCopies.Text));
                            cmd.Parameters.AddWithValue("@id", bookId);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving book: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtTitle_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                errorProvider.SetError(txtTitle, "Title is required");
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(txtTitle, "");
            }
        }

        private void txtAuthor_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAuthor.Text))
            {
                errorProvider.SetError(txtAuthor, "Author is required");
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(txtAuthor, "");
            }
        }

        private void txtYear_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!int.TryParse(txtYear.Text, out int year) || year < 1000 || year > DateTime.Now.Year + 1)
            {
                errorProvider.SetError(txtYear, "Please enter a valid year (1000-" + (DateTime.Now.Year + 1) + ")");
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(txtYear, "");
            }
        }

        private void txtCopies_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!int.TryParse(txtCopies.Text, out int copies) || copies < 0)
            {
                errorProvider.SetError(txtCopies, "Please enter a positive number");
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(txtCopies, "");
            }
        }
    }
}
