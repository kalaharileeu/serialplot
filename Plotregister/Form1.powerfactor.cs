﻿using System;
using System.Collections.Generic;

namespace Plotregister
{
    public partial class Form1
    {
        string pf_sign = "00";
        List<string> listofcommands;

        private string getpfsign(float pf)
        {
            if (pf < 0.0)
                return "01";
            else
                return "00";
        }
        //Request a reactive power
        private void button5_Click(object sender, EventArgs e)
        {
            //the list of command to be sent to the inverter
            listofcommands = new List<string>();
            //gets the value from the textbos and convert it to float
            float floatvalue = (float)Convert.ToDouble(textBox3.Text);
            //gets a bool value from the conversion to check if it is valid
            bool valid = float.TryParse(textBox3.Text.ToString(), out floatvalue);
            if(valid)
            {
                richTextBox2.AppendText(floatvalue.ToString());
                int pf_reg = (int)(0x400000 * Math.Abs(floatvalue));
                richTextBox2.AppendText("\r");
                richTextBox2.AppendText(pf_reg.ToString());

                string hexvalue = pf_reg.ToString("X");
                richTextBox2.AppendText("\r");
                richTextBox2.AppendText(hexvalue + ": the hex value");
                //sets the sign up for the reactive power phase shift
                pf_sign = getpfsign(floatvalue);
                //loads the list with the command, ready to send
                listofcommands.Add(string.Format("wl E28 {0}\r", pf_sign));
                listofcommands.Add(string.Format("wl E29 {0}\r", hexvalue.Substring(0, 2)));
                listofcommands.Add(string.Format("wl E2A {0}\r", hexvalue.Substring(2, 2)));
                listofcommands.Add(string.Format("wl E2B {0}\r", hexvalue.Substring(4, 2)));
                listofcommands.Add(string.Format("wl E23 01\r"));//Enable fixed power mode

                sendgeneralcommand(listofcommands);

                foreach (string s in listofcommands)
                {
                    richTextBox2.AppendText(s);
                }

                //PCU.debugger_cmd("wl E29 %s\r" % pf_str[0:2])
                //PCU.debugger_cmd("wl E2A %s\r" % pf_str[2:4])
                //PCU.debugger_cmd("wl E2B %s\r" % pf_str[4:6])
                //PCU.debugger_cmd("wl E23 01\r")  # enable fixed power factor mode

            }
        }
    }
}
