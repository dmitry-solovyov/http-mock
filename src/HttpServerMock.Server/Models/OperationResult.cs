using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HttpServerMock.Server.Models
{
    public abstract class OperationResult<T>
    {
        protected OperationResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; }
        public abstract bool IsNoResult { get; }

        public static OperationResult<T> Success(T result)
        {
            return new SuccessOperationResult<T>(true, result);
        }
        public static OperationResult<T> Failed(IEnumerable<Error> errors)
        {
            return new FailedOperationResult<T>(errors);
        }
        public static OperationResult<T> Failed(params Error[] errors)
        {
            return new FailedOperationResult<T>(errors);
        }
    }

    public class SuccessOperationResult<T> : OperationResult<T>
    {
        public SuccessOperationResult(bool isSuccess, T result)
            : base(isSuccess)
        {
            Result = result;
        }

        public T Result { get; }

        public override bool IsNoResult => false;
    }

    public class FailedOperationResult<T> : OperationResult<T>
    {
        public FailedOperationResult(IEnumerable<Error> errors)
            : base(false)
        {
            Errors = new ReadOnlyCollection<Error>(errors.ToList());
        }

        public IReadOnlyCollection<Error> Errors { get; }

        public override bool IsNoResult => !Errors.Any();
    }
}
