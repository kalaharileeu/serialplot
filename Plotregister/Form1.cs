using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;

namespace Plotregister
{
    //public delegate void datedelegata(string value);

    public partial class Form1 : Form
    {
        Stopwatch readstw = new Stopwatch();
        //counter use to find the prompt
        int count = 0;
        //setup the items for the combo boxes below
        Tuple<string, string> comboitembin = new Tuple<string, string>("0", "Bin");
        Tuple<string, string> comboitemphase = new Tuple<string, string>("1", "Phase");
        Tuple<string, string> comboitemone = new Tuple<string, string>("1", "1");
        Tuple<string, string> comboitemtwo = new Tuple<string, string>("2", "2");
        Tuple<string, string> comboitemthree = new Tuple<string, string>("3", "3");
        //Serial port here
        SerialPort theport;
        //Mcommand data
        List<string> mcommandlist;
        List<string> ccommanddata;
        //List<string> ccommanddatainterrupt;
        int wd; //write delay
        static bool dataatport = false;
        public delegate void AddDataDelegate(String myString);

        public AddDataDelegate myDelegate;

        public Form1()
        {
            InitializeComponent();
            comboBox1.Items.Add(comboitembin.Item1);
            comboBox1.Items.Add(comboitemphase.Item1);
            comboBox2.Items.Add(comboitemone.Item1);
            comboBox2.Items.Add(comboitemtwo.Item1);
            comboBox2.Items.Add(comboitemthree.Item1);
            //Disable buttons
            button2.Enabled = false;
            button4.Enabled = false;
            mcommandlist = new List<string>();
            //ccommanddata = new List<string>();
            wd = Int32.Parse(textBox2.Text);
            comboBox1.SelectedItem = comboitemphase.Item1;//set defaults in combobox1
            comboBox2.SelectedItem = comboitemtwo.Item1;//set defaults in combobox2
            myDelegate = new AddDataDelegate(datafromevent);
        }
        //write text delay between characters
//***********************************************************Write delay*********************************************************************
        private int wdf()//return the write delay
        {
            int j;
            if (Int32.TryParse(textBox2.Text, out j))
            {
                wd = j;
                return wd;
            }
            else
                richTextBox1.AppendText("Only integers for delays.");

            return wd;
        }
//***********************************************************Filter listbox choices************************************************************
        //find the serial item in the listbox
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;//This is where the mouse is clicked
            string item = (string)listBox1.Items[index];
            //if theport is not open create a new port
            if (theport == null)
            {
                setupport(item);//creat a new port
                button2.Enabled = true;//The M command button
                button2.PerformClick();
            }
            else if(theport.IsOpen)
            {
                theport.Close();//Close the port
                richTextBox1.AppendText("The port current port will be closed: "
                    + theport.PortName + "\r");
                setupport(item);//setup a new port
                richTextBox1.AppendText(" New port created: " + theport.PortName);
                button2.Enabled = true;//The M command button
                button2.PerformClick();
            }
            else
            {
                theport = null;
                setupport(item);//setup a new port
                richTextBox1.AppendText("This port was selected previously but was not open: "
                    + theport.PortName  + "\r");
                richTextBox1.AppendText("error \r");
                button2.Enabled = true;//The M command button
                button2.PerformClick();
            }
        }
//***********************************************************Serial port setup here************************************************
        private void setupport(string portname)
        {
            theport = new SerialPort(portname, 115200);//hardcoded  boadrate, will always stay the same
            theport.Parity = Parity.None;
            theport.StopBits = StopBits.One;
            theport.DataBits = 8;
            theport.RtsEnable = false;

            theport.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);//Add event to the handler
        }
//************************************************interrupt for data at port********************************************************
        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            //if there is data on the serial port dataatport true
            dataatport = true;
            //SerialPort sp = (SerialPort)sender;
            //string indata = sp.ReadExisting();
            //Invo
            //richTextBox1.AppendText(dataatport.ToString());
        }

        private void Theport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void closenullport()
        {
            theport.Close();
            theport = null;
        }
    
