namespace HomeBlaze.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ThingTypeAttribute : Attribute
    {
        public ThingTypeAttribute(string fullName)
        {
            FullName = fullName;
        }

        public string FullName { get; }
    }
}
