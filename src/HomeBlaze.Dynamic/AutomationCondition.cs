using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HomeBlaze.Dynamic
{
    public class AutomationCondition
    {
        private readonly Engine _engine = new Engine(o => o.AddObjectConverter(new EnumsToStringConverter()));

        internal const string TimeInCurrentStateVariableName = "timeInCurrentState";

        public List<PropertyVariable> Variables { get; set; } = new List<PropertyVariable>();

        public string? EventTypeName { get; set; }

        public string Expression { get; set; } = "true";

        public AutomationCondition()
        {
            _engine.Execute(
                @"function toSeconds(duration) {
                    const [hours, minutes, seconds] = duration.toString().split(':');
                    return Number(hours) * 60 * 60 + Number(minutes) * 60 + Number(seconds);
                }");
        }

        public bool Evaluate(Automation automation, IEvent @event, IThingManager thingMangager, bool forceEvaluation)
        {
            if (EventTypeName != null)
            {
                var matchesEventType = @event.GetType().FullName == EventTypeName;
                if (matchesEventType)
                {
                    return EvaluateExpression(automation, @event, thingMangager);
                }
            }
            else if (forceEvaluation)
            {
                return EvaluateExpression(automation, null, thingMangager);
            }
            else if (@event is ThingStateChangedEvent stateChangedEvent &&
                     Variables.Any(p => p.ThingId == stateChangedEvent.Thing.Id &&
                                        p.Property == stateChangedEvent.PropertyName))
            {
                return EvaluateExpression(automation, null, thingMangager);
            }

            return false;
        }

        public bool EvaluateExpression(Automation? automation, IEvent? @event, IThingManager thingMangager)
        {
            if (Expression == "true")
            {
                return true;
            }

            if (@event != null)
            {
                _engine.SetValue("event", @event);
            }

            if (@event is TimerEvent timerEvent && automation != null)
            {
                var timeInCurrentState = timerEvent.DateTime - automation.LastStateChange;
                _engine.SetValue(TimeInCurrentStateVariableName, timeInCurrentState);
            }

            foreach (var variable in Variables)
            {
                var value = variable.TryGetValue(thingMangager);
                _engine.SetValue(variable.ActualName, value!);
            }

            var result = _engine
                .Evaluate(Expression!)
                .ToObject();

            return result is bool boolean && boolean;
        }

        public class EnumsToStringConverter : IObjectConverter
        {
            public bool TryConvert(Engine engine, object value, [NotNullWhen(true)] out JsValue? result)
            {
                if (value is Enum)
                {
                    result = value.ToString();
                    return true;
                }

                result = JsValue.Null;
                return false;
            }
        }
    }
}
