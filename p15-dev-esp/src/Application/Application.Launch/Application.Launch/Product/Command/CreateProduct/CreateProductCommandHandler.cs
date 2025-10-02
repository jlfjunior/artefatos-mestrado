using Domain.Models;
using FluentValidation;
using MediatR;
using Services.Interfaces;

namespace Application.Launch.Product.Command.CreateProduct;
public class CreateProductCommandHandler(IProductService _service, IValidator<CreateProductCommand> _validator) : IRequestHandler<CreateProductCommand, ApiResponse>
{

    public async Task<ApiResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new ApiResponse { Success = false, Message = string.Join(";", validationResult.Errors.Select(e => e.ErrorMessage).ToList()) };
        }

        return await _service.CreateProduct(
           request.Name,
           request.Price,
           request.Stock
        );
    }
}