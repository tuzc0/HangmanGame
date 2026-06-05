using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class AuthBusiness : IAuthBusiness
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public AuthBusiness(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                return await unitOfWork.Accounts.EmailExistsAsync(email);
            }
        }
    }
}
