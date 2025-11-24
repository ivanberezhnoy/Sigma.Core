using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Google.Protobuf.WellKnownTypes;

namespace Sigma.Core.logs
{
    public class DbLogger : ILogger
    {
        /// <summary>
        /// Instance of <see cref="DbLoggerProvider" />.
        /// </summary>
        private readonly DbLoggerProvider _dbLoggerProvider;
        private readonly MySqlConnection _sqlConnection;
        private readonly MySqlCommand _mySQLCommand;

        /// <summary>
        /// Creates a new instance of <see cref="FileLogger" />.
        /// </summary>
        /// <param name="fileLoggerProvider">Instance of <see cref="FileLoggerProvider" />.</param>
        public DbLogger([NotNull] DbLoggerProvider dbLoggerProvider)
        {
            _dbLoggerProvider = dbLoggerProvider;

            _sqlConnection = new MySqlConnection(_dbLoggerProvider.Options.ConnectionString);
            _sqlConnection.Open();

            _mySQLCommand = new MySqlCommand();
            _mySQLCommand.Connection = _sqlConnection;
            _mySQLCommand.CommandType = System.Data.CommandType.Text;
            _mySQLCommand.CommandText = string.Format("INSERT INTO {0} VALUES (@Values, @Created)", _dbLoggerProvider.Options.LogTable);

            _mySQLCommand.Parameters.Add("@Values", MySqlDbType.VarChar);
            _mySQLCommand.Parameters.Add("@Created", MySqlDbType.DateTime);
        }

        ~DbLogger()
        {
            _sqlConnection.Close();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <summary>
        /// Whether to log the entry.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }


        /// <summary>
        /// Used to log the entry.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel">An instance of <see cref="LogLevel"/>.</param>
        /// <param name="eventId">The event's ID. An instance of <see cref="EventId"/>.</param>
        /// <param name="state">The event's state.</param>
        /// <param name="exception">The event's exception. An instance of <see cref="Exception" /></param>
        /// <param name="formatter">A delegate that formats </param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                // Don't log the entry if it's not enabled.
                return;
            }

            var threadId = Thread.CurrentThread.ManagedThreadId; // Get the current thread ID to use in the log file. 

            var values = new JObject();

            if (_dbLoggerProvider?.Options?.LogFields?.Any() ?? false)
            {
                foreach (var logField in _dbLoggerProvider.Options.LogFields)
                {
                    switch (logField)
                    {
                        case "LogLevel":
                            if (!string.IsNullOrWhiteSpace(logLevel.ToString()))
                            {
                                values["LogLevel"] = logLevel.ToString();
                            }
                            break;
                        case "ThreadId":
                            values["ThreadId"] = threadId;
                            break;
                        case "EventId":
                            values["EventId"] = eventId.Id;
                            break;
                        case "EventName":
                            if (!string.IsNullOrWhiteSpace(eventId.Name))
                            {
                                values["EventName"] = eventId.Name;
                            }
                            break;
                        case "Message":
                            if (!string.IsNullOrWhiteSpace(formatter(state, exception)))
                            {
                                values["Message"] = formatter(state, exception);
                            }
                            break;
                        case "ExceptionMessage":
                            if (exception != null && !string.IsNullOrWhiteSpace(exception.Message))
                            {
                                values["ExceptionMessage"] = exception?.Message;
                            }
                            break;
                        case "ExceptionStackTrace":
                            if (exception != null && !string.IsNullOrWhiteSpace(exception.StackTrace))
                            {
                                values["ExceptionStackTrace"] = exception?.StackTrace;
                            }
                            break;
                        case "ExceptionSource":
                            if (exception != null && !string.IsNullOrWhiteSpace(exception.Source))
                            {
                                values["ExceptionSource"] = exception?.Source;
                            }
                            break;
                        case "TextMessage":
                            if (state != null)
                            {
                                values["Messge"] = state.ToString();
                            }
                            break;
                    }
                }
            }


            _mySQLCommand.Parameters[0] = new MySqlParameter("@Values", JsonConvert.SerializeObject(values, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.None
            }).ToString());

            _mySQLCommand.Parameters[1] = new MySqlParameter("@Created", DateTimeOffset.Now);

            _mySQLCommand.ExecuteNonQuery();
        }
    }
}
