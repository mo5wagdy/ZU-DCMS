
using FluentValidation;
using MediatR;

namespace ZU_DCMS.APPLICATION.Common.MediatR_Behaviors
{
    // __ Generic Validation Behavior For MediatR Pipelines __ //
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse> where TResponse : Result
    {

        // __ Holds All Validators Found For The Specifc TRequest __ //
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // __ Check If There Are Any Validators Registered For This Request __ // 
            if (_validators.Any())
            {
                // __ Create a Validation Context For The Current Request __ //
                var context = new ValidationContext<TRequest>(request);

                // __ Excute All Validators And Collect All Non Null Validation Errors __ //
                var failures = _validators.Select(v => v.Validate(context))
                                          .SelectMany(r => r.Errors)
                                          .Where(r => r != null)
                                          .ToList();

                // __ If Validation Fails, Stop The Pipeline And Return a Failure Result __ //
                if (failures.Count != 0)
                {
                    // __ Collect All Validation Error Messages __ //
                    var errorMessages = failures.Select(x => x.ErrorMessage).ToList();

                    // __ Handle Result<T> responses __ //
                    if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                    {
                        var resultType = typeof(TResponse).GetGenericArguments()[0];
                        var failureMethod = typeof(Result).GetMethods()
                            .First(m => m.Name == "Failure" && 
                                        m.IsGenericMethod && 
                                        m.GetParameters().Length == 1 && 
                                        m.GetParameters()[0].ParameterType == typeof(IEnumerable<string>))
                            .MakeGenericMethod(resultType);

                        return (TResponse)failureMethod.Invoke(null, new object[] { errorMessages })!;
                    }

                    // __ Handle base Result responses __ //
                    return (TResponse)(object)Result.Failure(errorMessages);
                }
            }

            // __ If Validation Succeeds, Proceed To The Next Behavior Or The Actual Request Handler __ //
            return await next(cancellationToken);    
        }
    }
}