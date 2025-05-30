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
    public partial class InventoryManagementForm : Form
    {
        public InventoryManagementForm()
        {
            InitializeComponent();
        }
        private string conn = "Data Source=DESKTOP-5DRFIOR;Initial Catalog=PharmacyInformationSystem;Integrated Security=True;Encrypt=False";
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbItem.SelectedItem == null || cmbZone.SelectedItem == null || cmbLocation.SelectedItem == null || string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                MessageBox.Show("Please fill all required fields.");
                return;
            }

            int itemId = Convert.ToInt32(cmbItem.SelectedValue);
            int zoneId = Convert.ToInt32(cmbZone.SelectedValue);
            int locationId = Convert.ToInt32(cmbLocation.SelectedValue);
            int quantity = Convert.ToInt32(txtQuantity.Text);
            DateTime expiry = dtpExpiry.Value;

            using (SqlConnection connection = new SqlConnection(conn))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. Check if the inventory already exists (same item, zone, location, expiry)
                    SqlCommand checkCmd = new SqlCommand(@"
            SELECT InventoryID, Quantity 
            FROM Inventory 
            WHERE ItemID = @ItemID AND ZoneID = @ZoneID AND LocationID = @LocationID AND ExpiryDate = @ExpiryDate",
                            connection, transaction);

                    checkCmd.Parameters.AddWithValue("@ItemID", itemId);
                    checkCmd.Parameters.AddWithValue("@ZoneID", zoneId);
                    checkCmd.Parameters.AddWithValue("@LocationID", locationId);
                    checkCmd.Parameters.AddWithValue("@ExpiryDate", expiry);

                    object result = checkCmd.ExecuteScalar();

                    if (result != null)
                    {
                        // 2. If exists, update quantity
                        SqlCommand updateCmd = new SqlCommand(@"
                UPDATE Inventory 
                SET Quantity = Quantity + @AddQuantity 
                WHERE ItemID = @ItemID AND ZoneID = @ZoneID AND LocationID = @LocationID AND ExpiryDate = @ExpiryDate",
                                connection, transaction);

                        updateCmd.Parameters.AddWithValue("@AddQuantity", quantity);
                        updateCmd.Parameters.AddWithValue("@ItemID", itemId);
                        updateCmd.Parameters.AddWithValue("@ZoneID", zoneId);
                        updateCmd.Parameters.AddWithValue("@LocationID", locationId);
                        updateCmd.Parameters.AddWithValue("@ExpiryDate", expiry);

                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // 3. If not, insert new row
                        SqlCommand insertInventory = new SqlCommand(@"
                INSERT INTO Inventory (ItemID, Quantity, ZoneID, LocationID, ExpiryDate)
                VALUES (@ItemID, @Quantity, @ZoneID, @LocationID, @ExpiryDate)",
                                connection, transaction);

                        insertInventory.Parameters.AddWithValue("@ItemID", itemId);
                        insertInventory.Parameters.AddWithValue("@Quantity", quantity);
                        insertInventory.Parameters.AddWithValue("@ZoneID", zoneId);
                        insertInventory.Parameters.AddWithValue("@LocationID", locationId);
                        insertInventory.Parameters.AddWithValue("@ExpiryDate", expiry);

                        insertInventory.ExecuteNonQuery();
                    }

                    // 4. Log into StockMovement
                    SqlCommand insertMovement = new SqlCommand(@"
            INSERT INTO StockMovement (ItemID, MovementType, Quantity, Reference, MovementDate)
            VALUES (@ItemID, 'IN', @Quantity, 'Manual Entry', @MovementDate)",
                            connection, transaction);

                    insertMovement.Parameters.AddWithValue("@ItemID", itemId);
                    insertMovement.Parameters.AddWithValue("@Quantity", quantity);
                    insertMovement.Parameters.AddWithValue("@MovementDate", DateTime.Now);

                    insertMovement.ExecuteNonQuery();

                    transaction.Commit();
                    MessageBox.Show("Inventory updated successfully.");
                    LoadInventoryData();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void LoadItems()
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT ItemID, Name FROM Items", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            cmbItem.DataSource = dt;
            cmbItem.DisplayMember = "Name";
            cmbItem.ValueMember = "ItemID";
        }

        private void LoadZones()
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT ZoneID, ZoneName FROM Zones", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            cmbZone.ValueMember = "ZoneID";
            cmbZone.DisplayMember = "ZoneName";
            cmbZone.DataSource = dt;
        }

        private void InventoryManagementForm_Load(object sender, EventArgs e)
        {
            LoadItems();
            LoadZones();
            LoadInventoryData();
            dgvInventory.CellContentClick += dgvInventory_CellContentClick;
            dgvInventory.ColumnHeadersHeight = 20;
        }
        private void LoadInventoryData()
        {
            using (SqlConnection connection = new SqlConnection(conn))
            {
                SqlDataAdapter da = new SqlDataAdapter(@"
        SELECT i.InventoryID, i.ItemID, itm.Name AS ItemName, i.Quantity, z.ZoneName,  l.LocationName, i.ExpiryDate
        FROM Inventory i
        INNER JOIN Items itm ON i.ItemID = itm.ItemID
        INNER JOIN Zones z ON i.ZoneID = z.ZoneID
        INNER JOIN Locations l ON z.LocationID = l.LocationID", connection);

                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvInventory.DataSource = dt;

                // Only add once
                if (dgvInventory.Columns["DeleteButton"] == null)
                {
                    DataGridViewImageColumn imgCol = new DataGridViewImageColumn
                    {
                        Name = "DeleteButton",
                        HeaderText = "",
                        Image = Properties.Resources.DeleteIcon,
                        Width = 30,
                        ImageLayout = DataGridViewImageCellLayout.Zoom
                    };
                    dgvInventory.Columns.Add(imgCol);
                }
            }
        }
        private void cmbZone_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbZone.SelectedValue != null && int.TryParse(cmbZone.SelectedValue.ToString(), out int zoneId))
            {
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    string query = @"
                SELECT l.LocationID, l.LocationName 
                FROM Locations l
                INNER JOIN Zones z ON z.LocationID = l.LocationID
                WHERE z.ZoneID = @ZoneID";

                    SqlDataAdapter da = new SqlDataAdapter(query, connection);
                    da.SelectCommand.Parameters.AddWithValue("@ZoneID", zoneId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbLocation.DataSource = dt;
                    cmbLocation.DisplayMember = "LocationName";
                    cmbLocation.ValueMember = "LocationID";
                }
            }
        }

        private void dgvInventory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvInventory.Columns["DeleteButton"].Index)
            {
                int inventoryId = Convert.ToInt32(dgvInventory.Rows[e.RowIndex].Cells["InventoryID"].Value);

                DialogResult result = MessageBox.Show("Are you sure you want to delete this inventory record?", "Confirm Deletion", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();
                        SqlTransaction transaction = connection.BeginTransaction();

                        try
                        {
                            // Delete Inventory
                            SqlCommand deleteCmd = new SqlCommand("DELETE FROM Inventory WHERE InventoryID = @InventoryID", connection, transaction);
                            deleteCmd.Parameters.AddWithValue("@InventoryID", inventoryId);
                            deleteCmd.ExecuteNonQuery();

                            // Log stock movement
                            SqlCommand insertMovement = new SqlCommand(@"
                        INSERT INTO StockMovement (ItemID, MovementType, Quantity, Reference, MovementDate)
                        VALUES (@ItemID, 'OUT', @Quantity, 'Manual Deletion', @MovementDate)",
                                connection, transaction);

                            int itemId = Convert.ToInt32(dgvInventory.Rows[e.RowIndex].Cells["ItemID"].Value);
                            int quantity = Convert.ToInt32(dgvInventory.Rows[e.RowIndex].Cells["Quantity"].Value);

                            insertMovement.Parameters.AddWithValue("@ItemID", itemId);
                            insertMovement.Parameters.AddWithValue("@Quantity", quantity);
                            insertMovement.Parameters.AddWithValue("@MovementDate", DateTime.Now);
                            insertMovement.ExecuteNonQuery();

                            transaction.Commit();
                            MessageBox.Show("Inventory deleted.");
                            LoadInventoryData();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Error: " + ex.Message);
                        }
                    }
                }
            }
        }
        
    }
}
