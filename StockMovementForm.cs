using Guna.Charts.WinForms;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Pharmacy_Information_System
{
    public partial class StockMovementForm : Form
    {
        public StockMovementForm()
        {
            InitializeComponent();
        }

        private void StockMovementForm_Load(object sender, EventArgs e)
        {
            dgvReprot.ColumnHeadersHeight = 20;
            dtpStartDate.Value = DateTime.Now.AddMonths(-1); // Set default start date
            dtpEndDate.Value = DateTime.Now; // Set default end date
        }
        private void LoadStockMovementData(DateTime? startDate = null, DateTime? endDate = null, string dateColumnName = "MovementDate")
        {
            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

            string query = @"
        SELECT 
            sm.MovementID, 
            i.Name,  
            sm.MovementType, 
            sm.Quantity, 
            sm.MovementDate, 
            sm.Reference
        FROM 
            StockMovement sm
        JOIN 
            Items i ON sm.ItemID = i.ItemID";

            if (startDate.HasValue && endDate.HasValue)
            {
                query += $" WHERE sm.{dateColumnName} BETWEEN @StartDate AND @EndDate"; // Use the passed column
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@StartDate", startDate.Value);
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@EndDate", endDate.Value);
                    }

                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    dgvReprot.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message);
                }
            }
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";
            string query = "";

            if (guna2ComboBox1.SelectedItem.ToString() == "Stock Movement")
            {
                query = @"
        SELECT 
            sm.MovementID, 
            i.Name AS ItemName,  
            sm.MovementType, 
            sm.Quantity, 
            sm.MovementDate, 
            sm.Reference
        FROM 
            StockMovement sm
        JOIN 
            Items i ON sm.ItemID = i.ItemID";
            }
            else if (guna2ComboBox1.SelectedItem.ToString() == "Sales Details")
            {
                query = @"
     SELECT 
    s.SaleID,
    s.SaleDate,
    c.FirstName + ' ' + c.LastName AS CustomerName,
    i.Name AS ItemName,
    i.Category,
    sd.Quantity,
    sd.UnitPrice,
    (sd.Quantity * sd.UnitPrice) AS TotalLinePrice,
    s.TotalAmount
FROM 
    SaleDetails sd
JOIN 
    Sales s ON sd.SaleID = s.SaleID
INNER JOIN 
    Customers c ON s.CustomerID = c.CustomerID
JOIN 
    Items i ON sd.ItemID = i.ItemID
ORDER BY 
    s.SaleDate DESC, s.SaleID";
            }
            else if (guna2ComboBox1.SelectedItem.ToString() == "Items In Main Storage")
            {
                query = @"
        SELECT 
            i.Name AS ItemName,
            i.Category,
            inv.Quantity,
            l.LocationName,
            z.ZoneName,
            inv.ExpiryDate
        FROM 
            Inventory inv
        JOIN 
            Items i ON inv.ItemID = i.ItemID
        JOIN 
            Locations l ON inv.LocationID = l.LocationID
        JOIN 
            Zones z ON inv.ZoneID = z.ZoneID
        WHERE 
            l.LocationID = '1'
        ORDER BY 
            i.Name, l.LocationName, z.ZoneName";
            }
            else if (guna2ComboBox1.SelectedItem.ToString() == "Items In Front Storage")
            {
                query = @"
        SELECT 
            i.Name AS ItemName,
            i.Category,
            inv.Quantity,
            l.LocationName,
            z.ZoneName,
            inv.ExpiryDate
        FROM 
            Inventory inv
        JOIN 
            Items i ON inv.ItemID = i.ItemID
        JOIN 
            Locations l ON inv.LocationID = l.LocationID
        JOIN 
            Zones z ON inv.ZoneID = z.ZoneID
        WHERE 
            l.LocationID = '2'
        ORDER BY 
            i.Name, l.LocationName, z.ZoneName";
            }

            if (!string.IsNullOrEmpty(query))
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);
                        dgvReprot.DataSource = dataTable;

                        UpdateChart(dataTable); // <<< === Add this to update chart
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading data: " + ex.Message);
                    }
                }
            }


        }
        private void UpdateChart(DataTable dataTable)
        {
            guna2Chart1.Series.Clear();
           

            var series = guna2Chart1.Series.Add("Quantity");
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline; // <<< Smooth curve
            series.Color = Color.DodgerBlue;
            series.BorderWidth = 3;
            series.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series.MarkerSize = 8;
            series.MarkerColor = Color.OrangeRed;

            int totalQuantity = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                if (guna2ComboBox1.SelectedItem.ToString() == "Stock Movement" ||
                    guna2ComboBox1.SelectedItem.ToString() == "Sales Details")
                {
                    string itemName = row["ItemName"].ToString();
                    int quantity = 0;
                    int.TryParse(row["Quantity"].ToString(), out quantity);

                    series.Points.AddXY(itemName, quantity);
                    totalQuantity += quantity;
                }
                else if (guna2ComboBox1.SelectedItem.ToString() == "Items In Main Storage" ||
                         guna2ComboBox1.SelectedItem.ToString() == "Items In Front Storage")
                {
                    string itemName = row["ItemName"].ToString();
                    int quantity = 0;
                    int.TryParse(row["Quantity"].ToString(), out quantity);

                    series.Points.AddXY(itemName, quantity);
                    totalQuantity += quantity;
                }
            }

            guna2Chart1.ChartAreas[0].AxisX.LabelStyle.Angle = -45; // Rotate X labels
            guna2Chart1.ChartAreas[0].AxisX.Interval = 1;           // Show every item name
            guna2Chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            guna2Chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;

            guna2Chart1.Update();
            lblTotalQuantity.Text = $"Total Quantity: {totalQuantity}";
        }

        private void ExcelBtn_Click(object sender, EventArgs e)
        {


        }

        private void filterBtn_Click(object sender, EventArgs e)
        {
            DateTime? startDate = dtpStartDate.Value.Date;
            DateTime? endDate = dtpEndDate.Value.Date;

            string selectedReport = guna2ComboBox1.SelectedItem?.ToString();

            if (selectedReport == "Stock Movement")
            {
                LoadStockMovementData(startDate, endDate, "MovementDate");
            }
            else if (selectedReport == "Sales Details")
            {
                LoadSalesDetailsData(startDate, endDate, "SaleDate");
            }
            else if (selectedReport == "Items In Main Storage")
            {
                LoadInventoryData(startDate, endDate, locationId: "1");
            }
            else if (selectedReport == "Items In Front Storage")
            {
                LoadInventoryData(startDate, endDate, locationId: "2");
            }
            else
            {
                MessageBox.Show("Filtering is only available for Stock Movement, Sales Details, and Items In Storage reports.");
            }
        }
        private void LoadSalesDetailsData(DateTime? startDate = null, DateTime? endDate = null, string dateColumnName = "SaleDate")
        {
            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

            string query = @"
        SELECT 
            s.SaleID,
            s.SaleDate,
            s.CustomerName,
            i.Name AS ItemName,
            i.Category,
            sd.Quantity,
            sd.UnitPrice,
            (sd.Quantity * sd.UnitPrice) AS TotalLinePrice,
            s.TotalAmount
        FROM 
            SaleDetails sd
        JOIN 
            Sales s ON sd.SaleID = s.SaleID
        JOIN 
            Items i ON sd.ItemID = i.ItemID";

            if (startDate.HasValue && endDate.HasValue)
            {
                query += $" WHERE s.{dateColumnName} BETWEEN @StartDate AND @EndDate";
            }

            query += " ORDER BY s.SaleDate DESC, s.SaleID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@StartDate", startDate.Value);
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@EndDate", endDate.Value);
                    }

                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    dgvReprot.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading sales details data: " + ex.Message);
                }
            }

        }
        private void LoadInventoryData(DateTime? startDate = null, DateTime? endDate = null, string locationId = null)
        {
            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

            string query = @"
    SELECT 
        i.Name AS ItemName,
        i.Category,
        inv.Quantity,
        l.LocationName,
        z.ZoneName,
        inv.ExpiryDate
    FROM 
        Inventory inv
    JOIN 
        Items i ON inv.ItemID = i.ItemID
    JOIN 
        Locations l ON inv.LocationID = l.LocationID
    JOIN 
        Zones z ON inv.ZoneID = z.ZoneID
    WHERE 
        1=1"; // Always true to make adding conditions easier

            if (!string.IsNullOrEmpty(locationId))
            {
                query += " AND l.LocationID = @LocationID";
            }
            if (startDate.HasValue && endDate.HasValue)
            {
                query += " AND inv.ExpiryDate BETWEEN @StartDate AND @EndDate";
            }

            query += " ORDER BY i.Name, l.LocationName, z.ZoneName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, conn);
                    if (!string.IsNullOrEmpty(locationId))
                    {
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@LocationID", locationId);
                    }
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@StartDate", startDate.Value);
                        dataAdapter.SelectCommand.Parameters.AddWithValue("@EndDate", endDate.Value);
                    }

                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    dgvReprot.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading inventory data: " + ex.Message);
                }
            }
        }
        private void ExportDataGridViewToExcel(DataGridView dgv, string filePath)
        {
            try
            {
                var excelApp = new Excel.Application();
                var workbook = excelApp.Workbooks.Add(Type.Missing);
                Excel._Worksheet worksheet = null;
                worksheet = workbook.Sheets["Sheet1"];
                worksheet = workbook.ActiveSheet;
                worksheet.Name = "ExportedData";

                // Add column headers
                for (int i = 1; i < dgv.Columns.Count + 1; i++)
                {
                    worksheet.Cells[1, i] = dgv.Columns[i - 1].HeaderText;
                }

                // Add rows
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    for (int j = 0; j < dgv.Columns.Count; j++)
                    {
                        if (dgv.Rows[i].Cells[j].Value != null)
                        {
                            worksheet.Cells[i + 2, j + 1] = dgv.Rows[i].Cells[j].Value.ToString();
                        }
                    }
                }

                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();

                MessageBox.Show("Data exported successfully!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting data: " + ex.Message);
            }
        }
        private void ExcelBtn_Click_1(object sender, EventArgs e)
        {
            if (dgvReprot.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report");

                        // Add column headers
                        for (int i = 0; i < dgvReprot.Columns.Count; i++)
                        {
                            worksheet.Cells[1, i + 1].Value = dgvReprot.Columns[i].HeaderText;
                        }

                        // Add rows
                        for (int i = 0; i < dgvReprot.Rows.Count; i++)
                        {
                            for (int j = 0; j < dgvReprot.Columns.Count; j++)
                            {
                                worksheet.Cells[i + 2, j + 1].Value = dgvReprot.Rows[i].Cells[j].Value?.ToString();
                            }
                        }

                        FileInfo fi = new FileInfo(sfd.FileName);
                        package.SaveAs(fi);
                        MessageBox.Show("Excel exported successfully!");
                    }
                }


            }
        }
    }
}
