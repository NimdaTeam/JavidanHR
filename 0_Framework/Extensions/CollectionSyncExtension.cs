
namespace _0_Framework.Extensions
{
    public static class CollectionUpdater
    {
        public static void SyncCollection<T, TKey>(
            ICollection<T> currentItems,
            IEnumerable<T> inputItems,
            Func<T, TKey> keySelector,
            Action<T, T> updateExisting,
            Func<T, T> createNew,
            Func<TKey, bool> isKeyNew)
        {
            var inputList = inputItems.ToList();
            var inputKeys = inputList.Where(i => !isKeyNew(keySelector(i)))
                .Select(keySelector)
                .ToHashSet();

            // 1. Remove deleted items
            var toRemove = currentItems
                .Where(c => !inputKeys.Contains(keySelector(c)))
                .ToList();

            foreach (var item in toRemove)
                currentItems.Remove(item);

            // 2. Update existing + Add new
            foreach (var input in inputList)
            {
                var key = keySelector(input);

                if (!isKeyNew(key)) // Existing
                {
                    var existing = currentItems.FirstOrDefault(c => keySelector(c).Equals(key));
                    if (existing != null)
                    {
                        updateExisting(existing, input);
                    }
                }
                else // New
                {
                    var newItem = createNew(input);
                    currentItems.Add(newItem);
                }
            }
        }
    }
}

    
