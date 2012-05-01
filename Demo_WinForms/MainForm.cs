/* Copyright 2007 - Morten Nielsen
 * Copyright (C) 2009  John Schmitt, Mike McBride, and Kevin Curtis
 * 
 * This file is part of SharperGps.
 * SharperGps is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * SharperGps is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with SharperGps; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * Change log
 * 
 * Date    | Author | Reason for change                | Description of change
 * -------- -------- ---------------------------------- ------------------------------------------------------
 * 12/9/09  jschmitt initial development                Fully implemented every method
 */
using System;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Ares.SharperGps;
using Ares.SharperGps.NMEA;

namespace Ares.SharperGpsDemo_WinForms
{
    public partial class MainForm : Form
    {

        public static FrmGpsSettings FrmGpsSettings;

        public MainForm()
        {
            InitializeComponent();

            Gps = new GpsHandler { TimeOut = 5 }; //Initialize GPS handler
            Gps.NewGpsFix += GpsEventHandler; //Hook up GPS data events to a handler
            FrmGpsSettings = new FrmGpsSettings();
        }
        public static GpsHandler Gps;

        private void menuItemGPS_Start_Click(object sender, EventArgs e)
        {


            if (!Gps.IsPortOpen)
            {
                try
                {
                    Gps.Start(FrmGpsSettings.SerialPort, FrmGpsSettings.BaudRate); //Open serial port
                    menuItemGPS_Start.Text = "Stop";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured when trying to open port: " + ex.Message);
                }
            }
            else
            {
                Gps.Stop(); //Close serial port
                menuItemGPS_Start.Text = "Start";
            }
        }

