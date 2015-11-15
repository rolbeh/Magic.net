﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magic
{
    public sealed class NestedSet<T> : IEnumerable<T> where T : class
    {
        private readonly SortedSet<NestedSetItem<T>> _set;

        public NestedSet()
        {
            _set = new SortedSet<NestedSetItem<T>>(NestedSetItem<T>.Comparer);
        }

        public T Root
        {
            get
            {
                var first = _set.FirstOrDefault(i => i.Left == 1);
                return first != null ? first.Value : null;
            }
            set
            {
                _set.Clear();
                _set.Add(new NestedSetItem<T>(1, 2, value, _set));
            }
        }

        /// <summary>
        /// Gibt die Gesamtanzahl der Element im Set an
        /// </summary>
        public int Lenght
        {
            get { return _set.Count; }
        }

        public NestedSetItem<T> RootItem
        {
            get { return _set.FirstOrDefault(i => i.Left == 1); }
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Liefert ein Enumerator über die gesamten Elemente im Set
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _set.Select(i => i.Value).AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

       }

    public class NestedSetItem<T> : ICollection<NestedSetItem<T>> where T : class
    {
        private readonly SortedSet<NestedSetItem<T>> _set;
        internal static IComparer<NestedSetItem<T>> Comparer = new Comparer<NestedSetItem<T>, long>(item => item.Left);


        internal NestedSetItem(long left, long right, T value, SortedSet<NestedSetItem<T>> set)
        {
            _set = set;
            Left = left;
            Right = right;
            Value = value;
        }

        internal long Left { get; private set; }

        internal long Right { get; private set; }

        public T Value { get; set; }

        #region Implementation of IEnumerable

        public IEnumerator<NestedSetItem<T>> GetEnumerator()
        {
            var n = new NextLevelEnumerator(this);
            var left = Left;
            var right = Right;
            var t = _set.Where(i => i.Left > left && i.Right < right).AsEnumerable().GetEnumerator();
            return n;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public IEnumerable<NestedSetItem<T>> AsAllChildren()
        {
            var left = Left;
            var right = Right;
            var t = _set.Where(i => i.Left > left && i.Right < right).AsEnumerable();
            return t;
        }

        internal class NextLevelEnumerable : IEnumerable<NestedSetItem<T>>
        {
            private readonly NestedSetItem<T> _rootItem;

            public NextLevelEnumerable(NestedSetItem<T> rootItem)
            {
                _rootItem = rootItem;
            }

            public IEnumerator<NestedSetItem<T>> GetEnumerator()
            {
                //var left = _rootItem.Left;
                //var right = _rootItem.Right;
                var t = _rootItem._set.Where(i => i.Left > _rootItem.Left && i.Right < _rootItem.Right).AsEnumerable().GetEnumerator();
                return t;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        class NextLevelEnumerator : IEnumerator<NestedSetItem<T>>
        {
            private readonly NestedSetItem<T> _rootItem;
            private NestedSetItem<T> _item;


            public NextLevelEnumerator(NestedSetItem<T> rootItem)
            {
                _rootItem = rootItem;
            }

            public void Dispose()
            {
                
            }

            public bool MoveNext()
            {
                var left = _item != null ? _item.Right + 1 : _rootItem.Left + 1;
                _item = _rootItem._set.FirstOrDefault(i => i.Left == left);
                return _item != null;
            }

            public void Reset()
            {
                _item = null;
            }

            public NestedSetItem<T> Current { get { return _item; } }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
        #region NestedSetItemEnumerator

        internal class NestedSetItemEnumerator : IEnumerator<T>
        {
            private readonly NestedSetItem<T> _rootItem;
            private IEnumerator<NestedSetItem<T>> _enumerator;

            internal NestedSetItemEnumerator(NestedSetItem<T> rootItem)
            {
                _rootItem = rootItem;
                _enumerator = new NextLevelEnumerator(rootItem);
                // bedingung erstellen nur elemente des nächsten levels
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                try
                {
                    _enumerator.Dispose();
                }
                catch { }
                _enumerator = null;
            }

            #endregion

            #region Implementation of IEnumerator

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            /// <summary>
            /// Ruft das Element in der Auflistung an der aktuellen Position des Enumerators ab.
            /// </summary>
            /// <returns>
            /// Das Element in der Auflistung an der aktuellen Position des Enumerators.
            /// </returns>
            public T Current
            {
                get { return _enumerator.Current.Value; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            #endregion
        }

        #endregion


        #region Implementation of ICollection<NestedSetItem<T>>

        public NestedSetItem<T> Add(T item)
        {
            var right = Right;
            _set.Each(i => 
            {
                if (i.Left > right)
                {
                    i.Left += 2;
                    i.Right += 2;
                    return;
                }
                if (i.Right > right)
                {
                    i.Right += 2;
                }
            });
            Right += 2;
            var result = new NestedSetItem<T>(right, right + 1, item, _set);
            _set.Add(result);
            return result; 
        }

        void ICollection<NestedSetItem<T>>.Add(NestedSetItem<T> item)
        {
            Add(item.Value);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(NestedSetItem<T> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(NestedSetItem<T>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(NestedSetItem<T> item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get
            {
                return (int) ((Right - Left - 1)/2);
            }
        }

        public int TotalCount
        {
            get
            {
                return (int)((Right - Left - 1) / 2);
            }
        }

        public bool IsReadOnly { get { return false; } }

        #endregion
    }
}
