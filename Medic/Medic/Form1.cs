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
    public partial class Form1 : Form
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
        SqlConnection con;
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox15_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label3_MouseEnter(object sender, EventArgs e)
        {
            label3.ForeColor = Color.FromArgb(64, 64, 64);
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            label3.ForeColor = Color.Gray;
        }

        private void label4_MouseEnter(object sender, EventArgs e)
        {
            label4.ForeColor = Color.FromArgb(64, 64, 64);
        }

        private void label4_MouseLeave(object sender, EventArgs e)
        {
            label4.ForeColor = Color.Gray;
        }

        private void label5_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Image = Medic.Properties.Resources.btn_2;
            label5.BackColor = Color.FromArgb(247, 147, 30);
        }

        private void label5_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = Medic.Properties.Resources.btn_1;
            label5.BackColor = Color.FromArgb(251, 176, 59);
        }

        private void label3_Click(object sender, EventArgs e)
        {
            panel1.Visible = true;
            FirstNameBx.Select();
        }

        private void label14_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            FirstNameBx.Clear();
            LastNameBx.Clear();
            EmailBx.Clear();
            PhoneBx.Clear();
            AddressBx.Clear();
            PasswordBx.Clear();
        }

        private void label14_MouseEnter(object sender, EventArgs e)
        {
            label14.ForeColor = Color.FromArgb(64, 64, 64);
        }

        private void label14_MouseLeave(object sender, EventArgs e)
        {
            label14.ForeColor = Color.Gray;
        }

        private void label15_MouseEnter(object sender, EventArgs e)
        {
            pictureBox4.Image = Medic.Properties.Resources.btn_2;
            label15.BackColor = Color.FromArgb(247, 147, 30);
        }

        private void label15_MouseLeave(object sender, EventArgs e)
        {
            pictureBox4.Image = Medic.Properties.Resources.btn_1;
            label15.BackColor = Color.FromArgb(251, 176, 59);
        }

        private void label16_MouseEnter(object sender, EventArgs e)
        {
            label16.ForeColor = Color.FromArgb(64, 64, 64);
        }

        private void label16_MouseLeave(object sender, EventArgs e)
        {
            label16.ForeColor = Color.Gray;
        }

        private void label13_MouseEnter(object sender, EventArgs e)
        {
            pictureBox3.Image = Medic.Properties.Resources.btn_2;
            label13.BackColor = Color.FromArgb(247, 147, 30);
        }

        private void label13_MouseLeave(object sender, EventArgs e)
        {
            pictureBox3.Image = Medic.Properties.Resources.btn_1;
            label13.BackColor = Color.FromArgb(251, 176, 59);
        }
        string rand;
        private void SendEmail()
        {
            Thread thread3 = new Thread(() =>
            {

                Random rd = new Random();
                int rand_num = rd.Next(100000, 900000);
                rand = rand_num.ToString();



                SmtpClient clinet = new SmtpClient()
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential()
                    {
                        UserName = "medic.ik.sp124@gmail.com",
                        Password = "jqnkoeprmxcrvpef",
                    }

                };
                string x;
                MailAddress formailaddress = new MailAddress("medic.ik.sp124@gmail.com", "Medic");
                string reciver;
                if (panel2.Visible)
                {
                    reciver = EmailBx2.Text;
                }
                else
                {
                    reciver = EmailBx.Text;
                }

                MailAddress Tomailaddress = new MailAddress(reciver, "New User");
                using (StreamReader reader = File.OpenText("Medic_ver_code.html")) // Path to your 
                {
                    x = reader.ReadToEnd();
                }

                string ReplaceFirstOccurrence(string Source, string Find, string Replace)
                {
                    int Place = Source.IndexOf(Find);
                    string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
                    return result;
                }

                x = ReplaceFirstOccurrence(x, "{Vr}", rand);

                MailMessage message = new MailMessage()
                {
                    From = formailaddress,
                    Subject = "Verification code",
                    Body = x,
                    IsBodyHtml = true,
                };

                message.IsBodyHtml = true;
                message.To.Add(Tomailaddress);
                try
                {

                    clinet.Send(message);
                    Action ac3 = () => panel2.Visible = false;
                    this.BeginInvoke(ac3);
                    Action ac2 = () => panel1.Visible = false;
                    this.BeginInvoke(ac2);
                    Action ac1 = () => panel3.Visible = true;
                    this.BeginInvoke(ac1);
                    MessageBox.Show("A verification code has been sent to your email please check your email and give us the code.", "Register successfully!", MessageBoxButtons.OK, MessageBoxIcon.Information);


                }
                catch (Exception)
                {
                    MessageBox.Show("The verification code email has not been sent due to some reason, please try again or contact our support.", "Email has not been Sent!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }



            });
            thread3.IsBackground = true;
            thread3.Start();
        }
        string x = "register";
        private void CheckEmail()
        {
            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT IdEmp FROM employe WHERE Email_Emp = @Email", con);
                cmd.Parameters.AddWithValue("@Email", SqlDbType.VarChar).Value = EmailBx.Text;
                con.Open();
                id = Convert.ToString(cmd.ExecuteScalar());
            }
            if (id == "")
            {
                x = "register";
                SendEmail();
            }
            else
            {
                MessageBox.Show("Unfortunately the email you have used it's already linked to an account, you can log in with it or if it's not yours you can use another email.", "Already used email!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void label15_Click(object sender, EventArgs e)
        {

            if (FirstNameBx.Text.Trim() != "")
            {
                if (LastNameBx.Text.Trim() != "")
                {
                    if (EmailBx.Text.Trim() != "")
                    {
                        if (PhoneBx.Text.Trim() != "")
                        {
                            if (AddressBx.Text.Trim() != "")
                            {
                                if (PasswordBx.Text.Trim() != "")
                                {
                                    CheckEmail();
                                }
                                else
                                {
                                    MessageBox.Show("Password is messing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Address is messing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Phone Number is messing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Email is messing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                else
                {
                    MessageBox.Show("Last Name is messing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("First Name is messing above.", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void label17_MouseEnter(object sender, EventArgs e)
        {
            label17.ForeColor = Color.FromArgb(64, 64, 64);
        }

        private void label17_MouseLeave(object sender, EventArgs e)
        {
            label17.ForeColor = Color.Gray;
        }

        private void label18_MouseEnter(object sender, EventArgs e)
        {
            label18.ForeColor = Color.FromArgb(64, 64, 64);
        }

        private void label18_MouseLeave(object sender, EventArgs e)
        {
            label18.ForeColor = Color.Gray;
        }

        private void label23_MouseEnter(object sender, EventArgs e)
        {
            pictureBox6.Image = Medic.Properties.Resources.btn_2;
            label23.BackColor = Color.FromArgb(247, 147, 30);
        }

        private void label23_MouseLeave(object sender, EventArgs e)
        {
            pictureBox6.Image = Medic.Properties.Resources.btn_1;
            label23.BackColor = Color.FromArgb(251, 176, 59);
        }

        private void label17_Click(object sender, EventArgs e)
        {
            panel1.Visible = true;
        }
        int count = 0;
        private void label18_Click(object sender, EventArgs e)
        {
            if (count < 3)
            {
                SendEmail();
                count++;
            }
            else
            {
                label18.Visible = false;
            }

        }

        private void panel3_VisibleChanged(object sender, EventArgs e)
        {
            label18.Visible = true;
            VerifactionBx.Clear();

        }



        private void label25_MouseEnter(object sender, EventArgs e)
        {
            label25.ForeColor = Color.FromArgb(64, 64, 64);
        }

        private void label25_MouseLeave(object sender, EventArgs e)
        {
            label25.ForeColor = Color.Gray;
        }

        private void label24_MouseEnter(object sender, EventArgs e)
        {
            pictureBox5.Image = Medic.Properties.Resources.btn_2;
            label24.BackColor = Color.FromArgb(247, 147, 30);
        }

        private void label24_MouseLeave(object sender, EventArgs e)
        {
            pictureBox5.Image = Medic.Properties.Resources.btn_1;
            label24.BackColor = Color.FromArgb(251, 176, 59);
        }

        private void ToBase() {
            if (rand == VerifactionBx.Text.Trim())
            {
                using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO employe VALUES (@firstName,@LastName,@Email,@Phone,@Address,@Password)", con);
                    cmd.Parameters.AddWithValue("@firstName", SqlDbType.VarChar).Value = FirstNameBx.Text;
                    cmd.Parameters.AddWithValue("@LastName", SqlDbType.VarChar).Value = LastNameBx.Text;
                    cmd.Parameters.AddWithValue("@Email", SqlDbType.VarChar).Value = EmailBx.Text;
                    cmd.Parameters.AddWithValue("@Phone", SqlDbType.VarChar).Value = PhoneBx.Text;
                    cmd.Parameters.AddWithValue("@Address", SqlDbType.VarChar).Value = AddressBx.Text;
                    cmd.Parameters.AddWithValue("@Password", SqlDbType.VarChar).Value = PasswordBx.Text;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Thank you for your registration with Medic, You can now sign into your account.", "Register successfully!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Text = EmailBx.Text;
                    panel3.Visible = false;
                    FirstNameBx.Clear();
                    LastNameBx.Clear();
                    EmailBx.Clear();
                    PhoneBx.Clear();
                    AddressBx.Clear();
                    PasswordBx.Clear();
                    panel1.Visible = false;
                    textBox2.Clear();
                    textBox2.Select();
                }
            }
            else
            {
                MessageBox.Show("Unfortunately your verification code is wrong!, double check it Or you can send a new one.", "Wrong verification code!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private void label23_Click(object sender, EventArgs e)
        {
            if (x == "register")
            {
                ToBase();
            }
            else
            {
                if (rand == VerifactionBx.Text.Trim())
                {
                    panel3.Visible = false;
                    panel4.Visible = true;
                }
                else
                {
                    MessageBox.Show("Unfortunately your verification code is wrong!, double check it Or you can send a new one.", "Wrong verification code!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

            }

        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            if (x == "register")
            {
                ToBase();
            }
            else
            {
                if (rand == VerifactionBx.Text.Trim())
                {
                    panel3.Visible = false;
                    panel4.Visible = true;
                }
                else
                {
                    MessageBox.Show("Unfortunately your verification code is wrong!, double check it Or you can send a new one.", "Wrong verification code!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
        string id = null;
        private void UpdatePass()
        {
            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("UPDATE employe SET Password_Emp = @Password WHERE IdEmp = @id", con);
                cmd.Parameters.AddWithValue("@Password", SqlDbType.VarChar).Value = PasswordBx2.Text;
                cmd.Parameters.AddWithValue("@id", SqlDbType.VarChar).Value = id;
                con.Open();
                cmd.ExecuteNonQuery();
                MessageBox.Show("Your password has been changed successfully you can now login your account with the new password.", "Password Changed Successfully!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                panel4.Visible = false;
            }
        }

        private void label24_Click(object sender, EventArgs e)
        {
            UpdatePass();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            UpdatePass();
        }

        private void Getid()
        {
            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT IdEmp FROM employe WHERE Email_Emp = @Email", con);
                cmd.Parameters.AddWithValue("@Email", SqlDbType.VarChar).Value = EmailBx2.Text;
                con.Open();
                id = Convert.ToString(cmd.ExecuteScalar());
            }
            if (id != "")
            {
                x = "Recover";
                SendEmail();
            }
            else
            {
                MessageBox.Show("Unfortunately there is no account with that email, please double check your email or create a new account.", "Invalid email!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void label13_Click(object sender, EventArgs e)
        {
            Getid();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Getid();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            panel2.Visible = true;
        }

        private void label16_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
        }

        private void label25_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;
        }
        private void testData()
        {
            if (textBox1.Text.Trim() != "")
            {
                if (textBox2.Text.Trim() != "")
                {
                    using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["PhDb"].ConnectionString))
                    {
                        DataTable dt = new DataTable();
                        SqlCommand cmd = new SqlCommand("SELECT * FROM employe WHERE Email_Emp = @Email", con);
                        cmd.Parameters.AddWithValue("@Email", SqlDbType.VarChar).Value = textBox1.Text;
                        con.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            dt.Load(dr);
                            if (dt.Rows[0][6].ToString() == textBox2.Text)
                            {
                                string username = dt.Rows[0][1].ToString() + " " + dt.Rows[0][2].ToString();
                                Form3 fr = new Form3(username);
                                fr.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Sorry you have entered wrong password, if you don't remember your password you can change it by clicking on forgot password.", "Wrong Password!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        else
                        {
                            MessageBox.Show("There is no account with that email you can create a new one instead.", "Wrong Email!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("You didn't provide an password to log in", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("You didn't provide an email to log in", "Info is Messing!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }
        private void label5_Click(object sender, EventArgs e)
        {
            testData();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            testData();
        }

        private void panel2_VisibleChanged(object sender, EventArgs e)
        {
            if (panel2.Visible)
            {
                EmailBx2.Select();
            }
        }
    }
}
