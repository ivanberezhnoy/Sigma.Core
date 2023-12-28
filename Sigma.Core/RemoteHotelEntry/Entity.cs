using Sigma.Core.DataStorage;
using System.Globalization;

namespace Sigma.Core.RemoteHotelEntry
{
    public class Entity
    {
        public Entity(string ID, string name, bool isDeleted)
        {
            Id = ID;
            Name = name;
            IsDeleted = isDeleted;
        }

        public bool IsDeleted { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public bool filterMatch(EntityFilter filter, out bool completeMismatch)
        {
            completeMismatch = false;
            return filter.StringFilter == null || filter.Culture.CompareInfo.IndexOf(Name, filter.StringFilter, CompareOptions.IgnoreCase) > 0;
        }
    }
}
