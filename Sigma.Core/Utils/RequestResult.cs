using HotelManager;
using System.Collections.Generic;

namespace Sigma.Core.Utils
{
    public enum ErrorCode
    {
        // Ошибки 1C
        // 1 - объект не найден по идентификатору
        // 2 - ошибка формата
        // 3 - не найден объект заданного типа
        // 4 - несоответствие типа
        UnableToFindObjectWithId = 1,
        FormatError = 2,
        ObjectTypeError = 3,
        TypeIncompatibility = 4,

        NotAuthorizied = 5,
        Unknown
    }

    public class Error
    {
        public ErrorCode errorCode;
        public string description;

        public Error(ErrorCode errorCode, string description)
        {
            this.errorCode = errorCode;
            this.description = description;
        }

        public Error(HotelManager.Error error)
        {
            errorCode = (ErrorCode)error.errorCode;
            description = error.errorDescription;
        }
    }

    public class RequestResult
    {
        List<Error>? errors;
        dynamic? data;

        public DocumentMapping[]? DocumentMappings { get; }

        public bool HasError
        {
            get
            {
                return errors != null && errors.Count > 0;
            }
        }

        public List<Error>? Errors
        {
            get
            {
                return errors;
            }
        }

        public dynamic? Data
        {
            get
            {
                return data;
            }
        }

        public RequestResult(Error? error, dynamic? data, bool val)
        {
            if (error != null)
            {
                errors = new List<Error>();
                errors.Add(error);
            }

            DocumentMappings = null;
            this.data = data;
        }

        public RequestResult(ErrorCode errorCode, string errorDescription) : this(new Error(errorCode, errorDescription), null, false)
        {

        }


        public RequestResult(HotelManager.Error[]? resultErrors, dynamic? data = null)
        {
            if (resultErrors != null && resultErrors.Length > 0)
            {
                errors = new List<Error>();

                foreach (HotelManager.Error resultError in resultErrors)
                {
                    errors.Add(new Error(resultError));
                }
            }

            DocumentMappings = null;
            this.data = data;
        }

        public RequestResult(HotelManager.Result result, dynamic? data = null)
        {
            if (result.errors != null && result.errors.Length > 0)
            {
                errors = new List<Error>();

                foreach (HotelManager.Error resultError in result.errors)
                {
                    errors.Add(new Error(resultError));
                }
            }

            DocumentMappings = result.documetsMapping;
            this.data = data;
        }

    }
}
