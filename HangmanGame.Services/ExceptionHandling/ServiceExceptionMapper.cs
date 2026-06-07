using Hangman.Business.Messages;
using System;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

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