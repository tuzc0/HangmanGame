using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Validators;
using Hangman.Contracts.Word;
using Hangman.DataAccess.Transporters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class WordBusiness : IWordBusiness
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public WordBusiness(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<GetCategoriesByLanguageResponse> GetCategoriesByLanguageAsync(
            GetCategoriesByLanguageRequest request)
        {
            WordMessageCode? validationResult =
                WordValidator.ValidateGetCategoriesByLanguage(request);

            if (validationResult.HasValue)
            {
                return BuildCategoriesResponse(
                    false,
                    validationResult.Value,
                    new List<CategoryDto>());
            }

            string languageCode = request.LanguageCode.Trim().ToLowerInvariant();

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                List<CategoryTransporter> categories =
                    await unitOfWork.Words.GetActiveCategoriesByLanguageAsync(languageCode);

                if (categories == null || categories.Count == 0)
                {
                    return BuildCategoriesResponse(
                        false,
                        WordMessageCode.NoCategoriesFound,
                        new List<CategoryDto>());
                }

                List<CategoryDto> categoryDtos = categories
                    .Select(BuildCategoryDto)
                    .ToList();

                return BuildCategoriesResponse(
                    true,
                    WordMessageCode.CategoriesRetrieved,
                    categoryDtos);
            }
        }

        private static CategoryDto BuildCategoryDto(CategoryTransporter category)
        {
            if (category == null)
            {
                return null;
            }

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                LanguageCode = category.LanguageCode
            };
        }

        private static GetCategoriesByLanguageResponse BuildCategoriesResponse(
            bool success,
            Enum messageCode,
            List<CategoryDto> categories)
        {
            return new GetCategoriesByLanguageResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Categories = categories
            };
        }
    }
}
