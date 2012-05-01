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
using System.ComponentModel;
using System.Windows.Forms;

namespace Ares.SharperGpsDemo_WinForms
{
    public partial class FrmGpsSettings : Form
    {
        readonly string[] _ports;
        public FrmGpsSettings()
        {
            InitializeComponent();
            _ports = System.IO.Ports.SerialPort.GetPortNames();
            cmbPorts.DataSource = _ports;
            LoadFromRegistry();
        }
        private void LoadFromRegistry()
        {
            const string port = "COM4";
            const string rate = "4800";

            for (int i = 0; i < _ports.Length; i++)
            {
                if(port==_ports[i]) cmbPorts.SelectedIndex = i;
            }

            int baudrate;
            tbBaudRate.Text = int.TryParse(rate, out baudrate) ? baudrate.ToString() : "4800";
        }
        public string SerialPort
        {
            get { return cmbPorts.SelectedValue.ToString(); }
        }
        public int BaudRate
        {
            get { return int.Parse(tbBaudRate.Text); }
        }

        public void DisableConfig()
        {
            EnableDisable(false);
        }
        public void EnableConfig()
        {
            EnableDisable(true);
        }
        private void EnableDisable(bool enable)
        {
            cmbPorts.Enabled = enable;
            tbBaudRate.Enabled = enable;
        }
		
        protected override void OnClosing(CancelEventArgs e)
        {
            //Prevent disposal of dialog
            e.Cancel = true;
            base.OnClosing(e);
            Hide();
        }		
    }
}