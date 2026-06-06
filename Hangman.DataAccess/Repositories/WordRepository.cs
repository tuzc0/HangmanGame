using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Model;
using Hangman.DataAccess.Transporters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Repositories
{
    public class WordRepository : IWordRepository
    {
        private readonly HangmanDBEntities context;

        private static readonly Expression<Func<WORD, WordTransporter>> WordProjection =
            word => new WordTransporter
            {
                WordId = word.word_id,
                CategoryId = word.category_id,
                CategoryName = word.CATEGORY.name,
                WordText = word.word_text,
                Description = word.description,
                LanguageCode = word.language_code,
                IsActive = word.is_active,
                CreatedAt = word.created_at
            };

        private static readonly Expression<Func<CATEGORY, CategoryTransporter>> CategoryProjection =
            category => new CategoryTransporter
            {
                CategoryId = category.category_id,
                Name = category.name,
                LanguageCode = category.language_code,
                IsActive = category.is_active
            };

        public WordRepository(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<WordTransporter> GetByIdAsync(int wordId)
        {
            return await context.WORDs
                .AsNoTracking()
                .Where(word => word.word_id == wordId)
                .Select(WordProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<WordTransporter> GetActiveByIdAsync(int wordId)
        {
            return await context.WORDs
                .AsNoTracking()
                .Where(word =>
                    word.word_id == wordId &&
                    word.is_active &&
                    word.CATEGORY.is_active)
                .Select(WordProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WordTransporter>> GetActiveByLanguageAsync(string languageCode)
        {
            return await context.WORDs
                .AsNoTracking()
                .Where(word =>
                    word.language_code == languageCode &&
                    word.is_active &&
                    word.CATEGORY.is_active)
                .OrderBy(word => word.CATEGORY.name)
                .ThenBy(word => word.word_text)
                .Select(WordProjection)
                .ToListAsync();
        }

        public async Task<List<WordTransporter>> GetActiveByCategoryIdAsync(int categoryId)
        {
            return await context.WORDs
                .AsNoTracking()
                .Where(word =>
                    word.category_id == categoryId &&
                    word.is_active &&
                    word.CATEGORY.is_active)
                .OrderBy(word => word.word_text)
                .Select(WordProjection)
                .ToListAsync();
        }

        public async Task<List<WordTransporter>> GetActiveByCategoryIdAndLanguageAsync(
            int categoryId,
            string languageCode)
        {
            return await context.WORDs
                .AsNoTracking()
                .Where(word =>
                    word.category_id == categoryId &&
                    word.language_code == languageCode &&
                    word.is_active &&
                    word.CATEGORY.is_active)
                .OrderBy(word => word.word_text)
                .Select(WordProjection)
                .ToListAsync();
        }

        public async Task<List<CategoryTransporter>> GetActiveCategoriesByLanguageAsync(string languageCode)
        {
            return await context.CATEGORies
                .AsNoTracking()
                .Where(category =>
                    category.language_code == languageCode &&
                    category.is_active)
                .OrderBy(category => category.name)
                .Select(CategoryProjection)
                .ToListAsync();
        }

        public async Task<bool> ExistsActiveAsync(int wordId)
        {
            return await context.WORDs
                .AsNoTracking()
                .AnyAsync(word =>
                    word.word_id == wordId &&
                    word.is_active &&
                    word.CATEGORY.is_active);
        }
    }
}
