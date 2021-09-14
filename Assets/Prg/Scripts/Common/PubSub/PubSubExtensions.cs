using System;

namespace Prg.Scripts.Common.PubSub
{
    internal static class PubSubExtensions
    {
        private static readonly Hub Hub = new Hub();

        public static bool Exists<T>(this object obj)
        {
            foreach (var h in Hub.handlers)
            {
                if (Equals(h.Sender.Target, obj) &&
                    typeof(T) == h.Type)
                {
                    return true;
                }
            }

            return false;
        }

        public static void Publish<T>(this object obj)
        {
            Hub.Publish(obj, default(T));
        }

        public static void Publish<T>(this object obj, T data)
        {
            Hub.Publish(obj, data);
        }

        public static void Subscribe<T>(this object obj, Action<T> handler)
        {
            Hub.Subscribe(obj, handler);
        }

        public static void Unsubscribe(this object obj)
        {
            Hub.Unsubscribe(obj);
        }

        public static void Unsubscribe<T>(this object obj)
        {
            Hub.Unsubscribe(obj, (Action<T>) null);
        }

        public static void Unsubscribe<T>(this object obj, Action<T> handler)
        {
            Hub.Unsubscribe(obj, handler);
        }
    }
}