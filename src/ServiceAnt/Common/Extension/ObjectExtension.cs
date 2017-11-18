using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceAnt.Common.Extension
{
    public static class ObjectExtension
    {
        public static void Locking<T>(this T @this, Action<T> action)
        {
            lock (@this)
            {
                action(@this);
            }
        }
    }
}
