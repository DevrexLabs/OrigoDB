using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Todo.Core;
using LiveDomain.Core;
using Todo.Core.Commands;
using System.Windows.Data;

namespace Todo.Wpf
{
    public class MainWindowViewModel : ViewModelBase
    {

        private ITransactionHandler<TodoModel> _engine;

        private DelegateCommand _newListCommand;
        private DelegateCommand _newTaskCommand;


        private List<TaskViewModel> allTasks;
        private List<TaskViewModel> incompleteTasks; 

        public ObservableCollection<string> Lists { get; private set; }
        
        public ObservableCollection<TaskViewModel> Tasks
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
                    NotifyPropertyChanged("CurrentList");
                }
            }
        }

        private void LoadTasks()
        {
            var query = new GetTasksByListName(CurrentList);
            allTasks = _engine.Execute(query).Select(t => new TaskViewModel(t)).ToList();
            incompleteTasks = allTasks.Where(t => !t.Completed.HasValue).ToList();

            foreach (TaskViewModel task in allTasks)
            {
                AttachEvents(task);
            }

            SetTasks();
            NotifyPropertyChanged("Tasks");
        }


        private void SetTasks()
        {
            Tasks = _showCompleted
                        ? new ObservableCollection<TaskViewModel>(allTasks)
                        : new ObservableCollection<TaskViewModel>(incompleteTasks);
        }

        private void AttachEvents(TaskViewModel task)
        {
            task.CompleteChanged += CompleteChanged;
        }



        private void CompleteChanged(object sender, EventArgs e)
        {
            var task = (TaskViewModel)sender;

            if (task.IsCompleted)
            {
                var command = new SetCompletedCommand(task.Id, DateTime.Now);
                _engine.Execute(command);
            }
            else
            {
                var command = new ClearCompletedCommand(task.Id);
                _engine.Execute(command);
            }
        }


        private bool _showCompleted;

        public bool ShowCompleted 
        {
            get { return _showCompleted; }
            set 
            { 
                if(value != _showCompleted)
                {
                    _showCompleted = value;
                    SetTasks();
                    NotifyPropertyChanged("ShowCompleted");
                    NotifyPropertyChanged("Tasks");
                }
            }
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

        


        public MainWindowViewModel(ITransactionHandler<TodoModel> engine)
        {
            _engine = engine;
            _newListCommand = new DelegateCommand(() => CreateNewList(), () => CanCreateNewList);
            _newTaskCommand = new DelegateCommand(() => CreateNewTask(), () => CanCreateNewTask);
            Lists = new ObservableCollection<string>(_engine.Execute(new GetListNames()));
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
                bool taskNameAlreadyExists = Lists.Any(name => String.Equals(name, NewTaskTitle, StringComparison.InvariantCultureIgnoreCase));
                return !String.IsNullOrWhiteSpace(NewTaskTitle) && !taskNameAlreadyExists && !string.IsNullOrEmpty(CurrentList);
            }
        }

        private void CreateNewList()
        {
            AddListCommand command = new AddListCommand(NewListName);
            _engine.Execute(command);
            Lists.Add(NewListName);

            if (Lists.Count == 1)
                CurrentList = NewListName;

            NewListName = String.Empty;
        }

        private void CreateNewTask()
        {
            Task task = new Task(NewTaskTitle);
            AddTaskCommand command = new AddTaskCommand(task, CurrentList);
            _engine.Execute(command);

            var taskViewModel = new TaskViewModel(new TaskInfo(task));
            AttachEvents(taskViewModel);
            allTasks.Add(taskViewModel);
            Tasks.Add(taskViewModel);
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
