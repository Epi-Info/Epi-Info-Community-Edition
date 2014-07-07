using System;
using System.Collections;
using System.Collections.Generic;
using Epi;

namespace Epi.Collections
{
    /// <summary>
    /// Class NamedObjectCollection
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public class NamedObjectCollection<T> : ICollection, IDisposable
    {
        #region Private Class Members
        private object syncRoot = null;
        private Dictionary<string, T> master = null;
        private Dictionary<string, object> tags = null;
        #endregion Private Class Members

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public NamedObjectCollection()
        {
            ////Only objects that implement INamedObject interface can be used in this collection.
            //string interfaceName = typeof(Epi.INamedObject).FullName;
            //if (typeof(T).GetInterface(interfaceName, true) == null)
            //{
            //    throw new System.Exception("Only objects that implement Epi.INamedObject interface can be used in NamedObjectCollection");
            //}

            master = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            tags = new Dictionary<string, object>();
            syncRoot = new object();
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Gets the number of key/value pairs contained in
        /// <see cref="System.Collections.Generic.Dictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        public int Count
        {
            get
            {
                return master.Count;
            }
        }

        /// <summary>
        /// Gets/sets Synchronization Root.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return this.syncRoot;
            }
        }

        /// <summary>
        /// Gets the is Synchronized flag.
        /// </summary>
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a collection containing the keys in the
        /// <see cref="System.Collections.Generic.Dictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        public Dictionary<string, T>.KeyCollection Keys
        {
            get
            {
                return master.Keys;
            }
        }

        /// <summary>
        /// Returns a list of names of the objects contained in the collection
        /// </summary>
        public List<string> Names
        {
            get
            {
                List<string> namesList = new List<string>();
                foreach (INamedObject obj in this)
                {
                    namesList.Add(obj.Name);
                }
                return namesList;
            }
        }

        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// Addes the specified key and value to the dictionary.
        /// </summary>
        /// <param name="obj">The value of the element to add.</param>
        public virtual void Add(T obj)
        {
            string name = ((INamedObject)obj).Name.ToLower();
            master.Add(name, obj);
        }

        /// <summary>
        /// Addes the specified key and value to the dictionary.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="tag">The value of the element to add.</param>
        public void Add(T obj, object tag)
        {
            string name = ((INamedObject)obj).Name.ToLower();
            Add(obj);
            tags.Add(name, tag);
        }

        /// <summary>
        /// Removes the value with the specified key from the System.Collections.Generic.SortedDictionary&lt;T1,T2&gt;.
        /// </summary>
        /// <param name="obj">Named object to remove.</param>
        /// <returns>true if the value is successfully removed; otherwise, false.  
        /// This method also returns false if key is not found in the System.Collections.Generic.SortedDictionary&lt;T1,T2&gt;.</returns>
        public virtual bool Remove(T obj)
        {
            INamedObject namedObject = obj as INamedObject;
            return master.Remove(namedObject.Name.ToLower());
        }

        /// <summary>
        /// Remove a string
        /// </summary>
        /// <param name="name">Name to remove</param>
        public virtual void Remove(string name)
        {
            T obj = this[name.ToLower()];
            Remove(obj);
        }

        /// <summary>
        /// Check to see if an object is contained
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Boolean</returns>
        public virtual bool Contains(T obj)
        {
            INamedObject namedObject = obj as INamedObject;
            return Contains(namedObject.Name.ToLower());
        }

        /// <summary>
        /// Check to see if an string name is contained
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Boolean</returns>
        public virtual bool Contains(string name)
        {
            return master.ContainsKey(name.ToLower());
        }

        /// <summary>
        /// Copies and array to a given index
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="index">Index</param>
        public virtual void CopyTo(T[] array, int index)
        {
            master.Values.CopyTo(array, index);
        }

        /// <summary>
        /// Copies and array to a given index
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="index">Index</param>
        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }

        /// <summary>
        /// Add an array of Named objects to collection
        /// </summary>
        /// <param name="collection">A generic NamedObjectCollection</param>
        public void Add(NamedObjectCollection<T> collection)
        {
            foreach (T obj in collection)
            {
                Add(obj);
            }
        }


        /// <summary>
        /// Get Enum
        /// </summary>
        /// <returns>IEnumerator</returns>
        public virtual IEnumerator GetEnumerator()
        {
            return this.master.Values.GetEnumerator();
        }

        /// <summary>
        /// Type Name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>NamedObjectCollection</returns>
        public T this[string name]
        {
            get
            {
                return master[name];
            }
        }

        /// <summary>
        /// Dispose master
        /// </summary>
        public virtual void Dispose()
        {
            if (master != null)
            {
                master.Clear();
                master = null;
            }
        }

        /// <summary>
        /// Check if name exists
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Boolean</returns>
        public bool Exists(string name)
        {
            return master.ContainsKey(name.ToLower());
        }

        /// <summary>
        /// Check if object exists
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Boolean</returns>
        public bool Exists(T obj)
        {
            return master.ContainsValue(obj);
        }

        /// <summary>
        /// Clear master
        /// </summary>
        public void Clear()
        {
            master.Clear();
        }

        /// <summary>
        /// Returns the tag associated with the object.
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>object</returns>
        public object GetTag(string name)
        {
            return tags[name];
        }

        #endregion Public Methods
    }
}