        /// <summary>
        /// Responds to sentence events from GPS receiver
        /// </summary>
        private void GpsEventHandler(object sender, GpsHandler.GpsEventArgs e)
        {

            if (InvokeRequired)
            {
                // Execute the same method, but this time on the GUI thread
                BeginInvoke(new ThreadStart(() => GpsEventHandler(sender, e)));
                return;
            }


            tbRawLog.Text += e.Sentence + "\r\n";
            if (tbRawLog.Text.Length > 20 * 1024 * 1024) //20Kb maximum - prevents crash
            {
                tbRawLog.Text = tbRawLog.Text.Substring(10 * 1024 * 1024);
            }
            tbRawLog.ScrollToCaret(); //Scroll to bottom

            switch (e.TypeOfEvent)
            {
                case GpsEventType.GPRMC:  //Recommended minimum specific GPS/Transit data
                    if (Gps.HasGpsFix) //Is a GPS fix available?
                    {
                        //lbRMCPosition.Text = GPS.GPRMC.Position.ToString("#.000000");
                        lbRMCPosition.Text = Gps.GPRMC.Position.ToString("DMS");
                        lbRMCCourse.Text = Gps.GPRMC.Course.ToString();
                        lbRMCSpeed.Text = Gps.GPRMC.Speed + " mph";
                        lbRMCTimeOfFix.Text = Gps.GPRMC.TimeOfFix.ToString("F");
                        lbRMCMagneticVariation.Text = Gps.GPRMC.MagneticVariation.ToString();
                    }
                    else
                    {
                        statusBar1.Text = "No fix";
                        lbRMCCourse.Text = "N/A";
                        lbRMCSpeed.Text = "N/A";
                        lbRMCTimeOfFix.Text = Gps.GPRMC.TimeOfFix.ToString();
                    }
                    break;
                case GpsEventType.GPGGA: //Global Positioning System Fix Data
                    if (Gps.GPGGA.Position != null)
                        lbGGAPosition.Text = Gps.GPGGA.Position.ToString("DM");
                    else
                        lbGGAPosition.Text = "";
                    lbGGATimeOfFix.Text = Gps.GPGGA.TimeOfFix.Hour + ":" + Gps.GPGGA.TimeOfFix.Minute + ":" + Gps.GPGGA.TimeOfFix.Second;
                    lbGGAFixQuality.Text = Gps.GPGGA.FixQuality.ToString();
                    lbGGANoOfSats.Text = Gps.GPGGA.NoOfSats.ToString();
                    lbGGAAltitude.Text = Gps.GPGGA.Altitude + " " + Gps.GPGGA.AltitudeUnits;
                    lbGGAHDOP.Text = Gps.GPGGA.Dilution.ToString();
                    lbGGAGeoidHeight.Text = Gps.GPGGA.HeightOfGeoid.ToString();
                    lbGGADGPSupdate.Text = Gps.GPGGA.DGPSUpdate.ToString();
                    lbGGADGPSID.Text = Gps.GPGGA.DGPSStationID;
                    break;
                case GpsEventType.GPGLL: //Geographic position, Latitude and Longitude
                    lbGLLPosition.Text = Gps.GPGLL.Position.ToString();
                    lbGLLTimeOfSolution.Text = (Gps.GPGLL.TimeOfSolution.HasValue ? Gps.GPGLL.TimeOfSolution.Value.Hours + ":" + Gps.GPGLL.TimeOfSolution.Value.Minutes.ToString() + ":" + Gps.GPGLL.TimeOfSolution.Value.Seconds.ToString() : "");
                    lbGLLDataValid.Text = Gps.GPGLL.DataValid.ToString();
                    break;
                case GpsEventType.GPGSA: //GPS DOP and active satellites
                    if (Gps.GPGSA.Mode == 'A')
                        lbGSAMode.Text = "Auto";
                    else if (Gps.GPGSA.Mode == 'M')
                        lbGSAMode.Text = "Manual";
                    else lbGSAMode.Text = "";
                    lbGSAFixMode.Text = Gps.GPGSA.FixMode.ToString();
                    lbGSAPRNs.Text = "";
                    if (Gps.GPGSA.PRNInSolution.Count > 0)
                        foreach (string prn in Gps.GPGSA.PRNInSolution)
                            lbGSAPRNs.Text += prn + " ";
                    else
                        lbGSAPRNs.Text += "none";
                    lbGSAPDOP.Text = Gps.GPGSA.PDOP + " (" + DOPtoWord(Gps.GPGSA.PDOP) + ")";
                    lbGSAHDOP.Text = Gps.GPGSA.HDOP + " (" + DOPtoWord(Gps.GPGSA.HDOP) + ")";
                    lbGSAVDOP.Text = Gps.GPGSA.VDOP + " (" + DOPtoWord(Gps.GPGSA.VDOP) + ")";
                    break;
                case GpsEventType.GPGSV: //Satellites in view
                    if (NMEAtabs.TabPages[NMEAtabs.SelectedIndex].Text == "GPGSV") //Only update this tab when it is active
                        DrawGSV();
                    break;
                case GpsEventType.PGRME: //Garmin proprietary sentences.
                    lbRMEHorError.Text = Gps.PGRME.EstHorisontalError.ToString();
                    lbRMEVerError.Text = Gps.PGRME.EstVerticalError.ToString();
                    lbRMESphericalError.Text = Gps.PGRME.EstSphericalError.ToString();
                    break;
                case GpsEventType.TimeOut: //Serialport timeout.
                    statusBar1.Text = "Serialport timeout";
                    break;
            }
        }
        private string DOPtoWord(double dop)
        {
            if (dop < 1.5) return "Ideal";
            if (dop < 3) return "Excellent";
            if (dop < 6) return "Good";
            if (dop < 8) return "Moderate";
            return dop < 20 ? "Fair" : "Poor";
        }
        private void DrawGSV()
        {
            Color[] colors = { Color.Blue , Color.Red , Color.Green, Color.Yellow, Color.Cyan, Color.Orange,
                               Color.Gold , Color.Violet, Color.YellowGreen, Color.Brown, Color.GreenYellow,
                               Color.Blue , Color.Red , Color.Green, Color.Yellow, Color.Aqua, Color.Orange};
            //Generate signal level readout
            int satCount = Gps.GPGSV.SatsInView;
            Bitmap imgSignals = new Bitmap(picGSVSignals.Width, picGSVSignals.Height);
            Graphics g = Graphics.FromImage(imgSignals);
            g.Clear(Color.White);
            Pen penBlack = new Pen(Color.Black, 1);
            Pen penBlackDashed = new Pen(Color.Black, 1);
            penBlackDashed.DashPattern = new[] { 2f, 2f };
            Pen penGray = new Pen(Color.LightGray, 1);
            const int iMargin = 4;
            const int iPadding = 4;
            g.DrawRectangle(penBlack, 0, 0, imgSignals.Width - 1, imgSignals.Height - 1);

            StringFormat sFormat = new StringFormat();
            int barWidth = 1;
            if (satCount > 0)
                barWidth = (imgSignals.Width - 2 * iMargin - iPadding * (satCount - 1)) / satCount;

            //Draw horisontal lines
            for (int i = imgSignals.Height - 15; i > iMargin; i -= (imgSignals.Height - 15 - iMargin) / 5)
                g.DrawLine(penGray, 1, i, imgSignals.Width - 2, i);
            sFormat.Alignment = StringAlignment.Center;
            //Draw satellites
            for (int i = 0; i < Gps.GPGSV.Satellites.Count; i++)
            {
                GPGSV.Satellite sat = Gps.GPGSV.Satellites[i];
                int startx = i * (barWidth + iPadding) + iMargin;
                int starty = imgSignals.Height - 15;
                int height = (imgSignals.Height - 15 - iMargin) / 50 * sat.SNR;
                if (Gps.GPGSA.PRNInSolution.Contains(sat.PRN))
                {
                    g.FillRectangle(new SolidBrush(colors[i]), startx, starty - height + 1, barWidth, height);
                    g.DrawRectangle(penBlack, startx, starty - height, barWidth, height);
                }
                else
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(50, colors[i])), startx, starty - height + 1, barWidth, height);
                    g.DrawRectangle(penBlackDashed, startx, starty - height, barWidth, height);
                }

