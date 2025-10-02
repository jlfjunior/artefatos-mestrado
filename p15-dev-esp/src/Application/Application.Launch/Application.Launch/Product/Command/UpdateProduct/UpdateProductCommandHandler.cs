using Domain.Models;
using FluentValidation;
using MediatR;
using Services.Interfaces;

namespace Application.Launch.Product.Command.UpdateProduct;
public class UpdateProductCommandHandler(IProductService _service, IValidator<UpdateProductCommand> _validator) : IRequestHandler<UpdateProductCommand, ApiResponse>
{

    public async Task<ApiResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new ApiResponse { Success = false, Message = string.Join(";", validationResult.Errors.Select(e => e.ErrorMessage).ToList()) };
        }

        return await _service.UpdateProduct(
           request.Id,
           request.Name,
           request.Price,
           request.Stock
        );
    }
}