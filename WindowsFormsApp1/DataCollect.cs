using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {
        System.Timers.Timer timer;

        MccDaq.MccBoard ourboard = new MccDaq.MccBoard(0); //reading the data acquistion board

        System.UInt16 dataval1, dataval2, dataval3, dataval4, dataval5, dataval6, dataval7, dataval8; //places for the raw loadcell values

        float engunit1, engunit2, engunit3, engunit4, engunit5, engunit6, engunit7, engunit8;
        float lc_offset1, lc_offset2, lc_offset3, lc_offset4, lc_offset5, lc_offset6, lc_offset7, lc_offset8;


        public double body_weight = 0;
        double cop_z_front, cop_z_rear;
        double a = 0.03; //dimensions for Treadmill model mercury

        public List<double> GlobalMaxForceL = new List<double>();

        private void chart2_Click(object sender, EventArgs e)
        {
           
        }

        public List<double> GlobalMaxTimeL = new List<double>();
        public List<double> GlobalMaxForceR = new List<double>();
        public List<double> GlobalMaxTimeR = new List<double>();
        public List<double> left_load_percent = new List<double>();
        public List<double> right_load_percent = new List<double>();

        int sampling_rate = 250;
        int counter = 0;
        int Trial = 1;
        int frequency = 10;
        double range = 0.05;
        double left_percent = 0;
        double right_percent = 0;
        double PBW;

        string Alimb, SubNo;

        List<double> front_fp_max_force_list = new List<double>();
        List<double> front_fp_time_max_force_list = new List<double>();
        List<double> rear_fp_max_force_list = new List<double>();
        List<double> rear_fp_time_max_force_list = new List<double>();
        List<double> cop_z_arr = new List<double>();
        List<double> front_fp_list = new List<double>();
        List<double> rear_fp_list = new List<double>();
        List<double> total_fp_list = new List<double>();
        List<double> PBW_L = new List<double>();
        List<double> PBW_R = new List<double>();

        private void button4_Click(object sender, EventArgs e) //Loading subject-specific bodyweight
        {
            var body_weight_input = textBox1.Text;
            double mass;
            if (double.TryParse(body_weight_input, out mass))
            {
                body_weight = mass * 9.81;
                textBox1.Clear();
            }
            else
            {
                MessageBox.Show("ERROR: Invalid Value");
            }
        }

        private void button7_Click(object sender, EventArgs e) //Adding Subject Number (or Trial name) to output
        {
            SubNo = textBox3.Text;
            textBox3.Clear();
        }

        private void button8_Click(object sender, EventArgs e) //Recording the amputated limb
        {
            Alimb = textBox2.Text;
            textBox2.Clear();
        }
        private void button6_Click(object sender, EventArgs e) // Manually adjust positive range frome pre-set 0.05
        {
            var range_input = textBox4.Text;
            if (double.TryParse(range_input, out range))
            {
                if (range >= 1 || range < 0)
                {
                    MessageBox.Show("ERROR: Please enter a value between 0 and 1");
                }
                else
                {
                    textBox4.Clear();
                }
                
            }
            else
            {
                MessageBox.Show("ERROR: Please enter a valid number");
            }
        }

        public Form1()
        {
            timer = new System.Timers.Timer(frequency); //Initalising System timer so data collection occurs on each new tick           
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_elapsed);
          
            timer.AutoReset = true;
            timer.Enabled = false;
         
            InitializeComponent();
            ourboard.DBitOut(MccDaq.DigitalPortType.AuxPort0, 0, MccDaq.DigitalLogicState.High); // Turning on the front force plate
            ourboard.DBitOut(MccDaq.DigitalPortType.AuxPort0, 1, MccDaq.DigitalLogicState.High); // Turning on the rear force plate
        }

        private void button1_Click(object sender, EventArgs e) //calculating any offset on the treadmill
        {
            float[] lc_offset_arr1 = new float[sampling_rate];
            float[] lc_offset_arr2 = new float[sampling_rate];
            float[] lc_offset_arr3 = new float[sampling_rate];
            float[] lc_offset_arr4 = new float[sampling_rate];

            float[] lc_offset_arr5 = new float[sampling_rate];
            float[] lc_offset_arr6 = new float[sampling_rate];
            float[] lc_offset_arr7 = new float[sampling_rate];
            float[] lc_offset_arr8 = new float[sampling_rate];

            for (int ii = 0; ii < sampling_rate; ii++)  // collecting 250 samples of data from each load cell
            {
                ourboard.AIn(0, MccDaq.Range.Bip10Volts, out dataval1);
                ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval1, out lc_offset_arr1[ii]);

                ourboard.AIn(1, MccDaq.Range.Bip10Volts, out dataval2);
                ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval2, out lc_offset_arr2[ii]);

                ourboard.AIn(2, MccDaq.Range.Bip10Volts, out dataval3);
                ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval3, out lc_offset_arr3[ii]);

                ourboard.AIn(3, MccDaq.Range.Bip10Volts, out dataval4);
                ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval4, out lc_offset_arr4[ii]);


                ourboard.AIn(4, MccDaq.Range.Bip10Volts, out dataval5);
                ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval5, out lc_offset_arr5[ii]);

                ourboard.AIn(5, MccDaq.Range.Bip10Volts, out dataval6);
                ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval6, out lc_offset_arr6[ii]);

                ourboard.AIn(6, MccDaq.Range.Bip10Volts, out dataval7);
                ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval7, out lc_offset_arr7[ii]);

                ourboard.AIn(7, MccDaq.Range.Bip10Volts, out dataval8);
                ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval8, out lc_offset_arr8[ii]);
            }

            lc_offset1 = lc_offset_arr1.Average(); //Averagint eh offset collected
            lc_offset2 = lc_offset_arr2.Average();
            lc_offset3 = lc_offset_arr3.Average();
            lc_offset4 = lc_offset_arr4.Average();

            lc_offset5 = lc_offset_arr5.Average();
            lc_offset6 = lc_offset_arr6.Average();
            lc_offset7 = lc_offset_arr7.Average();
            lc_offset8 = lc_offset_arr8.Average();

            MessageBox.Show("Load cell 1 offset = " + lc_offset1.ToString() + //displaying the offset so any large variations can be spotted.
              "\nLoad cell 2 offset = " + lc_offset2.ToString() +
              "\nLoad cell 3 offset = " + lc_offset3.ToString() +
              "\nLoad cell 4 offset = " + lc_offset4.ToString() +
              "\nLoad cell 5 offset = " + lc_offset5.ToString() +
              "\nLoad cell 6 offset = " + lc_offset6.ToString() +
              "\nLoad cell 7 offset = " + lc_offset7.ToString() +
              "\nLoad cell 8 offset = " + lc_offset8.ToString());
            
        }

        //Start button 
        //Displays errors if required inputs have not been entered.
        private void button2_Click(object sender, EventArgs e)
        {
            if (body_weight == 0)
            {
                MessageBox.Show("ERROR: Enter mass");
            }

            else if (string.IsNullOrEmpty(SubNo))
            {
                MessageBox.Show("ERROR: Enter subject number");
            }

            else if (string.IsNullOrEmpty(Alimb))
            {
                MessageBox.Show("ERROR: Identify amputated limb");
            }
            else
            {
                GlobalMaxForceL.Clear();
                GlobalMaxForceR.Clear();
                GlobalMaxTimeL.Clear();
                GlobalMaxTimeR.Clear();
                left_load_percent.Clear();
                right_load_percent.Clear();
                front_fp_list.Clear();
                rear_fp_list.Clear();
                total_fp_list.Clear();
                PBW_L.Clear();
                PBW_R.Clear();

                counter = 0;
               
                Form2 f2 = new Form2(body_weight, range, lc_offset1, lc_offset2, lc_offset3, lc_offset4, lc_offset5, lc_offset6, lc_offset7, lc_offset8);  //sends information to Form 2 for the participant display
                f2.Show();

               chart2.Series["FrontFP"].Points.Clear();
               chart2.Series["RearFP"].Points.Clear();
               chart2.Series["TotalFP"].Points.Clear();

                timer.Enabled = true;
                timer1.Start();

            }



        }

        //Collects values
        void timer_elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            counter++; 


            double front_fp, rear_fp, total_fp;
            //calibration constants of the loadcells
            double loadcell_cal1 = 208.5485;
            double loadcell_cal2 = 213.5624;
            double loadcell_cal3 = 203.5699;
            double loadcell_cal4 = 211.9208;
            double loadcell_cal5 = 206.3199;
            double loadcell_cal6 = 210.4023;
            double loadcell_cal7 = 210.1317;
            double loadcell_cal8 = 212.0018;
            double force5, force6, force7, force8;


            double force1, force2, force3, force4;

            // collecting values from the loadcells
            //front force plate
            ourboard.AIn(0, MccDaq.Range.Bip10Volts, out dataval1);
            ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval1, out engunit1);

            ourboard.AIn(1, MccDaq.Range.Bip10Volts, out dataval2);
            ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval2, out engunit2);

            ourboard.AIn(2, MccDaq.Range.Bip10Volts, out dataval3);
            ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval3, out engunit3);

            ourboard.AIn(3, MccDaq.Range.Bip10Volts, out dataval4);
            ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval4, out engunit4);

            // back force plate
            ourboard.AIn(4, MccDaq.Range.Bip10Volts, out dataval5);
            ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval5, out engunit5);

            ourboard.AIn(5, MccDaq.Range.Bip10Volts, out dataval6);
            ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval6, out engunit6);

            ourboard.AIn(6, MccDaq.Range.Bip10Volts, out dataval7);
            ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval7, out engunit7);

            ourboard.AIn(7, MccDaq.Range.Bip10Volts, out dataval8);
            ourboard.ToEngUnits(MccDaq.Range.Bip10Volts, dataval8, out engunit8);

            // calibrating front force plates and removing offset
            force1 = loadcell_cal1 * (engunit1 - lc_offset1);
            force2 = loadcell_cal2 * (engunit2 - lc_offset2);
            force3 = loadcell_cal3 * (engunit3 - lc_offset3);
            force4 = loadcell_cal4 * (engunit4 - lc_offset4);
            front_fp = force1 + force2 + force3 + force4;
            front_fp_list.Add(front_fp);

            // rear FP forces 
            force5 = loadcell_cal5 * (engunit5 - lc_offset5);
            force6 = loadcell_cal6 * (engunit6 - lc_offset6);
            force7 = loadcell_cal7 * (engunit7 - lc_offset7);
            force8 = loadcell_cal8 * (engunit8 - lc_offset8);
            rear_fp = force5 + force6 + force7 + force8;
            total_fp = front_fp + rear_fp;
            rear_fp_list.Add(rear_fp);
            total_fp_list.Add(total_fp);

            cop_z_front = -a * (force1 - force2 - force3 + force4) / front_fp; // calculating Z position of COP
            cop_z_rear = -a * (force5 - force6 - force7 + force8) / rear_fp;

            double maxforce = 0;
            double rearforce = 0;
            double frontforcemax = 0;
            double maxforce_time = 0;
            int index_max_force_time;

            if (front_fp > 0.3 * body_weight) //start when enters peak region
            {
                front_fp_max_force_list.Add(front_fp); //adds all values of force to list
                front_fp_time_max_force_list.Add(counter); //adds associated time when they occur
                cop_z_arr.Add(cop_z_front); //adds COP of when they occur
                rear_fp_max_force_list.Add(rear_fp);
            }



            else //as soon as it leaves the max value area it heads to this point
            {
                if (front_fp_max_force_list.Count > 0)
                {
                    frontforcemax = front_fp_max_force_list.Max(); //placeholder for the max force on the list from recent peak

                    index_max_force_time = front_fp_max_force_list.IndexOf(frontforcemax);
                    rearforce = rear_fp_max_force_list[index_max_force_time];
                    maxforce_time = front_fp_time_max_force_list[index_max_force_time]; //finds counter of associated peak
                    maxforce = rearforce + frontforcemax;

                    PBW = maxforce / body_weight;

                    cop_z_arr.Average(); //calculate average COP

                    if (cop_z_arr.Average() < 0) //Left Foot
                    {
                        GlobalMaxForceL.Add(maxforce); //adds the max peak value to a list
                        GlobalMaxTimeL.Add(maxforce_time); //adds time peak value occured to a list
                        PBW_L.Add(PBW);
                    }

                    else //Right Foot
                    {
                        GlobalMaxForceR.Add(maxforce);
                        GlobalMaxTimeR.Add(maxforce_time);
                        PBW_R.Add(PBW);
                    }

                    if (GlobalMaxForceR.Count() > 0 && GlobalMaxForceL.Count() > 0)
                    {
                        left_percent = (PBW_L[PBW_L.Count - 1] / (PBW_L[PBW_L.Count - 1] + PBW_R[PBW_R.Count - 1]))*100; //Calculating Percentages
                        right_percent = 100 - left_percent;
                        left_load_percent.Add(left_percent);
                        right_load_percent.Add(right_percent);
                      
                    }

                    front_fp_max_force_list.Clear();
                    front_fp_time_max_force_list.Clear();
                    rear_fp_max_force_list.Clear();

                    cop_z_arr.Clear();
                }
            }

            }

        //displays on pie charts for the user and a line graph to see the moving force 
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (front_fp_list.Count() > 0)
            {
                chart2.Series["FrontFP"].Points.AddXY(counter, front_fp_list[front_fp_list.Count - 1]);
                chart2.Series["RearFP"].Points.AddXY(counter, rear_fp_list[rear_fp_list.Count - 1]);
                chart2.Series["TotalFP"].Points.AddXY(counter, total_fp_list[total_fp_list.Count - 1]);
            }
            if (left_load_percent.Count() > 0)
            {
                chart1.Series["Series1"].Points.Clear();
                chart1.Series["Series1"].Points.AddXY("Left \n" + left_load_percent[left_load_percent.Count - 1].ToString("#.#"), GlobalMaxForceL[GlobalMaxForceL.Count - 1]);
                chart1.Series["Series1"].Points.AddXY("Right \n" + right_load_percent[right_load_percent.Count - 1].ToString("#.#"), GlobalMaxForceR[GlobalMaxForceR.Count - 1]);
            }

            if (PBW_L.Count > 0 && PBW_R.Count > 0) //displaying thumbs up/down depending on the ratio of the last two steps
            {
                double PBW_Dif = Math.Abs(PBW_L[PBW_L.Count - 1] - PBW_R[PBW_R.Count - 1]);

                if (PBW_Dif >= range)
                {
                    pictureBox1.Image = Properties.Resources.thumbdown_;

                }
                else
                {
                    pictureBox1.Image = Properties.Resources.thumbup_;
                }
            }

        }



        private void button3_Click(object sender, EventArgs e)
        { //Stops data collection
            timer.Enabled = false;
            timer1.Stop();
           // Displays the average percent through the duration of the data collection
            MessageBox.Show("Left GRF %: " + left_load_percent.Average().ToString("#.##") + 
                            "\nRight GRF %: " + right_load_percent.Average().ToString("#.##") +
                            "\n Left Force/Bodyweight: " +PBW_L.Average().ToString("#.##") +
                            "\n Right Force/Bodyweight: " + PBW_R.Average().ToString("#.##"));
        }

        
        private void button5_Click(object sender, EventArgs e)
        {
             //Outputs collected data in a tab seperate csv.
             int i,LC,RC;
             string filename;
             System.Data.DataTable data = new System.Data.DataTable();

             data.Columns.Add("Left Max Force Time", typeof(double));
             data.Columns.Add("Left Max Force", typeof(double));
             data.Columns.Add("Left Percent", typeof(double));
             data.Columns.Add("Left Percentage Bodyweight", typeof(double));
             data.Columns.Add("Right Max Force Time", typeof(double));
             data.Columns.Add("Right Max Force", typeof(double));
             data.Columns.Add("Right Percent", typeof(double));
            data.Columns.Add("Right Percentage Bodyweight", typeof(double));
            data.Columns.Add("Front FP", typeof(double));
             data.Columns.Add("Rear FP", typeof(double));
             data.Columns.Add("Total FP", typeof(double));
             DataRow rowff;
             LC = GlobalMaxTimeL.Count;
             RC = GlobalMaxTimeR.Count;

            for (i = LC; i < front_fp_list.Count; i++)
            {
                GlobalMaxForceL.Add(0);
                GlobalMaxTimeL.Add(0);
                left_load_percent.Add(0);
                PBW_L.Add(0);
            }
            for (i = RC; i < front_fp_list.Count; i++)
            {
                GlobalMaxForceR.Add(0);
                GlobalMaxTimeR.Add(0);
                right_load_percent.Add(0);
                PBW_R.Add(0);
            }

            for (i = 0; i < front_fp_list.Count; i++)
            {
                rowff = data.NewRow();
                rowff["Front FP"] = front_fp_list[i];
                rowff["Rear FP"] = rear_fp_list[i];
                rowff["Total FP"] = total_fp_list[i];
                rowff["Left Max Force Time"] = GlobalMaxTimeL[i];
                rowff["Left Max Force"] = GlobalMaxForceL[i];
                rowff["Left Percentage Bodyweight"] = PBW_L[i];
                rowff["Left Percent"] = left_load_percent[i];
                rowff["Right Max Force Time"] = GlobalMaxTimeR[i];
                rowff["Right Max Force"] = GlobalMaxForceR[i];
                rowff["Right Percent"] = right_load_percent[i];
                rowff["Right Percentage Bodyweight"] = PBW_R[i];
                data.Rows.Add(rowff);
            }
             
            
             StringBuilder sb = new StringBuilder();
             string[] columnnames = data.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
             sb.AppendLine(string.Join("\t", columnnames));

             // Fatch rows from datatable and append values as tab seperated to the object of StringBuilder class 
             foreach (DataRow row in data.Rows)
             {
                 IEnumerable<string> fields = row.ItemArray.Select(field => string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                 sb.AppendLine(string.Join("\t", fields));
             }

            filename = "Data_" +SubNo + "_" + Alimb +"_"+ Trial.ToString() + ".txt";
            MessageBox.Show(filename);


             // save the file
             File.WriteAllText(@"E:\Rehab_Minty\" + filename, sb.ToString());
             Trial = Trial + 1; //Each additional data collection run on same session is saved with an incremental number increase.

        }

           
    }
}
