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

            _system = true;
            _web = true;
            _event = true;
            _repository = true;
            _contentOperation = true;
            _query = true;
            _index = true;
            _indexQueue = true;
            _database = true;
            _messaging = true;
            _workflow = true;
            _security = true;
            _securityQueue = true;
            _taskManagement = true;
            _custom = true;
            _test = true;
            _other = true;
        }

        private bool _system;
        public bool SystemVisible
        {
            get { return _system; }
            set
            {
                _system = value;
                OnPropertyChanged("SystemVisible");
            }
        }

        private bool _web;
        public bool Web
        {
            get { return _web; }
            set
            {
                _web = value;
                OnPropertyChanged("Web");
            }
        }

        private bool _event;
        public bool Event
        {
            get { return _event; }
            set
            {
                _event = value;
                OnPropertyChanged("Event");
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

        private bool _database;
        public bool Database
        {
            get { return _database; }
            set
            {
                _database = value;
                OnPropertyChanged("Database");
            }
        }

        private bool _messaging;
        public bool Messaging
        {
            get { return _messaging; }
            set
            {
                _messaging = value;
                OnPropertyChanged("Messaging");
            }
        }

        private bool _workflow;
        public bool Workflow
        {
            get { return _workflow; }
            set
            {
                _workflow = value;
                OnPropertyChanged("Workflow");
            }
        }

        private bool _security;
        public bool Security
        {
            get { return _security; }
            set
            {
                _security = value;
                OnPropertyChanged("Security");
            }
        }

        private bool _securityQueue;
        public bool SecurityQueue
        {
            get { return _securityQueue; }
            set
            {
                _securityQueue = value;
                OnPropertyChanged("SecurityQueue");
            }
        }

        private bool _taskManagement;
        public bool TaskManagement
        {
            get { return _taskManagement; }
            set
            {
                _taskManagement = value;
                OnPropertyChanged("TaskManagement");
            }
        }

        private bool _custom;
        public bool Custom
        {
            get { return _custom; }
            set
            {
                _custom = value;
                OnPropertyChanged("Custom");
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

        private bool _other;
        public bool Other
        {
            get { return _other; }
            set
            {
                _other = value;
                OnPropertyChanged("Other");
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
