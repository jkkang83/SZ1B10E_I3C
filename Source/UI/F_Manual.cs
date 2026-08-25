using FZ4P.Commons;
using FZ4P.Commons.Helper;
using FZ4P.Commons.Services.ActuatorServo.Context;
using FZ4P.Commons.Services.DriveICManualMove;
using FZ4P.DriverIc.ReadHall.Context;
using FZ4P.DriverIc.SlaveID.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P.UI
{
    public partial class F_Manual : ModelChangedBase
    {
        private Task t1;
        private CancellationTokenSource cts;

        string[] index = { "OIS", "AF" };
        string[] indexCh = { "0", "1" };
        string[] indexAxis = { "OIS", "AF" };

        private string _readHall;
        public string ReadHall
        {
            get => _readHall;
            set
            {
                if (_readHall != value)
                {
                    _readHall = value;
                    OnPropertyChanged(nameof(ReadHall), value);
                }
            }
        }
        private string _readHall2;
        public string ReadHall2
        {
            get => _readHall2;
            set
            {
                if (_readHall2 != value)
                {
                    _readHall2 = value;
                    OnPropertyChanged(nameof(ReadHall2), value);
                }
            }
        }

        private string _peakCurrent;
        public string PeakCurrent
        {
            get => _peakCurrent;
            set
            {
                if (_peakCurrent != value)
                {
                    _peakCurrent = value;
                    OnPropertyChanged(nameof(PeakCurrent), value);
                }
            }
        }

        public F_Manual()
        {
            InitializeComponent();
            PropertyChanged += F_Manual_PropertyChanged;
            cbb_Acturator_Model.DataSource = Enum.GetValues(typeof(ActuatorType));
            cbb_ADC_Select.DataSource = index;
            cbb_Aixs.DataSource = indexAxis;
            cbb_Channel.DataSource = indexCh;
        }

        private void F_Manual_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReadHall))
            {
                this.InvokeOnUIThread(() => { 
                    lbl_ReadHall.Text = PropertiesHelper.GetValue<string>(e);
                });
            }
            else if (e.PropertyName == nameof(ReadHall2))
            {
                this.InvokeOnUIThread(() => {
                    lbl_ReadHall2.Text = PropertiesHelper.GetValue<string>(e);
                });
            }
            else if (e.PropertyName == nameof(PeakCurrent))
            {
                this.InvokeOnUIThread(() => {
                    lbl_ADC.Text = PropertiesHelper.GetValue<string>(e);
                });
            }
        }

        private void F_Manual_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            bool State= ((CheckBox)sender).Checked;

            if (State)
                STATIC.Dln.PowerOnOff(0, true);
            else
                STATIC.Dln.PowerOnOff(0, false);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            var selected = (ActuatorType)cbb_Acturator_Model.SelectedItem;
            var Context = new DriverICServoContext(selected, STATIC.Process.DrvIC);

            Context.ServoOnOff(0, "AF", true);
            Context.ServoOnOff(0, "X", true);
            Context.ServoOnOff(0, "Y", true);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var selected = (ActuatorType)cbb_Acturator_Model.SelectedItem;

            var Context = new DriveMoveICContext(selected, STATIC.Process.DrvIC);
            Context.ManualMoveAxis(0, STATIC.DrvIC.AF_MIN_CODE, STATIC.DrvIC.AF_MIN_CODE, STATIC.DrvIC.AF_MIN_CODE);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var selected = (ActuatorType)cbb_Acturator_Model.SelectedItem;

            var Context = new DriveMoveICContext(selected, STATIC.Process.DrvIC);
            Context.ManualMoveAxis(0, STATIC.DrvIC.AF_MID_CODE, STATIC.DrvIC.AF_MID_CODE, STATIC.DrvIC.AF_MID_CODE);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            var selected = (ActuatorType)cbb_Acturator_Model.SelectedItem;

            var Context = new DriveMoveICContext(selected, STATIC.Process.DrvIC);
            Context.ManualMoveAxis(0, STATIC.DrvIC.AF_MAX_CODE, STATIC.DrvIC.AF_MAX_CODE, STATIC.DrvIC.AF_MAX_CODE);
        }

        private void btn_PositionMove_Click(object sender, EventArgs e)
        {
            STATIC.Dln.PowerOnOff(0, true);
            try
            {
                var selected = (ActuatorType)cbb_Acturator_Model.SelectedItem;
                var positionX = Convert.ToInt32(txt_PositionCode_AxisX.Text);
                var positionY = Convert.ToInt32(txt_PositionCode_AxisY.Text);
                var positionZ = Convert.ToInt32(txt_PositionCode_AxisZ.Text);
                var Context = new DriveMoveICContext(selected, STATIC.Process.DrvIC);
                Context.ManualMoveAxis(0, positionX, positionY, positionZ);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void EditCondition_CheckStateChanged(object sender, EventArgs e)
        {
            bool State = ((CheckBox)sender).Checked;
            int ch = Convert.ToInt32(cbb_Channel.Text);
            int iAixs = cbb_Aixs.SelectedIndex;
            if (State)
            {
                cts = new CancellationTokenSource();
                Task.Run(() => ReadHold(cts.Token, ch, iAixs));
            }
            else
                cts?.Cancel();
        }

        private void ReadHold(CancellationToken token,int iCh,int iAixs)
        {
            if (iAixs != 0) ReadHall2 = "-";

            while (!token.IsCancellationRequested)
            {
                if (iAixs == 0) //OIS
                {
                    ReadHall = STATIC.DrvIC.ReadHall(iCh, "X").ToString();
                    ReadHall2 = STATIC.DrvIC.ReadHall(iCh, "Y").ToString();
                }
                else
                   ReadHall = STATIC.DrvIC.ReadAFHall(iCh).ToString();

                Thread.Sleep(5);
                PeakCurrent = STATIC.DrvIC.GetPeakCurrent(iCh, iAixs).ToString();
                Thread.Sleep(5);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int ch = Convert.ToInt32(cbb_Channel.Text);
            int iAixs = cbb_Aixs.SelectedIndex;

            STATIC.DrvIC.CurrentSetRegister(ch, iAixs);
        }

        private void rb_DetectPin_Click(object sender, EventArgs e)
        {
            var stateIndex = Convert.ToInt32(((RadioButton)sender).Tag);
            int adcNumber = cbb_ADC_Select.SelectedIndex;
            switch (stateIndex)
            {
                case 0:
                    STATIC.Dln.PeakDetector(adcNumber, PeakDetectState.Hold);
                    break;
                case 1:
                    STATIC.Dln.PeakDetector(adcNumber, PeakDetectState.Detect);
                    break;
                case 2:
                    STATIC.Dln.PeakDetector(adcNumber, PeakDetectState.Reset);
                    break;
            }
            
        }

        private void EditCondition_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
