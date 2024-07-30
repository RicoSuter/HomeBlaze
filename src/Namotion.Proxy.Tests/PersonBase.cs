using System.ComponentModel.DataAnnotations;

namespace Namotion.Proxy.Tests
{
    [GenerateProxy]
    public abstract class PersonBase
    {
        [MaxLength(4)]
        public virtual string? FirstName { get; set; }

        public virtual string? LastName { get; set; }

        public virtual string FullName => $"{FirstName} {LastName}";

        public virtual Person? Father { get; set; }

        public virtual Person? Mother { get; set; }

        public virtual PersonBase[] Children { get; set; } = Array.Empty<PersonBase>();

        public override string ToString() => FullName;
    }
}