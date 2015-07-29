/*
 * Copyright 2015 Martin Garcia Carmueja 
 * 
 *  This file is part of EMGFramework.
 *
 *  EMGFramework is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  EMGFramework is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with EMGFramework.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EMGFramework.Utility
{
    /// <summary>
    /// Singleton able to manage instances of a given superclass T, retaining only one copy of each subclass of T. 
    /// In order to be supported by InstanceManager, the subclasses must be compatible with the GenericFactory Singleton. 
    /// They must not be actually generated using a GenericFactory though.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class InstanceManager<T>
    {

        public static InstanceManager<T> Instance
        {
            get { return Nested.instance; } 
        }

        private Dictionary<string, T> _registeredInstances;

        private readonly object syncRoot = new object();

        /// <summary>
        /// Triggered whenever a new item is added to the InstanceManager, either replacing an existing item or not.
        /// </summary>
        public event EventHandler ContentChanged;


        private void OnContentChanged(EventArgs e)
        {
            EventHandler handler = ContentChanged;
            if (handler != null)
                handler(this, e);
        }



        private InstanceManager()
        {
            _registeredInstances = new Dictionary<string, T>();
            
        }

        /// <summary>
        /// Registers an instance of a subclase of T, eventually replacing an existing instance of the same type.
        /// </summary>
        /// <param name="instance">The instance to add to the InstanceManager</param>
        public void Register(T instance)
        {
            lock (syncRoot)
            {
                string typeName = (string)instance.GetType().GetProperty("ID").GetValue(null, null);
                if (_registeredInstances.ContainsKey(typeName))
                    _registeredInstances.Remove(typeName);

                _registeredInstances.Add(typeName, instance);
            }

            OnContentChanged(EventArgs.Empty);
        }


        /// <summary>
        /// Retrieves the instance with the specified type identification
        /// </summary>
        /// <param name="typeName">Identification of the type to be retrieved</param>
        /// <returns></returns>
        public T Retrieve(string typeName)
        {
            T item;

            if (_registeredInstances.TryGetValue(typeName, out item))
                return item;
            else return default(T);
        }


        /// <summary>
        /// Returs a list with the type identifications of the registered instances
        /// </summary>
        public List<string> RegisteredInstances
        {
            get 
            {
                List<string> resultList = new List<string>();

                foreach (string item in _registeredInstances.Keys)
                    resultList.Add(String.Copy(item));

                return resultList;
            }
        }


        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly InstanceManager<T> instance = new InstanceManager<T>();
        }

    }
}
