using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Services;
using Hangman.Contracts.Contracts;
using Hangman.Contracts.Word;
using HangmanGame.Services.ExceptionHandling;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HangmanGame.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WordService : IWordService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WordService));

        private readonly IWordBusiness wordBusiness;

        public WordService()
            : this(new WordBusiness(new UnitOfWorkFactory()))
        {
        }

        internal WordService(IWordBusiness wordBusiness)
        {
            this.wordBusiness = wordBusiness ??
                throw new ArgumentNullException(nameof(wordBusiness));
        }

        public async Task<GetCategoriesByLanguageResponse> GetCategoriesByLanguageAsync(
            GetCategoriesByLanguageRequest request)
        {
            try
            {
                return await wordBusiness.GetCategoriesByLanguageAsync(request);
            }
            catch (Exception exception)
            {
                WordMessageCode messageCode = ServiceExceptionMapper.MapWord(exception);

                Log.Error(
                    string.Format("Error executing GetCategoriesByLanguageAsync. MessageCode: {0}. LanguageCode: {1}",
                    messageCode,
                    request != null ? request.LanguageCode : string.Empty),
                    exception);

                return new GetCategoriesByLanguageResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    Categories = null
                };
            }
        }
    }
}