using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.Reporting.WinForms;
using System.IO;

namespace Medic
{
    public partial class Form2 : Form
    {
        // that code makes the forme move and add shadow
        private bool Drag;
        private int MouseX;
        private int MouseY;

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        private bool m_aeroEnabled;

        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;


        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]

        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
            );

        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();
                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW; return cp;
            }
        }
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0; DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }
        private const int WM_NCLBUTTONDBLCLK = 0x00A3;
        protected override void WndProc(ref Message m)
        {


            //this code stop's maximize forme by double click th mouse
            if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
            //end


            switch (m.Msg)
            {
                case WM_NCPAINT:
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 0,
                            rightWidth = 0,
                            topHeight = 0
                        }; DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                    }
                    break;
                default: break;
            }
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT) m.Result = (IntPtr)HTCAPTION;
        }
        private void PanelMove_MouseDown(object sender, MouseEventArgs e)
        {
            Drag = true;
            MouseX = Cursor.Position.X - this.Left;
            MouseY = Cursor.Position.Y - this.Top;
        }
        private void PanelMove_MouseMove(object sender, MouseEventArgs e)
        {
            if (Drag)
            {
                this.Top = Cursor.Position.Y - MouseY;
                this.Left = Cursor.Position.X - MouseX;
            }
        }
        private void PanelMove_MouseUp(object sender, MouseEventArgs e) { Drag = false; }
        string UserName = "";
        public Form2()
        {
            InitializeComponent();
    
        }
        public Form2(string name)
        {
            InitializeComponent();
            this.UserName = name;

        }

        private void pictureBox15_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        DataSet DS = new DataSet();
        SqlConnection con;
        SqlDataAdapter da;
        int pos = 0;
        void nav()
        {
            if (pos != -1)
            {
                txtId.Text = DS.Tables["Medicaments"].Rows[pos][0].ToString();
                txtName.Text = DS.Tables["Medicaments"].Rows[pos][1].ToString();
                Enddate.Value = Convert.ToDateTime(DS.Tables["Medicaments"].Rows[pos][2].ToString());
                Quantity.Value = Convert.ToInt32(DS.Tables["Medicaments"].Rows[pos][3].ToString());
                txtPrice.Text = DS.Tables["Medicaments"].Rows[pos][4].ToString();
                comboSupplier.SelectedValue = DS.Tables["Medicaments"].Rows[pos][5].ToString();
                label17.Text = pos + 1 + "/" + DS.Tables["Medicaments"].Rows.Count;

            }

        }
        private void getDataMed()
        {
            
            con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString);
            da = new SqlDataAdapter("Select * From Medicaments", con);
            da.Fill(DS, "Medicaments");
            dataGridView1.DataSource = DS.Tables["Medicaments"];
            dataGridView1.Columns[5].Visible = false;

            //ComboBox
            da = new SqlDataAdapter("Select DISTINCT * From supplier", con);
            da.Fill(DS, "supplier");
            comboSupplier.DataSource = DS.Tables["supplier"];
            comboSupplier.DisplayMember = "Full_name_supplier";
            comboSupplier.ValueMember = "Id_supplier";
            if (DS.Tables["Medicaments"].Rows.Count !=0)
            {
                pos = 0;

            }
            else
            {
                pos = -1;
            }
        }
        

        private void Form2_Load(object sender, EventArgs e)
        {
            getDataMed();
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            if (txtId.Text.Trim() != "")
            {
                if (txtName.Text.Trim() != "")
                {
                    if (Enddate.Value > DateTime.Now)
                    {
                        if (txtPrice.Text.Trim() != "")
                        {
                            if (Quantity.Value > 0)
                            {
                                if (comboSupplier.Text.Trim() != "")
                                {
                                    //search for medication if it's already been added
                                    if (DS.Tables["Medicaments"] != null)
                                    {
                                        bool trouve = false;
                                        foreach (DataRow row in DS.Tables["Medicaments"].Rows)
                                        {
                                            if (row[0].ToString() == txtId.Text)
                                            {
                                                trouve = true;
                                            }
                                        }
                                        //Add new medication
                                        if (!trouve)
                                        {
                                            DataRow newRow = DS.Tables["Medicaments"].NewRow();
                                            newRow["Id_Med"] = Convert.ToInt32(txtId.Text);
                                            newRow["Name_Med"] = txtName.Text;
                                            newRow["End_date"] = Enddate.Value.ToShortDateString();
                                            newRow["Quantity"] = Quantity.Value;
                                            newRow["Price"] = Convert.ToDecimal(txtPrice.Text);
                                            newRow["Id_supplier"] = Convert.ToInt32(comboSupplier.SelectedValue);
                                            DS.Tables["Medicaments"].Rows.Add(newRow);
                                            pictureBox1.Image = null;
                                            txtId.Clear();
                                            txtName.Clear();
                                            Enddate.Value = DateTime.Now;
                                            Quantity.Value = 0;
                                            txtPrice.Clear();
                                            comboSupplier.SelectedIndex = -1;
                                            label17.Text = pos + 1 + "/" + DS.Tables["Medicaments"].Rows.Count;
                                        }
                                        else
                                        {
                                            MessageBox.Show("This medication has been already added!", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("You are missing a supplier for this medication.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                            else
                            {
                                MessageBox.Show("The quantity cannot be zero.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Medication price is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                    }
                    else
                    {
                        MessageBox.Show("This medication you trying to add has expired.", "Wrong expiration date!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("Medication name is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Medication ID is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void txtId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) )
            {
                e.Handled = true;
            }
        }

        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&(e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            if (txtId.Text.Trim() != "")
            {
                if (txtName.Text.Trim() != "")
                {
                    if (Enddate.Value > DateTime.Now)
                    {
                        if (txtPrice.Text.Trim() != "")
                        {
                            if (Quantity.Value > 0)
                            {
                                if (comboSupplier.Text.Trim() != "")
                                {
                                    //search for medication if it's already been added
                                    if (DS.Tables["Medicaments"] != null)
                                    {

                                        foreach (DataRow row in DS.Tables["Medicaments"].Rows)
                                        {
                                            if (txtId.Text == row[0].ToString())
                                            {
                                                if (MessageBox.Show("Are you sure do you wanna update this medication ?", "Update Medication", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                                {
                                                    row.BeginEdit();
                                                    row[1] = txtName.Text;
                                                    row[2] = Enddate.Value.ToShortDateString();
                                                    row[3] = Quantity.Value;
                                                    row[4] = Convert.ToDecimal(txtPrice.Text);
                                                    row[5] = Convert.ToInt32(comboSupplier.SelectedValue);
                                                    row.EndEdit();
                                                    pictureBox1.Image = null;
                                                    txtId.Clear();
                                                    txtName.Clear();
                                                    Enddate.Value = DateTime.Now;
                                                    Quantity.Value = 0;
                                                    txtPrice.Clear();
                                                    comboSupplier.SelectedIndex = -1;
                                                }
                                                    
                                            }
                                        }
                                        
                                        
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("You are missing a supplier for this medication.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                            else
                            {
                                MessageBox.Show("The quantity cannot be zero.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Medication price is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }

                    }
                    else
                    {
                        MessageBox.Show("This medication you trying to add has expired.", "Wrong expiration date!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("Medication name is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Medication ID is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (txtId.Text.Trim() != "")
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    if (MessageBox.Show("Are you sure do you wanna delete this medication ?", "Delete Medication", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        if (DS.Tables["Medicaments"].Rows != null)
                        {
                            foreach (DataRow item in DS.Tables["Medicaments"].Rows)
                            {
                                if (item[0].ToString() == txtId.Text)
                                {
                                    item.Delete();
                                    break;
                                }
                            }
                            UpdateDataBase();
                            DS.Tables["Medicaments"].Clear();
                            getDataMed();


                        }
                    }
                }
                else
                {
                    MessageBox.Show("There is nothing to delete!", "Medication Delete", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                
            }
            else
            {
                MessageBox.Show("Medication ID is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void label36_Click(object sender, EventArgs e)
        {
            if (label36.Text == "ID")
            {
                label36.Text = "Name";
            }
            else
            {
                label36.Text = "ID";
            }
        }

        private void pictureBox12_MouseEnter(object sender, EventArgs e)
        {
            pictureBox12.Image = Medic.Properties.Resources.search_btn_finel;
        }

        private void pictureBox12_MouseLeave(object sender, EventArgs e)
        {
            pictureBox12.Image = Medic.Properties.Resources.search_gray;
        }

        private void pictureBox11_MouseEnter(object sender, EventArgs e)
        {
            pictureBox11.Image = Medic.Properties.Resources.A_Z_black;
        }

        private void pictureBox11_MouseLeave(object sender, EventArgs e)
        {
            pictureBox11.Image = Medic.Properties.Resources.A_Z_Gray;
        }

        private void pictureBox10_MouseEnter(object sender, EventArgs e)
        {
            pictureBox10.Image = Medic.Properties.Resources.Z_A_black;
        }

        private void pictureBox10_MouseLeave(object sender, EventArgs e)
        {
            pictureBox10.Image = Medic.Properties.Resources.Z_A_Gray;
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
            if (label36.Text == "ID")
            {
                if (txtSearchMed.Text.Trim() != "")
                {
                    DS.Tables["Medicaments"].Rows.Clear();
                    con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString);
                    da = new SqlDataAdapter("Select * From Medicaments WHERE Id_Med =@id", con);
                    da.SelectCommand.Parameters.AddWithValue("@id", int.Parse(txtSearchMed.Text));
                    da.Fill(DS, "Medicaments");
                    dataGridView1.DataSource = DS.Tables["Medicaments"];
                    dataGridView1.Columns[5].Visible = false;

                    if (DS.Tables["Medicaments"].Rows.Count != 0)
                    {
                        pos = 0;

                    }
                    else
                    {
                        pictureBox1.Image = null;
                        txtId.Clear();
                        txtName.Clear();
                        Enddate.Value = DateTime.Now;
                        Quantity.Value = 0;
                        txtPrice.Clear();
                        comboSupplier.SelectedIndex = -1;
                        label17.Text = "0/0";
                        pos = -1;
                    }
                }
                else
                {
                    MessageBox.Show("You didn't provid any id to search for.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                }

            }
            else
            {
                if (txtSearchMed.Text.Trim() != "")
                {
                    DS.Tables["Medicaments"].Rows.Clear();
                    con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString);
                    da = new SqlDataAdapter("Select * From Medicaments WHERE Name_Med  =@name", con);
                    da.SelectCommand.Parameters.AddWithValue("@name", txtSearchMed.Text);
                    da.Fill(DS, "Medicaments");
                    dataGridView1.DataSource = DS.Tables["Medicaments"];
                    dataGridView1.Columns[5].Visible = false;

                    if (DS.Tables["Medicaments"].Rows.Count != 0)
                    {
                        pos = 0;

                    }
                    else
                    {
                        pictureBox1.Image = null;
                        txtId.Clear();
                        txtName.Clear();
                        Enddate.Value = DateTime.Now;
                        Quantity.Value = 0;
                        txtPrice.Clear();
                        comboSupplier.SelectedIndex = -1;
                        label17.Text = "0/0";
                        pos = -1;
                    }
                }
                else
                {
                    MessageBox.Show("You didn't provid any medication name to search for.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                }
            }

        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            if (label36.Text == "ID")
            {

                DS.Tables["Medicaments"].Rows.Clear();
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString);
                da = new SqlDataAdapter("Select * From Medicaments order by Id_Med ASC", con);
                da.Fill(DS, "Medicaments");
                dataGridView1.DataSource = DS.Tables["Medicaments"];
                dataGridView1.Columns[5].Visible = false;

                if (DS.Tables["Medicaments"].Rows.Count != 0)
                {
                    pos = 0;

                }
                else
                {
                    pictureBox1.Image = null;
                    txtId.Clear();
                    txtName.Clear();
                    Enddate.Value = DateTime.Now;
                    Quantity.Value = 0;
                    txtPrice.Clear();
                    comboSupplier.SelectedIndex = -1;
                    label17.Text = "0/0";
                    pos = -1;
                }



            }
            else
            {

                DS.Tables["Medicaments"].Rows.Clear();
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString);
                da = new SqlDataAdapter("Select * From Medicaments order by Name_Med ASC", con);
                da.Fill(DS, "Medicaments");
                dataGridView1.DataSource = DS.Tables["Medicaments"];
                dataGridView1.Columns[5].Visible = false;

                if (DS.Tables["Medicaments"].Rows.Count != 0)
                {
                    pos = 0;

                }
                else
                {
                    pictureBox1.Image = null;
                    txtId.Clear();
                    txtName.Clear();
                    Enddate.Value = DateTime.Now;
                    Quantity.Value = 0;
                    txtPrice.Clear();
                    comboSupplier.SelectedIndex = -1;
                    label17.Text = "0/0";
                    pos = -1;
                }

            }
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            if (label36.Text == "ID")
            {

                DS.Tables["Medicaments"].Rows.Clear();
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString);
                da = new SqlDataAdapter("Select * From Medicaments order by Id_Med DESC", con);
                da.Fill(DS, "Medicaments");
                dataGridView1.DataSource = DS.Tables["Medicaments"];
                dataGridView1.Columns[5].Visible = false;

                if (DS.Tables["Medicaments"].Rows.Count != 0)
                {
                    pos = 0;

                }
                else
                {
                    pictureBox1.Image = null;
                    txtId.Clear();
                    txtName.Clear();
                    Enddate.Value = DateTime.Now;
                    Quantity.Value = 0;
                    txtPrice.Clear();
                    comboSupplier.SelectedIndex = -1;
                    label17.Text = "0/0";
                    pos = -1;
                }



            }
            else
            {

                DS.Tables["Medicaments"].Rows.Clear();
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString);
                da = new SqlDataAdapter("Select * From Medicaments order by Name_Med DESC", con);
                da.Fill(DS, "Medicaments");
                dataGridView1.DataSource = DS.Tables["Medicaments"];
                dataGridView1.Columns[5].Visible = false;

                if (DS.Tables["Medicaments"].Rows.Count != 0)
                {
                    pos = 0;

                }
                else
                {
                    pictureBox1.Image = null;
                    txtId.Clear();
                    txtName.Clear();
                    Enddate.Value = DateTime.Now;
                    Quantity.Value = 0;
                    txtPrice.Clear();
                    comboSupplier.SelectedIndex = -1;
                    label17.Text = "0/0";
                    pos = -1;
                }

            }
        }
        private void SaveXMLBtn_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt = DS.Tables["Medicaments"];
            if (dt.Rows.Count != 0)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "XML-File | *.xml";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    dt.WriteXml(saveFileDialog.FileName);
                    
                }
            }
            else
            {
                MessageBox.Show("There is nothing to save!", "Save XML Medication!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (DS.Tables["Medicaments"].Rows.Count != 0 )
            {
                Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
                string str = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString() +" "+ dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString() + " " + dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString() + " " + dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString() + " " + dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString() + " " + dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
                pictureBox1.Image = qrcode.Draw(str,50);
                txtId.Text = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                txtName.Text = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                Enddate.Value = Convert.ToDateTime(dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString());
                Quantity.Value = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString());
                txtPrice.Text = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
                comboSupplier.SelectedValue = dataGridView1.Rows[e.RowIndex].Cells[5].Value.ToString();
                pos = e.RowIndex;
                label17.Text = pos + 1 + "/" + DS.Tables["Medicaments"].Rows.Count;

            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure do you wanna clear everything ?","Clear Data",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (DataRow item in DS.Tables["Medicaments"].Rows)
                {
                    item.Delete();    
                }
                pictureBox1.Image = null;
                txtId.Clear();
                txtName.Clear();
                Enddate.Value = DateTime.Now;
                Quantity.Value = 0;
                txtPrice.Clear();
                comboSupplier.SelectedIndex = -1;
                pos = -1;
                label17.Text = pos + 1 + "/" + DS.Tables["Medicaments"].Rows.Count;
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure do you wanna open XML file because all of your changes will be gone ?", "Open XML File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string xmlFilePath = openFileDialog.FileName;
                    using (DataSet ds = new DataSet())
                    {
                        ds.ReadXml(xmlFilePath);


                        foreach (DataRow item in DS.Tables["Medicaments"].Rows)
                        {
                            item.Delete();
                        }
                        UpdateDataBase();
                        getDataMed();


                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            DataRow newRow = DS.Tables["Medicaments"].NewRow();
                            newRow["Id_Med"] = ds.Tables[0].Rows[i][0];
                            newRow["Name_Med"] = ds.Tables[0].Rows[i][1];
                            newRow["End_date"] = ds.Tables[0].Rows[i][2].ToString().Split('T')[0].Replace('-', '/');
                            newRow["Quantity"] = ds.Tables[0].Rows[i][3];
                            newRow["Price"] = ds.Tables[0].Rows[i][4];
                            newRow["Id_supplier"] = ds.Tables[0].Rows[i][5];
                            DS.Tables["Medicaments"].Rows.Add(newRow);
                        }
                        dataGridView1.DataSource = DS.Tables["Medicaments"];
                        label17.Text = pos + 1 + "/" + DS.Tables["Medicaments"].Rows.Count;

                    }
                }
            
                
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (pos != -1)
            {
                pos = 0;
                nav();
                dataGridView1.Rows[pos].Cells[0].Selected = true;
            }
            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (pos != -1)
            {
                if (pos > 0)
                {
                    pos--;
                }
                nav();
                dataGridView1.Rows[pos].Cells[0].Selected = true;
            }
            
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (pos != -1)
            {
                if (pos < DS.Tables["Medicaments"].Rows.Count - 1)
                {
                    pos++;
                }
                nav();
                dataGridView1.Rows[pos].Cells[0].Selected = true;
            }
            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (pos != -1)
            {
                pos = DS.Tables["Medicaments"].Rows.Count - 1;
                nav();
                dataGridView1.Rows[pos].Cells[0].Selected = true;
            }
            
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            UpdateDataBase();
            DS.Tables["Medicaments"].Clear();
            DS.Tables["supplier"].Clear();
            panel1.Visible = true;
            panel2.Visible = false;
            panel3.Visible = false;
            pictureBox4.Image = Medic.Properties.Resources.Client_black1;
            pictureBox3.Image = Medic.Properties.Resources.Medicament_white;
            pictureBox5.Image = Medic.Properties.Resources.Supplier_white;
            pictureBox6.Image = Medic.Properties.Resources.Order_white;
        }




        private void UpdateDataBase()
        {
            con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString);
            da = new SqlDataAdapter("SELECT * FROM Medicaments", con);
            SqlCommandBuilder sqlcmdBuilder = new SqlCommandBuilder();
            sqlcmdBuilder.DataAdapter = da;
            da.Update(DS, "Medicaments");
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            UpdateDataBase();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        //Client Code start


        PharmacienLinqDataContext linq = new PharmacienLinqDataContext();
        private void panel1_VisibleChanged(object sender, EventArgs e)
        {
            if (panel1.Visible)
            {
                Thread th1 = new Thread(() =>
                {
                    var tab = from E in linq.clients select new { E.Id_client, E.Full_name_client, E.Email_client, E.Date_Naissance_client, E.Tele_client, E.Address_client, E.Maladie };
                    Action action1 = () => ClientDGV.DataSource = tab;
                    this.BeginInvoke(action1);
                    Action action2 = () => tab.ToList();
                    this.BeginInvoke(action2);
                });
                th1.IsBackground = true;
                th1.Start();
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            pictureBox3.Image = Medic.Properties.Resources.Medicaments_black;
            pictureBox4.Image = Medic.Properties.Resources.Client_white;
            pictureBox5.Image = Medic.Properties.Resources.Supplier_white;
            pictureBox6.Image = Medic.Properties.Resources.Order_white;
            getDataMed();

        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (txtClientID.Text.Trim() != "")
            {
                if (txtFullName.Text.Trim() != "")
                {
                    if (txtEmail.Text.Trim() != "" )
                    {
                        if (Regex.IsMatch(txtEmail.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
                        {
                            if (DateOfBirth.Value < DateTime.Now)
                            {
                                if (txtPhone.Text.Trim() != "")
                                {
                                    if (txtAddress.Text.Trim() != "")
                                    {
                                        if (txtSickness.Text.Trim() != "")
                                        {
                                            client cc1 = new client();
                                            cc1 = linq.clients.SingleOrDefault(cl => cl.Id_client == int.Parse(txtClientID.Text));
                                            if (cc1 == null)
                                            {
                                                client cc = new client();
                                                cc.Id_client = int.Parse(txtClientID.Text);
                                                cc.Full_name_client = txtFullName.Text;
                                                cc.Email_client = txtEmail.Text;
                                                cc.Date_Naissance_client = DateOfBirth.Value;
                                                cc.Tele_client = txtPhone.Text;
                                                cc.Address_client = txtAddress.Text;
                                                cc.Maladie = txtSickness.Text;
                                                linq.clients.InsertOnSubmit(cc);
                                                linq.SubmitChanges();
                                                var tab = from E in linq.clients select new { E.Id_client, E.Full_name_client, E.Email_client, E.Date_Naissance_client, E.Tele_client, E.Address_client, E.Maladie };
                                                ClientDGV.DataSource = tab;
                                                txtClientID.Clear();
                                                txtFullName.Clear();
                                                txtEmail.Clear();
                                                DateOfBirth.Value = DateTime.Now;
                                                txtPhone.Clear();
                                                txtAddress.Clear();
                                                txtSickness.Clear();
                                            }
                                            else
                                            {
                                                MessageBox.Show("You already insert this client.", "Duplicated client!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Client sickness is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Client address is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Client phone is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                            else
                            {
                                MessageBox.Show("The Date of birth that you entered is invalid.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        else
                        {
                            MessageBox.Show("This email format that you entered is invalid", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Client email is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("Client full name is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Client ID is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (txtClientID.Text.Trim() != "")
            {
                if (txtFullName.Text.Trim() != "")
                {
                    if (txtEmail.Text.Trim() != "")
                    {
                        if (Regex.IsMatch(txtEmail.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
                        {
                            if (DateOfBirth.Value < DateTime.Now)
                            {
                                if (txtPhone.Text.Trim() != "")
                                {
                                    if (txtAddress.Text.Trim() != "")
                                    {
                                        if (txtSickness.Text.Trim() != "")
                                        {
                                            client cc = new client();
                                            cc = linq.clients.SingleOrDefault(cl => cl.Id_client == int.Parse(txtClientID.Text));
                                            if (cc != null)
                                            {
                                                cc.Full_name_client = txtFullName.Text;
                                                cc.Email_client = txtEmail.Text;
                                                cc.Date_Naissance_client = DateOfBirth.Value;
                                                cc.Tele_client = txtPhone.Text;
                                                cc.Address_client = txtAddress.Text;
                                                cc.Maladie = txtSickness.Text;
                                                linq.SubmitChanges();
                                                var tab = from E in linq.clients select new { E.Id_client, E.Full_name_client, E.Email_client, E.Date_Naissance_client, E.Tele_client, E.Address_client, E.Maladie };
                                                ClientDGV.DataSource = tab;
                                                txtClientID.Clear();
                                                txtFullName.Clear();
                                                txtEmail.Clear();
                                                DateOfBirth.Value = DateTime.Now;
                                                txtPhone.Clear();
                                                txtAddress.Clear();
                                                txtSickness.Clear();
                                            }
                                            else
                                            {
                                                MessageBox.Show("There is no client with that ID to update.", "Update Client!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Client sickness is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Client address is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Client phone is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                            else
                            {
                                MessageBox.Show("The Date of birth that you entered is invalid.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        else
                        {
                            MessageBox.Show("This email format that you entered is invalid", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Client email is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("Client full name is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Client ID is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void button12_Click(object sender, EventArgs e)
        {
            if (txtClientID.Text.Trim() != "")
            {
                if (ClientDGV.Rows.Count > 0)
                {
                    if (MessageBox.Show("Are you sure do you wanna delete this client ?", "Delete Client ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        int ID = int.Parse(txtClientID.Text);
                        client obj = linq.clients.SingleOrDefault(cl => cl.Id_client == ID);
                        linq.clients.DeleteOnSubmit(obj);
                        linq.SubmitChanges();
                        var tab = from E in linq.clients select new { E.Id_client, E.Full_name_client, E.Email_client, E.Date_Naissance_client, E.Tele_client, E.Address_client, E.Maladie };
                        ClientDGV.DataSource = tab;
                        txtClientID.Clear();
                        txtFullName.Clear();
                        txtEmail.Clear();
                        DateOfBirth.Value = DateTime.Now;
                        txtPhone.Clear();
                        txtAddress.Clear();
                        txtSickness.Clear();
                    }
                }
            }
            else
            {
                MessageBox.Show("Client ID is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ClientDGV_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (ClientDGV.Rows.Count != 0)
            {
                txtClientID.Text = ClientDGV.Rows[e.RowIndex].Cells[0].Value.ToString();
                txtFullName.Text = ClientDGV.Rows[e.RowIndex].Cells[1].Value.ToString();
                txtEmail.Text = ClientDGV.Rows[e.RowIndex].Cells[2].Value.ToString();
                DateOfBirth.Value = DateTime.Parse(ClientDGV.Rows[e.RowIndex].Cells[3].Value.ToString());
                txtPhone.Text = ClientDGV.Rows[e.RowIndex].Cells[4].Value.ToString();
                txtAddress.Text = ClientDGV.Rows[e.RowIndex].Cells[5].Value.ToString();
                txtSickness.Text = ClientDGV.Rows[e.RowIndex].Cells[6].Value.ToString();
            }
        }

        private void txtClientID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }


        private void Clearbtn_Click(object sender, EventArgs e)
        {
            if (ClientDGV.Rows.Count > 0)
            {
                if (MessageBox.Show("Are you sure do you wanna clear everything ?", "Clear Everything", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    linq.ExecuteCommand("DELETE FROM client");
                    txtClientID.Clear();
                    txtFullName.Clear();
                    txtEmail.Clear();
                    DateOfBirth.Value = DateTime.Now;
                    txtPhone.Clear();
                    txtAddress.Clear();
                    txtSickness.Clear();
                    var tab = from E in linq.clients select new { E.Id_client, E.Full_name_client, E.Email_client, E.Date_Naissance_client, E.Tele_client, E.Address_client, E.Maladie };
                    ClientDGV.DataSource = tab;
                }
            }
            else
            {
                MessageBox.Show("There is nothing to clear!", "Clients Clear", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            UpdateDataBase();
            DS.Tables["Medicaments"].Clear();
            DS.Tables["supplier"].Clear();
            panel2.Visible = true;
            panel1.Visible = false;
            panel3.Visible = false;
            pictureBox5.Image = Medic.Properties.Resources.Supplier_black;
            pictureBox4.Image = Medic.Properties.Resources.Client_white;
            pictureBox3.Image = Medic.Properties.Resources.Medicament_white;
            pictureBox6.Image = Medic.Properties.Resources.Order_white;
        }

        //Supplier code start


        pharmacienEntities phEniti;
        private void GetDataSupplier()
        {
            using (phEniti = new pharmacienEntities())
            {
                var tab = from X in phEniti.supplier select new { X.Id_supplier, X.Full_name_supplier, X.Email_supplier, X.Tele_supplier, X.Address_supplier, X.Region_supplier };
                SupplierDGV.DataSource = tab.ToList();
            }
            label23.Text = SupplierDGV.Rows.Count.ToString();
        }
        private void AddSupplierBtn_Click(object sender, EventArgs e)
        {
            if (txtSupplierID.Text.Trim() != "")
            {
                if (txtSupplierName.Text.Trim() != "")
                {
                    if (txtSupplierEmail.Text.Trim() != "")
                    {
                        if (Regex.IsMatch(txtSupplierEmail.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
                        {
                            if (txtSupplierPhone.Text.Trim() != "")
                            {
                                if (txtSupplierAddress.Text.Trim() != "")
                                {
                                    if (ComboSupplierCountry.Text.Trim() != "")
                                    {
                                        supplier sp1 = new supplier();
                                        phEniti = new pharmacienEntities();
                                        sp1.Id_supplier = int.Parse(txtSupplierID.Text);
                                        sp1 = phEniti.supplier.SingleOrDefault(supp => supp.Id_supplier == sp1.Id_supplier);
                                        if (sp1 == null)
                                        {
                                            supplier sp2 = new supplier();
                                            sp2.Id_supplier = int.Parse(txtSupplierID.Text);
                                            sp2.Full_name_supplier = txtSupplierName.Text;
                                            sp2.Email_supplier = txtSupplierEmail.Text;
                                            sp2.Tele_supplier = txtSupplierPhone.Text;
                                            sp2.Address_supplier = txtSupplierAddress.Text;
                                            sp2.Region_supplier = ComboSupplierCountry.SelectedItem.ToString();
                                            phEniti.supplier.Add(sp2);
                                            phEniti.SaveChanges();
                                            GetDataSupplier();
                                            txtSupplierID.Clear();
                                            txtSupplierName.Clear();
                                            txtSupplierEmail.Clear();
                                            txtSupplierPhone.Clear();
                                            txtSupplierAddress.Clear();
                                            ComboSupplierCountry.SelectedIndex = -1;
                                        }
                                        else
                                        {
                                            MessageBox.Show("You already insert this supplier.", "Duplicated client!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Supplier country is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Supplier address is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Supplier phone is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        else
                        {
                            MessageBox.Show("This email format that you entered is invalid", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Supplier email is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("Supplier full name is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Supplier id is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void panel2_VisibleChanged(object sender, EventArgs e)
        {
            if (panel2.Visible)
            {
                GetDataSupplier();
            }
        }

        private void txtSupplierID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtSupplierPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void UpdateSupplierBtn_Click(object sender, EventArgs e)
        {
            if (txtSupplierID.Text.Trim() != "")
            {
                if (txtSupplierName.Text.Trim() != "")
                {
                    if (txtSupplierEmail.Text.Trim() != "")
                    {
                        if (Regex.IsMatch(txtSupplierEmail.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
                        {
                            if (txtSupplierPhone.Text.Trim() != "")
                            {
                                if (txtSupplierAddress.Text.Trim() != "")
                                {
                                    if (ComboSupplierCountry.Text.Trim() != "")
                                    {
                                        supplier sp1 = new supplier();
                                        phEniti = new pharmacienEntities();
                                        sp1.Id_supplier = int.Parse(txtSupplierID.Text);
                                        sp1 = phEniti.supplier.SingleOrDefault(supp => supp.Id_supplier == sp1.Id_supplier);
                                        if (sp1 != null)
                                        {
                                            sp1.Full_name_supplier = txtSupplierName.Text;
                                            sp1.Email_supplier = txtSupplierEmail.Text;
                                            sp1.Tele_supplier = txtSupplierPhone.Text;
                                            sp1.Address_supplier = txtSupplierAddress.Text;
                                            sp1.Region_supplier = ComboSupplierCountry.SelectedItem.ToString();
                                            phEniti.SaveChanges();
                                            GetDataSupplier();
                                            txtSupplierID.Clear();
                                            txtSupplierName.Clear();
                                            txtSupplierEmail.Clear();
                                            txtSupplierPhone.Clear();
                                            txtSupplierAddress.Clear();
                                            ComboSupplierCountry.SelectedIndex = -1;
                                        }
                                        else
                                        {
                                            MessageBox.Show("There is no supplier with that ID to update.", "Supplier update !", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Supplier country is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Supplier address is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Supplier phone is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        else
                        {
                            MessageBox.Show("This email format that you entered is invalid", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Supplier email is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("Supplier full name is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Supplier id is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void DeleteSupplierBtn_Click(object sender, EventArgs e)
        {
            if (txtSupplierID.Text.Trim() != "")
            {
                if (SupplierDGV.SelectedRows.Count > 0)
                {
                    if (MessageBox.Show("Are you sure do you wanna delete this supplier (All medication that you bought from him will be deleted also) ?", "Delete Supplier", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        supplier sp0 = new supplier();
                        using (phEniti = new pharmacienEntities())
                        {
                            sp0.Id_supplier = int.Parse(txtSupplierID.Text);
                            sp0 = phEniti.supplier.Where(sppu => sppu.Id_supplier == sp0.Id_supplier).SingleOrDefault();
                            phEniti.supplier.Remove(sp0);
                            phEniti.SaveChanges();
                            GetDataSupplier();
                            txtSupplierID.Clear();
                            txtSupplierName.Clear();
                            txtSupplierEmail.Clear();
                            txtSupplierPhone.Clear();
                            txtSupplierAddress.Clear();
                            ComboSupplierCountry.SelectedIndex = -1;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Supplier ID is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void SupplierDGV_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (SupplierDGV.Rows.Count != 0)
            {
                txtSupplierID.Text = SupplierDGV.Rows[e.RowIndex].Cells[0].Value.ToString();
                txtSupplierName.Text = SupplierDGV.Rows[e.RowIndex].Cells[1].Value.ToString();
                txtSupplierEmail.Text = SupplierDGV.Rows[e.RowIndex].Cells[2].Value.ToString();
                txtSupplierPhone.Text = SupplierDGV.Rows[e.RowIndex].Cells[3].Value.ToString();
                txtSupplierAddress.Text = SupplierDGV.Rows[e.RowIndex].Cells[4].Value.ToString();
                ComboSupplierCountry.SelectedItem = SupplierDGV.Rows[e.RowIndex].Cells[5].Value.ToString();
            }
        }

        private void ClearSupplierBtn_Click(object sender, EventArgs e)
        {
            if (SupplierDGV.Rows.Count > 0)
            {
                if (MessageBox.Show("Are you sure do you wanna clear everything (All medication will be deleted also) ?", "Clear Everything", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    phEniti = new pharmacienEntities();
                    phEniti.Database.ExecuteSqlCommand("DELETE FROM supplier");
                    GetDataSupplier();
                    txtSupplierID.Clear();
                    txtSupplierName.Clear();
                    txtSupplierEmail.Clear();
                    txtSupplierPhone.Clear();
                    txtSupplierAddress.Clear();
                    ComboSupplierCountry.SelectedIndex = -1;
                }
            }
            else
            {
                MessageBox.Show("There is nothing to clear!", "Supplier Clear", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }



        //Order Code Start


        SqlConnection con1;
        BindingSource sourceOrder = new BindingSource();
        BindingSource sourceMed = new BindingSource();
        BindingSource sourceClient = new BindingSource();
        public string minifyLong(decimal value)
        {
            if (value >= 100000000000)
                return (value / 1000000000).ToString("#,0") + "B";
            if (value >= 10000000000)
                return (value /  1000000000).ToString("0.#") + "B";
            if (value >= 100000000)
                return (value / 1000000).ToString("#,0") + "M";
            if (value >= 10000000)
                return (value / 1000000).ToString("0.#") + "M";
            if (value >= 100000)
                return (value / 1000).ToString("#,0") + "K";
            if (value >= 1000)
                return (value / 1000).ToString("0.#") + "K";
            return value.ToString("#,0");
        }
        decimal totalsaeles;
        private void GetDataOrder()
        {
            dataGridView1.DataSource = null;
            OrderDGV.DataBindings.Clear();
            ComboOrderClient.DataBindings.Clear();
            ComboOrderMedication.DataBindings.Clear();
            CounterOrderQuantity.DataBindings.Clear();
            OrderDate.DataBindings.Clear();
            txtOrderPrice.DataBindings.Clear();
            using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                SqlCommand cmd1 = new SqlCommand("SELECT Med_Order.Id_client,Med_Order.Id_Med,client.Full_name_client as [Client Name],Medicaments.Name_Med as [Medication Name],Med_Order.Quantity,Med_Order.Order_Date,Med_Order.Total_Price FROM Med_Order JOIN client ON Med_Order.Id_client=client.Id_client JOIN Medicaments ON Med_Order.Id_Med=Medicaments.Id_Med", con1);
                con1.Open();
                SqlDataReader dr1 = cmd1.ExecuteReader();
                if (dr1.HasRows)
                {
                    sourceOrder.DataSource = dr1;
                    OrderDGV.DataSource = sourceOrder;

                    ComboOrderClient.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Client Name"));
                    ComboOrderMedication.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Medication Name"));
                    CounterOrderQuantity.DataBindings.Add(new Binding("Value", sourceOrder, "Quantity"));
                    OrderDate.DataBindings.Add(new Binding("Value", sourceOrder, "Order_Date"));
                    txtOrderPrice.DataBindings.Add(new Binding("Text", sourceOrder, "Total_Price"));
                    label27.Text = (sourceOrder.Position + 1).ToString() + "/" + sourceOrder.Count.ToString();
                }
                else
                {
                    OrderDGV.Rows.Clear();
                    ComboOrderClient.SelectedIndex = -1;
                    ComboOrderMedication.SelectedIndex = -1;
                    CounterOrderQuantity.Value = 0;
                    OrderDate.Value = DateTime.Now;
                    txtOrderPrice.Clear();
                }
            }
            using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                SqlCommand cmd1 = new SqlCommand("SELECT sum(Med_Order.Total_Price) FROM Med_Order", con1);
                con1.Open();
                try
                {
                    totalsaeles = decimal.Parse(cmd1.ExecuteScalar().ToString());
                }
                catch (Exception)
                {

                    totalsaeles = 0;
                }
                
                
                lbTottalPrice.Text = minifyLong(totalsaeles)+" dh";
            }
        }
        private void panel4_VisibleChanged(object sender, EventArgs e)
        {
            if (panel3.Visible)
            {

                using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                {
                    SqlCommand cmd2 = new SqlCommand("SELECT * FROM Medicaments", con1);
                    con1.Open();
                    SqlDataReader dr2 = cmd2.ExecuteReader();
                    if (dr2.HasRows)
                    {
                        sourceMed.DataSource = dr2;
                        ComboOrderMedication.DataSource = sourceMed;
                        ComboOrderMedication.DisplayMember = "Name_Med";
                        ComboOrderMedication.ValueMember = "Id_Med";
                    }
                }
                using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                {
                    SqlCommand cmd3 = new SqlCommand("SELECT * FROM client", con1);
                    con1.Open();
                    SqlDataReader dr3 = cmd3.ExecuteReader();
                    if (dr3.HasRows)
                    {
                        sourceClient.DataSource = dr3;
                        ComboOrderClient.DataSource = sourceClient;
                        ComboOrderClient.DisplayMember = "Full_name_client";
                        ComboOrderClient.ValueMember = "Id_client";
                    }
                }
                GetDataOrder();
               
            }
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            UpdateDataBase();
            DS.Tables["Medicaments"].Clear();
            DS.Tables["supplier"].Clear();
            panel3.Visible = true;
            panel2.Visible = false;
            panel1.Visible = false;
            pictureBox6.Image = Medic.Properties.Resources.Order_black;
            pictureBox5.Image = Medic.Properties.Resources.Supplier_white;
            pictureBox4.Image = Medic.Properties.Resources.Client_white;
            pictureBox3.Image = Medic.Properties.Resources.Medicament_white;
        }

        private void txtOrderPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void OrderAddBtn_Click(object sender, EventArgs e)
        {
            if (ComboOrderClient.SelectedIndex != -1)
            {
                if (ComboOrderMedication.SelectedIndex != -1)
                {
                    if (CounterOrderQuantity.Value != 0)
                    {
                        if (txtOrderPrice.Text.Trim() != "")
                        {
                            decimal test = decimal.Parse(txtOrderPrice.Text);
                            if (test>0)
                            {
                                bool canAdd = false;
                                using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                                {
                                    SqlCommand cmd1 = new SqlCommand("SELECT * FROM Med_Order Where Id_client = @clientID AND Id_Med = @IdMed", con1);
                                    cmd1.Parameters.AddWithValue("@clientID", SqlDbType.Int).Value = ComboOrderClient.SelectedValue;
                                    cmd1.Parameters.AddWithValue("@IdMed", SqlDbType.Int).Value = ComboOrderMedication.SelectedValue;
                                    con1.Open();
                                    SqlDataReader dr1 = cmd1.ExecuteReader();
                                    if (!dr1.HasRows)
                                    {
                                        canAdd = true;
                                    }
                                    else
                                    {
                                        MessageBox.Show("You cannot duplicate orders but you can increase quantity instead.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }
                                }

                                if (canAdd)
                                {
                                    int Qt = 0;
                                    using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                                    {
                                        SqlCommand cmd1 = new SqlCommand("select Quantity from Medicaments where Id_Med = @id", con1);
                                        cmd1.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = ComboOrderMedication.SelectedValue;
                                        con1.Open();
                                        try
                                        {
                                            Qt = int.Parse(cmd1.ExecuteScalar().ToString());
                                        }
                                        catch (Exception)
                                        {

                                            Qt = 0;
                                        }

                                    }
                                    if (Qt != 0)
                                    {
                                        if (Qt >= CounterOrderQuantity.Value)
                                        {
                                            using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                                            {
                                                SqlCommand cmd1 = new SqlCommand("UPDATE Medicaments SET Quantity=@Qt WHERE Id_Med = @Id", con1);
                                                cmd1.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = ComboOrderMedication.SelectedValue;
                                                cmd1.Parameters.AddWithValue("@Qt", SqlDbType.Int).Value = Qt - CounterOrderQuantity.Value;
                                                con1.Open();
                                                cmd1.ExecuteNonQuery();

                                            }
                                            using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                                            {
                                                SqlCommand cmd4 = new SqlCommand("INSERT INTO Med_Order VALUES(@clientID,@IdMed,@Quantity,@orderDate,@orderPrice)", con1);
                                                cmd4.Parameters.AddWithValue("@clientID", SqlDbType.Int).Value = ComboOrderClient.SelectedValue;
                                                cmd4.Parameters.AddWithValue("@IdMed", SqlDbType.Int).Value = ComboOrderMedication.SelectedValue;
                                                cmd4.Parameters.AddWithValue("@Quantity", SqlDbType.Int).Value = CounterOrderQuantity.Value;
                                                cmd4.Parameters.AddWithValue("@orderDate", SqlDbType.Date).Value = OrderDate.Value;
                                                cmd4.Parameters.AddWithValue("@orderPrice", SqlDbType.Decimal).Value = decimal.Parse(txtOrderPrice.Text);
                                                con1.Open();
                                                cmd4.ExecuteNonQuery();
                                                GetDataOrder();
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Not enough quantity to sell this medication, Med Quantity Less Then Order Quantity.", "Not Enough Quantity!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("The quantity is equal to zero.", "Quantity Equal Zero!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }

                                }
                            }
                            else
                            {
                                MessageBox.Show("The Price is equal to zero.", "Price Equal Zero!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }

                        }
                        else
                        {
                            MessageBox.Show("Order Price is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Order quantity cannot be zero.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("Order medication is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Order client is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            sourceOrder.MoveFirst();
            label27.Text = (sourceOrder.Position + 1).ToString() + "/" + sourceOrder.Count.ToString();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            sourceOrder.MovePrevious();
            label27.Text = (sourceOrder.Position + 1).ToString() + "/" + sourceOrder.Count.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sourceOrder.MoveNext();
            label27.Text = (sourceOrder.Position + 1).ToString() + "/" + sourceOrder.Count.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sourceOrder.MoveLast();
            label27.Text = (sourceOrder.Position + 1).ToString() + "/" + sourceOrder.Count.ToString();
        }

        private void OrderDGV_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (OrderDGV.Rows.Count != 0)
            {
                ComboOrderClient.SelectedValue = OrderDGV.Rows[e.RowIndex].Cells[0].Value;
                ComboOrderMedication.SelectedValue = OrderDGV.Rows[e.RowIndex].Cells[1].Value;
                CounterOrderQuantity.Value = decimal.Parse( OrderDGV.Rows[e.RowIndex].Cells[4].Value.ToString());
                OrderDate.Value = DateTime.Parse( OrderDGV.Rows[e.RowIndex].Cells[5].Value.ToString());
                txtOrderPrice.Text= OrderDGV.Rows[e.RowIndex].Cells[6].Value.ToString();
                label27.Text = (e.RowIndex + 1).ToString() + "/" + sourceOrder.Count.ToString();
            }
            
        }

        private void OrderUpdateBtn_Click(object sender, EventArgs e)
        {
            if (ComboOrderClient.SelectedIndex != -1)
            {
                if (ComboOrderMedication.SelectedIndex != -1)
                {
                    if (CounterOrderQuantity.Value != 0)
                    {
                        if (txtOrderPrice.Text.Trim() != "")
                        {
                            decimal test = decimal.Parse(txtOrderPrice.Text);
                            if (test > (decimal) 0)
                            {
                                bool canAdd = false;
                                using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                                {
                                    SqlCommand cmd1 = new SqlCommand("SELECT * FROM Med_Order Where Id_client = @clientID AND Id_Med = @IdMed", con1);
                                    cmd1.Parameters.AddWithValue("@clientID", SqlDbType.Int).Value = ComboOrderClient.SelectedValue;
                                    cmd1.Parameters.AddWithValue("@IdMed", SqlDbType.Int).Value = ComboOrderMedication.SelectedValue;
                                    con1.Open();
                                    SqlDataReader dr1 = cmd1.ExecuteReader();
                                    if (dr1.HasRows)
                                    {
                                        canAdd = true;
                                    }
                                    else
                                    {
                                        MessageBox.Show("There is no order with that id to update.", "Order Not Found!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }
                                }
                                if (canAdd)
                                {
                                    int Qt = 0;
                                    using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                                    {
                                        SqlCommand cmd1 = new SqlCommand("select Quantity from Medicaments where Id_Med = @id", con1);
                                        cmd1.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = ComboOrderMedication.SelectedValue;
                                        con1.Open();
                                        try
                                        {
                                            Qt = int.Parse(cmd1.ExecuteScalar().ToString());
                                        }
                                        catch (Exception)
                                        {

                                            Qt = 0;
                                        }

                                    }
                                    if (Qt != 0)
                                    {
                                        if (Qt >= CounterOrderQuantity.Value)
                                        {
                                            using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                                            {
                                                SqlCommand cmd1 = new SqlCommand("UPDATE Medicaments SET Quantity=@Qt WHERE Id_Med = @Id", con1);
                                                cmd1.Parameters.AddWithValue("@Id", SqlDbType.Int).Value = ComboOrderMedication.SelectedValue;
                                                cmd1.Parameters.AddWithValue("@Qt", SqlDbType.Int).Value = Qt - CounterOrderQuantity.Value;
                                                con1.Open();
                                                cmd1.ExecuteNonQuery();

                                            }
                                            using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                                            {
                                                SqlCommand cmd4 = new SqlCommand("UPDATE Med_Order SET Quantity = @Quantity,Order_Date = @orderDate,Total_Price = @orderPrice WHERE Id_client = @clientID ANd Id_Med = @IdMed", con1);
                                                cmd4.Parameters.AddWithValue("@clientID", SqlDbType.Int).Value = ComboOrderClient.SelectedValue;
                                                cmd4.Parameters.AddWithValue("@IdMed", SqlDbType.Int).Value = ComboOrderMedication.SelectedValue;
                                                cmd4.Parameters.AddWithValue("@Quantity", SqlDbType.Int).Value = CounterOrderQuantity.Value;
                                                cmd4.Parameters.AddWithValue("@orderDate", SqlDbType.Date).Value = OrderDate.Value;
                                                cmd4.Parameters.AddWithValue("@orderPrice", SqlDbType.Decimal).Value = decimal.Parse(txtOrderPrice.Text);
                                                con1.Open();
                                                cmd4.ExecuteNonQuery();
                                                GetDataOrder();
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Not enough quantity to sell this medication, Med Quantity Less Then Order Quantity.", "Not Enough Quantity!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("The quantity is equal to zero.", "Quantity Equal Zero!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    }

                                }
                            }
                            else
                            {
                                MessageBox.Show("The Price is equal to zero.", "Price Equal Zero!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                           
                        }
                        else
                        {
                            MessageBox.Show("Order Price is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Order quantity cannot be zero.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("Order medication is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Order client is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void OrderDeleteBtn_Click(object sender, EventArgs e)
        {
            if (ComboOrderClient.SelectedIndex != -1)
            {
                if (ComboOrderMedication.SelectedIndex != -1)
                {
                    bool canAdd = false;
                    using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                    {
                        SqlCommand cmd1 = new SqlCommand("SELECT * FROM Med_Order Where Id_client = @clientID AND Id_Med = @IdMed", con1);
                        cmd1.Parameters.AddWithValue("@clientID", SqlDbType.Int).Value = ComboOrderClient.SelectedValue;
                        cmd1.Parameters.AddWithValue("@IdMed", SqlDbType.Int).Value = ComboOrderMedication.SelectedValue;
                        con1.Open();
                        SqlDataReader dr1 = cmd1.ExecuteReader();
                        if (dr1.HasRows)
                        {
                            canAdd = true;
                        }
                        else
                        {
                            MessageBox.Show("There is no order with that id to Delete.", "Order Not Found!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    if (canAdd)
                    {
                        if (MessageBox.Show("Are you sure do you wanna delete this Order ?", "Delete Order", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                            {
                                SqlCommand cmd4 = new SqlCommand("DELETE FROM Med_Order WHERE Id_client = @clientID AND Id_Med = @IdMed", con1);
                                cmd4.Parameters.AddWithValue("@clientID", SqlDbType.Int).Value = ComboOrderClient.SelectedValue;
                                cmd4.Parameters.AddWithValue("@IdMed", SqlDbType.Int).Value = ComboOrderMedication.SelectedValue;
                                con1.Open();
                                cmd4.ExecuteNonQuery();
                                GetDataOrder();
                            }
                        }
                       
                    }
                }
                else
                {
                    MessageBox.Show("Order medication is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Order client is missing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void OrderClearBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure do you wanna delete everything ?", "Clear Orders", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                {
                    SqlCommand cmd4 = new SqlCommand("DELETE FROM Med_Order", con1);
                    con1.Open();
                    cmd4.ExecuteNonQuery();
                    
                }
                GetDataOrder();
            }

        }

        private void panel3_MouseEnter(object sender, EventArgs e)
        {

        }

        private void pictureBox7_MouseEnter(object sender, EventArgs e)
        {
            pictureBox7.Image = Medic.Properties.Resources.search_btn_finel;
        }

        private void pictureBox7_MouseLeave(object sender, EventArgs e)
        {
            pictureBox7.Image = Medic.Properties.Resources.search_gray;
        }

        private void pictureBox8_MouseEnter(object sender, EventArgs e)
        {
            pictureBox8.Image = Medic.Properties.Resources.A_Z_black;
        }

        private void pictureBox8_MouseLeave(object sender, EventArgs e)
        {
            pictureBox8.Image = Medic.Properties.Resources.A_Z_Gray;
        }

        private void pictureBox9_MouseEnter(object sender, EventArgs e)
        {
            pictureBox9.Image = Medic.Properties.Resources.Z_A_black;
        }

        private void pictureBox9_MouseLeave(object sender, EventArgs e)
        {
            pictureBox9.Image = Medic.Properties.Resources.Z_A_Gray;
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        { 
            if (label32.Text == "Client")
            {
                ComboOrderClient.DataBindings.Clear();
                ComboOrderMedication.DataBindings.Clear();
                CounterOrderQuantity.DataBindings.Clear();
                OrderDate.DataBindings.Clear();
                txtOrderPrice.DataBindings.Clear();
                using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                {
                    SqlCommand cmd1 = new SqlCommand("SELECT client.Full_name_client as [Client Name],Medicaments.Name_Med as [Medication Name],Med_Order.Quantity,Med_Order.Order_Date,Med_Order.Total_Price FROM Med_Order JOIN client ON Med_Order.Id_client=client.Id_client JOIN Medicaments ON Med_Order.Id_Med=Medicaments.Id_Med WHERE client.Full_name_client = @name order by client.Full_name_client ASC", con1);
                    cmd1.Parameters.AddWithValue("@name", SqlDbType.Int).Value = txtSearch.Text;
                    con1.Open();
                    SqlDataReader dr1 = cmd1.ExecuteReader();
                    if (dr1.HasRows)
                    {
                        sourceOrder.DataSource = dr1;
                        OrderDGV.DataSource = sourceOrder;
                        ComboOrderClient.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Client Name"));
                        ComboOrderMedication.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Medication Name"));
                        CounterOrderQuantity.DataBindings.Add(new Binding("Value", sourceOrder, "Quantity"));
                        OrderDate.DataBindings.Add(new Binding("Value", sourceOrder, "Order_Date"));
                        txtOrderPrice.DataBindings.Add(new Binding("Text", sourceOrder, "Total_Price"));
                        label27.Text = (sourceOrder.Position + 1).ToString() + "/" + sourceOrder.Count.ToString();
                    }
                    else
                    {
                        GetDataOrder();
                        MessageBox.Show("There is no order with that client name.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);


                    }
                }
            }
            else
            {
                ComboOrderClient.DataBindings.Clear();
                ComboOrderMedication.DataBindings.Clear();
                CounterOrderQuantity.DataBindings.Clear();
                OrderDate.DataBindings.Clear();
                txtOrderPrice.DataBindings.Clear();
                using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                {
                    SqlCommand cmd1 = new SqlCommand("SELECT client.Full_name_client as [Client Name],Medicaments.Name_Med as [Medication Name],Med_Order.Quantity,Med_Order.Order_Date,Med_Order.Total_Price FROM Med_Order JOIN client ON Med_Order.Id_client=client.Id_client JOIN Medicaments ON Med_Order.Id_Med=Medicaments.Id_Med WHERE Medicaments.Name_Med = @name order by client.Full_name_client ASC", con1);
                    cmd1.Parameters.AddWithValue("@name", SqlDbType.Int).Value = txtSearch.Text;
                    con1.Open();
                    SqlDataReader dr1 = cmd1.ExecuteReader();
                    if (dr1.HasRows)
                    {
                        sourceOrder.DataSource = dr1;
                        OrderDGV.DataSource = sourceOrder;
                        ComboOrderClient.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Client Name"));
                        ComboOrderMedication.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Medication Name"));
                        CounterOrderQuantity.DataBindings.Add(new Binding("Value", sourceOrder, "Quantity"));
                        OrderDate.DataBindings.Add(new Binding("Value", sourceOrder, "Order_Date"));
                        txtOrderPrice.DataBindings.Add(new Binding("Text", sourceOrder, "Total_Price"));
                        label27.Text = (sourceOrder.Position + 1).ToString() + "/" + sourceOrder.Count.ToString();
                    }
                    else
                    {
                        GetDataOrder();
                        MessageBox.Show("There is no order with that Medicatoin name.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    }
                }
            }
        }

        private void label32_Click(object sender, EventArgs e)
        {
            if (label32.Text == "Client")
            {
                label32.Text = "Medic";
            }
            else
            {
                label32.Text = "Client";
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text.Trim()=="")
            {
                GetDataOrder();
            }
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            if (label32.Text == "Client")
            {
                ComboOrderClient.DataBindings.Clear();
                ComboOrderMedication.DataBindings.Clear();
                CounterOrderQuantity.DataBindings.Clear();
                OrderDate.DataBindings.Clear();
                txtOrderPrice.DataBindings.Clear();
                using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                {
                    SqlCommand cmd1 = new SqlCommand("SELECT client.Full_name_client as [Client Name],Medicaments.Name_Med as [Medication Name],Med_Order.Quantity,Med_Order.Order_Date,Med_Order.Total_Price FROM Med_Order JOIN client ON Med_Order.Id_client=client.Id_client JOIN Medicaments ON Med_Order.Id_Med=Medicaments.Id_Med order by client.Full_name_client ASC", con1);
                    cmd1.Parameters.AddWithValue("@name", SqlDbType.Int).Value = txtSearch.Text;
                    con1.Open();
                    SqlDataReader dr1 = cmd1.ExecuteReader();
                    if (dr1.HasRows)
                    {
                        sourceOrder.DataSource = dr1;
                        OrderDGV.DataSource = sourceOrder;
                        ComboOrderClient.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Client Name"));
                        ComboOrderMedication.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Medication Name"));
                        CounterOrderQuantity.DataBindings.Add(new Binding("Value", sourceOrder, "Quantity"));
                        OrderDate.DataBindings.Add(new Binding("Value", sourceOrder, "Order_Date"));
                        txtOrderPrice.DataBindings.Add(new Binding("Text", sourceOrder, "Total_Price"));
                        label27.Text = (sourceOrder.Position + 1).ToString() + "/" + sourceOrder.Count.ToString();
                    }
                    
                }
            }
            else
            {
                ComboOrderClient.DataBindings.Clear();
                ComboOrderMedication.DataBindings.Clear();
                CounterOrderQuantity.DataBindings.Clear();
                OrderDate.DataBindings.Clear();
                txtOrderPrice.DataBindings.Clear();
                using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                {
                    SqlCommand cmd1 = new SqlCommand("SELECT client.Full_name_client as [Client Name],Medicaments.Name_Med as [Medication Name],Med_Order.Quantity,Med_Order.Order_Date,Med_Order.Total_Price FROM Med_Order JOIN client ON Med_Order.Id_client=client.Id_client JOIN Medicaments ON Med_Order.Id_Med=Medicaments.Id_Med order by Medicaments.Name_Med ASC", con1);
                    cmd1.Parameters.AddWithValue("@name", SqlDbType.Int).Value = txtSearch.Text;
                    con1.Open();
                    SqlDataReader dr1 = cmd1.ExecuteReader();
                    if (dr1.HasRows)
                    {
                        sourceOrder.DataSource = dr1;
                        OrderDGV.DataSource = sourceOrder;
                        ComboOrderClient.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Client Name"));
                        ComboOrderMedication.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Medication Name"));
                        CounterOrderQuantity.DataBindings.Add(new Binding("Value", sourceOrder, "Quantity"));
                        OrderDate.DataBindings.Add(new Binding("Value", sourceOrder, "Order_Date"));
                        txtOrderPrice.DataBindings.Add(new Binding("Text", sourceOrder, "Total_Price"));
                        label27.Text = (sourceOrder.Position + 1).ToString() + "/" + sourceOrder.Count.ToString();
                    }
                }
            }
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            if (label32.Text == "Client")
            {
                ComboOrderClient.DataBindings.Clear();
                ComboOrderMedication.DataBindings.Clear();
                CounterOrderQuantity.DataBindings.Clear();
                OrderDate.DataBindings.Clear();
                txtOrderPrice.DataBindings.Clear();
                using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                {
                    SqlCommand cmd1 = new SqlCommand("SELECT client.Full_name_client as [Client Name],Medicaments.Name_Med as [Medication Name],Med_Order.Quantity,Med_Order.Order_Date,Med_Order.Total_Price FROM Med_Order JOIN client ON Med_Order.Id_client=client.Id_client JOIN Medicaments ON Med_Order.Id_Med=Medicaments.Id_Med order by client.Full_name_client DESC", con1);
                    cmd1.Parameters.AddWithValue("@name", SqlDbType.Int).Value = txtSearch.Text;
                    con1.Open();
                    SqlDataReader dr1 = cmd1.ExecuteReader();
                    if (dr1.HasRows)
                    {
                        sourceOrder.DataSource = dr1;
                        OrderDGV.DataSource = sourceOrder;
                        ComboOrderClient.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Client Name"));
                        ComboOrderMedication.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Medication Name"));
                        CounterOrderQuantity.DataBindings.Add(new Binding("Value", sourceOrder, "Quantity"));
                        OrderDate.DataBindings.Add(new Binding("Value", sourceOrder, "Order_Date"));
                        txtOrderPrice.DataBindings.Add(new Binding("Text", sourceOrder, "Total_Price"));
                        label27.Text = (sourceOrder.Position + 1).ToString() + "/" + sourceOrder.Count.ToString();
                    }

                }
            }
            else
            {
                ComboOrderClient.DataBindings.Clear();
                ComboOrderMedication.DataBindings.Clear();
                CounterOrderQuantity.DataBindings.Clear();
                OrderDate.DataBindings.Clear();
                txtOrderPrice.DataBindings.Clear();
                using (con1 = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                {
                    SqlCommand cmd1 = new SqlCommand("SELECT client.Full_name_client as [Client Name],Medicaments.Name_Med as [Medication Name],Med_Order.Quantity,Med_Order.Order_Date,Med_Order.Total_Price FROM Med_Order JOIN client ON Med_Order.Id_client=client.Id_client JOIN Medicaments ON Med_Order.Id_Med=Medicaments.Id_Med order by Medicaments.Name_Med DESC", con1);
                    cmd1.Parameters.AddWithValue("@name", SqlDbType.Int).Value = txtSearch.Text;
                    con1.Open();
                    SqlDataReader dr1 = cmd1.ExecuteReader();
                    if (dr1.HasRows)
                    {
                        sourceOrder.DataSource = dr1;
                        OrderDGV.DataSource = sourceOrder;
                        ComboOrderClient.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Client Name"));
                        ComboOrderMedication.DataBindings.Add(new Binding("SelectedItem", sourceOrder, "Medication Name"));
                        CounterOrderQuantity.DataBindings.Add(new Binding("Value", sourceOrder, "Quantity"));
                        OrderDate.DataBindings.Add(new Binding("Value", sourceOrder, "Order_Date"));
                        txtOrderPrice.DataBindings.Add(new Binding("Text", sourceOrder, "Total_Price"));
                        label27.Text = (sourceOrder.Position + 1).ToString() + "/" + sourceOrder.Count.ToString();
                    }
                }
            }
        }

        private void txtSearchMed_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (label36.Text == "ID")
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Form3 fr = new Form3(UserName);
            fr.Show();
            this.Hide();
        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            Form3 fr = new Form3(UserName);
            fr.Show();
            this.Hide();
        }

        private void button13_Click_1(object sender, EventArgs e)
        {
            Form3 fr = new Form3(UserName);
            fr.Show();
            this.Hide();
        }

        private void button14_Click_1(object sender, EventArgs e)
        {
            Form3 fr = new Form3(UserName);
            fr.Show();
            this.Hide();
        }


        private void button15_Click(object sender, EventArgs e)
        {
            if (OrderDGV.SelectedRows.Count>0)
            {
                PharmacienLinqDataContext dx = new PharmacienLinqDataContext();
                RAP.Form_Repport rpfrm = new RAP.Form_Repport();
                try
                {
                    int IdClient = (int)OrderDGV.CurrentRow.Cells[0].Value;
                    int IdMed = (int)OrderDGV.CurrentRow.Cells[1].Value;
                    var Order1 = dx.Med_Orders.Where(a => a.Id_client == IdClient && a.Id_Med == IdMed).ToList();
                    RAP.Form_Repport rpt = new RAP.Form_Repport();
                    rpt.reportViewer1.LocalReport.ReportEmbeddedResource = "Medic.RAP.RPT_Orders.rdlc";
                    rpt.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", Order1));
                    ReportParameter pr1 = new ReportParameter("ClientName", ComboOrderClient.Text);
                    ReportParameter pr2 = new ReportParameter("MedName", ComboOrderMedication.Text);
                    ReportParameter pr3 = new ReportParameter("TotalQuantity", CounterOrderQuantity.Value.ToString());
                    ReportParameter pr4 = new ReportParameter("OrderDate", OrderDate.Value.ToShortDateString());
                    ReportParameter pr5 = new ReportParameter("TotalPrice", txtOrderPrice.Text+" Dh");
                    rpt.reportViewer1.LocalReport.SetParameters(new ReportParameter[] { pr1, pr2, pr3, pr4, pr5 });
                    rpt.reportViewer1.RefreshReport();
                    rpt.ShowDialog();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            }
            
            
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                PharmacienLinqDataContext dx = new PharmacienLinqDataContext();
                RAP.Form_Repport rpfrm = new RAP.Form_Repport();
                try
                {
                    int idMed = (int)dataGridView1.CurrentRow.Cells[0].Value;
                    var Med1 = dx.Med_Orders.Where(a => a.Medicament.Id_Med == idMed ).ToList();
                    RAP.Form_Repport rpt = new RAP.Form_Repport();
                    rpt.reportViewer1.LocalReport.ReportEmbeddedResource = "Medic.RAP.RPT_Med.rdlc";
                    rpt.reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DataSet2", Med1));
                    ReportParameter pr1 = new ReportParameter("MedID", txtId.Text);
                    ReportParameter pr2 = new ReportParameter("MedName", txtName.Text);
                    ReportParameter pr3 = new ReportParameter("EndDate", Enddate.Value.ToShortDateString());
                    ReportParameter pr4 = new ReportParameter("Quantity", Quantity.Value.ToString());
                    ReportParameter pr5 = new ReportParameter("Price", txtPrice.Text + " Dh");
                    ReportParameter pr6 = new ReportParameter("SupplierName", comboSupplier.Text);
                    
                    MemoryStream mr = new MemoryStream();
                    
                    rpt.reportViewer1.LocalReport.SetParameters(new ReportParameter[] { pr1, pr2, pr3, pr4, pr5,pr6 });
                    rpt.reportViewer1.RefreshReport();
                    rpt.ShowDialog();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            }

        }
    }
}
