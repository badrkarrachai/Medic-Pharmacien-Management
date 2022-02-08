using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Medic.RAP
{
    public partial class Form_Repport : Form
    {
        public Form_Repport()
        {
            InitializeComponent();
        }

        private void Form_Repport_Load(object sender, EventArgs e)
        {

            this.reportViewer1.RefreshReport();
        }
    }
}
