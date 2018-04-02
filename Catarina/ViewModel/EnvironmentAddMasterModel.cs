using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Catarina.ViewModel
{
    public class EnumToItemsSource : System.Windows.Markup.MarkupExtension
    {
        private readonly Type _type;

        public EnumToItemsSource(Type type)
        {
            _type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _type.GetMembers().SelectMany(member => member.GetCustomAttributes(typeof(DescriptionAttribute), true).Cast<DescriptionAttribute>()).Select(x => x.Description).ToList();
        }
    }

    public enum Direction
    {
        [Description("Встречное")]
        Income = 0,
        [Description("Попутное")]
        Outcome = 1 
    }

    


    public class EnvironmentAddMasterModel : ViewModelBase
    {
        //TODO Part of hardcode serial devices

        public ObservableCollection<string> AvaibleSerialPortList { get; set; } =
           new ObservableCollection<string>(System.IO.Ports.SerialPort.GetPortNames().ToList<string>());

        public bool IsSerialPortsExist { get => AvaibleSerialPortList.Count > 0; }

        public string SelectesImitatorPort
        {
            get => (AvaibleImitatorsSelectedItem as Interfaces.ISerialDeviceBuilder).PortName;
            set
            {               
                (AvaibleImitatorsSelectedItem as Interfaces.ISerialDeviceBuilder).PortName = value;
                OnPropertyChanged(nameof(SelectesImitatorPort));

                IsImitatorSelectedComplete = Environment.Imitator != null && SelectesImitatorPort != null;
                OnPropertyChanged(nameof(IsImitatorSelectedComplete));
            }
        }



        //END

        

        public bool IsSelectedNumberAlreadyExist
        {
            get
            {
                return !ViewModel.Instance.Environments
                   .Where(item => item.Number == CameraNumber)
                   .Select(item => item.Number == CameraNumber)
                   .DefaultIfEmpty(false)
                   .FirstOrDefault();
            }
        }

        public int CameraNumber
        {
            get => Environment.Number;
            set
            {
                Environment.Number = value;              
                OnPropertyChanged(nameof(IsSelectedNumberAlreadyExist));
                OnPropertyChanged(nameof(CameraNumber));
                OnPropertyChanged(nameof(CameraName));
            }
        }

        public string CameraName
        {
            get => Environment.Title; 
        }



        public bool IsMasterFinished { get; set; } = false;

        public ObservableCollection<Interfaces.IImitatorBuilder> AvaibleImitators { get; set; } = new ObservableCollection<Interfaces.IImitatorBuilder>();

        //private Interfaces.IImitatorBuilder _avaibleImitatorsSelectedItem = null;

        public Interfaces.IImitatorBuilder AvaibleImitatorsSelectedItem
        {
            get => Environment.Imitator;
            set {
                Environment.Imitator = value;
                IsImitatorSelectedComplete = Environment.Imitator != null && SelectesImitatorPort != null;
                OnPropertyChanged(nameof(IsImitatorSelectedComplete));

                //TODO Part of hardcode serial devices
                OnPropertyChanged(nameof(SelectesImitatorPort));
            }
        }

        public uint ImitationSpeed { get; set; } = 60;

        public uint ImitationDistance { get; set; } = 20;

        public Direction ImitationDirection { get; set; } = Direction.Income;

        public ObservableCollection<Interfaces.IDeviceBuilder> AvalibleDevices { get; set; } = new ObservableCollection<Interfaces.IDeviceBuilder>();

        public ObservableCollection<Interfaces.IDeviceBuilder> SelectedDevices
        {
            get => Environment.DeviceTypes;
            set { Environment.DeviceTypes = value; }
        }

        public EnvironmentModel Environment { get; set; } = null;

        private Interfaces.IDeviceBuilder _selectedDevicesSelectedItem = null;

        private Interfaces.IDeviceBuilder _avalibleDevicesSelectedItem = null;

        public Interfaces.IDeviceBuilder SelectedDevicesSelectedItem
        {
            get => _selectedDevicesSelectedItem;
            set {
                _selectedDevicesSelectedItem = value;
                if (_avalibleDevicesSelectedItem != null)
                {
                    _avalibleDevicesSelectedItem = null;
                    OnPropertyChanged(nameof(AvalibleDevicesSelectedItem));
                }
            }
        } 

        public Interfaces.IDeviceBuilder AvalibleDevicesSelectedItem
        {
            get => _avalibleDevicesSelectedItem;
            set {
                _avalibleDevicesSelectedItem = value;
                if (_selectedDevicesSelectedItem != null)
                {
                    _selectedDevicesSelectedItem = null;
                    OnPropertyChanged(nameof(SelectedDevicesSelectedItem));
                }
            }
        }

        public ICommand FromAvaibleToSelected { get; set; }

        public ICommand FromSelectedToAvaible { get; set; }

        public ICommand FromAvaibleToSelectedAll { get; set; }

        public ICommand FromSelectedToAvaibleAll { get; set; }

        public ICommand FinishSetup { get; set; }

        public bool IsDevicesSelectedComplete { get; set; } = false;

        public bool IsImitatorSelectedComplete { get; set; } = false;

        public EnvironmentAddMasterModel(IEnumerable<Interfaces.IDeviceBuilder> Devices, EnvironmentModel NewEnvironment, IEnumerable<Interfaces.IImitatorBuilder> Imitators)
        {
            Environment = NewEnvironment;

            foreach (var Device in Devices) { AvalibleDevices.Add(Device); }

            foreach (var Imitator in Imitators) { AvaibleImitators.Add(Imitator); }

            if (AvaibleImitators.Count > 0) AvaibleImitatorsSelectedItem = AvaibleImitators.First();

  
  
            FromAvaibleToSelected = new ViewModel.RelayCommand(o => 
            {
                SelectedDevices.Add(AvalibleDevicesSelectedItem);
                AvalibleDevices.Remove(AvalibleDevicesSelectedItem);
            }, o => AvalibleDevicesSelectedItem != null);

            FromSelectedToAvaible = new ViewModel.RelayCommand(o =>
            {
                AvalibleDevices.Add(SelectedDevicesSelectedItem);
                SelectedDevices.Remove(SelectedDevicesSelectedItem);
            }, o => SelectedDevicesSelectedItem != null);

            FromAvaibleToSelectedAll = new ViewModel.RelayCommand(o =>
            {
                foreach (var item in AvalibleDevices) { SelectedDevices.Add(item); }
                AvalibleDevices.Clear();
            }, o => AvalibleDevices.Count != 0);

            FromSelectedToAvaibleAll = new ViewModel.RelayCommand(o =>
            {
                foreach (var item in SelectedDevices) { AvalibleDevices.Add(item); }
                SelectedDevices.Clear();
            }, o => SelectedDevices.Count != 0);

            FinishSetup = new ViewModel.RelayCommand(o =>
            {
                IsMasterFinished = true;
                OnPropertyChanged(nameof(IsMasterFinished));
            }, o => true);

            CameraNumber = 1;
            SelectedDevices.CollectionChanged += SelectedDevices_CollectionChanged;


        }

        private void SelectedDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsDevicesSelectedComplete = SelectedDevices.Count > 0;
            OnPropertyChanged(nameof(IsDevicesSelectedComplete));

        }
    }
}