//***********************************************************Find the prompt****************************************************
        private bool findprompt()
        {
            count += 1;
            if (theport.ReadExisting().EndsWith(">"))
                return true;
            else
            {
                theport.Write("\r");
                Thread.Sleep(10);
            }
            if (count > 10)
            {
                count = 0;
                return false;
            }
            if (findprompt())
                return true;
            else
                return false;
        }
//***********************************************************M Read: the M command*********************************************
        private void readmcommand()
        {
            int breakout = 0;
            while (theport.BytesToRead < 700)
            {
                Thread.Sleep(20);//Wait until the buffer is full
                breakout += 1;
                if (breakout > 20)
                {
                    richTextBox1.AppendText("No data found in 100ms, select again: " + theport.PortName);
                    closenullport();
                    return;
                }

            }
            int numberofbytes = theport.BytesToRead;
            string v = theport.ReadExisting();
            //ccommanddata = new List<string>();
            mcommandlist = v.Split('\n').ToList();//Plit the c command data on /r into a list
            //foreach(var mstring in mcommandlist)
            richTextBox1.AppendText(v);
        }
        //M button clicked
        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
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
        //Get the serial ports
        private void button1_Click(object sender, EventArgs e)
        {
            button4.Enabled = true;
            string[] serialitems = SerialPort.GetPortNames();
            foreach (var v in serialitems)
                listBox1.Items.Add(v);

        }
        //click the clear button to clear the richtext box
        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
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
        //*******************************Send list of general command************************************************
        private async void sendgeneralcommand(List<string> stringcommand)
        {
            richTextBox2.Clear();
            if (theport != null)
                theport.Open();
            else
            {
                richTextBox1.Clear();
                richTextBox1.AppendText("Error: Try again. The serial port is NULL, not initiated!");
                return;
            }
            foreach (string vstring in stringcommand)
            {
                if (findprompt())
                {
                    foreach (char v in vstring)
                    {
                        theport.Write(v.ToString());
                        //Thread.Sleep(wdf());//does nto seem that i need a delay
                        await Task.Delay(wdf());
                    }
                    //Thread.Sleep(1);
                    await Task.Delay(wdf());
                    //I want to use the interrupt functionality so set data at port to false
                }
                else
                {
                    richTextBox1.Clear();
                    richTextBox1.AppendText("error: Could not find the prompt!");
                }
            }
            theport.Close();
        }
        //*******************************Send C command**********************************************************
        private async void sendcommand(string stringcommand)
        {

            button4.Enabled = false;//Disable the button till Im done sending
            //string fake = "c F38F 1 1";
            stringcommand.Trim();// removes leading and trailing whitespace
            richTextBox1.Clear();
            if(theport != null)
                theport.Open();
            else
            {
                richTextBox1.Clear();
                richTextBox1.AppendText("Error: Try again. The serial port is NULL, not initiated!");
                return;
            }
            string vstring;
            if (theport.IsOpen)
            {
                if (findprompt())
                {
                    foreach (char v in stringcommand)
                    {
                        vstring = v.ToString();
                        theport.Write(vstring);
                        //Thread.Sleep(wdf());//does nto seem that i need a delay
                        await Task.Delay(wdf());
                    }
                    //Thread.Sleep(1);
                    await Task.Delay(wdf());
                    theport.Write("\r");//write this to enter the text
                    await Task.Delay(wdf());
                    //Thread.Sleep(1);//does not seem that i need this delay
                    theport.Write("s");//write this to enter the text
                    //Thread.Sleep(1);
                    await Task.Delay(wdf());
                    //    readccommand();//Sit and wait for the data
                    //I want to use the interrupt functionality so set data at port to false
                    dataatport = false;
                    monitorbooldata();
                    //readccommand();
                }
                else
                {
                    richTextBox1.Clear();
                    richTextBox1.AppendText("error: Could not find the prompt!");
                }
            }
            theport.Close();
            button4.Enabled = true;
        }
