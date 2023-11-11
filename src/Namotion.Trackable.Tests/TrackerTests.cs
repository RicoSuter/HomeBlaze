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

            tracker.AddProperty(firstName);
            tracker.AddProperty(lastName);
            tracker.AddProperty(new DerivedTrackedProperty<string>("FullName", 
                () => $"{firstName.Value} {lastName.Value}", 
                null, 
                tracker, subject));

            var propertyChanges = new List<TrackedPropertyChange>();
            subject.Subscribe(propertyChanges.Add);

            // Act
            firstName.Value = "Foo";
            lastName.Value = "Bar";

            var getValueFullName = tracker.Properties["FullName"].GetValue();
            var lastKnownValueFullName = tracker.Properties["FullName"].LastKnownValue;

            // Assert
            Assert.Equal(4, propertyChanges.Count);
            Assert.Equal("Foo", propertyChanges[0].Value);
            Assert.Equal("Foo Bar", getValueFullName);
            Assert.Equal(getValueFullName, lastKnownValueFullName);
        }
    }
}
