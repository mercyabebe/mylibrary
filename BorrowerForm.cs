using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MyLibrary
{
    public partial class BorrowerForm : Form
    {
        private string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\MyLibraryDB.mdf;Integrated Security=True";
        private int borrowerId = -1; // -1 indicates a new borrower

        public BorrowerForm()
        {
            InitializeComponent();
            this.Text = "Add New Borrower";
        }

        public BorrowerForm(int borrowerId) : this()
        {
            this.borrowerId = borrowerId;
            this.Text = "Edit Borrower";
        }

        private void BorrowerForm_Load(object sender, EventArgs e)
        {
            if (borrowerId != -1) // Editing existing borrower
            {
                LoadBorrowerData();
            }
        }

        private void LoadBorrowerData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(
                        "SELECT Name, Email, Phone FROM Borrowers WHERE BorrowerID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", borrowerId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtName.Text = reader["Name"].ToString();
                            txtEmail.Text = reader["Email"].ToString();
                            txtPhone.Text = reader["Phone"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading borrower data: " + ex.Message, "Error",
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

                        if (borrowerId == -1) // Insert new borrower
                        {
                            SqlCommand cmd = new SqlCommand(
                                "INSERT INTO Borrowers (Name, Email, Phone) " +
                                "VALUES (@name, @email, @phone)", conn);

                            cmd.Parameters.AddWithValue("@name", txtName.Text);
                            cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                            cmd.Parameters.AddWithValue("@phone", txtPhone.Text);

                            cmd.ExecuteNonQuery();
                        }
                        else // Update existing borrower
                        {
                            SqlCommand cmd = new SqlCommand(
                                "UPDATE Borrowers SET Name = @name, Email = @email, " +
                                "Phone = @phone WHERE BorrowerID = @id", conn);

                            cmd.Parameters.AddWithValue("@name", txtName.Text);
                            cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                            cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                            cmd.Parameters.AddWithValue("@id", borrowerId);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving borrower: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtName_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorProvider.SetError(txtName, "Name is required");
                e.Cancel = true;
            }
            else if (txtName.Text.Length > 100)
            {
                errorProvider.SetError(txtName, "Name must be 100 characters or less");
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(txtName, "");
            }
        }

        private void txtEmail_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                errorProvider.SetError(txtEmail, "Email is required");
                e.Cancel = true;
            }
            else if (!IsValidEmail(email))
            {
                errorProvider.SetError(txtEmail, "Please enter a valid email address");
                e.Cancel = true;
            }
            else if (email.Length > 100)
            {
                errorProvider.SetError(txtEmail, "Email must be 100 characters or less");
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(txtEmail, "");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                // Simple regex for basic email validation
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void txtPhone_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string phone = txtPhone.Text.Trim();

            if (string.IsNullOrWhiteSpace(phone))
            {
                errorProvider.SetError(txtPhone, "Phone is required");
                e.Cancel = true;
            }
            else if (!IsValidPhone(phone))
            {
                errorProvider.SetError(txtPhone, "Please enter a valid phone number");
                e.Cancel = true;
            }
            else if (phone.Length > 20)
            {
                errorProvider.SetError(txtPhone, "Phone must be 20 characters or less");
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(txtPhone, "");
            }
        }

        private bool IsValidPhone(string phone)
        {
            // Simple validation - allows numbers, spaces, parentheses, and hyphens
            return Regex.IsMatch(phone, @"^[0-9\(\)\-\s]+$");
        }
    }
}
