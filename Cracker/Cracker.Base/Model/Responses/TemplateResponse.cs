namespace Cracker.Base.Model.Responses
{
    public record TemplateResponse(long keyspace, // -1 если ошибка
        string error); // null если все хорошо 
}