using System.Text;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Chat;

public class ResponseGenerator
{
    private readonly string _apiKey;
    private readonly string _model;
    private List<QuestionWithAnswer> _questions;

    public ResponseGenerator(IConfiguration configuration)
    {
        _apiKey = configuration["ChatGpt:ApiKey"];
        _model = configuration["ChatGpt:Model"];
        _questions = configuration
            .GetSection("ChatGpt:Questions")
            .Get<List<QuestionWithAnswer>>();
    }

    public async Task<ResponseResult> GenerateResponse(
        EmailMessage emailMessage
    )
    {
        var openAiClient = new OpenAIClient(_apiKey);
        var chatClient = openAiClient.GetChatClient(_model);

        var tools = new List<ChatTool>();
        var options = new ChatCompletionOptions();

        var messageContent =
            $"Here is the subject: {emailMessage.Subject} and message body {emailMessage.Body}. Write a response.";
        var userMessage = new UserChatMessage(messageContent);

        var chatMessages = new List<ChatMessage>();
        chatMessages.Add(userMessage);

        var tool = GenerateTool(_questions);
        options.Tools.Add(tool);

        options.ToolChoice = ChatToolChoice.CreateFunctionChoice("respondMail");

        var completionResult = await chatClient.CompleteChatAsync(
            chatMessages,
            options
        );

        var result = JObject
            .Parse(
                completionResult.Value.ToolCalls[0].FunctionArguments.ToString()
            )
            .ToObject<ResponseResult>();

        return result;
    }

    private ChatTool GenerateTool(IEnumerable<QuestionWithAnswer> questions)
    {
        var description = GenerateToolPrompt(questions);

        var functionDefinition = new
        {
            type = "object",
            properties = new
            {
                canRespond = new
                {
                    type = "boolean",
                    description = "indicates wether chat is able to respond based on system messages content",
                },
                subject = new
                {
                    type = "string",
                    description = "general subject of conversation",
                    @enum = new[]
                    {
                        "delegati",
                        "nastavnici",
                        "donori",
                        "ostalo",
                    },
                },
                responseContent = new { type = "string" },
            },
            required = new[] { "canRespond", "responseContent", "subject" },
            additionalProperties = false,
        };

        var parameters = BinaryData.FromObjectAsJson(functionDefinition);
        var tool = ChatTool.CreateFunctionTool(
            "respondMail",
            description,
            parameters,
            true
        );

        return tool;
    }

    private string GenerateToolPrompt(IEnumerable<QuestionWithAnswer> questions)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(
            "Here is the list of questions that you should answer to. Respond only if you can based on these examples."
        );
        stringBuilder.AppendLine();

        foreach (var item in questions)
        {
            stringBuilder.AppendLine($"question: {item.Question}");
            stringBuilder.AppendLine($"answer: {item.Answer}");
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine(
            "Write response into 'responseContent' property"
        );
        stringBuilder.AppendLine(
            "If you are not able to respond set 'canRespond' to false and leave 'responseContent' empty"
        );

        return stringBuilder.ToString();
    }
}
