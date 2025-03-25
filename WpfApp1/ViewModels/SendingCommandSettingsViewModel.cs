using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
    public class SendingCommandSettingsViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<SendingCommand> SendingCommands { get; set; }
        public ICommand SaveCommand { get; }

        public SendingCommandSettingsViewModel()
        {
            SendingCommands = LoadSettings();
            SaveCommand = new DelegateCommand(SaveSettings);
        }

        private ObservableCollection<SendingCommand> LoadSettings()
        {
            try
            {
                if (File.Exists("sendingCommands.xml"))
                {
                    var serializer = new XmlSerializer(typeof(ObservableCollection<SendingCommand>));
                    using (var reader = new StreamReader("sendingCommands.xml"))
                    {
                        return (ObservableCollection<SendingCommand>)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载设置时出错: {ex.Message}");
            }
            return new ObservableCollection<SendingCommand>();
        }

        private void SaveSettings(object parameter)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<SendingCommand>));
                using (var writer = new StreamWriter("sendingCommands.xml"))
                {
                    serializer.Serialize(writer, SendingCommands);
                }
                Console.WriteLine("设置已保存");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存设置时出错: {ex.Message}");
                MessageBox.Show($"保存设置时出错:{ex.Message}");
            }
        }
    }
}
