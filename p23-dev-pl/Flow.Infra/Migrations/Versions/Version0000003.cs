using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Infra.Migrations.Versions
{
    [Migration(3, "Create SeedUser")]
    public class Version0000003 : Migration
    {
        public override void Down()
        {
            throw new NotImplementedException();
        }

        public override void Up()
        {
            Insert.IntoTable("Users").Row(new
            {
                Id = Guid.NewGuid(),
                Login = "admin",
                Password = "admin"
            });
        }
    }
}
