using LLama.Common;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace LLama.Examples.Examples
{
    public class SemanticKernelChatEx
    {
        public static async Task Run()
        {
            Environment.SetEnvironmentVariable("LLAMA_LOG_LEVEL", "0");

            string modelPath = UserSettings.GetModelPath();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("This example is from: \n" +
                "https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/KernelSyntaxExamples/Example17_ChatGPT.cs");

            // Setup cancellation token
            using var cts = new CancellationTokenSource();
            Console.WriteLine("Press 'c' to cancel loading the model...");
            var cancelTask = Task.Run(() =>
            {
                if (Console.ReadKey(true).KeyChar == 'c')
                {
                    cts.Cancel();
                }
            });

            // Setup progress reporter
            var progressReporter = new Progress<float>(progress =>
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[Progress] {progress}");
                Console.ResetColor();
            });

            var parameters = new ModelParams(modelPath);
            using var model = await LLamaWeights.LoadFromFileAsync(parameters, cts.Token, progressReporter);
            var ex = new StatelessExecutor(model, parameters);

            IChatClient chatGPT = new LLamaSharpChatCompletion(ex);

            // Create a Semantic Kernel builder
            var builder = Kernel.CreateBuilder();

            // Add chat completion service to the builder
            builder.Services.AddSingleton(chatGPT);

            var kernel = builder.Build();

            // Use the kernel to get a chat completion
            var chat = kernel.GetRequiredService<IChatClient>();

            Console.WriteLine("");

            var msg = "How does Semantic Kernel work with Apertus?";
            Console.WriteLine("User: " + msg);

            var history = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.User, msg)
            };

            Console.WriteLine("AI: " + msg);

            await foreach (var chunk in chat.GetStreamingResponseAsync(history))
            {
                if (chunk != null && chunk.Text != null)
                    Spectre.Console.AnsiConsole.Write(chunk.Text);
            }
        }
    }
}