//****************************************** check if data at port true*******************************
        private void monitorbooldata()
        {
            int count = 0;
            while(!dataatport)
            {
                richTextBox1.AppendText(".");
                Task.Delay(20);
                //await Task.Delay(20);
                count += 1;
                if (count > 60)//If more than 90ms return with nothing
                {
                    richTextBox1.AppendText("!");
                    richTextBox1.AppendText("Waited to long");
                    count = 0;
                    return;
                }
            }
            //richTextBox1.AppendText(dataatport.ToString() + "!");
            readccommand();
        }
        //this is called from the interrupt
        private void datafromevent(string ccommandvalues)
        {
            ccommanddata = new List<string>();
            ccommanddata = ccommandvalues.Split('\r').ToList();//Plit the c command data on \r into a list

            if (ccommanddata.Count > 3)//The first 3 values are sent data. remove it
            {
                if (ccommanddata[0].Contains("?")) richTextBox1.AppendText("Found ? in data, make write delay bigger \r");
                ccommanddata.RemoveAt(0);//TODO: find fore intelligent way to remove this
                if (ccommanddata[0].Contains("?")) richTextBox1.AppendText("Found ? in data, make write delay bigger \r");
                ccommanddata.RemoveAt(0);//TODO: find fore intelligent way to remove this
                if (ccommanddata[0].Contains("?")) richTextBox1.AppendText("Found ? in data, make write delay bigger \r");
                ccommanddata.RemoveAt(0);//TODO: find fore intelligent way to remove this
            }
            else
            {
                foreach (string str in ccommanddata)
                    richTextBox1.AppendText(str);
                richTextBox1.AppendText("Does not seem like any data out of the port!!");
            }
            for (int i = 0; i < ccommanddata.Count; i++)
            {
                if (ccommanddata[i].Contains(">"))//Do want prompts in the data
                {
                    richTextBox1.Clear();
                    richTextBox1.AppendText("Error: Unwanted characters!");
                    ccommanddata.Clear();
                    return;
                }
                if (ccommanddata[i].Length < 4)//if the data is shorter tham length then it is invalid
                    ccommanddata.RemoveAt(i);
                else
                    ccommanddata[i] = ccommanddata[i].Substring(1);//remove the \n character
            }

            richTextBox1.AppendText("Elapsed time, Read port: " + readstw.ElapsedMilliseconds.ToString() + "\r");//Stop watch out put

            if (ccommanddata.Count > 250)
                plotcdata();
            else
                richTextBox1.AppendText("The capture command failed, less than 250 values");

            richTextBox1.AppendText("Elapsed time, Plot data: " + readstw.ElapsedMilliseconds.ToString() + "\r");//Stop watch out put
            readstw.Stop();
            readstw.Reset();
            //  textBox.AppendText(v);
            richTextBox1.AppendText("The amount of data capture was: " + ccommanddata.Count.ToString() + "\r");
            //theport.DataReceived
        }
        //Read data from the asic
        private void readccommand()
        {
            readstw.Start();//Stopwatch to measure read time
            int breakout = 0;
            string v = "";//This will be the total value from the port
            //check that the data count keeps on changing, else done
            //Thread.Sleep(50);
            while (theport.BytesToRead != 0)
            {
                v += theport.ReadExisting();
                Thread.Sleep(20);
                breakout += 1;

                if (breakout > 100)//a big number to stop working
                {
                    richTextBox1.AppendText("No data found in 100ms");
                }
            }
            int numberofbytes = theport.BytesToRead;

            ccommanddata = new List<string>();
            ccommanddata = v.Split('\r').ToList();//Plit the c command data on \r into a list

            if (ccommanddata.Count > 3)//The first 3 values are sent data. remove it
            {
                if (ccommanddata[0].Contains("?")) richTextBox1.AppendText("Found ? in data, make write delay bigger \r");
                ccommanddata.RemoveAt(0);//TODO: find fore intelligent way to remove this
                if (ccommanddata[0].Contains("?")) richTextBox1.AppendText("Found ? in data, make write delay bigger \r");
                ccommanddata.RemoveAt(0);//TODO: find fore intelligent way to remove this
                if (ccommanddata[0].Contains("?")) richTextBox1.AppendText("Found ? in data, make write delay bigger \r");
                ccommanddata.RemoveAt(0);//TODO: find fore intelligent way to remove this
            }
            else
            {
                foreach (string str in ccommanddata)
                    richTextBox1.AppendText(str);
                richTextBox1.AppendText("Does not seem like any data out of the port!!");
            }
            for (int i = 0; i < ccommanddata.Count; i++)
            {
                if (ccommanddata[i].Contains(">"))//Do want prompts in the data
                {
                    richTextBox1.Clear();
                    richTextBox1.AppendText("Error: Unwanted characters!");
                    ccommanddata.Clear();
                    return;
                }
                if (ccommanddata[i].Length < 4)//if the data is shorter tham length then it is invalid
                    ccommanddata.RemoveAt(i);
                else
                    ccommanddata[i] = ccommanddata[i].Substring(1);//remove the \n character
            }

            richTextBox1.AppendText("Elapsed time, Read port: " + readstw.ElapsedMilliseconds.ToString() + "\r");//Stop watch out put

            if (ccommanddata.Count > 250)
                plotcdata();
            else
                richTextBox1.AppendText("The capture command failed, less than 250 values");

            richTextBox1.AppendText("Elapsed time, Plot data: " + readstw.ElapsedMilliseconds.ToString() + "\r");//Stop watch out put
            readstw.Stop();
            readstw.Reset();
            //  textBox.AppendText(v);
            richTextBox1.AppendText("The amount of data capture was: " + ccommanddata.Count.ToString() + "\r");
            //theport.DataReceived
        }

        private void chartdefaults()
        {
            //reset defaults values that I want for the chart
            chart1.ChartAreas[0].BackColor = Color.White;
            chart1.Series.Clear();
            chart1.Titles.Clear();
            chart1.ChartAreas[0].AxisX.Minimum = Double.NaN;
            chart1.ChartAreas[0].AxisY.Minimum = Double.NaN;
            chart1.ChartAreas[0].AxisX.Maximum = Double.NaN;
            chart1.ChartAreas[0].AxisY.Maximum = Double.NaN;
            chart1.ChartAreas[0].AxisX.Title = "";
            chart1.ChartAreas[0].AxisY.Title = "";
            chart1.ChartAreas[0].AxisX.MinorGrid.Enabled = true;
            chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
            chart1.ChartAreas[0].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart1.ChartAreas[0].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;
        }

        private void plotcdata() 
        {
            //reset some chart defaults
            chartdefaults();

            Title title = new Title("Capture pot: " + textBox1.Text + " " + comboBox2.SelectedText.ToString() + " " + comboBox1.SelectedText.ToString(),
                Docking.Top, new Font("Verdana", 10, FontStyle.Regular), Color.Black);
            chart1.Titles.Add(title);
            title.IsDockedInsideChartArea = false;//do not want the tule in the chartarea
            title.DockedToChartArea = chart1.ChartAreas[0].Name;
            List<float> values = new List<float>();
            //Name the series
            string chartseries = "capture";
            //Add a series
            chart1.Series.Add(chartseries);

            chart1.Series[chartseries].ChartType = SeriesChartType.Point;
            chart1.Series[chartseries].MarkerStyle = MarkerStyle.Circle;
            chart1.Series[chartseries].MarkerSize = 7;
            chart1.Series[chartseries].Color = Color.Black;
            //used to split single data point out from the ref number
            String[] splitlist = new string[2];
            for(int i = 0; i < ccommanddata.Count; i++)
            {
                splitlist = ccommanddata[i].Split(' ');
                if(splitlist.Length > 1)
                {
                    if ((splitlist[0].Length > 0) && (splitlist[1].Length > 0))
                        chart1.Series[chartseries].Points.AddXY(Convert.ToInt32(splitlist[0], 16), Convert.ToInt32(splitlist[1], 16));
                }
                else
                    richTextBox1.AppendText("\r There was a plot designator without a value in plotcdata()!\r");
            }
        }
    }
}
