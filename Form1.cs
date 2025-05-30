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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void PasswordTxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void UsernameTxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            string username = UsernameTxt.Text.Trim();
            string password = PasswordTxt.Text.Trim();

            string connectionString = "Data Source=.;Initial Catalog=PharmacyInformationSystem;Integrated Security=True";
            string query = "SELECT Role FROM Users WHERE Username = @Username AND PasswordHash = @Password";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                try
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        string role = result.ToString(); // Get the role (Cashier, Stock, Manager)

                        MenuForm menuForm = new MenuForm(role); // Pass the role to MenuForm
                        menuForm.Show();
                        this.Hide(); // Hide the login form
                        menuForm.WindowState = FormWindowState.Maximized;
                    }
                    else
                    {
                        MessageBox.Show("Invalid username or password!", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            /*MenuForm mf = new MenuForm();
            this.Hide();
           
            mf.Show();*/

        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
