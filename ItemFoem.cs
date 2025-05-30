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
    public partial class ItemFoem : Form
    {
        public ItemFoem()
        {
            InitializeComponent();
        }
        private string connectionString = "Data Source=DESKTOP-5DRFIOR;Initial Catalog=PharmacyInformationSystem;Integrated Security=True;Encrypt=False";
        private void ItemFoem_Load(object sender, EventArgs e)
        {
            LoadItems();
            dgvItems.ColumnHeadersHeight = 20;
            dgvItems.CellValueChanged += dgvItems_CellValueChanged;
            dgvItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        }
        private void LoadItems()
        {

            string query = @"
        SELECT 
            i.ItemID, 
            i.Name, 
            i.Description,
            SUM(inv.Quantity) AS Quantity, 
            l.LocationName, 
            z.ZoneName
        FROM 
            Items i
        LEFT JOIN 
            Inventory inv ON i.ItemID = inv.ItemID
        LEFT JOIN 
            Locations l ON inv.LocationID = l.LocationID
        LEFT JOIN 
            Zones z ON inv.ZoneID = z.ZoneID
        GROUP BY 
            i.ItemID, i.Name, i.Description, l.LocationName, z.ZoneName
        ORDER BY 
            i.ItemID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    dgvItems.DataSource = dataTable;

                    // Set specific column widths
                    dgvItems.Columns["ItemID"].Width = 50;
                    dgvItems.Columns["Name"].Width = 200;
                    dgvItems.Columns["Description"].Width = 250;
                    dgvItems.Columns["Quantity"].Width = 50;
                    dgvItems.Columns["LocationName"].Width = 100;
                    dgvItems.Columns["ZoneName"].Width = 100;

                    // Add a filter row at the top
                    DataRow filterRow = dataTable.NewRow();
                    dataTable.Rows.InsertAt(filterRow, 0);

                    // Make only the filter row editable
                    dgvItems.ReadOnly = false;
                    for (int i = 1; i < dgvItems.Rows.Count; i++)
                    {
                        dgvItems.Rows[i].ReadOnly = true;
                    }

                    // Column headers
                    dgvItems.Columns["ItemID"].HeaderText = "Item ID";
                    dgvItems.Columns["Name"].HeaderText = "Item Name";
                    dgvItems.Columns["Description"].HeaderText = "Description";
                    dgvItems.Columns["Quantity"].HeaderText = "Qty";
                    dgvItems.Columns["LocationName"].HeaderText = "Location";
                    dgvItems.Columns["ZoneName"].HeaderText = "Zone";

                    // Modern styling
                    dgvItems.EnableHeadersVisualStyles = false;
                    dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 144, 255);
                    dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    dgvItems.ColumnHeadersHeight = 30;

                    dgvItems.DefaultCellStyle.BackColor = Color.White;
                    dgvItems.DefaultCellStyle.ForeColor = Color.Black;
                    dgvItems.DefaultCellStyle.Font = new Font("Segoe UI", 10);
                    dgvItems.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 230, 250);
                    dgvItems.DefaultCellStyle.SelectionForeColor = Color.Black;

                    dgvItems.RowTemplate.Height = 30;
                    dgvItems.GridColor = Color.LightGray;
                    dgvItems.BorderStyle = BorderStyle.None;

                    dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgvItems.MultiSelect = false;
                    dgvItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                    dgvItems.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                    dgvItems.RowHeadersVisible = false;

                    // Highlight filter row
                    dgvItems.Rows[0].DefaultCellStyle.BackColor = Color.LightYellow;
                    dgvItems.Rows[0].DefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                    dgvItems.Rows[0].DefaultCellStyle.ForeColor = Color.Black;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message);
                }
            }
        }
        
        
        private void dgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == 0) // Filter row
            {
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            StringBuilder filterBuilder = new StringBuilder();
            DataGridViewRow filterRow = dgvItems.Rows[0];

            foreach (DataGridViewCell cell in filterRow.Cells)
            {
                string colName = dgvItems.Columns[cell.ColumnIndex].Name;
                string val = cell.Value?.ToString().Trim().Replace("'", "''");

                if (!string.IsNullOrEmpty(val))
                {
                    // Check if the value is numeric (integer or decimal)
                    if (decimal.TryParse(val, out decimal numericVal))
                    {
                        // Use '=' for numeric values
                        if (filterBuilder.Length > 0) filterBuilder.Append(" AND ");
                        filterBuilder.AppendFormat("[{0}] = {1}", colName, numericVal);
                    }
                    else
                    {
                        // Use LIKE for non-numeric values (text)
                        if (filterBuilder.Length > 0) filterBuilder.Append(" AND ");
                        filterBuilder.AppendFormat("[{0}] LIKE '%{1}%'", colName, val);
                    }
                }
            }

            DataView dv = ((DataTable)dgvItems.DataSource).DefaultView;
            dv.RowFilter = filterBuilder.ToString();
        }
        private void dgvItems_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is TextBox tb)
            {
                tb.TextChanged -= FilterTextBox_TextChanged; // Avoid multiple subscriptions
                if (dgvItems.CurrentCell.RowIndex == 0)
                {
                    tb.TextChanged += FilterTextBox_TextChanged;
                }
            }
        }
        private void FilterTextBox_TextChanged(object sender, EventArgs e)
        {
            if (dgvItems.CurrentCell.RowIndex == 0)
            {
                ApplyFilter();
            }
        }
        private void dgvItems_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvItems.IsCurrentCellDirty)
                dgvItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            LoadItems();
        }
    }
}
