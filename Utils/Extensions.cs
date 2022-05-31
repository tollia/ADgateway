using System.Collections;
using System.DirectoryServices;

namespace ADgateway.Utils
{
    public static class Extensions
    {
        public static Dictionary<string, object> CopyPropertyCollectionToDictionary(this PropertyCollection collection, bool simplify = false)
        {
            Dictionary<string, object> result = new();
            foreach (string name in collection.PropertyNames)
            {
                object valueList = collection[name];
                if (valueList != null && valueList is ICollection)
                {
                    ArrayList list = new ArrayList((ICollection)valueList);
                    valueList = list.Count == 1 && simplify
                        ? list[0]
                        : list;
                }
                if (valueList == null)
                {
                    valueList = string.Empty;
                }
                result.Add(name, valueList);
            }
            return result;
        }

        public static string GetPropertyValueString(this PropertyCollection collection, string name)
        {
            string result = string.Empty;
            PropertyValueCollection valueCollection = collection[name];
            if (valueCollection != null) result = (valueCollection[0] as string) ?? string.Empty;
            return result;
        }

        public static string RemoveStart(this string source, string remove)
        {
            int index = source.IndexOf(remove);
            return (index == 0)
                ? source.Remove(0, remove.Length)
                : source;
        }
    }
}
