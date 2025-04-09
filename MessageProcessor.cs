using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

public class MessageProcessor
{
    private readonly string _email;
    private readonly string _emailName;
    private readonly string _password;
    private readonly string _imap;
    private readonly string _smtp;
    private readonly ResponseGenerator _responseGenerator;
    private readonly ApplicationDbContext _dbContext;
    private readonly TimeService _timeService;

    public MessageProcessor(
        IConfiguration configuration,
        ResponseGenerator responseGenerator,
        ApplicationDbContext dbContext,
        TimeService timeService
    )
    {
        _email = configuration["Mail:Email"];
        _emailName = configuration["Mail:Name"];
        _password = configuration["Mail:Password"];
        _imap = configuration["Mail:Imap"];
        _smtp = configuration["Mail:Smtp"];
        _responseGenerator = responseGenerator;
        _dbContext = dbContext;
        _timeService = timeService;
    }

    public async Task ProcessUnreadMessages()
    {
        var sinceDate = await GetProcessingStartTime();

        using var client = new ImapClient();
        await client.ConnectAsync(_imap, 993, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(_email, _password);

        var inbox = client.Inbox;
        inbox.Open(FolderAccess.ReadWrite);

        var query = SearchQuery.NotSeen.And(
            SearchQuery.DeliveredAfter(sinceDate)
        );
        var uids = inbox.Search(query);

        foreach (var uid in uids)
        {
            var message = inbox.GetMessage(uid);
            var response = await TryToRespond(uid, message);
            await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true);
            await SaveResult(uid, message, response);
        }

        client.Disconnect(true);
    }

    private async Task<DateTime> GetProcessingStartTime()
    {
        var serverStartTime = _timeService.GetStartTimestamp();

        var lastProcessedMessage = await _dbContext
            .ProcessedEmails.OrderByDescending(x => x.Timestamp)
            .LastOrDefaultAsync();

        if (lastProcessedMessage == null)
            return serverStartTime;

        return serverStartTime > lastProcessedMessage.Timestamp
            ? serverStartTime
            : lastProcessedMessage.Timestamp;
    }

    private async Task SaveResult(
        UniqueId uid,
        MimeMessage message,
        string response
    )
    {
        var processedEmail = new ProcessedEmail
        {
            Id = uid.ToString(),
            Subject = message.Subject,
            Body = message.HtmlBody,
            Timestamp = DateTime.UtcNow,
            Response = response,
            Status = ProcessingStatus.Done,
        };
        _dbContext.ProcessedEmails.Add(processedEmail);
        await _dbContext.SaveChangesAsync();
    }

    private async Task<string> TryToRespond(
        UniqueId messageId,
        MimeMessage message
    )
    {
        var responseResult = await GenerateResponse(message);
        var messageContent = responseResult.ResponseContent;

        if (!responseResult.CanRespond)
            return "Cannot respond for message: " + message.Body;

        var reply = new MimeMessage();
        reply.From.Add(new MailboxAddress(_emailName, _email));
        reply.To.AddRange(
            message.ReplyTo.Count > 0 ? message.ReplyTo : message.From
        );
        reply.Subject = "Re: " + message.Subject;

        var bodyBuilder = new BodyBuilder();
        var response = responseResult.ResponseContent;

        bodyBuilder.HtmlBody =
            $"<p>{response}</p>"
            + $"<p></p>"
            + $"<p>Ovu poruku je generisao MailBot.</p>"
            + $"<p></p>"
            + "<p>----- Original Message -----</p>"
            + message.HtmlBody;

        var responseMesage = bodyBuilder.ToMessageBody();
        reply.Body = responseMesage;

        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync(_smtp, 587, false);
        await smtpClient.AuthenticateAsync(_email, _password);
        await smtpClient.SendAsync(reply);
        await smtpClient.DisconnectAsync(true);

        return response;
    }

    private async Task<ResponseResult> GenerateResponse(MimeMessage message)
    {
        var bodyContent = message.GetTextBody(MimeKit.Text.TextFormat.Html);
        return await _responseGenerator.GenerateResponse(
            new EmailMessage { Subject = message.Subject, Body = bodyContent }
        );
    }
}
