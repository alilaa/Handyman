﻿using Handyman.Mediator.EventPipelineCustomization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Handyman.Mediator.Internals
{
    internal class DefaultEventHandlerExecutionStrategy : IEventHandlerExecutionStrategy
    {
        public static readonly IEventHandlerExecutionStrategy Instance = new DefaultEventHandlerExecutionStrategy();

        public Task Execute<TEvent>(List<IEventHandler<TEvent>> handlers, EventPipelineContext<TEvent> context) where TEvent : IEvent
        {
            return Task.WhenAll(handlers.Select(x => x.Handle(context.Event, context.CancellationToken)));
        }
    }
}