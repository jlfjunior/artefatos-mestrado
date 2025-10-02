using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Cashflow.Transactions.Api.Bootstrap
{
    public static class SwaggerConfiguration
    {
        public static OpenApiSecurityScheme Scheme => new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            In = ParameterLocation.Header,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJh...\"",
            Reference = new OpenApiReference
            {
                Id = "Bearer",
                Type = ReferenceType.SecurityScheme,
            },


        };

        public static void Configure(SwaggerGenOptions option)
        {
            option.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Cashflow Transactions Api",
                Version = "v1",
                Description = "Service for handling transactions such as income and expense entries. APIs to create, update, and list/details transactions",
                Contact = new OpenApiContact
                {
                    Name = "Renan Alves Medina",
                    Url = new Uri("https://github.com/renanalvesmedina"),
                }
            });

            //option.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    {
            //        new OpenApiSecurityScheme
            //        {
            //            Reference = new OpenApiReference
            //            {
            //                Type = ReferenceType.SecurityScheme,
            //                Id = "Bearer"
            //            }
            //        },
            //        Array.Empty<string>()
            //    }
            //});

            option.ResolveConflictingActions(apiDesc => apiDesc.First());
            //option.AddSecurityDefinition(Scheme.Reference.Id, Scheme);

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            option.IncludeXmlComments(xmlPath);
        }
    }
}
