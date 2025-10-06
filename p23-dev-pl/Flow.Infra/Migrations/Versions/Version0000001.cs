using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Infra.Migrations.Versions
{
    [Migration(1, "Create table Transaction")]
    public class Version0000001 : Migration
    {
        public override void Down()
        {
            Delete.Table("Transactions");
        }

        public override void Up()
        {
            Create.Table("Transactions")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Value").AsDecimal().NotNullable()
                .WithColumn("Type").AsByte().NotNullable()
                .WithColumn("Date").AsDateTime().NotNullable();
        }
    }
}
