using HomeBlaze.Services.Json;
using System.Text.Json;

namespace HomeBlaze.Services.Tests.Json
{
    public class JsonUtilitiesTests
    {
        [Fact]
        public void SystemTextJson()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            var person = new Person
            {
                FirstName = "Abc",
                Child = new Person
                {
                    FirstName = "Foo"
                },
                Children =
                {
                    new Person { FirstName = "Xyz" }
                }
            };

            var json = @"{ ""lastName"": ""Def"", ""child"": { ""lastName"": ""Bar"" }, ""children"": [ { ""lastName"": ""mno"" } ] }";
            JsonUtilities.PopulateObject(json, person, options);

            Assert.Equal("Abc", person.FirstName);
            Assert.Equal("Def", person.LastName);

            Assert.Null(person.Child.FirstName);
            Assert.Equal("Bar", person.Child.LastName);

            Assert.Single(person.Children);
            Assert.Null(person.Children.First().FirstName);
            Assert.Equal("mno", person.Children.First().LastName);
        }
    }

    public class Person
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public Person? Child { get; set; }

        public List<Person> Children { get; set; } = new List<Person>();
    }
}
