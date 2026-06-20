using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Services;
using Hangman.Contracts.Contracts;
using Hangman.Contracts.Match;
using HangmanGame.Services.ExceptionHandling;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HangmanGame.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class ScoreService : IScoreService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScoreService));

        private readonly IScoreBusiness scoreBusiness;

        public ScoreService()
            : this(new ScoreBusiness(new UnitOfWorkFactory()))
        {
        }

        internal ScoreService(IScoreBusiness scoreBusiness)
        {
            this.scoreBusiness = scoreBusiness ?? throw new ArgumentNullException(nameof(scoreBusiness));
        }

        public async Task<GetPlayerScoreResponse> GetPlayerScoreAsync(GetPlayerScoreRequest request)
        {
            try
            {
                return await scoreBusiness.GetPlayerScoreAsync(request);
            }
            catch (Exception exception)
            {
                ScoreMessageCode messageCode = ServiceExceptionMapper.MapScore(exception);

                Log.Error(
                    string.Format("Error executing GetPlayerScoreAsync. MessageCode: {0}. PlayerId: {1}",
                    messageCode,
                    request != null ? request.PlayerId : 0),
                    exception);

                return new GetPlayerScoreResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString()
                };
            }
        }
    }
}
