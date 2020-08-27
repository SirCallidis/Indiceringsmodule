using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indiceringsmodule.Common
{
    public static class EventRaiser
    {
        /// <summary>
        /// 1
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="sender"></param>
        public static void Raise(this EventHandler handler, object sender)
        {
            handler?.Invoke(sender, EventArgs.Empty);
        }

        /// <summary>
        /// 2
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        public static void Raise<T>(this EventHandler<EventArgs<T>> handler, object sender, T value)
        {
            handler?.Invoke(sender, new EventArgs<T>(value));
        }

        /// <summary>
        /// 3
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        public static void Raise<T>(this EventHandler<T> handler, object sender, T value) where T : EventArgs
        {
            handler?.Invoke(sender, value);
        }

        /// <summary>
        /// 4
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        public static void Raise<T>(this EventHandler<EventArgs<T>> handler, object sender, EventArgs<T> value)
        {
            handler?.Invoke(sender, value);
        }
    }
}
