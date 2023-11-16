using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Sigma.Core.DataStorage
{
    public class EntityFilter
    {
        public String? StringFilter { get; }

        private CultureInfo? _cultureInfo;

        public EntityFilter()
        { 
        }
        public EntityFilter(string? stringFilter)
        {
            StringFilter = stringFilter;
        }

        public virtual bool IsEmpty()
        {
            return StringFilter == null;
        }

        [JsonIgnore]
        public CultureInfo Culture
        {
            get
            {
                if (_cultureInfo == null)
                {
                    _cultureInfo = CultureInfo.CreateSpecificCulture("uk-UA");
                }

                return _cultureInfo;
            }
        }
    }
}
