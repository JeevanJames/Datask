// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System;

namespace Datask.Common.Events
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
            EventHandler<StatusEventArgs<TType>>? handlerCopy = handler;
            if (handlerCopy is not null)
            {
                var args = new StatusEventArgs<TType>(statusType, message, metadata);
                handlerCopy(caller, args);
            }
        }

        public static void Fire<TType>(this EventHandler<StatusEventArgs<TType>>? handler,
            TType statusType,
            object? metadata = null,
            string? message = null)
            where TType : Enum
        {
            EventHandler<StatusEventArgs<TType>>? handlerCopy = handler;
            if (handlerCopy is not null)
            {
                var args = new StatusEventArgs<TType>(statusType, message, metadata);
                handlerCopy(null, args);
            }
        }
    }
}
