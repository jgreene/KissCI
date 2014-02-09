using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Helpers
{
    public static class CollectionHelpers
    {
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> items)
        {
            return items ?? new List<T>();
        }
        public static IList<T> NotNull<T>(this IList<T> items)
        {
            return items ?? new List<T>();
        }

        public static T[] NotNull<T>(this T[] items)
        {
            return items ?? new T[0];
        }
    }
}
