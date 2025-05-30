using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pharmacy_Information_System
{
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            InitializeComponent();
        }
        private string userRole;

        public MenuForm(string role)
        {
            InitializeComponent();
            userRole = role;
        }

        private Stack<Form> formStack = new Stack<Form>();
        private Button activeButton = null;
        private void ActivateButton(Button clickedButton)
        {
            if (activeButton != null)
            {
                // Reset the previous button
                activeButton.BackColor = Color.Teal; // Normal color
                activeButton.ForeColor = Color.White;     // Normal text
            }

            // Set the new active button
            activeButton = clickedButton;
            activeButton.BackColor = Color.DodgerBlue;    // Highlight color
            activeButton.ForeColor = Color.White;         // Highlight text
        }
        private void OpenForm(Form form, Panel targetPanel)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            targetPanel.Controls.Clear(); // Clear previous forms in that panel
            targetPanel.Controls.Add(form);
            form.Show();
            formStack.Push(form);
        }
        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void ItemsBTN_Click(object sender, EventArgs e)
        {
            ItemFoem itemForm = new ItemFoem(); // Correct class name
            OpenForm(itemForm, panel3); // Now loads into panel3
            ActivateButton((Button)sender);
        }

        private void SalesBtn_Click(object sender, EventArgs e)
        {
            SalesOrderForm SOF = new SalesOrderForm();
            OpenForm(SOF, panel3);
            ActivateButton((Button)sender);
        }

        private void InventoryMgtBtn_Click(object sender, EventArgs e)
        {
            InventoryManagementForm IMF = new InventoryManagementForm();
            OpenForm(IMF, panel3);
            ActivateButton((Button)sender);
        }

        private void PoBtn_Click(object sender, EventArgs e)
        {
            PurchaseOrder PO = new PurchaseOrder();
            OpenForm(PO,panel3);
            ActivateButton((Button)sender);
        }

        private void WarehouseBtn_Click(object sender, EventArgs e)
        {
            WarehouseForm WH = new WarehouseForm();
            OpenForm(WH , panel3);
            ActivateButton((Button)sender);
        }

        private void StockMovement_Click(object sender, EventArgs e)
        {
            StockMovementForm smf = new StockMovementForm();
            OpenForm(smf, panel3);
            ActivateButton((Button)sender);
        }

        private void Dashboard_Click(object sender, EventArgs e)
        {
            
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void MenuForm_Load(object sender, EventArgs e)
        {
            if (userRole == "Cashier")
            {
                // Cashier can only click Items and Sales
                InventoryMgtBtn.Enabled = false;
                PoBtn.Enabled = false;
                WarehouseBtn.Enabled = false;
                StockMovement.Enabled = false;
                
            }
            else if (userRole == "Stock")
            {
                // Stock can only click Inventory, PO, Warehouse,items
                
                SalesBtn.Enabled = false;
                StockMovement.Enabled = false;
                
            }
            else if (userRole == "Manager")
            {
                // Manager sees everything (no button disabled)
            }
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            CustomerForm Cf = new CustomerForm();
            OpenForm(Cf,panel3);
            ActivateButton((Button)sender);

        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PaymentDataForm PDF = new PaymentDataForm();
            OpenForm(PDF,panel3);
            ActivateButton((Button)sender);

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Form1 f1 = new Form1();
            f1.Show();
            this.Hide();
        }

        private void MenuPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
