using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Todo.Core;
using LiveDomain.Core;
using Todo.Core.Queries;

namespace Todo.Wpf
{
    public class MainWindowViewModel : ViewModelBase
    {

        private ITransactionHandler<TodoModel> _engine;

        private DelegateCommand _newListCommand;
        private DelegateCommand _newTaskCommand;


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
                    NotifyPropertyChanged(CurrentList);
                }
            }
        }

        private void LoadTasks()
        {
            var query = new GetTasksByListNameQuery(CurrentList);
            var tasks = _engine.Execute(query).Select(t => new TaskViewModel(t)).ToList();

            foreach (TaskViewModel task in tasks)
            {
                AttachEvents(task);
            }
            
            Tasks = new ObservableCollection<TaskViewModel>(tasks);
            
            NotifyPropertyChanged("Tasks");
        }

        private void AttachEvents(TaskViewModel task)
        {
            task.CompleteChanged += CompleteChanged;
            task.SaveRequested += SaveTask;
        }


        private void SaveTask(object sender, EventArgs e)
        {
            TaskViewModel task = (TaskViewModel)sender;
            //TODO: Create a SaveTaskCommand
        }

        private void CompleteChanged(object sender, EventArgs e)
        {
            TaskViewModel task = (TaskViewModel)sender;
            //TODO: Create SetCompletedCommand and ClearCompletedCommand classes
           
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
            Lists = new ObservableCollection<string>(_engine.Execute(new GetListNamesQuery()));
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

            var taskViewModel = new TaskViewModel(new TaskInfo(task));
            AttachEvents(taskViewModel);
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
