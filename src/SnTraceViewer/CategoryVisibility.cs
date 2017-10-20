using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnTraceViewer
{
    public class CategoryVisibility : INotifyPropertyChanged
    {
        private MainWindow _mainWindow;
        public CategoryVisibility(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            _systemVisible = true;
            _repository = true;
            _contentOperation = true;
            _query = true;
            _index = true;
            _indexQueue = true;
            _test = true;
        }

        private bool _systemVisible;
        public bool SystemVisible
        {
            get { return _systemVisible; }
            set
            {
                _systemVisible = value;
                OnPropertyChanged("SystemVisible");
            }
        }

        private bool _repository;
        public bool Repository
        {
            get { return _repository; }
            set
            {
                _repository = value;
                OnPropertyChanged("Repository");
            }
        }

        private bool _contentOperation;
        public bool ContentOperation
        {
            get { return _contentOperation; }
            set
            {
                _contentOperation = value;
                OnPropertyChanged("ContentOperation");
            }
        }

        private bool _query;
        public bool Query
        {
            get { return _query; }
            set
            {
                _query = value;
                OnPropertyChanged("Query");
            }
        }

        private bool _index;
        public bool Index
        {
            get { return _index; }
            set
            {
                _index = value;
                OnPropertyChanged("Index");
            }
        }

        private bool _indexQueue;
        public bool IndexQueue
        {
            get { return _indexQueue; }
            set
            {
                _indexQueue = value;
                OnPropertyChanged("IndexQueue");
            }
        }

        private bool _test;
        public bool Test
        {
            get { return _test; }
            set
            {
                _test = value;
                OnPropertyChanged("Test");
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            Application.Current.Dispatcher.Invoke(() => _mainWindow.ApplyCategoryFilters());
        }
    }
}
