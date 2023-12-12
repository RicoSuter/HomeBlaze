using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using System;
using HomeBlaze.Messages;
using DynamicExpresso;

namespace HomeBlaze.Dynamic
{
    public class PropertyCondition
    {
        private bool? _state;
        private DateTimeOffset? _lastStateChange;

        [Configuration]
        public string? ThingId { get; set; }

        [Configuration]
        public string? PropertyName { get; set; }

        [Configuration]
        public string? Expression { get; set; }

        [Configuration]
        public string? Operator { get; set; }

        [Configuration]
        public string? Value { get; set; }

        public TimeSpan? MinimumHoldDuration { get; set; }

        public bool Result
        {
            get
            {
                lock (this)
                {
                    return _state == true &&
                        (MinimumHoldDuration == null || DateTimeOffset.Now - _lastStateChange > MinimumHoldDuration);
                }
            }
        }

        protected bool Evaluate(IThingManager thingManager)
        {
            var interpreter = new Interpreter(InterpreterOptions.Default | InterpreterOptions.LateBindObject);

            var state = thingManager.TryGetPropertyState(ThingId, PropertyName, true);
            interpreter.SetVariable("value", state?.Value is Enum ? state?.Value.ToString() : state?.Value);

            var expression = Expression;
            if (string.IsNullOrEmpty(expression))
            {
                expression =
                    Operator == "==" ? "value == " + Value :
                    Operator == "!=" ? "value != " + Value :

                    Operator == "<" ? "value < " + Value :
                    Operator == "<=" ? "value <= " + Value :

                    Operator == ">" ? "value > " + Value :
                    Operator == ">=" ? "value >= " + Value :

                    "false";
            }

            var result = interpreter.Eval<bool>(expression);
            return result is bool boolean && boolean;
        }

        public void Apply(IThingManager thingManager, ThingStateChangedEvent stateChangedEvent)
        {
            if (ThingId == stateChangedEvent.Thing.Id &&
                PropertyName == stateChangedEvent.PropertyName)
            {
                lock (this)
                {
                    var state = Evaluate(thingManager);
                    if (state != _state)
                    {
                        _state = state;
                        _lastStateChange = DateTimeOffset.Now;
                    }
                }
            }
        }
    }
}