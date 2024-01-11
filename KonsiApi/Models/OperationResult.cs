namespace KonsiApi.Models
{
    public class OperationResult<T>
    {
        public T Value { get; set; }
        public bool Succeeded { get; set; }
        public string Error { get; set; }

        public OperationResult(T value, bool succeeded, string error)
        {
            Value = value;
            Succeeded = succeeded;
            Error = error;
        }

        public static OperationResult<T> Success(T value) => new OperationResult<T>(value, true, null);
        public static OperationResult<T> Failure(string error) => new OperationResult<T>(default(T), false, error);
    }

}
