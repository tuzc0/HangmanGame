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
    public class MatchGuessRepository : IMatchGuessRepository
    {
        private readonly HangmanDBEntities context;

        public MatchGuessRepository(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MatchGuessTransporter> GetByIdAsync(int guessId)
        {
            return await context.MATCH_GUESS
                .AsNoTracking()
                .Where(guess => guess.guess_id == guessId)
                .Select(guess => new MatchGuessTransporter
                {
                    GuessId = guess.guess_id,
                    MatchId = guess.match_id,
                    GuessedById = guess.guessed_by_id,
                    Letter = guess.letter,
                    IsCorrect = guess.is_correct,
                    CreatedAt = guess.created_at
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<MatchGuessTransporter>> GetByMatchIdAsync(int matchId)
        {
            return await context.MATCH_GUESS
                .AsNoTracking()
                .Where(guess => guess.match_id == matchId)
                .OrderBy(guess => guess.created_at)
                .Select(guess => new MatchGuessTransporter
                {
                    GuessId = guess.guess_id,
                    MatchId = guess.match_id,
                    GuessedById = guess.guessed_by_id,
                    Letter = guess.letter,
                    IsCorrect = guess.is_correct,
                    CreatedAt = guess.created_at
                })
                .ToListAsync();
        }

        public async Task<MatchGuessTransporter> GetByMatchAndLetterAsync(
            int matchId,
            string letter)
        {
            return await context.MATCH_GUESS
                .AsNoTracking()
                .Where(guess =>
                    guess.match_id == matchId &&
                    guess.letter == letter)
                .Select(guess => new MatchGuessTransporter
                {
                    GuessId = guess.guess_id,
                    MatchId = guess.match_id,
                    GuessedById = guess.guessed_by_id,
                    Letter = guess.letter,
                    IsCorrect = guess.is_correct,
                    CreatedAt = guess.created_at
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> LetterExistsAsync(int matchId, string letter)
        {
            return await context.MATCH_GUESS
                .AsNoTracking()
                .AnyAsync(guess =>
                    guess.match_id == matchId &&
                    guess.letter == letter);
        }

        public void Add(CreateMatchGuessTransporter guess)
        {
            if (guess == null)
            {
                throw new ArgumentNullException(nameof(guess));
            }

            MATCH_GUESS entity = new MATCH_GUESS
            {
                match_id = guess.MatchId,
                guessed_by_id = guess.GuessedById,
                letter = guess.Letter,
                is_correct = guess.IsCorrect,
                created_at = DateTime.UtcNow
            };

            context.MATCH_GUESS.Add(entity);
        }
    }
}
