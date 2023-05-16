using MudBlazor;

namespace HomeBlaze.Abstractions.Presentation
{
    public interface IIconProvider
    {
        string IconName { get; }

        public Color IconColor => Color.Default;
    }
}
