using Prism.Mvvm;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using ProMik.SvfPlayer_sharp;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ProgrammerSettingsViewModel : BindableBase
    {
        private const string TITLEPREFIX = "Programmer settings for JTAG device: ";
        private string title;
        private string ip;
        private uint port;
        private int target;
        private int slot;
        private uint supplyVoltage;
        private uint ioVoltage;
        private uint frequency;
        private uint cableCompensation;

        public uint CableCompensation
        {
            get
            {
                return cableCompensation;
            }

            set
            {
                SetProperty(ref cableCompensation, value);
            }
        }

        public uint Frequency
        {
            get
            {
                return frequency;
            }

            set
            {
                SetProperty(ref frequency, value);
            }
        }

        public uint IoVoltage
        {
            get
            {
                return ioVoltage;
            }

            set
            {
                SetProperty(ref ioVoltage, value);
            }
        }

        public uint SupplyVoltage
        {
            get
            {
                return supplyVoltage;
            }

            set
            {
                SetProperty(ref supplyVoltage, value);
            }
        }

        public int Target
        {
            get
            {
                return target;
            }

            set
            {
                SetProperty(ref target, value);
            }
        }

        public int Slot
        {
            get
            {
                return slot;
            }

            set
            {
                SetProperty(ref slot, value);
            }
        }

        public uint Port
        {
            get
            {
                return port;
            }

            set
            {
                SetProperty(ref port, value);
            }
        }

        public string Title
        {
            get
            {
                if (!string.IsNullOrEmpty(title))
                {
                    return title;
                }
                else
                {
                    return TITLEPREFIX;
                }
            }

            set
            {
                SetProperty(ref title, value);
            }
        }

        public string IP
        {
            get
            {
                return ip;
            }

            set
            {
                SetProperty(ref ip, value);
            }
        }

        public void SetJtagDevice(string jtag)
        {
            Title = TITLEPREFIX + jtag;
        }

        public void InitValues(WrappedProgrammerSettings settingData)
        {
            IP = settingData.Ip;
            Port = settingData.Port;
            SupplyVoltage = settingData.SupplyVoltage;
            IoVoltage = settingData.IoVoltage;
            Target = settingData.Target;
            Frequency = settingData.Frequency;
            CableCompensation = settingData.CableCompensation;
            Slot = settingData.Slot;
        }
    }
}
