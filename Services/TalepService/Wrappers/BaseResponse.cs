namespace TalepService.Wrappers
{
    public class BaseResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string? ErrorCode { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }

        public BaseResponse()
        {
        }

        public BaseResponse(T data, string? message = null)
        {
            IsSuccess = true;
            Data = data;
            Message = message;
            ErrorCode = null;
        }

        public BaseResponse(string message)
        {
            IsSuccess = false;
            Message = message;
            Data = default;
        }

        public BaseResponse(string message, string errorCode)
        {
            IsSuccess = false;
            Message = message;
            ErrorCode = errorCode;
            Data = default;
        }

        public static BaseResponse<T> Success(T data, string? message = null)
        {
            return new BaseResponse<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                ErrorCode = null
            };
        }

        public static BaseResponse<T> Fail(string message, string? errorCode = null)
        {
            return new BaseResponse<T>
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = errorCode,
                Data = default
            };
        }
    }
}
