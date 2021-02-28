namespace Kracker.Base.Services.Model.Responses
{
    public record HashListResponse(long count, // -1 если ошибка
        string? error); // null если все хорошо 
}