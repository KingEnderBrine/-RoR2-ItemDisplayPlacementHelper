using System.Collections;
using System.Collections.Generic;

namespace ItemDisplayPlacementHelper.Native.Structs
{
    public unsafe class MultimapEnumerator<TKey, TValue> : IEnumerator<MultimapNode<TKey, TValue>> where TKey : unmanaged where TValue : unmanaged
    {
        private readonly Multimap<TKey, TValue> map;
        private MultimapNode<TKey, TValue>* current;

        public MultimapNode<TKey, TValue> Current => *current;

        object IEnumerator.Current => Current;

        public MultimapEnumerator(Multimap<TKey, TValue> map)
        {
            this.map = map;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (map.count == 0)
            {
                return false;
            }

            if (current == null)
            {
                current = map.root->left;
                return current != map.root;
            }

            if (current == map.root)
            {
                return false;
            }

            if (!current->right->isnil)
            {
                bool isNil;
                MultimapNode<TKey, TValue>* next = current->right;
                do
                {
                    current = next;
                    next = next->left;
                    isNil = next->isnil;
                } while (!isNil);
            }
            else
            {
                bool isNil;
                MultimapNode<TKey, TValue>* previous;
                do
                {
                    previous = current;
                    current = current->parent;
                    isNil = current->isnil;
                } while (!isNil && previous == current->right);
            }

            return current != map.root;
        }

        public void Reset()
        {
            current = null;
        }
    }
}
