using System;
using System.Collections.Generic;
using System.Linq;

namespace RepoZ.Api.Common
{
	/// <summary>
	/// A simple queue which makes sure that each item is just available one single time.
	/// By enqueueing an item, the queue will remove any pointers to it if it was already enqueued before.
	/// </summary>
	/// <remarks>This queue is not designed to be threadsafe.</remarks>
	/// <typeparam name="T">The type of the items to hold.</typeparam>
	public class SingularityQueue<T>
	{
		private readonly List<T> _list = new List<T>();

		/// <summary>
		/// Enqueues the specified item at the end of the queue.
		/// Note that the queue will remove this item before enqueueing it to make sure it 
		/// does not appear multiple times.
		/// </summary>
		/// <param name="item">The item to add to the queue.</param>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void Enqueue(T item)
		{
			if (item == null)
				throw new ArgumentNullException();

			_list.RemoveAll(i => i.Equals(item));
			_list.Add(item);
		}

		/// <summary>
		/// Adds a specified item to the beginning of the queue.
		/// Note that the queue will remove this item before enqueueing it to make sure it 
		/// does not appear multiple times.
		/// </summary>
		/// <param name="item">The item to add the the queue.</param>
		/// <exception cref="System.ArgumentNullException"></exception>
		public void PushIn(T item)
		{
			if (item == null)
				throw new ArgumentNullException();

			_list.RemoveAll(i => i.Equals(item));
			_list.Insert(0, item);
		}
		
		/// <summary>
		/// Returns the first item from the queue and removes it from the queue.
		/// </summary>
		/// <returns></returns>
		public T Dequeue()
		{
			if (!_list.Any())
				return default(T);

			var item = _list.First();
			_list.RemoveAt(0);

			return item;
		}

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the entire queue.
		/// </summary>
		/// <param name="item">The object to locate in the queue. The value can be null for reference types.</param>
		/// <returns>The zero-based index of the first occurrence of item within the entire queue, if found; otherwise, –1.</returns>
		public int IndexOf(T item) => _list.IndexOf(item);

		/// <summary>
		/// Gets the number of elements actually contained in the queue.
		/// </summary>
		/// <value>
		/// The number of elements actually contained in the queue.
		/// </value>
		public int Count => _list.Count;

		/// <summary>
		///  Determines whether a sequence contains any elements.
		/// </summary>
		/// <returns>true if the source sequence contains any elements; otherwise, false.</returns>
		public bool Any() => _list.Any();

		/// <summary>
		/// Removes all elements from the queue.
		/// </summary>
		public void Clear() => _list.Clear();
	}
}
