using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Todo.Core
{
    [Serializable]
    public class Task : IComparable<Task>
    {

        private Guid _id;

        public Guid Id
        {
            get { return _id; }
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private DateTime? _dueBy;

        public DateTime? DueBy
        {
            get { return _dueBy; }
            set { _dueBy = value; }
        }

        private DateTime? _completed;

        public DateTime? Completed
        {
            get { return _completed; }
            set { _completed = value; }
        }

        private HashSet<Category> _categories;

        public HashSet<Category> Categories
        {
            get { return _categories; }
            set { _categories = value; }
        }

        public Task(string Title)
        {
            _title = Title;
            _categories = new HashSet<Category>();
            _id = Guid.NewGuid();
        }


        public int CompareTo(Task other)
        {
            return Id.CompareTo(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Task && (obj as Task).Id == Id;
        }

    }
}
