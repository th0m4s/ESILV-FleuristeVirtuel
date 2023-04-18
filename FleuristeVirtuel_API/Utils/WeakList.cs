using FleuristeVirtuel_API.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Utils
{
    public class WeakList<T> : IEnumerable<T> where T : class
    {
        private readonly List<WeakReference<T>> _items;

        public WeakList()
        {
            _items = new List<WeakReference<T>>();
        }

        public void Add(T item)
        {
            _items.Add(new WeakReference<T>(item));
        }

        public void Remove(T item)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (!_items[i].TryGetTarget(out var target) || target.Equals(item))
                    _items.RemoveAt(i);
            }
        }

        public List<T> GetItems()
        {
            var items = new List<T>();
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].TryGetTarget(out var target))
                    items.Add(target);
                else _items.RemoveAt(i);
            }
            return items;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in GetItems())
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
