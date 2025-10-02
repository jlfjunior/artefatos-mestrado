using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boxflux.Infra.Context
{
    public class BoxfluxContext : DbContext
    {
        public BoxfluxContext(DbContextOptions<BoxfluxContext> options)
                  : base(options) { }
        public DbSet<ConsolidatedDaily> ConslidatedDaylies { get; set; }
        public DbSet<Lauching> Lauchings { get; set; }

    }
}
