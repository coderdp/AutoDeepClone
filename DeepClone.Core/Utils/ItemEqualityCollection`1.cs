namespace HcAutoDeepClone.Core.Utils;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ItemEqualityCollection<T> : ICollection<T>
{
    private readonly List<T> _items;

    public ItemEqualityCollection(IEnumerable<T> items = null)
    {
        _items = items?.ToList() ?? new List<T>();
    }

    public void AddIfNotContains(T item)
    {
        if (Contains(item))
        {
            return;
        }

        Add(item);
    }

    public void Add(T item)
    {
        _items.Add(item);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public bool Contains(T item)
    {
        return _items.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return _items.Remove(item);
    }

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_items).GetEnumerator();
    }

    #region Equals

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        var other = obj as ItemEqualityCollection<T>;

        return Equals(other);
    }

    private bool Equals(ItemEqualityCollection<T> other)
    {
        if (other == null)
        {
            return false;
        }

        if (ReferenceEquals(other, this))
        {
            return true;
        }

        var otherItems = other.ToList();

        return _items.Count == otherItems.Count
               && _items.TrueForAll(s => otherItems.Contains(s))
               && otherItems.TrueForAll(o => _items.Contains(o));
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + _items.GetHashCode();

            return hash;
        }
    }

    public static bool operator ==(ItemEqualityCollection<T> left, ItemEqualityCollection<T> right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if ((object)left == null || (object)right == null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(ItemEqualityCollection<T> left, ItemEqualityCollection<T> right)
    {
        return !(left == right);
    }

    #endregion
}
