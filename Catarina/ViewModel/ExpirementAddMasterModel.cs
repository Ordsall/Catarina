using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.ViewModel
{
    public class ExpirementAddMasterModel : ViewModelBase
    {

        ViewModel.EnvironmentModel _selecteEnvironmentModel = null;

        public ViewModel.EnvironmentModel selecteEnvironmentModel
        {
            get => _selecteEnvironmentModel;
            set { _selecteEnvironmentModel = value; OnPropertyChanged(nameof(selecteEnvironmentModel)); }
        }

        Interfaces.IDeviceFactory _selectedDeviceFactory = null;

        public Interfaces.IDeviceFactory selectedDeviceFactory
        {
            get => _selectedDeviceFactory;
            set { _selectedDeviceFactory = value; OnPropertyChanged(nameof(selectedDeviceFactory)); }
        }
    }
}
