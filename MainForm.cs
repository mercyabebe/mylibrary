using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace MyLibrary
{
    public partial class MainForm : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\MyLibraryDB.mdf;Integrated Security=True";

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadBooks();
            LoadBorrowers();
        }

        private void LoadBooks()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT BookID, Title, Author, Year, AvailableCopies FROM Books", conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvBooks.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading books: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBorrowers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT BorrowerID, Name, Email, Phone FROM Borrowers", conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvBorrowers.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading borrowers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddBook_Click(object sender, EventArgs e)
        {
            BookForm bookForm = new BookForm();
            if (bookForm.ShowDialog() == DialogResult.OK)
            {
                LoadBooks();
            }
        }

        private void btnEditBook_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book to edit", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int bookId = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["BookID"].Value);
            BookForm bookForm = new BookForm(bookId);
            if (bookForm.ShowDialog() == DialogResult.OK)
            {
                LoadBooks();
            }
        }

        private void btnDeleteBook_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book to delete", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this book?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int bookId = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["BookID"].Value);

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Books WHERE BookID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", bookId);
                        cmd.ExecuteNonQuery();
                    }
                    LoadBooks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting book: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnAddBorrower_Click(object sender, EventArgs e)
        {
            BorrowerForm borrowerForm = new BorrowerForm();
            if (borrowerForm.ShowDialog() == DialogResult.OK)
            {
                LoadBorrowers();
            }
        }

        private void btnEditBorrower_Click(object sender, EventArgs e)
        {
            if (dgvBorrowers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a borrower to edit", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int borrowerId = Convert.ToInt32(dgvBorrowers.SelectedRows[0].Cells["BorrowerID"].Value);
            BorrowerForm borrowerForm = new BorrowerForm(borrowerId);
            if (borrowerForm.ShowDialog() == DialogResult.OK)
            {
                LoadBorrowers();
            }
        }

        private void btnDeleteBorrower_Click(object sender, EventArgs e)
        {
            if (dgvBorrowers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a borrower to delete", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this borrower?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int borrowerId = Convert.ToInt32(dgvBorrowers.SelectedRows[0].Cells["BorrowerID"].Value);

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Borrowers WHERE BorrowerID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", borrowerId);
                        cmd.ExecuteNonQuery();
                    }
                    LoadBorrowers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting borrower: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnIssueBook_Click(object sender, EventArgs e)
        {
            IssueReturnForm issueForm = new IssueReturnForm(false);
            if (issueForm.ShowDialog() == DialogResult.OK)
            {
                LoadBooks();
                LoadBorrowers();
            }
        }

        private void btnReturnBook_Click(object sender, EventArgs e)
        {
            IssueReturnForm returnForm = new IssueReturnForm(true);
            if (returnForm.ShowDialog() == DialogResult.OK)
            {
                LoadBooks();
                LoadBorrowers();
            }
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void overdueBooksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "SELECT b.Title, br.Name, ib.IssueDate, ib.DueDate " +
                        "FROM IssuedBooks ib " +
                        "JOIN Books b ON ib.BookID = b.BookID " +
                        "JOIN Borrowers br ON ib.BorrowerID = br.BorrowerID " +
                        "WHERE ib.Returned = 0 AND ib.DueDate < @today", conn);
                    cmd.Parameters.AddWithValue("@today", DateTime.Today);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        ReportForm reportForm = new ReportForm("Overdue Books", dt);
                        reportForm.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("No overdue books found", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading overdue books: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show("Are you sure you want to exit?", "Confirm Exit",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}