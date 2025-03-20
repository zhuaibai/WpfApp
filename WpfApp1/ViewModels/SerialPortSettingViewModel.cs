using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using WpfApp1.Command;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    public class SerialPortSettingViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private SerialPortSettings _settings;

        public ObservableCollection<string> AvailablePorts { get; }
        public ObservableCollection<int> BaudRates { get; }
        public ObservableCollection<int> DataBits { get; }
        public ObservableCollection<StopBits> StopBitsOptions { get; }
        public ObservableCollection<Parity> ParityOptions { get; }

        public string SelectedPort
        {
            get { return _settings.PortName; }
            set
            {
                if (_settings.PortName != value)
                {
                    _settings.PortName = value;
                    OnPropertyChanged(nameof(SelectedPort));
                }
            }
        }

        public int SelectedBaudRate
        {
            get { return _settings.BaudRate; }
            set
            {
                if (_settings.BaudRate != value)
                {
                    _settings.BaudRate = value;
                    OnPropertyChanged(nameof(SelectedBaudRate));
                }
            }
        }

        public int SelectedDataBits
        {
            get { return _settings.DataBits; }
            set
            {
                if (_settings.DataBits != value)
                {
                    _settings.DataBits = value;
                    OnPropertyChanged(nameof(SelectedDataBits));
                }
            }
        }

        public StopBits SelectedStopBits
        {
            get { return _settings.StopBits; }
            set
            {
                if (_settings.StopBits != value)
                {
                    _settings.StopBits = value;
                    OnPropertyChanged(nameof(SelectedStopBits));
                }
            }
        }

        public Parity SelectedParity
        {
            get { return _settings.Parity; }
            set
            {
                if (_settings.Parity != value)
                {
                    _settings.Parity = value;
                    OnPropertyChanged(nameof(SelectedParity));
                }
            }
        }

        public ICommand SaveCommand { get; }

        public SerialPortSettingViewModel()
        {
            _settings = LoadSettings();

            AvailablePorts = new ObservableCollection<string>(SerialPort.GetPortNames());
            BaudRates = new ObservableCollection<int> { 2400, 9600, 115200 };
            DataBits = new ObservableCollection<int> { 7, 8 };
            StopBitsOptions = new ObservableCollection<StopBits> { StopBits.One, StopBits.Two };
            ParityOptions = new ObservableCollection<Parity> { Parity.None, Parity.Even, Parity.Odd };

            if (AvailablePorts.Count > 0 && string.IsNullOrEmpty(_settings.PortName))
            {
                SelectedPort = AvailablePorts[0];
            }
            if (_settings.BaudRate == 0)
            {
                SelectedBaudRate = BaudRates[0];
            }
            if (_settings.DataBits == 0)
            {
                SelectedDataBits = DataBits[0];
            }
            if (_settings.StopBits == 0)
            {
                SelectedStopBits = StopBitsOptions[0];
            }
            if (_settings.Parity == 0)
            {
                SelectedParity = ParityOptions[0];
            }

            SaveCommand = new DelegateCommand(SaveSettings);
        }

        private SerialPortSettings LoadSettings()
        {
            try
            {
                if (File.Exists("serialSettings.xml"))
                {
                    var serializer = new XmlSerializer(typeof(SerialPortSettings));
                    using (var reader = new StreamReader("serialSettings.xml"))
                    {
                        return (SerialPortSettings)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载设置时出错: {ex.Message}");
            }
            return new SerialPortSettings();
        }

        private void SaveSettings(object parameter)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(SerialPortSettings));
                using (var writer = new StreamWriter("serialSettings.xml"))
                {
                    serializer.Serialize(writer, _settings);
                }
                Console.WriteLine("设置已保存");
                MessageBox.Show("设置已保存");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存设置时出错: {ex.Message}");
                MessageBox.Show($"保存设置时出错:{ex.Message}");
            }
        }
    }
}
