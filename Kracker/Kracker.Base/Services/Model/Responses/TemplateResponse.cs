namespace Kracker.Base.Services.Model.Responses
{
    public record TemplateResponse(long keyspace, // -1 если ошибка
        string? error); // null если все хорошо 
}