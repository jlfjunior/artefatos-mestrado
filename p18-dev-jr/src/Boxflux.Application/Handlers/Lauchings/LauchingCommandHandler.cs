using Boxflux.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Boxflux.Application.Commands.Lauchings.LauchingCommand;

namespace Boxflux.Application.Handlers.Lauchings
{
    public class LauchingCommandHandler :
        IRequestHandler<CreateLauchingCommand, bool>,
        IRequestHandler<GetAllLauchingQuery, IEnumerable<Lauching>>

    {
        private readonly IGeralRepository<Lauching> _lauchingRepository;
        public LauchingCommandHandler(IGeralRepository<Lauching> lauchingRepository)
        {
            _lauchingRepository = lauchingRepository;
        }

        public async Task<bool> Handle(CreateLauchingCommand request, CancellationToken cancellationToken)
        {
            var normalizated = request.Type.ToLower();

            if (normalizated != "credito" && normalizated!= "debito")
            {
                throw new ArgumentException("O tipo deve ser 'credito' ou 'debito'.");
            }

            var lauch = new Lauching
            {
                Type = request.Type,
                Value = request.Value,
                DateLauching = DateTime.UtcNow,
            };
            await _lauchingRepository.CreateAsync(lauch);
            return true;
        }

        public async Task<IEnumerable<Lauching>> Handle(GetAllLauchingQuery request, CancellationToken cancellationToken)
        {
            return await _lauchingRepository.GetAllAsync();
        }

    }
}
