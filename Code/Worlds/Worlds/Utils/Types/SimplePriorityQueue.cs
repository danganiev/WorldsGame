using System.Collections.Generic;
using System.Linq;

namespace WorldsGame.Utils.Types
{
    internal class SimplePriorityQueue<T>
    {
        private readonly SortedDictionary<int, List<T>> _queueDictionary = new SortedDictionary<int, List<T>>();
        private readonly Dictionary<string, int> _priorityMap = new Dictionary<string, int>();
        private int _maxPriority;

        internal int Count { get; private set; }

        internal SimplePriorityQueue()
        {
            _queueDictionary[0] = new List<T>();
            Count = 0;
        }

        internal void Enqueue(T element)
        {
            _queueDictionary[0].Add(element);
            _priorityMap.Add(element.GetHashCode().ToString(), 0);
            Count++;
        }

        internal void IncreasePriority(T element)
        {
            string elementHash = element.GetHashCode().ToString();

            if (_priorityMap.ContainsKey(elementHash))
            {
                _queueDictionary[_priorityMap[elementHash]].Remove(element);
                _priorityMap[elementHash]++;

                if (!_queueDictionary.ContainsKey(_priorityMap[elementHash]))
                {
                    _queueDictionary[_priorityMap[elementHash]] = new List<T>();
                }

                _queueDictionary[_priorityMap[elementHash]].Add(element);

                if (_priorityMap[elementHash] > _maxPriority)
                {
                    _maxPriority = _priorityMap[elementHash];
                }
            }
            else
            {
                Enqueue(element);
            }
        }

        internal T Dequeue()
        {
            T element = _queueDictionary[_maxPriority].First();
            _queueDictionary[_maxPriority].Remove(element);
            _priorityMap.Remove(element.GetHashCode().ToString());

            if (_queueDictionary[_maxPriority].Count == 0 && _maxPriority != 0)
            {
                _queueDictionary.Remove(_maxPriority);
                _maxPriority--;
            }

            Count--;

            return element;
        }
    }
}