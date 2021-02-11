using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public class GenericFactory<KeyType,ValueType>
    {
        private readonly Dictionary<KeyType, ValueType> Items = new Dictionary<KeyType, ValueType>();

        public ValueType DefaultValue { get; set; }

        public void RegisterItem(KeyType key, ValueType item)
        {
            Items.Add(key, item);
        }

        public ValueType GetItem(KeyType key)
        {
            return Items.ContainsKey(key) ? Items[key] : DefaultValue;
        }

    }
}
