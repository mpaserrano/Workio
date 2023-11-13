using Workio.Services.Events;

namespace Workio.Services.BackgroundServices
{
    /// <summary>
    /// Gere a execução de tarefas em background
    /// </summary>
    public class PeriodicHostedService : BackgroundService
    {
        /// <summary>
        /// Gestor de logs do serviço.
        /// </summary>
        private readonly ILogger<PeriodicHostedService> _logger;
        /// <summary>
        /// Provedor de serviços.
        /// </summary>
        private readonly IServiceScopeFactory _factory;
        /// <summary>
        /// Intrevalo entre a execução de tarefas.
        /// </summary>
        private readonly TimeSpan _period = TimeSpan.FromHours(24);
        /// <summary>
        /// Número de execuções da atualização de eventos.
        /// </summary>
        private int _executionCount = 0;
        /// <summary>
        /// Tempo até a primeira atualização.
        /// </summary>
        private TimeSpan _timeLeft;
        /// <summary>
        /// Indica se as atualizações estão ativadas.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Inicializa o necessário ao correto funcionamento.
        /// </summary>
        /// <param name="logger">Gestor de logs.</param>
        /// <param name="factory">Provedor de serviços.</param>
        public PeriodicHostedService(ILogger<PeriodicHostedService> logger, IServiceScopeFactory factory)
        {
            DateTime now = DateTime.Now;
            DateTime endOfDay = now.Date.AddDays(1);
            _timeLeft = endOfDay - now;
            _logger = logger;
            _factory = factory;
        }

        /// <summary>
        /// Executa o serviço para alterar o estado dos eventos de forma a ssíncrona em uma outra thread.
        /// </summary>
        /// <returns>Tarefa assíncrona.</returns>
        private async Task ChangeEventsStateAsync()
        {
            Task.Run(async () =>
            {
                await using AsyncServiceScope asyncScope = _factory.CreateAsyncScope();
                IEventsService eventsService = asyncScope.ServiceProvider.GetService<IEventsService>();
                await eventsService.RefreshAllEventsState();
            });
        }

        /// <summary>
        /// Executa a ação de atualizar o estado dos eventos na primeira execução e depois uma vez por dia todos os dias
        /// à 00:00.
        /// </summary>
        /// <param name="stoppingToken">Token de paragem da operação.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        private async Task EventsStateUpdateShedule(CancellationToken stoppingToken)
        {
            if (_executionCount == 0)
            {
                ChangeEventsStateAsync();

                await Task.Delay((int)Math.Round(_timeLeft.TotalMilliseconds));
                _logger.LogInformation(
                            $"Server time: {DateTime.Now}.");

                ChangeEventsStateAsync();

                _executionCount++;
                _logger.LogInformation(
                    $"Executed Events PeriodicHostedService - Count: {_executionCount}");
            }

            using PeriodicTimer timer = new PeriodicTimer(_period);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    if (IsEnabled)
                    {
                        _logger.LogInformation(
                           $"Server time: {DateTime.Now}.");
                        ChangeEventsStateAsync();


                        _executionCount++;
                        _logger.LogInformation(
                            $"Executed Events PeriodicHostedService - Count: {_executionCount}");
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Skipped PeriodicHostedService");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(
                        $"Failed to execute PeriodicHostedService with exception message {ex.Message}. Good luck next round!");
                }
            }
        }

        /// <summary>
        /// Executa as tarefas a realizar de background.
        /// </summary>
        /// <param name="stoppingToken">Token de paragem.</param>
        /// <returns>Uma tarfa assíncrona.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await EventsStateUpdateShedule(stoppingToken);
        }
    }
}
