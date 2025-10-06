using System;

namespace FluxoDeCaixa.Caixa.Domain.Repositories.Base
{
    public class Entity
    {
        public Guid Id { get; set; }

        public Entity()
        {
            Id = Guid.NewGuid();
        }
    }
}