                sFormat.LineAlignment = StringAlignment.Near;
                g.DrawString(sat.PRN, new Font("Verdana", 9, FontStyle.Regular), new SolidBrush(Color.Black), startx + barWidth / 2, imgSignals.Height - 15, sFormat);
                sFormat.LineAlignment = StringAlignment.Far;
                g.DrawString(sat.SNR.ToString(), new Font("Verdana", 9, FontStyle.Regular), new SolidBrush(Color.Black), startx + barWidth / 2, starty - height, sFormat);
            }
            picGSVSignals.Image = imgSignals;

            //Generate sky view
            Bitmap imgSkyview = new Bitmap(picGSVSkyview.Width, picGSVSkyview.Height);
            g = Graphics.FromImage(imgSkyview);
            g.Clear(Color.Transparent);
            g.FillEllipse(Brushes.White, 0, 0, imgSkyview.Width - 1, imgSkyview.Height - 1);
            g.DrawEllipse(penGray, 0, 0, imgSkyview.Width - 1, imgSkyview.Height - 1);
            g.DrawEllipse(penGray, imgSkyview.Width / 4, imgSkyview.Height / 4, imgSkyview.Width / 2, imgSkyview.Height / 2);
            g.DrawLine(penGray, imgSkyview.Width / 2, 0, imgSkyview.Width / 2, imgSkyview.Height);
            g.DrawLine(penGray, 0, imgSkyview.Height / 2, imgSkyview.Width, imgSkyview.Height / 2);
            sFormat.LineAlignment = StringAlignment.Near;
            sFormat.Alignment = StringAlignment.Near;
            const float radius = 6f;
            for (int i = 0; i < Gps.GPGSV.Satellites.Count; i++)
            {
                GPGSV.Satellite sat = Gps.GPGSV.Satellites[i];
                double ang = 90.0 - sat.Azimuth;
                ang = ang / 180.0 * Math.PI;
                int x = imgSkyview.Width / 2 + (int)Math.Round((Math.Cos(ang) * ((90.0 - sat.Elevation) / 90.0) * (imgSkyview.Width / 2.0 - iMargin)));
                int y = imgSkyview.Height / 2 - (int)Math.Round((Math.Sin(ang) * ((90.0 - sat.Elevation) / 90.0) * (imgSkyview.Height / 2.0 - iMargin)));
                g.FillEllipse(new SolidBrush(colors[i]), x - radius * 0.5f, y - radius * 0.5f, radius, radius);

                if (Gps.GPGSA.PRNInSolution.Contains(sat.PRN))
                {
                    g.DrawEllipse(penBlack, x - radius * 0.5f, y - radius * 0.5f, radius, radius);
                    g.DrawString(sat.PRN, new Font("Verdana", 9, FontStyle.Bold), new SolidBrush(Color.Black), x, y, sFormat);
                }
                else
                    g.DrawString(sat.PRN, new Font("Verdana", 8, FontStyle.Italic), new SolidBrush(Color.Gray), x, y, sFormat);
            }
            picGSVSkyview.Image = imgSkyview;
        }

        private void menuItem_File_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuItemGPS_Settings_Click(object sender, EventArgs e)
        {
            if (Gps.IsPortOpen) FrmGpsSettings.DisableConfig();
            else FrmGpsSettings.EnableConfig();

            FrmGpsSettings.Show();
        }

        private void NMEAtabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (NMEAtabs.TabPages[NMEAtabs.SelectedIndex].Text == "GPGSV")
                DrawGSV();
        }

        private void btnNTRIPGetSourceTable_Click(object sender, EventArgs e)
        {
            SharperGps.NTRIP.NTRIPClient ntrip = new SharperGps.NTRIP.NTRIPClient(new IPEndPoint(IPAddress.Parse(tbNTRIPServerIP.Text.Trim()), int.Parse(tbNTRIPPort.Text)));
            // http://igs.ifag.de/root_ftp/misc/ntrip/streamlist_euref-ip.htm

            SharperGps.NTRIP.SourceTable table = ntrip.GetSourceTable();
            if (table != null)
            {
                dgNTRIPCasters.DataSource = table.Casters;
                dgNTRIPNetworks.DataSource = table.Networks;
                if (table.DataStreams.Count > 0)
                    ntrip.StartNTRIP("FFMJ2");

                else
                    MessageBox.Show("Sourcetable doesn't contain any datastreams");
            }
            else
                MessageBox.Show("Failed to request or parse the DataSource Table");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Gps.Dispose();  //Closes serial port and cleans up. This is important !
        }
    }
}