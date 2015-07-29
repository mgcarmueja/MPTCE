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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;


namespace EMGFramework.Utility
{

    /// <summary>
    /// Implementation of a single writer-multiple-reader communication infrastructure by using 
    /// an independent object queue for each reader. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectServer<T>
    {
        /*
        public static ObjectServer<T> Instance
        {
            get { return Nested.instance; }
        }
        */

        private Dictionary<Guid, BlockingCollection<T>> _registeredConsumers;


        private BlockingCollection<T> _producerQueue;

        private Guid _producerGuid;



        private readonly object syncRoot = new object();






        //private ObjectServer()
        public ObjectServer()
        {
            _registeredConsumers = new Dictionary<Guid, BlockingCollection<T>>();
            _producerQueue = new BlockingCollection<T>();
            _producerGuid = Guid.Empty;
        }


        /// <summary>
        /// Registers a producer and assigns it a Guid.
        /// </summary>
        /// <returns></returns>
        public Guid RegisterProducer()
        {

            Debug.Write("Registering new producer... ");

            lock (syncRoot)
            {
                if (_producerGuid == Guid.Empty)
                    _producerGuid = Guid.NewGuid();
                else _producerGuid = Guid.Empty;
            }

            Debug.WriteLine("OK. GUID " + _producerGuid);


            return _producerGuid;
        }



        /// <summary>
        /// Unregisters the current producer and invalidates the consumer queues
        /// </summary>
        /// <param name="producerGuid"></param>
        public void UnregisterProducer(Guid producerGuid)
        {

            Debug.Write("Unregistering producer with GUID " + _producerGuid + "... ");

            if (producerGuid == _producerGuid)
            {
                InvalidateQueues(producerGuid);
                _producerGuid = Guid.Empty;
            }

            Debug.WriteLine("OK.");

        }



        /// <summary>
        /// Called by the producer to enqueue a new item.
        /// </summary>
        /// <param name="producerGuid">Guid of the calling object. Items will only be added when this Guid actually 
        /// matches that of the active producer.</param>
        /// <param name="item">Item to be added</param>
        public void Enqueue(Guid producerGuid, T item)
        {
            if (producerGuid == _producerGuid)
            {
                lock (syncRoot)
                {

                    foreach (BlockingCollection<T> queue in _registeredConsumers.Values)
                        queue.Add(item);
                }
            }

            Debug.Write(">");
        }



        /// <summary>
        /// Used by the producer to invalidate the all consumer queues. The queues will be marked as 
        /// AddingComplete and refreshed
        /// </summary>
        /// <param name="producerGuid"></param>
        public void InvalidateQueues(Guid producerGuid)
        {
            Debug.Write("Invalidating consumer queues... ");

            BlockingCollection<T> collection;
            lock (syncRoot)
            {
                if (producerGuid == _producerGuid)
                {
                    List<Guid> keyList = new List<Guid>();

                    //Mark current consumer queues as AddingComplete and create new ones
                    foreach (Guid guid in _registeredConsumers.Keys)
                    {
                        keyList.Add(guid);

                        _registeredConsumers.TryGetValue(guid, out collection);
                        collection.CompleteAdding();

                    }
                    _registeredConsumers.Clear();

                    foreach (Guid guid in keyList)
                        _registeredConsumers.Add(guid, new BlockingCollection<T>());
                }
            }

            Debug.WriteLine("OK.");
        }



        /// <summary>
        /// Registers a client object on the client dictionary.
        /// </summary>
        public Guid RegisterConsumer()
        {
            Guid consumerGuid;

            Debug.Write("Registering consumer... ");

            lock (syncRoot)
            {
                consumerGuid = Guid.NewGuid();
                _registeredConsumers.Add(consumerGuid, new BlockingCollection<T>());
            }

            Debug.WriteLine("OK. GUID " + consumerGuid);

            return consumerGuid;
        }



        /// <summary>
        /// Unregisters a client object 
        /// </summary>
        /// <param name="guid"></param>
        public void UnregisterConsumer(Guid guid)
        {
            lock (syncRoot)
            {

                if (_registeredConsumers.Keys.Contains(guid))
                {
                    BlockingCollection<T> temp;
                    _registeredConsumers.TryGetValue(guid, out temp);
                    _registeredConsumers.Remove(guid);
                    temp.CompleteAdding();
                }
            }
        }



        /// <summary>
        /// Retrieves the BlockingCollection associated to a given registered client
        /// </summary>
        /// <param name="consumerGuid">Guid assigned to the consumer</param>
        /// <returns></returns>
        public BlockingCollection<T> GetConsumerQueue(Guid consumerGuid)
        {
            BlockingCollection<T> item;

            if (_registeredConsumers.TryGetValue(consumerGuid, out item))
                return item;
            else return null;
        }


    }
}
