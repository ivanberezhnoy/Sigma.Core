using System.Globalization;

namespace Sigma.Core.DataStorage
{
    public class EntityFilter
    {
        public String? StringFilter { get; }
        public CultureInfo? _cultureInfo;

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
