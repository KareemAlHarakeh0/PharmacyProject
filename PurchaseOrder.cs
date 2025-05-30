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
    public partial class PurchaseOrder : Form
    {
        public PurchaseOrder()
        {
            InitializeComponent();
        }

        private void PurchaseOrder_Load(object sender, EventArgs e)
        {
            LoadItemsIntoGrid();
            dgvPurchaseOrder.ColumnHeadersHeight = 20;
        }
        private void LoadItemsIntoGrid()
        {
            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM Items";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvPurchaseOrder.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load items: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    // Get the last row before the new blank row
                    if (dgvPurchaseOrder.Rows.Count > 1)
                    {
                        DataGridViewRow lastRow = dgvPurchaseOrder.Rows[dgvPurchaseOrder.Rows.Count - 2];

                        // Basic validation
                        if (lastRow.Cells["Name"].Value == null || lastRow.Cells["UnitPrice"].Value == null)
                        {
                            MessageBox.Show("Please fill in the required fields (Name and UnitPrice).", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        string name = lastRow.Cells["Name"].Value.ToString();
                        string description = lastRow.Cells["Description"]?.Value?.ToString() ?? "";
                        string category = lastRow.Cells["Category"]?.Value?.ToString() ?? "";
                        decimal unitPrice = Convert.ToDecimal(lastRow.Cells["UnitPrice"].Value);
                        int minQty = Convert.ToInt32(lastRow.Cells["MinQty"].Value ?? 0);
                        int maxQty = Convert.ToInt32(lastRow.Cells["MaxQty"].Value ?? 0);

                        string query = @"INSERT INTO Items (Name, Description, Category, UnitPrice, MinQty, MaxQty)
                                 VALUES (@Name, @Description, @Category, @UnitPrice, @MinQty, @MaxQty)";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Name", name);
                            cmd.Parameters.AddWithValue("@Description", description);
                            cmd.Parameters.AddWithValue("@Category", category);
                            cmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                            cmd.Parameters.AddWithValue("@MinQty", minQty);
                            cmd.Parameters.AddWithValue("@MaxQty", maxQty);

                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Item added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadItemsIntoGrid(); // Refresh grid
                    }
                    else
                    {
                        MessageBox.Show("No data to insert.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while inserting item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void deletebtn_Click(object sender, EventArgs e)
        {
            if (dgvPurchaseOrder.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dgvPurchaseOrder.SelectedRows[0];

                // Get the ItemID from the selected row
                if (selectedRow.Cells["ItemID"].Value != null)
                {
                    int itemId = Convert.ToInt32(selectedRow.Cells["ItemID"].Value);

                    // Confirm delete
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            try
                            {
                                conn.Open();
                                string query = "DELETE FROM Items WHERE ItemID = @ItemID";
                                SqlCommand cmd = new SqlCommand(query, conn);
                                cmd.Parameters.AddWithValue("@ItemID", itemId);
                                cmd.ExecuteNonQuery();

                                MessageBox.Show("Item deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Reload grid after deletion
                                LoadItemsIntoGrid();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error deleting item: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Selected row does not contain a valid ItemID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a row to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
