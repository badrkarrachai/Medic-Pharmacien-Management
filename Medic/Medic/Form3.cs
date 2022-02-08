using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.IO;

namespace Medic
{
    public partial class Form3 : Form
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
        public Form3()
        {
            InitializeComponent();
        }
        string UserName = "";
        public Form3(string userName)
        {
            InitializeComponent();
            this.UserName = userName;
        }


        private void DeleteBtn_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox15_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        SqlConnection con;
        DataTable counter1 = new DataTable();
        int clientCounter = 0;
        int orderCounter = 0;
        int medCounter = 0;
        decimal salesorder = 0;
        int medQuantity = 0;
        int suppliersCount = 0;
        decimal stockPrice = 0;
        public string minifyLong(decimal value)
        {
            if (value >= 100000000000)
                return (value / 1000000000).ToString("#,0") + "B";
            if (value >= 10000000000)
                return (value / 1000000000).ToString("0.#") + "B";
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
        public string minifyLong(int value)
        {
            if (value >= 100000000000)
                return (value / 1000000000).ToString("#,0") + "B";
            if (value >= 10000000000)
                return (value / 1000000000).ToString("0.#") + "B";
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
        private void Form3_Load(object sender, EventArgs e)
        {
            timer1.Start();
            label15.Text = UserName;
            //Get User Name
            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand("Select count(Id_client) From client", con);
                con.Open();
                try
                {
                    clientCounter = int.Parse(cmd.ExecuteScalar().ToString());
                }
                catch (Exception)
                {
                    clientCounter = 0;
                }
                label23.Text = minifyLong(clientCounter);
            }
            //clients counter
            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand("Select count(Id_client) From client", con);
                con.Open();
                try
                {
                    clientCounter = int.Parse(cmd.ExecuteScalar().ToString());
                }
                catch (Exception)
                {
                    clientCounter = 0;
                }
                label23.Text = minifyLong(clientCounter);
            }
            //orders counter
            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand("select count(Id_client) from Med_Order", con);
                con.Open();
                try
                {
                    orderCounter = int.Parse(cmd.ExecuteScalar().ToString());
                }
                catch (Exception)
                {
                    orderCounter = 0;
                }
                label2.Text = minifyLong(orderCounter);

            }
            //med counter
            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand("select COUNT(Id_Med) from Medicaments", con);
                con.Open();
                try
                {
                    medCounter = int.Parse(cmd.ExecuteScalar().ToString());
                }
                catch (Exception)
                {
                    medCounter = 0;
                }
                label3.Text = minifyLong(medCounter);
            }
            //sales price
            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand("SELECT sum(Med_Order.Total_Price) FROM Med_Order", con);
                con.Open();
                try
                {
                    salesorder = decimal.Parse(cmd.ExecuteScalar().ToString());
                }
                catch (Exception)
                {
                    salesorder = 0;
                }
                label4.Text = minifyLong(salesorder) + " dh";
            }
            //Med Quantitys
            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand("select SUM(Quantity) from Medicaments", con);
                con.Open();
                try
                {
                    medQuantity = int.Parse(cmd.ExecuteScalar().ToString());
                }
                catch (Exception)
                {
                    medQuantity = 0;
                }
                label5.Text = minifyLong(medQuantity);
            }
            //Supplier count
            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand("select count(Id_supplier) from supplier", con);
                con.Open();
                try
                {
                    suppliersCount = int.Parse(cmd.ExecuteScalar().ToString());
                }
                catch (Exception)
                {
                    suppliersCount = 0;
                }
                label7.Text = minifyLong(suppliersCount);
            }
            //Supplier count
            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                DataTable dt = new DataTable();
                SqlCommand cmd = new SqlCommand("select SUM(Price) from Medicaments", con);
                con.Open();
                try
                {
                    stockPrice = decimal.Parse(cmd.ExecuteScalar().ToString());
                }
                catch (Exception)
                {
                    stockPrice = 0;
                }
                label8.Text = minifyLong(stockPrice);
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label12.Text = DateTime.Now.ToString();
        }

        private void panel14_MouseEnter(object sender, EventArgs e)
        {
            pictureBox5.Visible = true;
        }

        private void panel14_MouseLeave(object sender, EventArgs e)
        {
            pictureBox5.Visible = false;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2(UserName);
            frm.Show();
            this.Hide();
        }

        private void pictureBox4_MouseEnter(object sender, EventArgs e)
        {
            pictureBox5.Visible = true;
        }

        private void pictureBox4_MouseLeave(object sender, EventArgs e)
        {
            pictureBox5.Visible = false;
        }

        private void panel14_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
