using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Model;
using Hangman.DataAccess.Transporters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Repositories
{
    public class WordRepository : IWordRepository
    {
        private readonly HangmanDBEntities context;

        public WordRepository(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<WordTransporter> GetByIdAsync(int wordId, string languageCode)
        {
            return await BuildWordQuery(languageCode)
                .Where(word => word.WordId == wordId)
                .FirstOrDefaultAsync();
        }

        public async Task<WordTransporter> GetActiveByIdAsync(int wordId, string languageCode)
        {
            return await BuildWordQuery(languageCode)
                .Where(word =>
                    word.WordId == wordId &&
                    word.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WordTransporter>> GetActiveByLanguageAsync(string languageCode)
        {
            return await BuildWordQuery(languageCode)
                .Where(word => word.IsActive)
                .OrderBy(word => word.CategoryName)
                .ThenBy(word => word.WordText)
                .ToListAsync();
        }

        public async Task<List<WordTransporter>> GetActiveByCategoryIdAndLanguageAsync(
            int categoryId,
            string languageCode)
        {
            return await BuildWordQuery(languageCode)
                .Where(word =>
                    word.CategoryId == categoryId &&
                    word.IsActive)
                .OrderBy(word => word.WordText)
                .ToListAsync();
        }

        public async Task<List<CategoryTransporter>> GetActiveCategoriesByLanguageAsync(string languageCode)
        {
            return await
                (from category in context.CATEGORies.AsNoTracking()
                 join translation in context.CATEGORY_TRANSLATION.AsNoTracking()
                    on category.category_id equals translation.category_id
                 where category.is_active &&
                       translation.language_code == languageCode
                 orderby translation.name
                 select new CategoryTransporter
                 {
                     CategoryId = category.category_id,
                     CategoryKey = category.category_key,
                     Name = translation.name,
                     LanguageCode = translation.language_code,
                     IsActive = category.is_active
                 }).ToListAsync();
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

        public async Task<bool> ExistsActiveCategoryAsync(int categoryId)
        {
            return await context.CATEGORies
                .AsNoTracking()
                .AnyAsync(category =>
                    category.category_id == categoryId &&
                    category.is_active);
        }

        public async Task<bool> ExistsActiveTranslationAsync(int wordId, string languageCode)
        {
            return await context.WORD_TRANSLATION
                .AsNoTracking()
                .AnyAsync(translation =>
                    translation.word_id == wordId &&
                    translation.language_code == languageCode &&
                    translation.is_active &&
                    translation.WORD.is_active &&
                    translation.WORD.CATEGORY.is_active);
        }

        private IQueryable<WordTransporter> BuildWordQuery(string languageCode)
        {
            return
                from word in context.WORDs.AsNoTracking()
                join category in context.CATEGORies.AsNoTracking()
                    on word.category_id equals category.category_id
                join wordTranslation in context.WORD_TRANSLATION.AsNoTracking()
                    on word.word_id equals wordTranslation.word_id
                join categoryTranslation in context.CATEGORY_TRANSLATION.AsNoTracking()
                    on category.category_id equals categoryTranslation.category_id
                where wordTranslation.language_code == languageCode &&
                      categoryTranslation.language_code == languageCode &&
                      wordTranslation.is_active &&
                      category.is_active
                select new WordTransporter
                {
                    WordId = word.word_id,
                    CategoryId = category.category_id,
                    CategoryKey = category.category_key,
                    CategoryName = categoryTranslation.name,
                    WordText = wordTranslation.word_text,
                    Description = wordTranslation.description,
                    LanguageCode = wordTranslation.language_code,
                    IsActive = word.is_active && wordTranslation.is_active && category.is_active,
                    CreatedAt = word.created_at
                };
        }
    }
}
