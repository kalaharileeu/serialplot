using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;


namespace Plotregister
{
    public partial class Form1 : Form
    {

        //Get the serial ports
        private void button1_Click(object sender, EventArgs e)
        {
            button4.Enabled = true;
            string[] serialitems = SerialPort.GetPortNames();
            foreach (var v in serialitems)
                listBox1.Items.Add(v);

        }
        //M button clicked
        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            if ((!theport.IsOpen) && !(theport == null))
                theport.Open();
            if (theport.IsOpen)
            {
                if (findprompt())
                {
                    theport.Write("M");
                    Thread.Sleep(2);//Sleep 5ms, could make it 1
                    readmcommand();
                }
                else
                {
                    richTextBox1.Clear();
                    richTextBox1.AppendText("error: Could not find the prompt! :" + theport.PortName);
                }
            }
            theport.Close();
        }
        //click the clear button to clear the richtext box
        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox1.AppendText(ccommandeventdata);
        }

        //Cmommand buildler
        private void button4_Click(object sender, EventArgs e)
        {
            string ccommand = "c ";
            richTextBox1.Clear();
            ccommand += textBox1.Text + " ";//add the register number
            ccommand += comboBox2.SelectedItem.ToString() + " ";
            ccommand += comboBox1.SelectedItem.ToString();
            richTextBox1.AppendText("Your capturing this: " + ccommand);
            //send the capture command to the serial port
            sendcommand(ccommand);

        }
        //**************************************Enter Button*************************
        //Request a reactive power
        private void button5_Click(object sender, EventArgs e)
        {
            //the list of command to be sent to the inverter
            listofcommands.Clear();
            //gets the value from the textbos and convert it to float
            float floatvalue;
            //gets a bool value from the conversion to check if it is valid
            bool valid = float.TryParse(textBox3.Text.ToString(), out floatvalue);
            if (valid)
            {
                processpfrequest(floatvalue);
            }
            else
            {
                richTextBox2.AppendText("nofloat, moving on");
            }

            if (textBox4.Text != "")
            {
                listofcommands.Clear();
                listofcommands.Add(textBox4.Text.Trim());
                sendgeneralcommand(listofcommands);
            }
        }
    }
}
