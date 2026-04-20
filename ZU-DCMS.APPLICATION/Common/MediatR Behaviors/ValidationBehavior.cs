
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
                    // __ Map The Validation Error Message To The Result Pattern __ //
                    return (Result.Failure(failures.Select(x => x.ErrorMessage).First()) as TResponse)!;
                }
            }

            // __ If Validation Succeeds, Proceed To The Next Behavior Or The Actual Request Handler __ //
            return await next(cancellationToken);    
        }
    }
}