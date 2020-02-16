using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MissionPlanner.ArduPilot;
using MissionPlanner.Utilities;
using MissionPlanner.Controls;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using MissionPlanner;
using SharpDX.DirectInput;
using System.Reflection;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using GMap.NET;
using GMap.NET.WindowsForms;


namespace RACPluginPayloadControl
{
    public class Plugin : MissionPlanner.Plugin.Plugin
    {

        static DirectInput directInput = new DirectInput();
        SharpDX.DirectInput.Joystick joystick;

        JoystickState last_state;
        JoystickState current_state;

        SplitContainer sc;
        TableLayoutPanel tblMap;
        Label lab;

        MissionPlanner.Controls.MyButton ahd_q;
        MissionPlanner.Controls.MyButton ahd_ch1;
        MissionPlanner.Controls.MyButton ahd_ch2;
        MissionPlanner.Controls.MyButton ahd_ch3;
        MissionPlanner.Controls.MyButton ahd_ch4;

        int yaw;
        int pitch;
        int zoom;
        int ahd_switch;
        int mode_click = 0;
        int focus_dir = 0;

        int old_yaw = -1;
        int old_pitch = -1;
        int old_zoom = -1;

        long last_gimbal_command;


        Button b;
        int a = 0;
        int bs = 0;

        public override string Name
        {
            get { return "RACPluginPayloadControl"; }
        }

        public override string Version
        {
            get { return "0.1"; }
        }

        public override string Author
        {
            get { return "Andras Schaffer"; }
        }

        //[DebuggerHidden]
        public override bool Init()
        {
            loopratehz = 5;


         

            MainV2.instance.Invoke((Action)
     delegate
     {


         sc = Host.MainForm.FlightData.Controls.Find("splitContainer1", true).FirstOrDefault() as SplitContainer;
         //TrackBar tb = Host.MainForm.FlightData.Controls.Find("TRK_zoom", true).FirstOrDefault() as TrackBar;
         //Panel pn1 = Host.MainForm.FlightData.Controls.Find("panel1", true).FirstOrDefault() as Panel;
         //tblMap = Host.MainForm.FlightData.Controls.Find("tableMap", true).FirstOrDefault() as TableLayoutPanel;
         //SplitContainer SubMainLeft = Host.MainForm.FlightData.Controls.Find("SubMainLeft", true).FirstOrDefault() as SplitContainer;
         //HUD hud = SubMainLeft.Panel1.Controls["hud1"] as HUD;

         lab = new System.Windows.Forms.Label();
         lab.Name = "pLabel";
         lab.Location = new System.Drawing.Point(66, 15);
         lab.Text = "Ez itt ?";
         sc.Panel2.Controls.Add(lab);
         sc.Panel2.Controls.SetChildIndex(lab, 1);

         ahd_q   = new MissionPlanner.Controls.MyButton();
         ahd_ch1 = new MissionPlanner.Controls.MyButton();
         ahd_ch2 = new MissionPlanner.Controls.MyButton();
         ahd_ch3 = new MissionPlanner.Controls.MyButton();
         ahd_ch4 = new MissionPlanner.Controls.MyButton();

         int w = sc.Panel2.Width;
         int h = 40;
         int s = 30;
         int l = 120;

         ahd_q.Location = new System.Drawing.Point(w - l, h); h = h + s;
         ahd_q.Name = "ahd1";
         ahd_q.Text = "QUAD";
         ahd_q.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
         sc.Panel2.Controls.Add(ahd_q);
         sc.Panel2.Controls.SetChildIndex(ahd_q, 1);
         ahd_q.Click += new EventHandler(this.btn_Click);

         ahd_ch1.Location = new System.Drawing.Point(w - l, h); h = h + s;
         ahd_ch1.Text = "CH1";
         ahd_ch1.Name = "ahd2";
         ahd_ch1.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
         sc.Panel2.Controls.Add(ahd_ch1);
         sc.Panel2.Controls.SetChildIndex(ahd_ch1, 1);
         ahd_ch1.Click += new EventHandler(this.btn_Click);

         ahd_ch2.Location = new System.Drawing.Point(w - l, h); h = h + s;
         ahd_ch2.Text = "CH2";
         ahd_ch2.Name = "ahd3";
         ahd_ch2.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
         sc.Panel2.Controls.Add(ahd_ch2);
         sc.Panel2.Controls.SetChildIndex(ahd_ch2, 1);
         ahd_ch2.Click += new EventHandler(this.btn_Click);

         ahd_ch3.Location = new System.Drawing.Point(w - l, h); h = h + s;
         ahd_ch3.Text = "CH3";
         ahd_ch3.Name = "ahd4";
         ahd_ch3.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
         sc.Panel2.Controls.Add(ahd_ch3);
         sc.Panel2.Controls.SetChildIndex(ahd_ch3, 1);
         ahd_ch3.Click += new EventHandler(this.btn_Click);

         ahd_ch4.Location = new System.Drawing.Point(w - l, h);
         ahd_ch4.Text = "CH4";
         ahd_ch4.Name = "ahd5";
         ahd_ch4.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
         sc.Panel2.Controls.Add(ahd_ch4);
         sc.Panel2.Controls.SetChildIndex(ahd_ch4, 1);
         ahd_ch4.Click += new EventHandler(this.btn_Click);


     });
     
            AcquireJoystick("Logitech Dual Action");

            return true;
        }


