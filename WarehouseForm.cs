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
    public partial class WarehouseForm : Form
    {
        public WarehouseForm()
        {
            InitializeComponent();
        }
        public class Location
        {
            public int LocationID { get; set; }
            public string LocationName { get; set; }
            public override string ToString() => LocationName;
        }

        public class Zone
        {
            public int ZoneID { get; set; }
            public string ZoneName { get; set; }
            public override string ToString() => ZoneName;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            int fromZoneID = ((Zone)cmbFrom.SelectedItem).ZoneID;
            int toZoneID = ((Zone)cmbTo.SelectedItem).ZoneID;
            DateTime transferDate = dtpTransferDate.Value;

            using (SqlConnection con = new SqlConnection("Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True"))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    foreach (DataGridViewRow row in dgvTransferItems.Rows)
                    {
                        if (row.IsNewRow) continue;

                        int itemId = Convert.ToInt32(row.Cells["ItemID"].Value);
                        int quantityToTransfer = Convert.ToInt32(row.Cells["Quantity"].Value);

                        SqlCommand checkCmd = new SqlCommand(
                            "SELECT SUM(Quantity) FROM Inventory WHERE ItemID = @ItemID AND ZoneID = @FromZoneID",
                            con, transaction);
                        checkCmd.Parameters.AddWithValue("@ItemID", itemId);
                        checkCmd.Parameters.AddWithValue("@FromZoneID", fromZoneID);
                        object result = checkCmd.ExecuteScalar();
                        int availableQuantity = result != DBNull.Value ? Convert.ToInt32(result) : 0;

                        if (availableQuantity < quantityToTransfer)
                        {
                            MessageBox.Show($"Not enough quantity for Item ID {itemId}.", "Quantity Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            transaction.Rollback();
                            return;
                        }

                        SqlCommand updateFromCmd = new SqlCommand(
                            "UPDATE Inventory SET Quantity = Quantity - @Qty WHERE ItemID = @ItemID AND ZoneID = @FromZoneID",
                            con, transaction);
                        updateFromCmd.Parameters.AddWithValue("@Qty", quantityToTransfer);
                        updateFromCmd.Parameters.AddWithValue("@ItemID", itemId);
                        updateFromCmd.Parameters.AddWithValue("@FromZoneID", fromZoneID);
                        updateFromCmd.ExecuteNonQuery();

                        SqlCommand checkToCmd = new SqlCommand(
                            "SELECT COUNT(*) FROM Inventory WHERE ItemID = @ItemID AND ZoneID = @ToZoneID",
                            con, transaction);
                        checkToCmd.Parameters.AddWithValue("@ItemID", itemId);
                        checkToCmd.Parameters.AddWithValue("@ToZoneID", toZoneID);
                        int exists = (int)checkToCmd.ExecuteScalar();

                        if (exists > 0)
                        {
                            SqlCommand updateToCmd = new SqlCommand(
                                "UPDATE Inventory SET Quantity = Quantity + @Qty WHERE ItemID = @ItemID AND ZoneID = @ToZoneID",
                                con, transaction);
                            updateToCmd.Parameters.AddWithValue("@Qty", quantityToTransfer);
                            updateToCmd.Parameters.AddWithValue("@ItemID", itemId);
                            updateToCmd.Parameters.AddWithValue("@ToZoneID", toZoneID);
                            updateToCmd.ExecuteNonQuery();
                        }
                        else
                        {
                            SqlCommand insertToCmd = new SqlCommand(
                                "INSERT INTO Inventory (ItemID, Quantity, ZoneID) VALUES (@ItemID, @Qty, @ToZoneID)",
                                con, transaction);
                            insertToCmd.Parameters.AddWithValue("@ItemID", itemId);
                            insertToCmd.Parameters.AddWithValue("@Qty", quantityToTransfer);
                            insertToCmd.Parameters.AddWithValue("@ToZoneID", toZoneID);
                            insertToCmd.ExecuteNonQuery();
                        }

                        SqlCommand insertTransferCmd = new SqlCommand(
                            "INSERT INTO Transfers (ItemID, FromZoneID, ToZoneID, Quantity, TransferDate) VALUES (@ItemID, @FromZoneID, @ToZoneID, @Qty, @TransferDate)",
                            con, transaction);
                        insertTransferCmd.Parameters.AddWithValue("@ItemID", itemId);
                        insertTransferCmd.Parameters.AddWithValue("@FromZoneID", fromZoneID);
                        insertTransferCmd.Parameters.AddWithValue("@ToZoneID", toZoneID);
                        insertTransferCmd.Parameters.AddWithValue("@Qty", quantityToTransfer);
                        insertTransferCmd.Parameters.AddWithValue("@TransferDate", transferDate);
                        insertTransferCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Transfer saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        
        }
      
        private void LoadZones()
        {
            cmbFrom.Items.Clear();
            cmbTo.Items.Clear();

            cmbFrom.DisplayMember = "ZoneName";
            cmbTo.DisplayMember = "ZoneName";

            using (SqlConnection con = new SqlConnection("Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True"))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT ZoneID, ZoneName, LocationID FROM Zones", con);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Zone z = new Zone
                    {
                        ZoneID = reader.GetInt32(0),
                        ZoneName = reader.GetString(1)
                    };
                    int locID = reader.GetInt32(2);
                    if (locID == 1) cmbFrom.Items.Add(z);
                    if (locID == 2) cmbTo.Items.Add(z);
                }
            }

            if (cmbFrom.Items.Count > 0) cmbFrom.SelectedIndex = 0;
            if (cmbTo.Items.Count > 0) cmbTo.SelectedIndex = 0;
        }
        private void WarehouseForm_Load(object sender, EventArgs e)
        {
            LoadZones();

            dgvTransferItems.Columns.Add("ItemID", "Item ID");
            dgvTransferItems.Columns.Add("ItemName", "Item Name");
            dgvTransferItems.Columns["ItemName"].ReadOnly = true;
            dgvTransferItems.Columns.Add("Quantity", "Quantity");
            dgvTransferItems.ColumnHeadersHeight = 20;

            
        }
        private int GetZoneIdByLocationId(int locationId, SqlConnection con, SqlTransaction transaction)
        {
            SqlCommand cmd = new SqlCommand("SELECT TOP 1 ZoneID FROM Zones WHERE LocationID = @LocationID", con, transaction);
            cmd.Parameters.AddWithValue("@LocationID", locationId);
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;
        }

        private void dgvTransferItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            if (dgvTransferItems.Columns[e.ColumnIndex].Name == "ItemID")
            {
                DataGridViewRow row = dgvTransferItems.Rows[e.RowIndex];

                if (row.Cells["ItemID"].Value != null)
                {
                    int itemId;
                    if (int.TryParse(row.Cells["ItemID"].Value.ToString(), out itemId))
                    {
                        string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            string query = "SELECT Name FROM Items WHERE ItemID = @ItemID";
                            SqlCommand cmd = new SqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@ItemID", itemId);

                            try
                            {
                                conn.Open();
                                object result = cmd.ExecuteScalar();
                                if (result != null)
                                {
                                    row.Cells["ItemName"].Value = result.ToString();
                                }
                                else
                                {
                                    MessageBox.Show("Item not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    row.Cells["ItemName"].Value = "";
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error fetching item name: " + ex.Message);
                            }
                        }
                    }
                }
            }
        }
    }
}
