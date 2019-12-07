using System;
using System.Collections.Generic;
using System.Text;

namespace SantasToolbox
{
    public static class ListExtenstions
    {
        public static bool AddIfNotNull<T>(this List<T> list, T? element)
            where T:class
        {
            if (element == null)
            {
                return false;
            }

            list.Add(element);
            return true;
        }
    }
}
