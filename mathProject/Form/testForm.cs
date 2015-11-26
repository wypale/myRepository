using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mathProject
{
    public partial class testForm : Form
    {
        public testForm()
        {
            InitializeComponent();
        }

        private void txtA_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox curText=sender as TextBox;
            if (!(Char.IsDigit(e.KeyChar)))
            {
                if (e.KeyChar == '.' || e.KeyChar == ',' || e.KeyChar == (char)Keys.Back)
                {
                    if (e.KeyChar == (char)Keys.Back)
                    {

                    }
                    else
                        if (curText.Text.Contains(",") || curText.Text.Contains("."))
                        {
                            e.Handled = true;
                        }
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void butCalc_Click(object sender, EventArgs e)
        {
            if (txtA.Text.Trim() != "" && txtB.Text.Trim() != "")
                txtResult.Text = MyMath.triangleArea(MyMath.ifDecSepDbl(txtA.Text), MyMath.ifDecSepDbl(txtB.Text)).ToString();
            else
                txtResult.Text = "Неверные данные";
        }










    }
}
