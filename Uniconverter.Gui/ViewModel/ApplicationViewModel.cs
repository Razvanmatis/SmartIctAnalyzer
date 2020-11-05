using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Uniconverter.Core;


namespace Uniconverter.Gui
{
    public class ApplicationViewModel : BaseViewModel
    {
        private IPageViewModel _currentTabeleViewModel;
        private IPageViewModel _sidePageViewModel;

        
        private List<IPageViewModel> _listTableViewModel;
        private List<IPageViewModel> _listSideViewModel;

        private FileHandler _MainFileHandler;



        private ICommand _goTo2;
        public ICommand GoTo
        {
            get
            {
                string test = "Halllo vom GoTo2Screen";
                return _goTo2 ?? (_goTo2 = new RelayCommand(x =>
                {
                    Mediator.Notify("GoTo2Screen", test);
                }));
            }
        }



        #region TableViewModels
        public List<IPageViewModel> TablesViewModels
        {
            get
            {
                if (_listTableViewModel == null)
                    _listTableViewModel = new List<IPageViewModel>();

                return _listTableViewModel;
            }
        }

        public IPageViewModel CurrentTableViewModel
        {
            get
            {
                return _currentTabeleViewModel;
            }
            set
            {
                _currentTabeleViewModel = value;
                OnPropertyChanged("CurrentTableViewModel");
            }
        }

        private void ChangeTableViewModel(IPageViewModel viewModel)
        {
            if (!TablesViewModels.Contains(viewModel))
                TablesViewModels.Add(viewModel);

            CurrentTableViewModel = TablesViewModels
                .FirstOrDefault(vm => vm == viewModel);
        }

        #endregion

        #region SidePages
        public List<IPageViewModel> SideViewModels
        {
            get
            {
                if (_listSideViewModel == null)
                    _listSideViewModel = new List<IPageViewModel>();

                return _listSideViewModel;
            }
        }


        public IPageViewModel CurrentSidePageViewModel
        {
            get
            {
                return _sidePageViewModel;
            }
            set
            {
                _sidePageViewModel = value;
                OnPropertyChanged("CurrentSidePageViewModel");
            }
        }



        private void ChangeSideViewModel(IPageViewModel viewModel)
        {
            if (!_listSideViewModel.Contains(viewModel))
                _listSideViewModel.Add(viewModel);

            CurrentSidePageViewModel = _listSideViewModel
                .FirstOrDefault(vm => vm == viewModel);
        }

        #endregion

        private void OnGo1Screen(object obj)
        {
            //ChangeViewModel(PageViewModels[0]);
        }

        private void OnGo2Screen(object obj)
        {

            //ChangeViewModel(PageViewModels[1]);
        }
        private void OnOpenNewFile(object obj)
        {
            bool validFile = false;
            bool startReadInProjectName = false;
            //string fileFormat = _MainFileHandler.CheckFileFormat((string)obj, ref validFile, ref startReadInProjectName);

            _MainFileHandler.loadFile((string)obj, ref startReadInProjectName);



            //if (!validFile)
            //{
            //    MessageBox.Show("File : " + (string)obj + " \n" + "Format : " + fileFormat + "\n File is not usable" );
            //}




        }


        public ApplicationViewModel(ref FileHandler fileHandler)
        {

            _MainFileHandler = fileHandler;
            IPageViewModel viewModel = new StartPageViewModel();
            _listSideViewModel = new List<IPageViewModel>();

            _listSideViewModel.Add(viewModel);
            ChangeSideViewModel(_listSideViewModel[0]);


            Mediator.Subscribe("LoadNewFile", OnOpenNewFile);

           
            //FileHandler file = new FileHandler()
            // Add available pages and set page
            //PageViewModels.Add(new StartPageViewModel());
            //PageViewModels.Add(new TestPointsTabelViewModel());
            //PageViewModels.Add(new UserControl2ViewModel());

            //CurrentPageViewModel = PageViewModels[0];


            Mediator.Subscribe("GoTo1Screen", OnGo1Screen);
            Mediator.Subscribe("GoTo2Screen", OnGo2Screen);
        }
    }
}
