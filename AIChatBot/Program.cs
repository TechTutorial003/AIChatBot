using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.AI;
using System.ClientModel;


IChatClient chatClient = new ChatClient(
                         "gpt-4.1-mini",
                         new ApiKeyCredential("Insert your API key here "),
                         new OpenAIClientOptions
                         {
                             Endpoint = new Uri("https://models.github.ai/inference")
                         }
                        ).AsIChatClient();

AIAgent spanishAgent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
        Name = "Spanish Translator",
        Instructions = "You are a helpful assistant that translates English to Spanish.",
    }
);

AIAgent frenchAgent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
        Name = "French Translator",
        Instructions = "You are a helpful assistant that translates English to French.",
    }
);

string qualityReviewerAgentInstructions = """
You are a multilingual translation quality reviewer.
Check the translations for grammar accuracy, tone consistency, and cultural fit
compared to the original English text.

Give a brief summary with a quality rating (Excellent / Good / Needs Review).

Example output:
Quality: Excellent
Feedback: Accurate translation, friendly tone preserved, minor punctuation tweaks only.
""";

AIAgent qualityReviewerAgent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
      Name = "QualityReviewerAgent",
      Instructions = qualityReviewerAgentInstructions,
    });

string summaryAgentInstructions = """
You are a localization summary assistant.
Summarize the localization results below. For each language, list:
- Translation quality
- Tone feedback
- Any corrections made

Then, give an overall summary in 3-5 lines.

Example output:
=== Localization Summary ===

☑ French:
"Bienvenue dans notre application ! Veuillez vérifier votre adresse e-mail pour continuer."
Quality: Excellent
Feedback: Natural tone, no issues found.

☑ Spanish:
"Bienvenido a nuestra aplicación! Por favor, verifica tu correo electrónico para continuar."
Quality: Good
Feedback: Accurate translation, tone slightly formal.

Overall Summary:
Both translations reviewed successfully. No major issues detected.
""";

AIAgent summaryAgent = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
        Name = "SummaryAgent",
        Instructions = summaryAgentInstructions
    });

AIAgent workflowAgent = AgentWorkflowBuilder.BuildSequential(spanishAgent, frenchAgent,
                                qualityReviewerAgent, summaryAgent).AsAgent();

Console.WriteLine("\nEnter text to translate to Spanish and French:");

string inputText = Console.ReadLine() ?? string.Empty;

AgentRunResponse result = await workflowAgent.RunAsync(inputText);

foreach (var messages in result.Messages)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"\n--- {messages.AuthorName} ---");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(messages.Text);
}

