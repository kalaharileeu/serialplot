using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotregister
{
    public partial class Form1
    {

        private void button5_Click(object sender, EventArgs e)
        {
            float floatvalue = (float)Convert.ToDouble(textBox3.Text);
            bool valid = float.TryParse(textBox3.Text.ToString(), out floatvalue);
            if(valid)
            {
                richTextBox2.AppendText(e.ToString());
            }
        }
    }
}
