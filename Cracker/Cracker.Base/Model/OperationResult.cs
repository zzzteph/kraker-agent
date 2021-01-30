namespace Cracker.Base.Model
{
    public class OperationResult
    {
        protected OperationResult(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public string Error { get; }

        public static OperationResult Success => new(true, null);

        public static OperationResult Fail(string error)
        {
            return new(false, error);
        }
    }


    public class OperationResult<TResult> : OperationResult
    {
        private OperationResult(bool isSuccess, TResult result, string error) : base(isSuccess, error)
        {
            Result = result;
        }

        public TResult Result { get; }


        public static OperationResult<TResult> Success(TResult result)
        {
            return new(true, result, null);
        }

        public static OperationResult<TResult> Fail(string error)
        {
            return new(false, default, error);
        }
    }
}