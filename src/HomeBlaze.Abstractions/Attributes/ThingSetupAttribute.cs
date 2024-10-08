﻿namespace HomeBlaze.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ThingSetupAttribute : Attribute
    {
        public ThingSetupAttribute(Type thingType)
        {
            ThingType = thingType;
        }

        /// <summary>
        /// Gets or sets the type of the thing.
        /// </summary>
        public Type ThingType { get; internal set; }

        /// <summary>
        /// Gets or sets the type of the setup component.
        /// </summary>
        public Type? ComponentType { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether the setup can be used to edit the thing.
        /// </summary>
        public bool CanEdit { get; set; }

        public bool EditParentThing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the thing can be cloned. 
        /// Only things with a state property with [Configuration(IsIdentifier = true)] 
        /// which are used as part of the ID should be cloneable - otherwise the clone has
        /// not a unique ID which leads to unexpected side effects.
        /// </summary>
        public bool CanClone { get; set; }
        // TODO: Does this belong here?
    }
}