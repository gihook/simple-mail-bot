using MailKit;

public class ResponseResult
{
    public bool CanRespond { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string ResponseContent { get; set; } = string.Empty;
    public UniqueId UniqueId { get; set; }
}
