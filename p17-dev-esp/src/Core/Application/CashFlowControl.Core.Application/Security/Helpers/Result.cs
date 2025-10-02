using Flunt.Notifications;

namespace CashFlowControl.Core.Application.Security.Helpers
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public string? SystemError { get; private set; }
        public T? Value { get; private set; }
        public IReadOnlyCollection<Notification>? ValidationErrors { get; private set; }

        private Result(bool isSuccess, T? value = default(T), string? systemError = null, IReadOnlyCollection<Notification>? validationErrors = null)
        {
            IsSuccess = isSuccess;
            SystemError = systemError;
            Value = value;
            ValidationErrors = validationErrors ?? new List<Notification>();
        }

        // Sucesso
        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value);
        }

        // Falha sistêmica
        public static Result<T> Failure(string systemError)
        {
            return new Result<T>(false, default(T), systemError);
        }

        public static Result<T> ValidationFailure(string key, string mensagem)
        {
            IReadOnlyCollection<Notification> validationErrors = new List<Notification>()
            {
                new Notification() {
                    Key = key,
                    Message = mensagem }
            };
            return new Result<T>(false, default(T), validationErrors: validationErrors);
        }

        public static Result<T> ValidationFailure(string mensagem)
        {
            IReadOnlyCollection<Notification> validationErrors = new List<Notification>()
            {
                new Notification() {
                    Key = "aviso",
                    Message = mensagem }
            };
            return new Result<T>(false, default(T), validationErrors: validationErrors);
        }

        // Falha de validação
        public static Result<T> ValidationFailure(IReadOnlyCollection<Notification> validationErrors)
        {
            return new Result<T>(false, default(T), validationErrors: validationErrors);
        }

        // Verifica se há erros de validação
        public bool HasValidationErrors => ValidationErrors != null && ValidationErrors.Count > 0;
    }


}
