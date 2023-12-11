namespace Sigma.Core.Utils
{
    public enum ErrorCode
    { 
        UnableToFindObjectWithId = 1,
        NotAuthorizied,
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
    }
    public class RequestResult
    {
        public Error? error;
        public dynamic? data;

        public bool HasError
        {
            get
            {
                return error != null;
            }
        }

        public RequestResult(Error? error, dynamic? data)
        {
            this.error = error;
            this.data = data;
        }

        public RequestResult(ErrorCode errorCode, string errorDescription) : this(new Error(errorCode, errorDescription), null)
        { 

        }

        public RequestResult(HotelManager.Result requestResult)
        {
            if (requestResult.errorCode.HasValue && requestResult.errorCode != 0)
            {
                ErrorCode errorCode = ErrorCode.Unknown;
                switch (requestResult.errorCode.Value)
                {
                    case 1:
                        {
                            errorCode = ErrorCode.UnableToFindObjectWithId;
                            break;
                        }
                }

                error = new Error(errorCode, requestResult.errorDescription);
            }
            else
            {
                data = "OK";
            }
        }
        
    }
}
