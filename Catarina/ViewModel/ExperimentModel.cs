using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.ViewModel
{
    public class ExperimentModel : ViewModelBase
    {
        public ExperimentModel(ViewModel.ExpirementAddMasterModel ModelFrom)
        {
            SelectedDevice = ModelFrom.selectedDeviceFactory;
            Environment = ModelFrom.selecteEnvironmentModel;
            switch (ModelFrom.TerminateCause)
            {
                case TimeExpWatch.ByTime:
                    TerminateSpan = ModelFrom.TerminateDateTime - DateTime.Now;
                    break;
                case TimeExpWatch.ByInterval:
                    TerminateSpan = ModelFrom.TerminateSpan;
                    break;
            }
            FetchSpan = ModelFrom.FetchSpan;
        }

        public TimeSpan FetchSpan { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan TerminateSpan { get; set; } = TimeSpan.FromHours(24);

        public ViewModel.EnvironmentModel Environment { get; set; }

        public Interfaces.IDeviceFactory SelectedDevice { get; set; }




    }
}
