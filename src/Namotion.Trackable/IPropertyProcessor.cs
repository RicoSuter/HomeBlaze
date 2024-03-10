using System;

namespace Namotion.Trackable;

public interface IPropertyProcessor
{
    bool CanProcess(Type propertyType)
    {
        return true;
    }
}