        public override bool Loaded()
        {
            return true;
        }

        public override bool Loop()
        {

            if (joystick != null)
            {
                joystick.Poll();

                current_state = joystick.GetCurrentState();
             
                
                yaw = (current_state.X / 1000) - 32;
                pitch = (current_state.Y / 1000) - 32;
                zoom = (current_state.RotationZ / 1000) - 32;

                if (Math.Abs(yaw) <= 2) yaw = 0;
                if (Math.Abs(pitch) <= 2) pitch = 0;
                if (Math.Abs(zoom) <= 2) zoom = 0;

                
                ahd_switch =  (current_state.Buttons[0] ? 1 : 0) + (current_state.Buttons[1] ? 2 : 0) + (current_state.Buttons[2] ? 3 : 0) + (current_state.Buttons[3] ? 4 : 0) + (current_state.Buttons[5] ? 5 : 0);
                mode_click = (current_state.Buttons[4] ? 1 : 0);
                focus_dir = (current_state.Buttons[6] ? 1 : 0) + (current_state.Buttons[7] ? 2 : 0);

                if (( yaw != old_yaw) || (pitch != old_pitch) || (zoom != old_zoom) || (ahd_switch != 0) || (mode_click != 0) || (focus_dir != 0))
                {
                    if (Host.cs.connected)
                    {
                        bool x = Host.comPort.doCommand(1, 0, MAVLink.MAV_CMD.DO_CONTROL_VIDEO, -1, focus_dir, mode_click, zoom, pitch, yaw, ahd_switch, false);
                    }
                    old_zoom = zoom;
                    old_pitch = pitch;
                    old_yaw = pitch;
                }
            }

            //Must use BeginIvoke to avoid deadlock with OnClose in main form.
            MainV2.instance.BeginInvoke((Action)(() =>
                {
                    lab.Text = "y:" + yaw.ToString() + " p:" + pitch.ToString() + " z:" + zoom.ToString();
                }));

            return true;
        }
 
        public override bool Exit()
        {
            if (joystick != null)
            {
                joystick.Unacquire();
            }

         return true;
        }

        public static SharpDX.DirectInput.Joystick getJoyStickByName(string name)
        {
            var joysticklist = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);

            foreach (DeviceInstance device in joysticklist)
            {
                if (device.ProductName.TrimUnPrintable() == name)
                {
                    return new SharpDX.DirectInput.Joystick(directInput, device.InstanceGuid);
                }
            }

            return null;
        }

        public SharpDX.DirectInput.Joystick AcquireJoystick(string name)
        {
            joystick = getJoyStickByName(name);

            if (joystick == null)
                return null;

            joystick.Acquire();

            joystick.Poll();

            return joystick;
        }
        void btn_Click(Object sender, EventArgs e)
        {

            if (sender == ahd_q)     { bool x = Host.comPort.doCommand(1, 0, MAVLink.MAV_CMD.DO_CONTROL_VIDEO, -1, 0, 0, -0, 0, 0, 5, false); }
            if (sender == ahd_ch1)   { bool x = Host.comPort.doCommand(1, 0, MAVLink.MAV_CMD.DO_CONTROL_VIDEO, -1, 0, 0, -0, 0, 0, 1, false); }
            if (sender == ahd_ch2)   { bool x = Host.comPort.doCommand(1, 0, MAVLink.MAV_CMD.DO_CONTROL_VIDEO, -1, 0, 0, -0, 0, 0, 2, false); }
            if (sender == ahd_ch3)   { bool x = Host.comPort.doCommand(1, 0, MAVLink.MAV_CMD.DO_CONTROL_VIDEO, -1, 0, 0, -0, 0, 0, 3, false); }
            if (sender == ahd_ch4)   { bool x = Host.comPort.doCommand(1, 0, MAVLink.MAV_CMD.DO_CONTROL_VIDEO, -1, 0, 0, -0, 0, 0, 4, false); }
            
        }

    }
}

