using Hangman.Business.Messages;
using System;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.ServiceModel;

namespace HangmanGame.Services.ExceptionHandling
{
    public static class ServiceExceptionMapper
    {
        public static AuthMessageCode Map(Exception exception)
        {
            if (exception == null)
            {
                return AuthMessageCode.UnexpectedError;
            }

            SqlException sqlException = FindInnerException<SqlException>(exception);

            if (sqlException != null)
            {
                return MapSqlException(sqlException);
            }

            if (exception is EntityException || exception is DbUpdateException)
            {
                return AuthMessageCode.DatabaseUnavailable;
            }

            if (exception is TimeoutException)
            {
                return AuthMessageCode.DatabaseTimeout;
            }

            if (exception is ConfigurationErrorsException)
            {
                return AuthMessageCode.ConfigurationError;
            }

            if (exception is NullReferenceException ||
                exception is InvalidOperationException ||
                exception is ArgumentException)
            {
                return AuthMessageCode.RuntimeError;
            }

            return AuthMessageCode.UnexpectedError;
        }

        public static ProfileMessageCode MapProfile(Exception exception)
        {
            if (exception == null)
            {
                return ProfileMessageCode.UnexpectedError;
            }

            SqlException sqlException = FindInnerException<SqlException>(exception);

            if (sqlException != null)
            {
                return MapSqlExceptionToProfile(sqlException);
            }

            if (exception is EntityException || exception is DbUpdateException)
            {
                return ProfileMessageCode.DatabaseUnavailable;
            }

            if (exception is TimeoutException)
            {
                return ProfileMessageCode.DatabaseTimeout;
            }

            if (exception is ConfigurationErrorsException)
            {
                return ProfileMessageCode.ConfigurationError;
            }

            if (exception is NullReferenceException ||
                exception is InvalidOperationException ||
                exception is ArgumentException)
            {
                return ProfileMessageCode.RuntimeError;
            }

            return ProfileMessageCode.UnexpectedError;
        }

        private static AuthMessageCode MapSqlException(SqlException exception)
        {
            foreach (SqlError error in exception.Errors)
            {
                switch (error.Number)
                {
                    case 2:
                    case 53:
                    case 4060:
                    case 18456:
                        return AuthMessageCode.DatabaseConnectionError;

                    case -2:
                        return AuthMessageCode.DatabaseTimeout;

                    case 2627:
                    case 2601:
                        return AuthMessageCode.DatabaseDuplicateKey;

                    case 547:
                    case 515:
                        return AuthMessageCode.DatabaseConstraintError;
                }
            }

            return AuthMessageCode.DatabaseUnavailable;
        }

        private static ProfileMessageCode MapSqlExceptionToProfile(SqlException exception)
        {
            foreach (SqlError error in exception.Errors)
            {
                switch (error.Number)
                {
                    case 2:
                    case 53:
                    case 4060:
                    case 18456:
                        return ProfileMessageCode.DatabaseConnectionError;

                    case -2:
                        return ProfileMessageCode.DatabaseTimeout;

                    case 2627:
                    case 2601:
                        return ProfileMessageCode.DatabaseDuplicateKey;

                    case 547:
                    case 515:
                        return ProfileMessageCode.DatabaseConstraintError;
                }
            }

            return ProfileMessageCode.DatabaseUnavailable;
        }

        public static WordMessageCode MapWord(Exception exception)
        {
            if (exception == null)
            {
                return WordMessageCode.UnexpectedError;
            }

            Exception baseException = exception.GetBaseException();

            if (exception is TimeoutException || baseException is TimeoutException)
            {
                return WordMessageCode.DatabaseTimeout;
            }

            SqlException sqlException = exception as SqlException
                ?? baseException as SqlException;

            if (sqlException != null)
            {
                return MapWordSqlException(sqlException);
            }

            if (exception is DbUpdateException ||
                exception is EntityException ||
                baseException is DbUpdateException ||
                baseException is EntityException)
            {
                return WordMessageCode.DatabaseUnavailable;
            }

            if (exception is ConfigurationErrorsException ||
                baseException is ConfigurationErrorsException)
            {
                return WordMessageCode.ConfigurationError;
            }

            if (exception is CommunicationException ||
                exception is IOException ||
                baseException is CommunicationException ||
                baseException is IOException)
            {
                return WordMessageCode.RuntimeError;
            }

            if (exception is InvalidOperationException ||
                exception is ArgumentException ||
                baseException is InvalidOperationException ||
                baseException is ArgumentException)
            {
                return WordMessageCode.RuntimeError;
            }

            return WordMessageCode.UnexpectedError;
        }

