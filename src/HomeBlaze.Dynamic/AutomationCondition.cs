using DynamicExpresso;
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Components.Editors;
using HomeBlaze.Messages;
using Namotion.Devices.Abstractions.Messages;
using System.Collections.Generic;
using System.Linq;

namespace HomeBlaze.Dynamic
{
    public class AutomationCondition
    {
        internal const string TimeInCurrentStateVariableName = "timeInCurrentState";

        public List<PropertyVariable> Variables { get; set; } = new List<PropertyVariable>();

        public string? EventTypeName { get; set; }

        public string Expression { get; set; } = "true";

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
                     Variables.Any(p => p.ThingId == (stateChangedEvent.Source as IThing)?.Id &&
                                        p.PropertyName == stateChangedEvent.PropertyName))
            {
                foreach (var variable in Variables
                    .Where(v => v.ThingId == (stateChangedEvent.Source as IThing)?.Id &&
                                v.PropertyName == stateChangedEvent.PropertyName))
                {
                    variable.Apply(stateChangedEvent);
                }

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

            var interpreter = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LateBindObject);

            if (@event != null)
            {
                interpreter.SetVariable<object>("event", @event);
            }

            if (@event is TimerEvent timerEvent && automation != null)
            {
                var timeInCurrentState = timerEvent.DateTime - automation.LastStateChange;
                interpreter.SetVariable(TimeInCurrentStateVariableName, timeInCurrentState);
            }

            foreach (var variable in Variables)
            {
                var value = variable.TryGetValue(thingMangager);
                interpreter.SetVariable(variable.ActualName, value!);
            }

            var result = interpreter
                .Eval<bool>(Expression!);

            return result is bool boolean && boolean;
        }
    }
}
