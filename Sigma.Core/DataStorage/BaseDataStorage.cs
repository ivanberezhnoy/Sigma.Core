namespace Sigma.Core.DataStorage
{
    public class BaseDataStorage
    {
        protected ILogger _logger;
        protected StorageProvider _storageProvider;

        protected BaseDataStorage(ILogger logger, StorageProvider storageProvider)
        {
            _logger = logger;
            _storageProvider = storageProvider;
        }
    }
}