        private static WordMessageCode MapWordSqlException(SqlException sqlException)
        {
            switch (sqlException.Number)
            {
                case -2:
                    return WordMessageCode.DatabaseTimeout;

                case 2:
                case 26:
                case 40:
                case 53:
                case 4060:
                case 10060:
                case 10061:
                case 18456:
                    return WordMessageCode.DatabaseConnectionError;

                default:
                    return WordMessageCode.DatabaseUnavailable;
            }
        }

        public static MatchMessageCode MapMatch(Exception exception)
        {
            if (exception == null)
            {
                return MatchMessageCode.UnexpectedError;
            }

            Exception baseException = exception.GetBaseException();

            if (exception is TimeoutException || baseException is TimeoutException)
            {
                return MatchMessageCode.DatabaseTimeout;
            }

            SqlException sqlException = exception as SqlException
                ?? baseException as SqlException;

            if (sqlException != null)
            {
                return MapMatchSqlException(sqlException);
            }

            if (exception is DbUpdateException ||
                exception is EntityException ||
                baseException is DbUpdateException ||
                baseException is EntityException)
            {
                return MatchMessageCode.DatabaseUnavailable;
            }

            if (exception is ConfigurationErrorsException ||
                baseException is ConfigurationErrorsException)
            {
                return MatchMessageCode.ConfigurationError;
            }

            if (exception is CommunicationException ||
                exception is IOException ||
                baseException is CommunicationException ||
                baseException is IOException)
            {
                return MatchMessageCode.RuntimeError;
            }

            if (exception is InvalidOperationException ||
                exception is ArgumentException ||
                baseException is InvalidOperationException ||
                baseException is ArgumentException)
            {
                return MatchMessageCode.RuntimeError;
            }

            return MatchMessageCode.UnexpectedError;
        }

        public static ScoreMessageCode MapScore(Exception exception)
        {
            if (exception == null)
            {
                return ScoreMessageCode.UnexpectedError;
            }

            Exception baseException = exception.GetBaseException();

            if (exception is TimeoutException || baseException is TimeoutException)
            {
                return ScoreMessageCode.DatabaseTimeout;
            }

            SqlException sqlException = exception as SqlException
                ?? baseException as SqlException;

            if (sqlException != null)
            {
                return MapScoreSqlException(sqlException);
            }

            if (exception is DbUpdateException ||
                exception is EntityException ||
                baseException is DbUpdateException ||
                baseException is EntityException)
            {
                return ScoreMessageCode.DatabaseUnavailable;
            }

            if (exception is ConfigurationErrorsException ||
                baseException is ConfigurationErrorsException)
            {
                return ScoreMessageCode.ConfigurationError;
            }

            if (exception is InvalidOperationException ||
                exception is ArgumentException ||
                baseException is InvalidOperationException ||
                baseException is ArgumentException)
            {
                return ScoreMessageCode.RuntimeError;
            }

            return ScoreMessageCode.UnexpectedError;
        }

        private static ScoreMessageCode MapScoreSqlException(SqlException sqlException)
        {
            switch (sqlException.Number)
            {
                case -2:
                    return ScoreMessageCode.DatabaseTimeout;

                case 2:
                case 26:
                case 40:
                case 53:
                case 4060:
                case 10060:
                case 10061:
                case 18456:
                    return ScoreMessageCode.DatabaseConnectionError;

                default:
                    return ScoreMessageCode.DatabaseUnavailable;
            }
        }

        private static MatchMessageCode MapMatchSqlException(SqlException sqlException)
        {
            switch (sqlException.Number)
            {
                case -2:
                    return MatchMessageCode.DatabaseTimeout;

                case 2:
                case 26:
                case 40:
                case 53:
                case 4060:
                case 10060:
                case 10061:
                case 18456:
                    return MatchMessageCode.DatabaseConnectionError;

                default:
                    return MatchMessageCode.DatabaseUnavailable;
            }
        }

        private static TException FindInnerException<TException>(Exception exception)
            where TException : Exception
        {
            Exception currentException = exception;

            while (currentException != null)
            {
                TException typedException = currentException as TException;

                if (typedException != null)
                {
                    return typedException;
                }

                currentException = currentException.InnerException;
            }

            return null;
        }
    }
}