namespace HomeBlaze.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OperationAttribute : Attribute
    {
        public string? Title { get; set; }
    }
}