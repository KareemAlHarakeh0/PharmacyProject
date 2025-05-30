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

namespace Pharmacy_Information_System
{
    public partial class CustomerForm : Form
    {
        public CustomerForm()
        {
            InitializeComponent();
        }

        private void CustomerForm_Load(object sender, EventArgs e)
        {
            dgvCustomer.ColumnHeadersHeight = 20;
            LoadItemsIntoGrid();
        }
        private void LoadItemsIntoGrid()
        {
            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT CustomerID, FirstName, LastName, Email, Phone, Address FROM Customers";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvCustomer.DataSource = dt;
                    dgvCustomer.Columns["CustomerID"].ReadOnly = true; // Prevent editing the ID
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load customers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {

            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Get the DataSource as a DataTable
                    DataTable dt = (DataTable)dgvCustomer.DataSource;

                    // Get only newly added rows
                    DataTable newRows = dt.GetChanges(DataRowState.Added);

                    if (newRows == null || newRows.Rows.Count == 0)
                    {
                        MessageBox.Show("No new customers to add.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    foreach (DataRow row in newRows.Rows)
                    {
                        string firstName = row["FirstName"]?.ToString();
                        string lastName = row["LastName"]?.ToString();
                        string email = row["Email"]?.ToString();
                        string phone = row["Phone"]?.ToString();
                        string address = row["Address"]?.ToString();

                        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                            continue; // Basic validation

                        string insertQuery = @"INSERT INTO Customers (FirstName, LastName, Email, Phone, Address)
                                       VALUES (@FirstName, @LastName, @Email, @Phone, @Address)";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@FirstName", firstName);
                            cmd.Parameters.AddWithValue("@LastName", lastName);
                            cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Phone", (object)phone ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Address", (object)address ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("New customer(s) added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadItemsIntoGrid(); // Refresh
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add customer(s): " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (DataGridViewRow row in dgvCustomer.Rows)
                    {
                        if (row.IsNewRow || row.Cells["CustomerID"].Value == null)
                            continue;

                        int customerId = Convert.ToInt32(row.Cells["CustomerID"].Value);
                        string firstName = row.Cells["FirstName"].Value?.ToString();
                        string lastName = row.Cells["LastName"].Value?.ToString();
                        string email = row.Cells["Email"].Value?.ToString();
                        string phone = row.Cells["Phone"].Value?.ToString();
                        string address = row.Cells["Address"].Value?.ToString();

                        string updateQuery = @"UPDATE Customers SET 
                                        FirstName = @FirstName,
                                        LastName = @LastName,
                                        Email = @Email,
                                        Phone = @Phone,
                                        Address = @Address
                                       WHERE CustomerID = @CustomerID";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@CustomerID", customerId);
                            cmd.Parameters.AddWithValue("@FirstName", firstName);
                            cmd.Parameters.AddWithValue("@LastName", lastName);
                            cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Phone", (object)phone ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Address", (object)address ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Customer(s) updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadItemsIntoGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update customers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void deletebtn_Click(object sender, EventArgs e)
        {
            if (dgvCustomer.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete the selected customer?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (DataGridViewRow row in dgvCustomer.SelectedRows)
                    {
                        if (row.Cells["CustomerID"].Value == null) continue;

                        int customerId = Convert.ToInt32(row.Cells["CustomerID"].Value);

                        string deleteQuery = "DELETE FROM Customers WHERE CustomerID = @CustomerID";

                        using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@CustomerID", customerId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Customer(s) deleted successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadItemsIntoGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete customer(s): " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
