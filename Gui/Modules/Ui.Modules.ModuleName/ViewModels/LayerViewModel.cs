using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Regions;
using ProMik.Services.Interfaces;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class LayerViewModel : RegionViewModelBase
    {
        private IEventService eventService;
        private ObservableCollection<string> layers = new ObservableCollection<string>();
        private ICommand selectionChanged;
        private ObservableCollection<string> objects = new ObservableCollection<string>();
        private ICommand selectionChangedObjects;

        public LayerViewModel(IRegionManager regionManager, IEventService eventService)
            : base(regionManager)
        {
            this.eventService = eventService;
            eventService.Subscribe<SendLayersEvent>(GetLayers);
            SelectionChanged = new DelegateCommand<IList>(async (x) => await ChangedTheSelection(x).ConfigureAwait(true));
            SelectionChangedObjects = new DelegateCommand<IList>(async (x) => await ChangedTheSelectionObjects(x).ConfigureAwait(true));
            eventService.Subscribe<ResetViewEvent>(ResetViewEventHandling);
            eventService.Subscribe<TestCoverageForDeterminingObjectsPerformedEvent>(InitTestCoverageObjects);
        }

        public ObservableCollection<string> Layers
        {
            get
            {
                return layers;
            }

            set
            {
                SetProperty(ref layers, value);
            }
        }

        public ObservableCollection<string> Objects
        {
            get
            {
                return objects;
            }

            set
            {
                SetProperty(ref objects, value);
            }
        }

        public ICommand SelectionChanged
        {
            get
            {
                return selectionChanged;
            }

            set
            {
                SetProperty(ref selectionChanged, value);
            }
        }

        public ICommand SelectionChangedObjects
        {
            get
            {
                return selectionChangedObjects;
            }

            set
            {
                SetProperty(ref selectionChangedObjects, value);
            }
        }

        private async Task ChangedTheSelection(IList obj)
        {
            await Task.Run(() =>
            {
                IList<string> list = new List<string>();
                foreach (var item in obj)
                {
                    list.Add(item.ToString());
                }

                eventService.Publish<ShowLayersWithObjectsEvent>(new ShowLayersWithObjectsEvent(list));
            }).ConfigureAwait(true);
        }

        private async Task ChangedTheSelectionObjects(IList obj)
        {
            await Task.Run(() =>
            {
                IList<string> list = new List<string>();
                foreach (var item in obj)
                {
                    list.Add(item.ToString());
                }

                eventService.Publish<ShowObjectsEvent>(new ShowObjectsEvent(list));
            }).ConfigureAwait(true);
        }

        private void GetLayers(SendLayersEvent obj)
        {
            Layers.Clear();
            foreach (var layer in obj.Layers)
            {
                if (!Layers.Contains(layer))
                {
                    Layers.Add(layer);
                }
            }
        }

        private void InitTestCoverageObjects(TestCoverageForDeterminingObjectsPerformedEvent obj)
        {
            foreach (string text in obj.GetObjects())
            {
                if (!Objects.Contains(text))
                {
                    Objects.Add(text);
                }
            }
        }

        private void ResetViewEventHandling(ResetViewEvent obj)
        {
            Layers.Clear();
            Objects.Clear();
        }
    }
}
