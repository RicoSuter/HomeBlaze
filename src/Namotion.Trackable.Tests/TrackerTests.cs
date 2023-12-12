using Microsoft.Extensions.DependencyInjection;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using System.Reactive.Subjects;

namespace Namotion.Trackable.Tests
{
    public class TrackerTests
    {
        [Fact]
        public void WhenTrackerHasProperties_ThenDerivedPropertyHasChangeEvent()
        {
            // Arrange
            var subject = new Subject<TrackedPropertyChange>();

            var tracker = new Tracker();
            var firstName = new TrackedProperty<string>("FirstName", "Rico", tracker, subject);
            var lastName = new TrackedProperty<string>("LastName", "Suter", tracker, subject);
            var fullName = new DerivedTrackedProperty<string>("FullName",
                () => $"{firstName.Value} {lastName.Value}",
                null, tracker, subject);

            tracker.AddProperty(firstName);
            tracker.AddProperty(lastName);
            tracker.AddProperty(fullName);

            var propertyChanges = new List<TrackedPropertyChange>();
            subject.Subscribe(propertyChanges.Add);

            // Act
            firstName.Value = "Foo";
            lastName.Value = "Bar";

            var getValueFullName = fullName.GetValue();
            var lastKnownValueFullName = fullName.LastKnownValue;

            // Assert
            Assert.Equal(4, propertyChanges.Count);
            Assert.Equal("Foo", propertyChanges[0].Value);
            Assert.Equal("Foo Bar", getValueFullName);
            Assert.Equal(getValueFullName, lastKnownValueFullName);
        }

        public class Person : ITrackableInitializer
        {
            [Trackable]
            public virtual string? FirstName { get; set; }

            [Trackable]
            public virtual string? LastName { get; set; }

            public void Initialize(Tracker tracker, ITrackableContext context)
            {
                tracker.AddProperty(new DerivedTrackedProperty<string>("FullName",
                    () => $"{FirstName} {LastName}",
                    null, tracker, context));
            }
        }

        [Fact]
        public void WhenTrackableRegistersDynamicProprties_ThenItIsTracked()
        {
            // Act
            var serviceCollection = new ServiceCollection();
            var factory = new TrackableFactory(serviceCollection.BuildServiceProvider());
            var context = new TrackableContext<Person>(factory);

            var tracker = context.TryGetTracker(context.Object);
            var fullNameProperty = tracker!.Properties["FullName"];

            var propertyChanges = new List<TrackedPropertyChange>();
            context.Subscribe(propertyChanges.Add);

            // Act
            context.Object.FirstName = "Rico";
            context.Object.LastName = "Suter";

            // Assert
            Assert.Equal(4, propertyChanges.Count);
            Assert.Equal("Rico Suter", propertyChanges.Last().Value);
            Assert.Equal("Rico Suter", fullNameProperty.GetValue());
        }
    }
}
