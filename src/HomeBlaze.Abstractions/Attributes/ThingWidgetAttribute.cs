﻿namespace HomeBlaze.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ThingWidgetAttribute : Attribute
    {
        public ThingWidgetAttribute(Type componentType)
        {
            ComponentType = componentType;
        }

        public Type ComponentType { get; }

        public bool NoPointerEvents { get; set; }
    }
}