using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Todo.Core;
using LiveDomain.Core;

namespace Todo.Wpf
{
    public class MainWindowViewModel : ViewModelBase
    {

        private Engine<TodoModel> _engine;

        private DelegateCommand _newListCommand;
        private DelegateCommand _newTaskCommand;


        public ObservableCollection<string> Lists { get; private set; }
        
        public ObservableCollection<TaskView> Tasks
        { 
            get; 
            private set; 
        }

        private string _currentList;

        public string CurrentList
        {
            get { return _currentList; }
            set 
            {
                if (_currentList != value)
                {
                    _currentList = value;
                    LoadTasks();
                    NotifyPropertyChanged(CurrentList);
                }
            }
        }

        private void LoadTasks()
        {
            var tasks = _engine.Execute(db=>db.Lists.Single(list => list.IsNamed(CurrentList)).Tasks.Select(task => new TaskView(task)).ToList());
            tasks.ForEach( task => {
                task.CompleteChanged += CompleteChanged;
                task.SaveRequested += SaveTask;
                task.InitializeSaveCommand();
            });
            
            Tasks = new ObservableCollection<TaskView>(tasks);
            
            NotifyPropertyChanged("Tasks");
        }


        private void SaveTask(object sender, EventArgs e)
        {
            TaskView task = (TaskView)sender;
        }

        private void CompleteChanged(object sender, EventArgs e)
        {
            TaskView task = (TaskView)sender;
           
        }

        public ICommand NewListCommand
        {
            get { return _newListCommand; }
        }

        public ICommand NewTaskCommand
        {
            get { return _newTaskCommand; }
        }

        private string _newListName;

        public string NewListName
        {
            get { return _newListName; }
            set 
            {
                _newListName = value;
                NotifyPropertyChanged("NewListName");
            }
        }

        private string _newTaskTitle;

        public string NewTaskTitle
        {
            get { return _newTaskTitle; }
            set 
            { 
                _newTaskTitle = value;
                NotifyPropertyChanged("NewTaskTitle");
            }
        }


        public MainWindowViewModel(Engine<TodoModel> engine)
        {
            _engine = engine;
            _newListCommand = new DelegateCommand(() => CreateNewList(), () => CanCreateNewList);
            _newTaskCommand = new DelegateCommand(() => CreateNewTask(), () => CanCreateNewTask);
            Lists = new ObservableCollection<string>(_engine.Execute(db => db.Lists.Select(list => list.Name).ToArray()));
            if (Lists.Count > 0) CurrentList = Lists[0];
        }

        private bool CanCreateNewList
        {
            get
            {
                return !String.IsNullOrWhiteSpace(NewListName);
            }
        }

        private bool CanCreateNewTask
        {
            get
            {
                return !String.IsNullOrWhiteSpace(NewTaskTitle) && Lists.Count(s => s.ToLower() == NewTaskTitle.ToLower()) == 0;
            }
        }

        private void CreateNewList()
        {
            AddListCommand command = new AddListCommand(NewListName);
            _engine.Execute(command);
            Lists.Add(NewListName);
            NewListName = String.Empty;
        }

        private void CreateNewTask()
        {
            Task task = new Task(NewTaskTitle);
            AddTaskCommand command = new AddTaskCommand(task, CurrentList);
            _engine.Execute(command);
            Tasks.Add(new TaskView(task));
            NewTaskTitle = String.Empty;
        }

        protected override void NotifyPropertyChanged(string propertyName)
        {
            base.NotifyPropertyChanged(propertyName);
            _newListCommand.NotifyCanExecuteChanged();
            _newTaskCommand.NotifyCanExecuteChanged();
        }

    }
}
