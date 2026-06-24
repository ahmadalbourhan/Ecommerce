namespace EcommerceAPI.DTOs
{
    public class ResponseDto<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public bool Success { get; set; }

        public ResponseDto(int statusCode, string message, T? data = default, bool success = true)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Success = success;
        }
    }

    public class ResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }

        public ResponseDto(int statusCode, string message, bool success = true)
        {
            StatusCode = statusCode;
            Message = message;
            Success = success;
        }
    }
}
