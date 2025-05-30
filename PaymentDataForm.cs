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
    public partial class PaymentDataForm : Form
    {
        public PaymentDataForm()
        {
            InitializeComponent();
        }

        private void PaymentDataForm_Load(object sender, EventArgs e)
        {
            dgvPaymentData.ColumnHeadersHeight = 20;
            LoadItemsIntoGrid();

        }
        private void LoadItemsIntoGrid()
        {
            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                            SELECT 
                                p.PaymentID,
                                p.SaleID,
                                
                                c.FirstName + ' ' + c.LastName AS CustomerName,
                                p.PaymentDate,
                                p.Amount,
                                p.PaymentMethod
                            FROM Payments p
                            INNER JOIN Sales s ON p.SaleID = s.SaleID
                            INNER JOIN Customers c ON s.CustomerID = c.CustomerID
                        ";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Remove existing columns if reloading
                    dgvPaymentData.Columns.Clear();

                    // Bind data
                    dgvPaymentData.DataSource = dt;

                    // Create the ComboBox column
                    DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn();
                    comboBoxColumn.HeaderText = "Payment Method";
                    comboBoxColumn.Name = "PaymentMethodCombo";
                    comboBoxColumn.DataSource = new string[] { "Cash", "Credit" };
                    comboBoxColumn.DataPropertyName = "PaymentMethod"; // bind to this column in the DataTable

                    // Replace the last column (PaymentMethod) with the ComboBox
                    int paymentMethodIndex = dgvPaymentData.Columns["PaymentMethod"].Index;
                    dgvPaymentData.Columns.Remove("PaymentMethod");
                    dgvPaymentData.Columns.Insert(paymentMethodIndex, comboBoxColumn);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load payments: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (DataGridViewRow row in dgvPaymentData.Rows)
                    {
                        // Skip the new row placeholder
                        if (row.IsNewRow) continue;

                        // Get the values
                        var paymentId = row.Cells["PaymentID"].Value;
                        var paymentMethod = row.Cells["PaymentMethodCombo"].Value;

                        if (paymentId != null && paymentMethod != null)
                        {
                            string updateQuery = "UPDATE Payments SET PaymentMethod = @PaymentMethod WHERE PaymentID = @PaymentID";
                            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod.ToString());
                                cmd.Parameters.AddWithValue("@PaymentID", Convert.ToInt32(paymentId));
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    MessageBox.Show("Payment methods updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update payment methods: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Optionally reload grid
            LoadItemsIntoGrid();
        }
    }
}
