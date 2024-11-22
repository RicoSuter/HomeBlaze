using System.ComponentModel.DataAnnotations;

namespace Namotion.Proxy.Tests
{
    [GenerateProxy]
    public partial class Person
    {
        public Person()
        {
            Children = [];
        }

        [MaxLength(4)]
        public partial string? FirstName { get; set; }

        public partial string? LastName { get; set; }

        [Derived]
        public string FullName => $"{FirstName} {LastName}";

        public partial Person? Father { get; set; }

        public partial Person? Mother { get; set; }

        public partial Person[] Children { get; set; }

        public override string ToString() => FullName;
    }
}