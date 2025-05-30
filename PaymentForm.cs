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
    public partial class PaymentForm : Form
    {
        public PaymentForm()
        {
            InitializeComponent();
        }
        public int SaleID { get; set; }
        public decimal TotalAmount { get; set; }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void PaymentForm_Load(object sender, EventArgs e)
        {
            salesIDtxt.Text = SaleID.ToString();
            txtAmount.Text = TotalAmount.ToString("0.00");
            cmbPaymentMethod.Items.AddRange(new string[] { "Cash", "Credit" });
            cmbPaymentMethod.SelectedIndex = 0;
            salesIDtxt.ReadOnly = true;
        }

        private void btnAddPayment_Click(object sender, EventArgs e)
        {
            string connStr = "Data Source=DESKTOP-5DRFIOR;Initial Catalog=PharmacyInformationSystem;Integrated Security=True;Encrypt=False";
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Payments (SaleID, PaymentDate, Amount, PaymentMethod)
                    VALUES (@SaleID, @PaymentDate, @Amount, @PaymentMethod)", conn);

                    cmd.Parameters.AddWithValue("@SaleID", SaleID);
                    cmd.Parameters.AddWithValue("@PaymentDate", dateTimePicker1.Value);
                    cmd.Parameters.AddWithValue("@Amount", decimal.Parse(txtAmount.Text));
                    cmd.Parameters.AddWithValue("@PaymentMethod", cmbPaymentMethod.SelectedItem.ToString());

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Payment recorded successfully.");
                    this.Close(); // or clear the form if you prefer
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add payment: " + ex.Message);
            }
        }
    }
}
