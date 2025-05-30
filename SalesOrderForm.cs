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
    public partial class SalesOrderForm : Form
    {
        public SalesOrderForm()
        {
            InitializeComponent();dgvOrder.CellClick += dgvOrder_CellClick;

        }
        Dictionary<string, DataRow> itemsLookup = new Dictionary<string, DataRow>();
        AutoCompleteStringCollection autoItemIDs = new AutoCompleteStringCollection();
        AutoCompleteStringCollection autoItemNames = new AutoCompleteStringCollection();

        
        private void LoadCustomersIntoComboBox()
        {
            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT CustomerID, FirstName + ' ' + LastName AS FullName FROM Customers";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cmbCustomers.DisplayMember = "FullName";   // What user sees
                    cmbCustomers.ValueMember = "CustomerID";   // The value behind the scenes
                    cmbCustomers.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load customers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SalesOrderForm_Load(object sender, EventArgs e)
        {
            dgvOrder.EditingControlShowing += dgvOrder_EditingControlShowing;
            dgvOrder.CellEndEdit += dgvOrder_CellEndEdit;
            dgvOrder.KeyDown += dgvOrder_KeyDown;
            dgvOrder.CellValidating += dgvOrder_CellValidating;
            dgvOrder.ColumnHeadersHeight = 20;

            LoadCustomersIntoComboBox();


            // Add columns to DataGridView
            dgvOrder.Columns.Add("ItemID", "Item ID");
            dgvOrder.Columns.Add("ItemName", "Item Name");
            dgvOrder.Columns.Add("Description", "Description");
            dgvOrder.Columns.Add("UnitPrice", "Unit Price");
            dgvOrder.Columns.Add("Quantity", "Quantity");

            // Set read-only columns
            dgvOrder.Columns["Description"].ReadOnly = true;
            dgvOrder.Columns["UnitPrice"].ReadOnly = true;

            DataGridViewImageColumn deleteIconCol = new DataGridViewImageColumn();
            deleteIconCol.Name = "Delete";
            deleteIconCol.HeaderText = "";
            deleteIconCol.Image = Properties.Resources.DeleteIcon; // Make sure this matches your resource name
            deleteIconCol.Width = 30;
            deleteIconCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
            dgvOrder.Columns.Add(deleteIconCol);

            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(dgvOrder, "Click to delete this item");

            // Load data from the database
            DataTable dt = new DataTable();
            string connStr = "Data Source=DESKTOP-5DRFIOR;Initial Catalog=PharmacyInformationSystem;Integrated Security=True;Encrypt=False";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Items", conn);
                da.Fill(dt);
            }

            // Populate lookup dictionary and AutoComplete collections
            foreach (DataRow row in dt.Rows)
            {
                string itemId = row["ItemID"].ToString();
                string itemName = row["Name"].ToString();

                if (!itemsLookup.ContainsKey(itemId))
                    itemsLookup.Add(itemId, row);

                autoItemIDs.Add(itemId);
                autoItemNames.Add(itemName);
            }
        }
        private void dgvOrder_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // If the Delete button column was clicked
            if (e.RowIndex >= 0 && dgvOrder.Columns[e.ColumnIndex].Name == "Delete")
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete this item?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    dgvOrder.Rows.RemoveAt(e.RowIndex);
                }
            }
        }

        private void dgvOrder_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int col = e.ColumnIndex;
            string header = dgvOrder.Columns[col].HeaderText;
            string key = dgvOrder.Rows[e.RowIndex].Cells[col].Value?.ToString();

            if (!string.IsNullOrEmpty(key))
            {
                if (header == "ItemID")
                {
                    // Lookup using ItemID
                    if (itemsLookup.ContainsKey(key))
                    {
                        DataRow item = itemsLookup[key];
                        dgvOrder.Rows[e.RowIndex].Cells["ItemName"].Value = item["Name"].ToString();
                        dgvOrder.Rows[e.RowIndex].Cells["Description"].Value = item["Description"].ToString();
                        dgvOrder.Rows[e.RowIndex].Cells["UnitPrice"].Value = Convert.ToDecimal(item["UnitPrice"]);
                    }
                    else
                    {
                        MessageBox.Show("Item ID not found.");
                    }
                }
                else if (header == "ItemName")
                {
                    // Lookup using ItemName
                    var matchingItem = itemsLookup.Values.FirstOrDefault(item => item["Name"].ToString() == key);
                    if (matchingItem != null)
                    {
                        dgvOrder.Rows[e.RowIndex].Cells["ItemID"].Value = matchingItem["ItemID"].ToString();
                        dgvOrder.Rows[e.RowIndex].Cells["Description"].Value = matchingItem["Description"].ToString();
                        dgvOrder.Rows[e.RowIndex].Cells["UnitPrice"].Value = Convert.ToDecimal(matchingItem["UnitPrice"]);
                    }
                    else
                    {
                        MessageBox.Show("Item Name not found.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Item ID or Name cannot be empty.");
            }
            RecalculateTotalAmount();
        }
        private void RecalculateTotalAmount()
        {
            decimal totalAmount = 0;

            foreach (DataGridViewRow row in dgvOrder.Rows)
            {
                if (row.IsNewRow) continue;

                try
                {
                    int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                    decimal unitPrice = Convert.ToDecimal(row.Cells["UnitPrice"].Value);

                    totalAmount += quantity * unitPrice;
                }
                catch
                {
                    // silently skip rows with invalid values
                }
            }

            txtTotalAmount.Text = totalAmount.ToString("F2");
        }
        private void dgvOrder_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is TextBox tb)
            {
                string columnName = dgvOrder.Columns[dgvOrder.CurrentCell.ColumnIndex].HeaderText;

                if (columnName == "ItemID")
                {
                    tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    tb.AutoCompleteCustomSource = autoItemIDs;
                }
                else if (columnName == "ItemName")
                {
                    tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    tb.AutoCompleteCustomSource = autoItemNames;
                }
                else
                {
                    tb.AutoCompleteMode = AutoCompleteMode.None;
                }
            }
        }
        private void dgvOrder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                dgvOrder.EndEdit(); // commits the cell edit
                e.Handled = true;
            }
        }

        private void dgvOrder_CausesValidationChanged(object sender, EventArgs e)
        {

        }

        private void dgvOrder_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            string header = dgvOrder.Columns[e.ColumnIndex].HeaderText;
            string key = e.FormattedValue?.ToString();

            if (!string.IsNullOrEmpty(key))
            {
                if (header == "ItemID")
                {
                    if (itemsLookup.ContainsKey(key))
                    {
                        DataRow item = itemsLookup[key];
                        dgvOrder.Rows[e.RowIndex].Cells["ItemName"].Value = item["Name"].ToString();
                        dgvOrder.Rows[e.RowIndex].Cells["Description"].Value = item["Description"].ToString();
                        dgvOrder.Rows[e.RowIndex].Cells["UnitPrice"].Value = Convert.ToDecimal(item["UnitPrice"]);
                    }
                    else
                    {
                        MessageBox.Show("Item ID not found.");
                    }
                }
                else if (header == "ItemName")
                {
                    var matchingItem = itemsLookup.Values.FirstOrDefault(item => item["Name"].ToString() == key);
                    if (matchingItem != null)
                    {
                        dgvOrder.Rows[e.RowIndex].Cells["ItemID"].Value = matchingItem["ItemID"].ToString();
                        dgvOrder.Rows[e.RowIndex].Cells["Description"].Value = matchingItem["Description"].ToString();
                        dgvOrder.Rows[e.RowIndex].Cells["UnitPrice"].Value = Convert.ToDecimal(matchingItem["UnitPrice"]);
                    }
                    else
                    {
                        MessageBox.Show("Item Name not found.");
                    }
                }
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            string itemID = txtItemID.Text.Trim();
            string itemName = txtItemName.Text.Trim();

            if (string.IsNullOrEmpty(itemID) && string.IsNullOrEmpty(itemName))
            {
                MessageBox.Show("Please enter Item ID or Item Name.");
                return;
            }

            DataRow item = null;

            if (!string.IsNullOrEmpty(itemID) && itemsLookup.ContainsKey(itemID))
            {
                item = itemsLookup[itemID];
            }
            else if (!string.IsNullOrEmpty(itemName))
            {
                item = itemsLookup.Values.FirstOrDefault(i => i["Name"].ToString().Equals(itemName, StringComparison.OrdinalIgnoreCase));
            }

            if (item != null)
            {
                // Add a new row to the DataGridView with the item data
                dgvOrder.Rows.Add(item["ItemID"].ToString(), item["Name"].ToString(), item["Description"].ToString(), item["UnitPrice"], 1);
            }
            else
            {
                MessageBox.Show("Item not found.");
            }

            // Clear the input fields
            txtItemID.Clear();
            txtItemName.Clear();
        }

        private void txtItemName_Enter(object sender, EventArgs e)
        {
            txtItemName.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtItemName.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtItemName.AutoCompleteCustomSource = autoItemNames;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            decimal totalAmount = 0;

            foreach (DataGridViewRow row in dgvOrder.Rows)
            {
                if (row.IsNewRow) continue;

                try
                {
                    int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                    decimal unitPrice = Convert.ToDecimal(row.Cells["UnitPrice"].Value);

                    totalAmount += quantity * unitPrice;
                }
                catch
                {
                    MessageBox.Show("Invalid data in row. Please check Quantity and Unit Price.");
                    return;
                }
            }

            txtTotalAmount.Text = totalAmount.ToString("F2"); // format to 2 decimal places
        }

        private void InsertBtn_Click(object sender, EventArgs e)
        {
            string connStr = "Data Source=DESKTOP-5DRFIOR;Initial Catalog=PharmacyInformationSystem;Integrated Security=True;Encrypt=False";
            decimal totalAmount = 0;

            if (cmbCustomers.SelectedValue == null)
            {
                MessageBox.Show("Please select a customer.");
                return;
            }

            int customerId = Convert.ToInt32(cmbCustomers.SelectedValue);

            // Calculate total amount
            foreach (DataGridViewRow row in dgvOrder.Rows)
            {
                if (row.IsNewRow) continue;

                if (decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out decimal price) &&
                    int.TryParse(row.Cells["Quantity"].Value?.ToString(), out int qty))
                {
                    totalAmount += price * qty;
                }
            }

            txtTotalAmount.Text = totalAmount.ToString("0.00");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Insert into Sales table with CustomerID
                    SqlCommand insertSaleCmd = new SqlCommand(@"
                INSERT INTO Sales (CustomerID, SaleDate, TotalAmount)
                OUTPUT INSERTED.SaleID
                VALUES (@CustomerID, GETDATE(), @TotalAmount)", conn, transaction);

                    insertSaleCmd.Parameters.AddWithValue("@CustomerID", customerId);
                    insertSaleCmd.Parameters.AddWithValue("@TotalAmount", totalAmount);

                    int saleID = (int)insertSaleCmd.ExecuteScalar();

                    // Insert each item and update inventory
                    foreach (DataGridViewRow row in dgvOrder.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string itemId = row.Cells["ItemID"].Value?.ToString();
                        int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                        decimal unitPrice = Convert.ToDecimal(row.Cells["UnitPrice"].Value);

                        // Check stock in Front Storage
                        SqlCommand checkQtyCmd = new SqlCommand(@"
                    SELECT Quantity FROM Inventory
                    WHERE ItemID = @ItemID AND LocationID = '2'", conn, transaction);
                        checkQtyCmd.Parameters.AddWithValue("@ItemID", itemId);

                        object result = checkQtyCmd.ExecuteScalar();
                        if (result == null)
                        {
                            MessageBox.Show($"Item ID {itemId} not found in Front Storage.");
                            transaction.Rollback();
                            return;
                        }

                        int availableQty = Convert.ToInt32(result);
                        if (availableQty < quantity)
                        {
                            MessageBox.Show($"Not enough stock for Item ID {itemId}. Available: {availableQty}, Requested: {quantity}");
                            transaction.Rollback();
                            return;
                        }

                        // Insert into SaleDetails
                        SqlCommand insertDetailCmd = new SqlCommand(@"
                    INSERT INTO SaleDetails (SaleID, ItemID, Quantity, UnitPrice)
                    VALUES (@SaleID, @ItemID, @Quantity, @UnitPrice)", conn, transaction);
                        insertDetailCmd.Parameters.AddWithValue("@SaleID", saleID);
                        insertDetailCmd.Parameters.AddWithValue("@ItemID", itemId);
                        insertDetailCmd.Parameters.AddWithValue("@Quantity", quantity);
                        insertDetailCmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                        insertDetailCmd.ExecuteNonQuery();

                        // Update Inventory
                        SqlCommand updateInventoryCmd = new SqlCommand(@"
                    UPDATE Inventory
                    SET Quantity = Quantity - @SoldQuantity
                    WHERE ItemID = @ItemID AND LocationID = '2'", conn, transaction);
                        updateInventoryCmd.Parameters.AddWithValue("@SoldQuantity", quantity);
                        updateInventoryCmd.Parameters.AddWithValue("@ItemID", itemId);
                        updateInventoryCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    
                    MessageBox.Show("Sale recorded successfully!");

                    // Open the Payment Form and pass SaleID and TotalAmount
                    PaymentForm paymentForm = new PaymentForm();
                    paymentForm.SaleID = saleID;
                    paymentForm.TotalAmount = totalAmount;
                    paymentForm.ShowDialog(); // or Show() if you don't want modal

                    // Clear form
                    dgvOrder.Rows.Clear();
                    cmbCustomers.SelectedIndex = -1;
                    txtTotalAmount.Clear();
                }
                catch (Exception ex)
                {
                    
                    MessageBox.Show("Error: " + ex.Message);
                    
                }
            }
        }
        
    }

    
}
