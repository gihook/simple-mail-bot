using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;

public class MessageProcessor
{
    private readonly string _email;
    private readonly string _emailName;
    private readonly string _password;
    private readonly string _imap;
    private readonly string _smtp;
    private readonly ResponseGenerator _responseGenerator;
    private readonly ILogger<MessageProcessor> _logger;

    public MessageProcessor(
        IConfiguration configuration,
        ResponseGenerator responseGenerator,
        ILogger<MessageProcessor> logger
    )
    {
        _email = configuration["Mail:Email"];
        _emailName = configuration["Mail:Name"];
        _password = configuration["Mail:Password"];
        _imap = configuration["Mail:Imap"];
        _smtp = configuration["Mail:Smtp"];
        _responseGenerator = responseGenerator;
        _logger = logger;
    }

    public async Task ProcessUnreadMessages(DateTime sinceDate)
    {
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
            _logger.LogInformation(
                $"Subject: {message.Subject}, Date: {message.Date}"
            );
            await TryToRespond(uid, message);
            inbox.AddFlags(uid, MessageFlags.Seen, true);
        }

        client.Disconnect(true);
    }

    private async Task TryToRespond(UniqueId messageId, MimeMessage message)
    {
        var responseResult = await GenerateResponse(message);
        var messageContent = responseResult.ResponseContent;

        if (!responseResult.CanRespond)
            throw new ArgumentException(
                "Cannot respond for message: " + message.Subject
            );

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

        reply.Body = bodyBuilder.ToMessageBody();

        using var smtpClient = new SmtpClient();
        await smtpClient.ConnectAsync(_smtp, 587, false);
        await smtpClient.AuthenticateAsync(_email, _password);
        await smtpClient.SendAsync(reply);
        await smtpClient.DisconnectAsync(true);
    }

    private async Task<ResponseResult> GenerateResponse(MimeMessage message)
    {
        var bodyContent = message.GetTextBody(MimeKit.Text.TextFormat.Html);
        return await _responseGenerator.GenerateResponse(
            new EmailMessage { Subject = message.Subject, Body = bodyContent }
        );
    }
}
