using FluentMigrator;

namespace Flow.Infra.Migrations.Versions
{
    [Migration(2, "Create table Users")]
    public class Version0000002 : Migration
    {
        public override void Down()
        {
            Delete.Table("Users");
        }

        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Login").AsString().NotNullable()
                .WithColumn("Password").AsString().NotNullable();
        }
    }
}
