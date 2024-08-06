namespace HomeBlaze.Abstractions.Presentation
{
    public interface IIconProvider
    {
        string IconName { get; }

        public string IconColor => "Default";
    }
}
