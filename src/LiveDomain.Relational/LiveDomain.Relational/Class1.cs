using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using LiveDomain.Relational.Extensions;

namespace LiveDomain.Relational
{

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Task : IComparable<Task>
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public DateTime DueBy{ get; set; }
        public int CategoryId { get; set; }

        public int CompareTo(Task other)
        {
            return Id.CompareTo(other.Id);
        }
    }

    public class Task2
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public int MyCategoryId { get; set; }
    }

    public class Task3
    {
        [Key]
        public ulong MyId { get; set; }
        public string Name { get; set; }
        public int MyCategoryId { get; set; }
    }

    public class Task4
    {
        [Key]
        public int Id1 { get; set; }
        [Key]
        public int Id2 { get; set; }
        public string Name { get; set; }
        public int MyCategoryId { get; set; }
    }


    public interface IKeyStrategy<E>
    {
        void AssignKey(E entity);
    }

    public class NullKeyStrategy<E> : IKeyStrategy<E>
    {
        public void AssignKey(E entity)
        {
            
        }
    }

    public class AssignIfDefaultKeyStrategy<E,K> : IKeyStrategy<E>
    {
        Action<E, K> _keyWriter;
        Func<E, K> _keyReader;
        IKeyGenerator<K> _keyGenerator;

        public AssignIfDefaultKeyStrategy(Action<E,K> keyWriter, Func<E,K> keyReader, IKeyGenerator<K> keyGenerator )
        {
            _keyWriter = keyWriter;
            _keyReader = keyReader;
            _keyGenerator = keyGenerator;
        }

        public void AssignKey(E entity)
        {
            if (_keyReader.Invoke(entity).Equals(default(K)))
            {
                _keyWriter.Invoke(entity, _keyGenerator.NextKey());
            }
        }
    }

    public interface IKeyGenerator<E>
    {
        E NextKey();
    }

    public class SequentialIntegerKeyGenerator : IKeyGenerator<ulong>
    {
        ulong _next = 1;

        public SequentialIntegerKeyGenerator()
        {

        }
        public SequentialIntegerKeyGenerator(ulong seed)
        {
            _next = seed;
        }

        public ulong NextKey()
        {
            return _next++;
        }

    }

    public class GuidKeyGenerator : IKeyGenerator<Guid>
    {
        public Guid NextKey()
        {
            return Guid.NewGuid();
        }
    }


    public class DelegateComparer<T> : IComparer<T>
    {

        internal Func<T, T, int> _comparer;

        public DelegateComparer(Func<T,T,int> comparer)
        {
            _comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            return _comparer.Invoke(x, y);
        }
    }

    public class ReflectionComparer<T> : DelegateComparer<T>
    {
        public ReflectionComparer(params PropertyInfo[] properties) : base(null)
        {
            if (properties == null) throw new ArgumentNullException("properties");
            foreach (var propertyInfo in properties)
            {
                if (!(propertyInfo.PropertyType.Implements(typeof(IComparable)))) 
                    throw new ArgumentException("Key property must implement IComparable: " + propertyInfo.Name); 
            }
            Func<T, T, int> comparer = null;
            comparer = (a, b) =>
            {
                foreach (PropertyInfo propertyInfo in properties)
                {

                    int res = ((IComparable)propertyInfo.GetGetMethod().Invoke(a,null)).CompareTo(propertyInfo.GetGetMethod().Invoke(b,null));
                    if (res != 0) return res;
                }
                return 0;
            };
            this._comparer = comparer;
        }
    }

    public class EntitySet<E> 
    {

        SortedSet<E> _items;
        IKeyStrategy<E> _keyStrategy;

        public EntitySet(IComparer<E> comparer, IKeyStrategy<E> keyStrategy)
        {
            _items = new SortedSet<E>(comparer);
            _keyStrategy = keyStrategy;
        }

        static IKeyStrategy<E> BuildKeyStrategy()
        {

            //Guid
            PropertyInfo property = GetKeyOrIdProperty();
            if (property != null)
            {
                if (property.PropertyType == typeof(Guid))
                {
                    Action<E, Guid> keyWriter = (e, g) => property.SetValue(e, g, null);
                    Func<E, Guid> keyReader = (e) => (Guid)property.GetGetMethod().Invoke(e, null);
                    return new AssignIfDefaultKeyStrategy<E, Guid>(keyWriter, keyReader, new GuidKeyGenerator());
                }
                else if (property.PropertyType == typeof(ulong))
                {
                    Action<E, ulong> keyWriter = (e, n) => property.SetValue(e, n, null);
                    Func<E, ulong> keyReader = (e) => (ulong)property.GetValue(e, null);
                    return new AssignIfDefaultKeyStrategy<E, ulong>(keyWriter, keyReader, new SequentialIntegerKeyGenerator());
                }
            }
            return new NullKeyStrategy<E>();

        }

        public EntitySet() :this(BuildComparer(), BuildKeyStrategy())
        {
        }

        static IComparer<E> BuildComparer()
        {

            //Look for properties with [Key] attribute
            PropertyInfo keyProperty = GetKeyProperty();
            if (keyProperty != null)
            {
                return new ReflectionComparer<E>(keyProperty);
            }

            //See if entity is comparable
            if(typeof(E).Implements(typeof(IComparable<E>)))
                return new DelegateComparer<E>((a, b) => ((IComparable<E>)a).CompareTo(b));
            
            
            //See if there's an Id property
            PropertyInfo idProperty = GetIdProperty();
            if(idProperty != null)
            {
                return new ReflectionComparer<E>(idProperty.DeclaringType.GetProperty(idProperty.Name));    
            }
            else throw new InvalidOperationException("No key properties found");
        }

        static PropertyInfo GetKeyOrIdProperty()
        {
            return GetKeyProperty() ?? GetIdProperty();
        }

        static PropertyInfo GetIdProperty()
        {
            return typeof(E).GetProperties().Where(p => p.Name.ToLower() == "id").SingleOrDefault();
        }
        private static PropertyInfo GetKeyProperty()
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            return typeof(E)
                .GetProperties(flags)
                .Where(p => p.GetCustomAttributes(true)
                    .Any(a => a.GetType() == typeof(KeyAttribute))).SingleOrDefault();
        }



        public E Add(E entity)
        {
            _keyStrategy.AssignKey(entity);
            if (!_items.Add(entity)) throw new InvalidOperationException("Add entity failed");
            return entity;
        }

        public void Remove(E entity)
        {
            if (!_items.Remove(entity)) throw new InvalidOperationException("Key not found");
        }

        public void Replace(E entity)
        {
            _items.Remove(entity);
            _items.Add(entity);
        }
    }
}
