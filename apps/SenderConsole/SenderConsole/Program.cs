using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using SenderConsole.Core.Services;
using SenderConsole.Core.Services.Interfaces;

namespace SenderConsole
{
    class Program
    {

        private static ChallengeService _challengeService;
        private static DocumentService _documentService;

        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            
            serviceCollection.TryAddTransient<IStorageService, LocalStorageService>();
            serviceCollection.TryAddTransient(x => new HttpClient());
            serviceCollection.TryAddTransient<ChallengeService>();
            serviceCollection.TryAddTransient<DocumentService>();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            _challengeService = serviceProvider.GetService<ChallengeService>();
            _documentService = serviceProvider.GetService<DocumentService>();
            
            var token = string.Empty;
            var signedMessage = string.Empty;

            while (true)
            {
                Console.WriteLine("Choose an option. (1) Get challenge, (2) Post document, (3) clear challenge tokens, or (q) to quit");
                var response = Console.ReadLine();
                switch (response)
                {
                    case "1":
                        var challenge = await _challengeService!.GetChallenge();
                        var message = challenge.Message;
                        token = challenge.Token;
                        signedMessage = await _challengeService!.SignMessageWithPrivateKey(message);
                        break;
                    case "2":
                        Console.WriteLine("Posting document");
                        var isSuccess = await _documentService!.SendDocument(signedMessage, token, "hello.txt", "client");
                        Console.WriteLine(isSuccess ? "Document posted successfully" : "Document failed to post");
                        break;
                    case "3":
                        token = null;
                        signedMessage = null;
                        Console.WriteLine("Tokens cleared successfully.");
                        break;
                    case "q":
                    case "Q":
                        Console.WriteLine("Goodbye");
                        return;
                    default:
                        Console.WriteLine("Unrecognized command. Try again.");
                        break;
                }
            }
            
            // var challenge = await _challengeService.GetChallenge();
            
        }
    }
}