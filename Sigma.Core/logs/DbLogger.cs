using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;

namespace Sigma.Core.logs
{
    public class DbLogger : ILogger
    {
        private readonly DbLoggerProvider _dbLoggerProvider;

        private readonly MySqlConnection _sqlConnection;
        private readonly MySqlCommand _sqlCommand;

        // Потокобезопасность
        private readonly object _syncRoot = new object();

        public DbLogger([NotNull] DbLoggerProvider dbLoggerProvider)
        {
            _dbLoggerProvider = dbLoggerProvider;

            _sqlConnection = new MySqlConnection(_dbLoggerProvider.Options.ConnectionString);
            _sqlConnection.Open();

            // Готовим команду
            _sqlCommand = new MySqlCommand();
            _sqlCommand.Connection = _sqlConnection;
            _sqlCommand.CommandType = System.Data.CommandType.Text;

            // Указываем список колонок!
            _sqlCommand.CommandText = $@"
                        INSERT INTO `{_dbLoggerProvider.Options.LogTable}`
                        (`Values`, `Created`)
                        VALUES (@Values, @Created);
                        ";

            // Создаём параметры 1 раз
            _sqlCommand.Parameters.Add("@Values", MySqlDbType.LongText);
            _sqlCommand.Parameters.Add("@Created", MySqlDbType.DateTime);
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var threadId = Thread.CurrentThread.ManagedThreadId;
            var values = new JObject();

            // Заполняем json
            if (_dbLoggerProvider?.Options?.LogFields?.Any() ?? false)
            {
                foreach (var field in _dbLoggerProvider.Options.LogFields)
                {
                    switch (field)
                    {
                        case "LogLevel":
                            values["LogLevel"] = logLevel.ToString();
                            break;

                        case "ThreadId":
                            values["ThreadId"] = threadId;
                            break;

                        case "EventId":
                            values["EventId"] = eventId.Id;
                            break;

                        case "EventName":
                            if (!string.IsNullOrWhiteSpace(eventId.Name))
                                values["EventName"] = eventId.Name;
                            break;

                        case "Message":
                            var msg = formatter(state, exception);
                            if (!string.IsNullOrWhiteSpace(msg))
                                values["Message"] = msg;
                            break;

                        case "ExceptionMessage":
                            if (exception?.Message != null)
                                values["ExceptionMessage"] = exception.Message;
                            break;

                        case "ExceptionStackTrace":
                            if (!string.IsNullOrWhiteSpace(exception?.StackTrace))
                                values["ExceptionStackTrace"] = exception.StackTrace;
                            break;

                        case "ExceptionSource":
                            if (!string.IsNullOrWhiteSpace(exception?.Source))
                                values["ExceptionSource"] = exception.Source;
                            break;

                        case "TextMessage":
                            if (state != null)
                                values["Message"] = state.ToString();
                            break;
                    }
                }
            }

            var jsonString = JsonConvert.SerializeObject(values, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.None
            });

            try
            {
                lock (_syncRoot)
                {
                    // Проверяем соединение
                    if (_sqlConnection.State != System.Data.ConnectionState.Open)
                        _sqlConnection.Open();

                    _sqlCommand.Parameters["@Values"].Value = jsonString;
                    _sqlCommand.Parameters["@Created"].Value = DateTime.UtcNow;

                    _sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Никогда не выбрасываем наружу — лог должен быть безопасным
                System.Diagnostics.Debug.WriteLine("DbLogger error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }
    }
}
