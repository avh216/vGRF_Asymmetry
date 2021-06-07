using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
    //initialising time and DAQ board
        System.Timers.Timer timer2; 
        MccDaq.MccBoard ourboard = new MccDaq.MccBoard(0);
        System.UInt16 dataval1, dataval2, dataval3, dataval4, dataval5, dataval6, dataval7, dataval8;
        float engunit1, engunit2, engunit3, engunit4, engunit5, engunit6, engunit7, engunit8;

        double offset1, offset2, offset3, offset4, offset5, offset6, offset7, offset8;

        double cop_z_front, bodyweight, Range, PBW, left_percent, right_percent;
        double a = 0.03; //dimensions for Treadmill model mercury

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        public List<double> GlobalMaxForceL = new List<double>();
        public List<double> GlobalMaxTimeL = new List<double>();
        public List<double> GlobalMaxForceR = new List<double>();
        public List<double> GlobalMaxTimeR = new List<double>();
        public List<double> left_load_percent = new List<double>();
        public List<double> right_load_percent = new List<double>();

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


        int counter = 0;


        public Form2(double body_weight, double range, double lc_offset1, double lc_offset2, double lc_offset3, double lc_offset4, double lc_offset5, double lc_offset6, double lc_offset7, double lc_offset8)
        {
            timer2 = new System.Timers.Timer(15);

            timer2.Elapsed += new System.Timers.ElapsedEventHandler(timer2_elapsed);

            timer2.AutoReset = true;
            timer2.Enabled = false;

            InitializeComponent();
            //Translating information sent from the Data Collection Form
            bodyweight = body_weight;
            Range = range;
            offset1 = lc_offset1;
            offset2 = lc_offset2;
            offset3 = lc_offset3;
            offset4 = lc_offset4;
            offset5 = lc_offset5;
            offset6 = lc_offset6;
            offset7 = lc_offset7;
            offset8 = lc_offset8;

            timer1.Start();
            timer2.Start();
            
        }
               
        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        void timer2_elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
            force1 = loadcell_cal1 * (engunit1 - offset1);
            force2 = loadcell_cal2 * (engunit2 - offset2);
            force3 = loadcell_cal3 * (engunit3 - offset3);
            force4 = loadcell_cal4 * (engunit4 - offset4);
            front_fp = force1 + force2 + force3 + force4;
            front_fp_list.Add(front_fp);

            // rear FP forces 
            force5 = loadcell_cal5 * (engunit5 - offset5);
            force6 = loadcell_cal6 * (engunit6 - offset6);
            force7 = loadcell_cal7 * (engunit7 - offset7);
            force8 = loadcell_cal8 * (engunit8 - offset8);
            rear_fp = force5 + force6 + force7 + force8;
            total_fp = front_fp + rear_fp;
            rear_fp_list.Add(rear_fp);
            total_fp_list.Add(total_fp);

            cop_z_front = -a * (force1 - force2 - force3 + force4) / front_fp; // calculating Z position of COP
            

            double maxforce = 0;
            double rearforce = 0;
            double frontforcemax = 0;
            double maxforce_time = 0;
            int index_max_force_time;

            if (front_fp > 0.3 * bodyweight) //start when enters peak region
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

                    PBW = maxforce / bodyweight;  //Calculation of %BW. 

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
                        left_percent = (PBW_L[PBW_L.Count - 1] / (PBW_L[PBW_L.Count - 1] + PBW_R[PBW_R.Count - 1]))*100; // calculating percentages
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

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (left_load_percent.Count() > 0)
            {
                chart1.Series["Series1"].Points.Clear();
                chart1.Series["Series1"].Points.AddXY("Left \n" + left_load_percent[left_load_percent.Count - 1].ToString("#.#"), GlobalMaxForceL[GlobalMaxForceL.Count - 1]);
                chart1.Series["Series1"].Points.AddXY("Right \n" + right_load_percent[right_load_percent.Count - 1].ToString("#.#"), GlobalMaxForceR[GlobalMaxForceR.Count - 1]);
            }
//Displaying visual feedback with the thumbs up/down depending on the range
            if (PBW_L.Count > 0 && PBW_R.Count > 0)
            {
                double PBW_Dif = Math.Abs(PBW_L[PBW_L.Count - 1] - PBW_R[PBW_R.Count - 1]);

                if (PBW_Dif >= Range)
                {
                    pictureBox1.Image = Properties.Resources.thumbdown_;

                }
                else
                {
                    pictureBox1.Image = Properties.Resources.thumbup_;
                }
            }

        }
    }
}
