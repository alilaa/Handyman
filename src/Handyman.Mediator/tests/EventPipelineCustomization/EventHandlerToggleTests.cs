﻿using Handyman.Mediator.EventPipelineCustomization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Handyman.Mediator.Tests.EventPipelineCustomization
{
    public class EventHandlerToggleTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldToggleEventHandler(bool toggleEnabled)
        {
            var toggle = new EventHandlerToggle { Enabled = toggleEnabled };
            var toggledHandler = new ToggleEnabledEventHandler();
            var fallbackHandler = new ToggleDisabledEventHandler();

            var services = new ServiceCollection();

            services.AddScoped<IMediator>(x => new Mediator(x));
            services.AddSingleton<IEventHandlerToggle>(toggle);
            services.AddSingleton<IEventHandler<Event>>(toggledHandler);
            services.AddSingleton<IEventHandler<Event>>(fallbackHandler);

            await services.BuildServiceProvider().GetService<IMediator>().Publish(new Event());

            toggle.ToggleMetadata.Name.ShouldBe("test");
            toggle.ToggleMetadata.Tags.ShouldBe(new[] { "foo" });
            toggle.ToggleMetadata.ToggleDisabledHandlerTypes.ShouldBe(new[] { typeof(ToggleDisabledEventHandler) });
            toggle.ToggleMetadata.ToggleEnabledHandlerTypes.ShouldBe(new[] { typeof(ToggleEnabledEventHandler) });

            toggledHandler.Executed.ShouldBe(toggleEnabled);
            fallbackHandler.Executed.ShouldBe(!toggleEnabled);
        }

        [EventHandlerToggle(typeof(ToggleEnabledEventHandler), ToggleDisabledHandlerTypes = new[] { typeof(ToggleDisabledEventHandler) }, Name = "test", Tags = new[] { "foo" })]
        private class Event : IEvent { }

        private class ToggleEnabledEventHandler : EventHandler<Event>
        {
            public bool Executed { get; set; }

            protected override void Handle(Event @event)
            {
                Executed = true;
            }
        }

        private class ToggleDisabledEventHandler : EventHandler<Event>
        {
            public bool Executed { get; set; }

            protected override void Handle(Event @event)
            {
                Executed = true;
            }
        }

        private class EventHandlerToggle : IEventHandlerToggle
        {
            public bool Enabled { get; set; }
            public EventHandlerToggleMetadata ToggleMetadata { get; set; }

            public Task<bool> IsEnabled<TEvent>(EventHandlerToggleMetadata toggleMetadata,
                EventPipelineContext<TEvent> pipelineContext) where TEvent : IEvent
            {
                ToggleMetadata = toggleMetadata;
                return Task.FromResult(Enabled);
            }
        }
    }
}