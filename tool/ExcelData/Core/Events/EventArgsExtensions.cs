using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datask.Tool.ExcelData.Core.Events
{
    public static class EventArgsExtensions
    {
        public static void FireStatusEvent<TType>(this object caller,
            EventHandler<StatusEventArgs<TType>>? handler,
            TType statusType,
            object? metadata = null,
            string? message = null)
            where TType : Enum
        {
            var handlerCopy = handler;
            if (handlerCopy != null)
            {
                var args = new StatusEventArgs<TType>(statusType, metadata, message);
                handlerCopy(caller, args);
            }
        }

        public static void Fire<TType>(this EventHandler<StatusEventArgs<TType>>? handler,
            TType statusType,
            object? metadata = null,
            string? message = null)
            where TType : Enum
        {
            var handlerCopy = handler;
            if (handlerCopy != null)
            {
                var args = new StatusEventArgs<TType>(statusType, metadata, message);
                handlerCopy(null, args);
            }
        }
    }
}
