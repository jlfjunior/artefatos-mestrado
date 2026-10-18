namespace ControleLancamentos.Api.Dtos;

public record GerarTokenTesteResponse(
    string AccessToken,
    string TipoToken,
    DateTime ExpiraEmUtc);
