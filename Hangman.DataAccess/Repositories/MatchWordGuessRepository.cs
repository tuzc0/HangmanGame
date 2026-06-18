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
    public class MatchWordGuessRepository : IMatchWordGuessRepository
    {
        private readonly HangmanDBEntities context;

        public MatchWordGuessRepository(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MatchWordGuessTransporter> GetByIdAsync(int wordGuessId)
        {
            return await context.MATCH_WORD_GUESS
                .AsNoTracking()
                .Where(guess => guess.word_guess_id == wordGuessId)
                .Select(guess => new MatchWordGuessTransporter
                {
                    WordGuessId = guess.word_guess_id,
                    MatchId = guess.match_id,
                    GuessedById = guess.guessed_by_id,
                    GuessedWord = guess.guessed_word,
                    IsCorrect = guess.is_correct,
                    CreatedAt = guess.created_at
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<MatchWordGuessTransporter>> GetByMatchIdAsync(
            int matchId)
        {
            return await context.MATCH_WORD_GUESS
                .AsNoTracking()
                .Where(guess => guess.match_id == matchId)
                .OrderBy(guess => guess.created_at)
                .Select(guess => new MatchWordGuessTransporter
                {
                    WordGuessId = guess.word_guess_id,
                    MatchId = guess.match_id,
                    GuessedById = guess.guessed_by_id,
                    GuessedWord = guess.guessed_word,
                    IsCorrect = guess.is_correct,
                    CreatedAt = guess.created_at
                })
                .ToListAsync();
        }

        public void Add(CreateMatchWordGuessTransporter guess)
        {
            if (guess == null)
            {
                throw new ArgumentNullException(nameof(guess));
            }

            MATCH_WORD_GUESS entity = new MATCH_WORD_GUESS
            {
                match_id = guess.MatchId,
                guessed_by_id = guess.GuessedById,
                guessed_word = guess.GuessedWord,
                is_correct = guess.IsCorrect,
                created_at = DateTime.UtcNow
            };

            context.MATCH_WORD_GUESS.Add(entity);
        }
    }
}
