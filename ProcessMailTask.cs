public class ProcessMailTask
{
    private readonly MessageProcessor _messageProcessor;

    public ProcessMailTask(MessageProcessor messageProcessor)
    {
        _messageProcessor = messageProcessor;
    }

    public async Task Process()
    {
        await _messageProcessor.ProcessUnreadMessages();
    }
}
