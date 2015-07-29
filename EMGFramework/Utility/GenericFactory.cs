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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EMGFramework.Utility
{
    /// <summary>
    /// Singleton with full-lazy instantiation implementing a factory for producing instances of any
    /// identifiable class hierarchy. 
    /// Adapted from the example shown at http://csharpindepth.com/articles/general/singleton.aspx
    /// </summary>
    public sealed class GenericFactory<T>
    {

        public static GenericFactory<T> Instance
        {
            get { return Nested.instance; }
        }


        private Dictionary<string, Type> _registeredProducts;

        /// <summary>
        /// This constructor searches for derived types of T if T is an abstract class, or for types implementing T
        /// if T is an interface. If those types or their base class have a static string property named ID, they are added 
        /// to the list of registered products using that ID string as key. Classes having an ID attribute that is already 
        /// in the dictionary (for instance due to forgetting to overwrite that of the base class in two or more subclasses) 
        /// WILL NOT be added. The order by which classes are added to the factory is determined by the ascending alphabetical order of the 
        /// source code file name where they are declared - this is a C# "feature".
        /// </summary>
        private GenericFactory()
        {

            _registeredProducts = new Dictionary<string, Type>();



            //Automatic registration of derived or implementing types 

            List<Assembly> assemblyList = new List<Assembly>();

            assemblyList.Add(Assembly.GetExecutingAssembly());
            assemblyList.Add(Assembly.GetCallingAssembly());
            assemblyList.Add(Assembly.GetEntryAssembly());

            foreach (Assembly assembly in assemblyList)
            {
                Type[] assemblyTypes = assembly.GetTypes();

                foreach (Type type in assemblyTypes)
                {
                    if (
                        (type.IsInterface == false) && (type.IsAbstract==false) &&
                        (
                         (/*type.BaseType == typeof(T) || */type.IsSubclassOf(typeof(T)) ||
                         (typeof(T).IsInterface && type.GetInterfaces().Contains(typeof(T))))
                        )
                       )
                    {
                        PropertyInfo idProperty = type.GetProperty("ID");
                        if (idProperty != null)
                        {
                            Type dummyType;

                            string label = (string)idProperty.GetValue(null, null);

                            if (!_registeredProducts.TryGetValue(label, out dummyType))
                                _registeredProducts.Add(label, type);

                        }
                    }
                }
            }
        }




        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly GenericFactory<T> instance = new GenericFactory<T>();
        }




        /*
        /// <summary>
        /// Registers a new generic type product
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="product"></param>
        public void RegisterProduct(string productName, Type type)
        {
            _registeredProducts.Add(productName, type);
        }
        */

        /// <summary>
        /// Creates an instance of one of the known types
        /// </summary>
        /// <param name="ProductName">Name of the class to be instantiated as defined by its ID property</param>
        /// <returns>An object of the requested class or the default value for the type T if no matching class was found.</returns>
        public T CreateProduct(string ProductName)
        {
            Type product;

            bool found = _registeredProducts.TryGetValue(ProductName, out product);

            if (found) return (T)Activator.CreateInstance(product);

            return default(T);
        }


        public List<string> SupportedProducts
        {
            get
            {
                List<string> resultList = new List<string>();
                foreach (string item in _registeredProducts.Keys)
                    resultList.Add(String.Copy(item));
                return resultList;
            }
        }

    }
}
