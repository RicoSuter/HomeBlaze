using HomeBlaze.Services.Json;
using System.Text.Json;

namespace HomeBlaze.Services.Tests.Json
{
    public class JsonUtilitiesTests
    {
        [Fact]
        public void WhenCallingPopulateObject_ThenJsonIsMergedIntoObject()
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
            JsonUtilities.Populate(person, json, options);

            Assert.Equal("Abc", person.FirstName);
            Assert.Equal("Def", person.LastName);

            Assert.Equal("Foo", person.Child.FirstName);
            Assert.Equal("Bar", person.Child.LastName);

            Assert.Equal(2, person.Children.Count);
            Assert.Null(person.Children.Last().FirstName);
            Assert.Equal("mno", person.Children.Last().LastName);
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
