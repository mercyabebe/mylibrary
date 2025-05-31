using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace MyLibrary
{
    public partial class IssueReturnForm : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\MyLibraryDB.mdf;Integrated Security=True";
        private bool isReturnMode;

        public IssueReturnForm(bool isReturnMode)
        {
            InitializeComponent();
            this.isReturnMode = isReturnMode;

            if (isReturnMode)
            {
                this.Text = "Return Book";
                btnAction.Text = "Return Book";
                lblBook.Visible = false;
                cboBooks.Visible = false;
                lblDueDate.Visible = false;
                dtpDueDate.Visible = false;
            }
        }

        private void IssueReturnForm_Load(object sender, EventArgs e)
        {
            dtpDueDate.Value = DateTime.Today.AddDays(14); // Default 2 weeks from today

            LoadBorrowers();

            if (!isReturnMode)
            {
                LoadAvailableBooks();
            }
        }

        private void LoadBorrowers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "SELECT BorrowerID, Name FROM Borrowers ORDER BY Name", conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cboBorrowers.DataSource = dt;
                    cboBorrowers.DisplayMember = "Name";
                    cboBorrowers.ValueMember = "BorrowerID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading borrowers: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAvailableBooks()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "SELECT BookID, Title FROM Books WHERE AvailableCopies > 0 ORDER BY Title", conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cboBooks.DataSource = dt;
                    cboBooks.DisplayMember = "Title";
                    cboBooks.ValueMember = "BookID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading available books: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboBorrowers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboBorrowers.SelectedValue != null && isReturnMode)
            {
                LoadBorrowerIssuedBooks();
            }
        }

        private void LoadBorrowerIssuedBooks()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "SELECT ib.IssueID, b.Title, ib.IssueDate, ib.DueDate " +
                        "FROM IssuedBooks ib " +
                        "JOIN Books b ON ib.BookID = b.BookID " +
                        "WHERE ib.BorrowerID = @borrowerId AND ib.Returned = 0 " +
                        "ORDER BY ib.DueDate", conn);
                    cmd.Parameters.AddWithValue("@borrowerId", cboBorrowers.SelectedValue);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvIssuedBooks.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading issued books: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAction_Click(object sender, EventArgs e)
        {
            if (isReturnMode)
            {
                ReturnBook();
            }
            else
            {
                IssueBook();
            }
        }

        private void IssueBook()
        {
            if (cboBorrowers.SelectedValue == null || cboBooks.SelectedValue == null)
            {
                MessageBox.Show("Please select both a borrower and a book", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Begin transaction
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        // 1. Insert issued book record
                        SqlCommand cmdInsert = new SqlCommand(
                            "INSERT INTO IssuedBooks (BookID, BorrowerID, IssueDate, DueDate) " +
                            "VALUES (@bookId, @borrowerId, @issueDate, @dueDate)", conn, transaction);
                        cmdInsert.Parameters.AddWithValue("@bookId", cboBooks.SelectedValue);
                        cmdInsert.Parameters.AddWithValue("@borrowerId", cboBorrowers.SelectedValue);
                        cmdInsert.Parameters.AddWithValue("@issueDate", DateTime.Today);
                        cmdInsert.Parameters.AddWithValue("@dueDate", dtpDueDate.Value.Date);
                        cmdInsert.ExecuteNonQuery();

                        // 2. Update available copies
                        SqlCommand cmdUpdate = new SqlCommand(
                            "UPDATE Books SET AvailableCopies = AvailableCopies - 1 " +
                            "WHERE BookID = @bookId", conn, transaction);
                        cmdUpdate.Parameters.AddWithValue("@bookId", cboBooks.SelectedValue);
                        cmdUpdate.ExecuteNonQuery();

                        // Commit transaction
                        transaction.Commit();

                        MessageBox.Show("Book issued successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error issuing book: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReturnBook()
        {
            if (dgvIssuedBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book to return", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int issueId = Convert.ToInt32(dgvIssuedBooks.SelectedRows[0].Cells["IssueID"].Value);
            int bookId = GetBookIdFromIssuedBook(issueId);

            if (MessageBox.Show("Are you sure you want to return this book?", "Confirm Return",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        // Begin transaction
                        SqlTransaction transaction = conn.BeginTransaction();

                        try
                        {
                            // 1. Update issued book as returned
                            SqlCommand cmdUpdateIssued = new SqlCommand(
                                "UPDATE IssuedBooks SET Returned = 1 WHERE IssueID = @issueId",
                                conn, transaction);
                            cmdUpdateIssued.Parameters.AddWithValue("@issueId", issueId);
                            cmdUpdateIssued.ExecuteNonQuery();

                            // 2. Update available copies
                            SqlCommand cmdUpdateBook = new SqlCommand(
                                "UPDATE Books SET AvailableCopies = AvailableCopies + 1 " +
                                "WHERE BookID = @bookId", conn, transaction);
                            cmdUpdateBook.Parameters.AddWithValue("@bookId", bookId);
                            cmdUpdateBook.ExecuteNonQuery();

                            // Commit transaction
                            transaction.Commit();

                            MessageBox.Show("Book returned successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error returning book: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int GetBookIdFromIssuedBook(int issueId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "SELECT BookID FROM IssuedBooks WHERE IssueID = @issueId", conn);
                    cmd.Parameters.AddWithValue("@issueId", issueId);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch
            {
                throw new Exception("Could not retrieve book information");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void dtpDueDate_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}