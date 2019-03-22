using System;
using System.Collections.Generic;
using System.Text;

namespace PeopleMatching.Infrastructure.Services
{
    public interface IPropertyMapping
    {
        Dictionary<string, List<MappedProperty>> MappingDictionary { get; }
    }
}
