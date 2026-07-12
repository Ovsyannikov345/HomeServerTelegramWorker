using HomeServerTelegramWorker.Background;
using HomeServerTelegramWorker.Configuration;
using HomeServerTelegramWorker.Seerr;
using HomeServerTelegramWorker.Seerr.Handlers;
using HomeServerTelegramWorker.Telegram;
using HomeServerTelegramWorker.Telegram.CallbackQueryHandlers;
using HomeServerTelegramWorker.Telegram.CommandHandlers;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

var services = builder.Services;

var configuration = builder.Configuration;









var host = builder.Build();

await host.RunAsync();
