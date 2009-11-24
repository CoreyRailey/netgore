﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetGore.Graphics.ParticleEngine
{
    /// <summary>
    /// A collection of <see cref="ParticleModifier"/>s.
    /// </summary>
    public class ParticleModifierCollection : IList<ParticleModifier>, IList
    {
        /// <summary>
        /// Creates a deep copy of the <see cref="ParticleModifierCollection"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="ParticleModifierCollection"/>.</returns>
        public ParticleModifierCollection DeepCopy()
        {
            var ret = new ParticleModifierCollection();
            ret._allModifiers.AddRange(_allModifiers);
            ret._updateModifiers.AddRange(_updateModifiers);
            ret._releaseModifiers.AddRange(_releaseModifiers);
            return ret;
        }

        /// <summary>
        /// Gets if this <see cref="ParticleModifierCollection"/> has any update modifiers.
        /// </summary>
        public bool HasUpdateModifiers { get { return _updateModifiers.Count > 0; } }

        /// <summary>
        /// Gets if this <see cref="ParticleModifierCollection"/> has any release modifiers.
        /// </summary>
        public bool HasReleaseModifiers { get { return _releaseModifiers.Count > 0; } }

        /// <summary>
        /// Modifiers that process released <see cref="Particle"/>s.
        /// </summary>
        readonly List<ParticleModifier> _releaseModifiers = new List<ParticleModifier>(2);

        /// <summary>
        /// Modifiers that process updated <see cref="Particle"/>s.
        /// </summary>
        readonly List<ParticleModifier> _updateModifiers = new List<ParticleModifier>(2);

        /// <summary>
        /// All of the modifiers.
        /// </summary>
        readonly List<ParticleModifier> _allModifiers = new List<ParticleModifier>(2);

        /// <summary>
        /// Gets an IEnumerable of all the <see cref="ParticleModifier"/>s that process <see cref="Particle"/>s
        /// when they are released.
        /// </summary>
        public IEnumerable<ParticleModifier> ReleaseModifiers { get { return _releaseModifiers; } }

        /// <summary>
        /// Gets an IEnumerable of all the <see cref="ParticleModifier"/>s that process <see cref="Particle"/>s
        /// when they are updated.
        /// </summary>
        public IEnumerable<ParticleModifier> UpdateModifiers { get { return _updateModifiers; } }

        /// <summary>
        /// Updates the current time on all processors.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        internal void UpdateCurrentTime(int currentTime)
        {
            foreach (var modifier in this)
                modifier.UpdateCurrentTime(currentTime);
        }

        /// <summary>
        /// Calls all modifiers that process released <see cref="Particle"/>s on the given <paramref name="particle"/>.
        /// </summary>
        /// <param name="emitter">The <see cref="ParticleEmitter"/> that created the <paramref name="particle"/>.</param>
        /// <param name="particle">The <see cref="Particle"/> to process.</param>
        public void ProcessReleasedParticle(ParticleEmitter emitter, Particle particle)
        {
            foreach (var modifier in _releaseModifiers)
                modifier.ProcessReleased(emitter, particle);
        }

        /// <summary>
        /// Calls all modifiers that process updated <see cref="Particle"/>s on the given <paramref name="particle"/>.
        /// </summary>
        /// <param name="emitter">The <see cref="ParticleEmitter"/> that created the <paramref name="particle"/>.</param>
        /// <param name="particle">The <see cref="Particle"/> to process.</param>
        /// <param name="elapsedTime">The amount of time that has elapsed since the <paramref name="emitter"/>
        /// was last updated.</param>
        public void ProcessUpdatedParticle(ParticleEmitter emitter, Particle particle, int elapsedTime)
        {
            foreach (var modifier in _updateModifiers)
                modifier.ProcessUpdated(emitter, particle, elapsedTime);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<ParticleModifier> GetEnumerator()
        {
            return _allModifiers.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Add(ParticleModifier item)
        {
            if (item == null)
                return;

            _allModifiers.Add(item);

            if (item.ProcessOnRelease)
                _releaseModifiers.Add(item);

            if (item.ProcessOnUpdate)
                _updateModifiers.Add(item);
        }

        /// <summary>
        /// Adds multiple items to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="items">The objects to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void AddRange(IEnumerable<ParticleModifier> items)
        {
            foreach (var item in items)
                Add(item);
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <returns>
        /// The position into which the new element was inserted.
        /// </returns>
        /// <param name="value">The <see cref="T:System.Object"/> to add to the
        /// <see cref="T:System.Collections.IList"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.IList"/> is read-only.
        ///     -or- 
        ///     The <see cref="T:System.Collections.IList"/> has a fixed size. 
        /// </exception>
        int IList.Add(object value)
        {
            return ((IList)_allModifiers).Add(value);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IList"/> contains a specific value.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"/> to locate in the
        /// <see cref="T:System.Collections.IList"/>.</param>
        /// <returns>
        /// true if the <see cref="T:System.Object"/> is found in the <see cref="T:System.Collections.IList"/>;
        /// otherwise, false.
        /// </returns>
        bool IList.Contains(object value)
        {
            return ((IList)_allModifiers).Contains(value);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
        public void Clear()
        {
            _allModifiers.Clear();
            _releaseModifiers.Clear();
            _updateModifiers.Clear();
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"/> to locate in the
        /// <see cref="T:System.Collections.IList"/>.</param>
        /// <returns>
        /// The index of <paramref name="value"/> if found in the list; otherwise, -1.
        /// </returns>
        int IList.IndexOf(object value)
        {
            return ((IList)_allModifiers).IndexOf(value);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.IList"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value"/> should be inserted.</param>
        /// <param name="value">The <see cref="T:System.Object"/> to insert into the
        /// <see cref="T:System.Collections.IList"/>.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the
        /// <see cref="T:System.Collections.IList"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.
        ///                     -or- 
        ///                     The <see cref="T:System.Collections.IList"/> has a fixed size. 
        /// </exception>
        /// <exception cref="T:System.NullReferenceException"><paramref name="value"/> is null reference in the
        /// <see cref="T:System.Collections.IList"/>.</exception>
        void IList.Insert(int index, object value)
        {
            ((IList)_allModifiers).Insert(index, value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Object"/> to remove from the
        /// <see cref="T:System.Collections.IList"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="T:System.Collections.IList"/> is read-only.
        ///      -or- 
        ///     The <see cref="T:System.Collections.IList"/> has a fixed size. 
        /// </exception>
        void IList.Remove(object value)
        {
            ((IList)_allModifiers).Remove(value);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>;
        /// otherwise, false.
        /// </returns>
        public bool Contains(ParticleModifier item)
        {
            return _allModifiers.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>,
        /// starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the
        /// elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.
        ///                     -or-
        ///                 <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
        ///                     -or-
        ///                     The number of elements in the source
        ///                     <see cref="T:System.Collections.Generic.ICollection`1"/>
        ///                     is greater than the available space from <paramref name="arrayIndex"/> to the
        ///                     end of the destination
        ///                     <paramref name="array"/>.
        ///                     -or-
        ///                     Type <see cref="ParticleModifier"/> cannot be cast automatically to the type of the destination
        ///                     <paramref name="array"/>.
        /// </exception>
        void ICollection<ParticleModifier>.CopyTo(ParticleModifier[] array, int arrayIndex)
        {
            _allModifiers.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/>is read-only.</exception>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the
        /// <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if
        /// <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public bool Remove(ParticleModifier item)
        {
            if (!_allModifiers.Remove(item))
                return false;

            _releaseModifiers.Remove(item);
            _updateModifiers.Remove(item);
            return true;
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>,
        /// starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements
        /// copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have
        /// zero-based indexing.</param><param name="index">The zero-based index in <paramref name="array"/> at which
        /// copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.
        ///                     -or- 
        ///                 <paramref name="index"/> is equal to or greater than the length of <paramref name="array"/>.
        ///                     -or- 
        ///                     The number of elements in the source <see cref="T:System.Collections.ICollection"/> is
        ///                     greater than the available space from <paramref name="index"/> to the end of the destination
        ///                     <paramref name="array"/>. 
        /// </exception>
        /// <exception cref="T:System.ArgumentException">The type of the source <see cref="T:System.Collections.ICollection"/>
        /// cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_allModifiers).CopyTo(array, index);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get { return _allModifiers.Count; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        object ICollection.SyncRoot
        {
            get { return ((ICollection)_allModifiers).SyncRoot; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/>
        /// is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe);
        /// otherwise, false.
        /// </returns>
        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)_allModifiers).IsSynchronized; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        bool IList.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        bool ICollection<ParticleModifier>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.IList"/> has a fixed size; otherwise, false.
        /// </returns>
        bool IList.IsFixedSize
        {
            get { return ((IList)_allModifiers).IsFixedSize; }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(ParticleModifier item)
        {
            return _allModifiers.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid
        /// index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void Insert(int index, ParticleModifier item)
        {
            if (item == null)
                return;

            _allModifiers.Insert(index, item);

            if (item.ProcessOnRelease)
                _releaseModifiers.Add(item);

            if (item.ProcessOnUpdate)
                _updateModifiers.Add(item);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a
        /// valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        public void RemoveAt(int index)
        {
            var item = _allModifiers[index];
            if (item != null)
            {
                if (item.ProcessOnRelease)
                    _releaseModifiers.Remove(item);

                if (item.ProcessOnUpdate)
                    _updateModifiers.Remove(item);
            }

            _allModifiers.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index
        /// in the <see cref="T:System.Collections.IList"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the
        /// <see cref="T:System.Collections.IList"/> is read-only.</exception>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        object IList.this[int index]
        {
            get { return ((IList)_allModifiers)[index]; }
            set { ((IList)_allModifiers)[index] = value; }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the
        /// <see cref="T:System.Collections.Generic.IList`1"/>.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the
        /// <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        public ParticleModifier this[int index]
        {
            get { return _allModifiers[index]; }
            set
            {
                if (value == null)
                    return;

                var current = _allModifiers[index];
                if (current == null || current == value)
                    return;

                if (current.ProcessOnRelease)
                    _releaseModifiers.Remove(current);

                if (current.ProcessOnUpdate)
                    _updateModifiers.Remove(current);

                _allModifiers[index] = value;

                if (value.ProcessOnRelease)
                    _releaseModifiers.Add(value);

                if (value.ProcessOnUpdate)
                    _updateModifiers.Add(value);
            }
        }
    }
}
