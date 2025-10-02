using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Boxflux.Application.Commands.Lauchings
{
    public class LauchingCommand
    {
        public class CreateLauchingCommand : IRequest<bool>
        {
            [Required(ErrorMessage = " O campo Tipo é obrigatório ")]
            public string Type { get; set; }

            [Required(ErrorMessage = " O campo Valor é obrigatório ")]
            public decimal Value { get; set; }
        }
        public class GetAllLauchingQuery : IRequest<IEnumerable<Lauching>>
        {

        }

    }
}
