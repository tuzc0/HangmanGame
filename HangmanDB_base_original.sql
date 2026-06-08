USE master;
GO

IF DB_ID(N'HangmanDB') IS NOT NULL
BEGIN
    ALTER DATABASE HangmanDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HangmanDB;
END
GO

CREATE DATABASE HangmanDB;
GO

USE HangmanDB;
GO

CREATE TABLE dbo.LANGUAGE (
    language_code NVARCHAR(5) NOT NULL,
    name NVARCHAR(50) NOT NULL,

    CONSTRAINT PK_LANGUAGE PRIMARY KEY (language_code),
    CONSTRAINT UQ_LANGUAGE_name UNIQUE (name)
);
GO

CREATE TABLE dbo.PLAYER (
    player_id INT IDENTITY(1,1) NOT NULL,
    full_name NVARCHAR(120) NOT NULL,
    date_of_birth DATE NOT NULL,
    phone NVARCHAR(20) NOT NULL,
    creation_date DATETIME2 NOT NULL
        CONSTRAINT DF_PLAYER_creation_date DEFAULT SYSUTCDATETIME(),
    is_active BIT NOT NULL
        CONSTRAINT DF_PLAYER_is_active DEFAULT 1,
    preferred_language_code NVARCHAR(5) NOT NULL,

    CONSTRAINT PK_PLAYER PRIMARY KEY (player_id),

    CONSTRAINT FK_PLAYER_LANGUAGE
        FOREIGN KEY (preferred_language_code)
        REFERENCES dbo.LANGUAGE(language_code)
);
GO

CREATE TABLE dbo.ACCOUNT (
    account_id INT IDENTITY(1,1) NOT NULL,
    player_id INT NOT NULL,
    email NVARCHAR(200) NOT NULL,
    password_hash NVARCHAR(255) NOT NULL,

    is_email_verified BIT NOT NULL
        CONSTRAINT DF_ACCOUNT_is_email_verified DEFAULT 0,

    email_verified_at DATETIME2 NULL,

    account_status NVARCHAR(30) NOT NULL
        CONSTRAINT DF_ACCOUNT_account_status DEFAULT N'PendingVerification',

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_ACCOUNT_created_at DEFAULT SYSUTCDATETIME(),

    updated_at DATETIME2 NULL,

    CONSTRAINT PK_ACCOUNT PRIMARY KEY (account_id),

    CONSTRAINT FK_ACCOUNT_PLAYER
        FOREIGN KEY (player_id)
        REFERENCES dbo.PLAYER(player_id)
);
GO

CREATE UNIQUE INDEX UX_ACCOUNT_player_id
ON dbo.ACCOUNT(player_id);
GO

CREATE UNIQUE INDEX UX_ACCOUNT_email
ON dbo.ACCOUNT(email);
GO

CREATE TABLE dbo.EMAIL_VERIFICATION (
    email_verification_id INT IDENTITY(1,1) NOT NULL,
    account_id INT NOT NULL,

    verification_code_hash NVARCHAR(255) NOT NULL,

    expires_at DATETIME2 NOT NULL,
    verified_at DATETIME2 NULL,

    attempts INT NOT NULL
        CONSTRAINT DF_EMAIL_VERIFICATION_attempts DEFAULT 0,

    is_used BIT NOT NULL
        CONSTRAINT DF_EMAIL_VERIFICATION_is_used DEFAULT 0,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_EMAIL_VERIFICATION_created_at DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_EMAIL_VERIFICATION PRIMARY KEY (email_verification_id),

    CONSTRAINT FK_EMAIL_VERIFICATION_ACCOUNT
        FOREIGN KEY (account_id)
        REFERENCES dbo.ACCOUNT(account_id)
);
GO

CREATE INDEX IX_EMAIL_VERIFICATION_account_used_created
ON dbo.EMAIL_VERIFICATION(account_id, is_used, created_at DESC);
GO

CREATE TABLE dbo.PASSWORD_RESET_TOKEN (
    password_reset_token_id INT IDENTITY(1,1) NOT NULL,
    account_id INT NOT NULL,

    reset_code_hash NVARCHAR(255) NOT NULL,

    expires_at DATETIME2 NOT NULL,
    used_at DATETIME2 NULL,

    attempts INT NOT NULL
        CONSTRAINT DF_PASSWORD_RESET_TOKEN_attempts DEFAULT 0,

    is_used BIT NOT NULL
        CONSTRAINT DF_PASSWORD_RESET_TOKEN_is_used DEFAULT 0,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_PASSWORD_RESET_TOKEN_created_at DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_PASSWORD_RESET_TOKEN PRIMARY KEY (password_reset_token_id),

    CONSTRAINT FK_PASSWORD_RESET_TOKEN_ACCOUNT
        FOREIGN KEY (account_id)
        REFERENCES dbo.ACCOUNT(account_id)
);
GO

CREATE INDEX IX_PASSWORD_RESET_TOKEN_account_used_created
ON dbo.PASSWORD_RESET_TOKEN(account_id, is_used, created_at DESC);
GO

CREATE TABLE dbo.CATEGORY (
    category_id INT IDENTITY(1,1) NOT NULL,
    category_key NVARCHAR(80) NOT NULL,

    is_active BIT NOT NULL
        CONSTRAINT DF_CATEGORY_is_active DEFAULT 1,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_CATEGORY_created_at DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_CATEGORY PRIMARY KEY (category_id),
    CONSTRAINT UQ_CATEGORY_category_key UNIQUE (category_key)
);
GO

CREATE TABLE dbo.CATEGORY_TRANSLATION (
    category_translation_id INT IDENTITY(1,1) NOT NULL,
    category_id INT NOT NULL,
    language_code NVARCHAR(5) NOT NULL,
    name NVARCHAR(80) NOT NULL,

    CONSTRAINT PK_CATEGORY_TRANSLATION PRIMARY KEY (category_translation_id),

    CONSTRAINT FK_CATEGORY_TRANSLATION_CATEGORY
        FOREIGN KEY (category_id)
        REFERENCES dbo.CATEGORY(category_id),

    CONSTRAINT FK_CATEGORY_TRANSLATION_LANGUAGE
        FOREIGN KEY (language_code)
        REFERENCES dbo.LANGUAGE(language_code)
);
GO

CREATE UNIQUE INDEX UX_CATEGORY_TRANSLATION_category_language
ON dbo.CATEGORY_TRANSLATION(category_id, language_code);
GO

CREATE UNIQUE INDEX UX_CATEGORY_TRANSLATION_name_language
ON dbo.CATEGORY_TRANSLATION(name, language_code);
GO

CREATE TABLE dbo.WORD (
    word_id INT IDENTITY(1,1) NOT NULL,
    category_id INT NOT NULL,

    is_active BIT NOT NULL
        CONSTRAINT DF_WORD_is_active DEFAULT 1,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_WORD_created_at DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_WORD PRIMARY KEY (word_id),

    CONSTRAINT FK_WORD_CATEGORY
        FOREIGN KEY (category_id)
        REFERENCES dbo.CATEGORY(category_id)
);
GO

CREATE INDEX IX_WORD_category_id
ON dbo.WORD(category_id);
GO

CREATE TABLE dbo.WORD_TRANSLATION (
    word_translation_id INT IDENTITY(1,1) NOT NULL,
    word_id INT NOT NULL,
    language_code NVARCHAR(5) NOT NULL,

    word_text NVARCHAR(80) NOT NULL,
    description NVARCHAR(300) NOT NULL,

    is_active BIT NOT NULL
        CONSTRAINT DF_WORD_TRANSLATION_is_active DEFAULT 1,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_WORD_TRANSLATION_created_at DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_WORD_TRANSLATION PRIMARY KEY (word_translation_id),

    CONSTRAINT FK_WORD_TRANSLATION_WORD
        FOREIGN KEY (word_id)
        REFERENCES dbo.WORD(word_id),

    CONSTRAINT FK_WORD_TRANSLATION_LANGUAGE
        FOREIGN KEY (language_code)
        REFERENCES dbo.LANGUAGE(language_code)
);
GO

CREATE UNIQUE INDEX UX_WORD_TRANSLATION_word_language
ON dbo.WORD_TRANSLATION(word_id, language_code);
GO

CREATE INDEX IX_WORD_TRANSLATION_language_active
ON dbo.WORD_TRANSLATION(language_code, is_active);
GO

CREATE TABLE dbo.[MATCH] (
    match_id INT IDENTITY(1,1) NOT NULL,

    host_id INT NOT NULL,
    guest_id INT NULL,

    host_language_code NVARCHAR(5) NOT NULL,
    guest_language_code NVARCHAR(5) NULL,

    selected_category_id INT NULL,
    selected_word_id INT NULL,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_MATCH_created_at DEFAULT SYSUTCDATETIME(),

    joined_at DATETIME2 NULL,
    category_voting_started_at DATETIME2 NULL,
    category_voting_ends_at DATETIME2 NULL,
    word_selection_started_at DATETIME2 NULL,
    word_selection_ends_at DATETIME2 NULL,
    started_at DATETIME2 NULL,
    finished_at DATETIME2 NULL,

    match_status NVARCHAR(30) NOT NULL
        CONSTRAINT DF_MATCH_status DEFAULT N'WaitingForGuest',

    winner_id INT NULL,
    penalized_user_id INT NULL,

    failed_attempts INT NOT NULL
        CONSTRAINT DF_MATCH_failed_attempts DEFAULT 0,

    max_attempts INT NOT NULL
        CONSTRAINT DF_MATCH_max_attempts DEFAULT 6,

    CONSTRAINT PK_MATCH PRIMARY KEY (match_id),

    CONSTRAINT FK_MATCH_HOST_PLAYER
        FOREIGN KEY (host_id)
        REFERENCES dbo.PLAYER(player_id),

    CONSTRAINT FK_MATCH_GUEST_PLAYER
        FOREIGN KEY (guest_id)
        REFERENCES dbo.PLAYER(player_id),

    CONSTRAINT FK_MATCH_HOST_LANGUAGE
        FOREIGN KEY (host_language_code)
        REFERENCES dbo.LANGUAGE(language_code),

    CONSTRAINT FK_MATCH_GUEST_LANGUAGE
        FOREIGN KEY (guest_language_code)
        REFERENCES dbo.LANGUAGE(language_code),

    CONSTRAINT FK_MATCH_SELECTED_CATEGORY
        FOREIGN KEY (selected_category_id)
        REFERENCES dbo.CATEGORY(category_id),

    CONSTRAINT FK_MATCH_SELECTED_WORD
        FOREIGN KEY (selected_word_id)
        REFERENCES dbo.WORD(word_id),

    CONSTRAINT FK_MATCH_WINNER_PLAYER
        FOREIGN KEY (winner_id)
        REFERENCES dbo.PLAYER(player_id),

    CONSTRAINT FK_MATCH_PENALIZED_PLAYER
        FOREIGN KEY (penalized_user_id)
        REFERENCES dbo.PLAYER(player_id)
);
GO

CREATE INDEX IX_MATCH_status
ON dbo.[MATCH](match_status);
GO

CREATE INDEX IX_MATCH_host_id
ON dbo.[MATCH](host_id);
GO

CREATE INDEX IX_MATCH_guest_id
ON dbo.[MATCH](guest_id);
GO

CREATE INDEX IX_MATCH_selected_category_id
ON dbo.[MATCH](selected_category_id);
GO

CREATE INDEX IX_MATCH_selected_word_id
ON dbo.[MATCH](selected_word_id);
GO

CREATE TABLE dbo.MATCH_CATEGORY_VOTE (
    match_category_vote_id INT IDENTITY(1,1) NOT NULL,
    match_id INT NOT NULL,
    player_id INT NOT NULL,
    category_id INT NOT NULL,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_MATCH_CATEGORY_VOTE_created_at DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_MATCH_CATEGORY_VOTE PRIMARY KEY (match_category_vote_id),

    CONSTRAINT FK_MATCH_CATEGORY_VOTE_MATCH
        FOREIGN KEY (match_id)
        REFERENCES dbo.[MATCH](match_id),

    CONSTRAINT FK_MATCH_CATEGORY_VOTE_PLAYER
        FOREIGN KEY (player_id)
        REFERENCES dbo.PLAYER(player_id),

    CONSTRAINT FK_MATCH_CATEGORY_VOTE_CATEGORY
        FOREIGN KEY (category_id)
        REFERENCES dbo.CATEGORY(category_id)
);
GO

CREATE UNIQUE INDEX UX_MATCH_CATEGORY_VOTE_match_player
ON dbo.MATCH_CATEGORY_VOTE(match_id, player_id);
GO

CREATE INDEX IX_MATCH_CATEGORY_VOTE_match_id
ON dbo.MATCH_CATEGORY_VOTE(match_id);
GO

CREATE INDEX IX_MATCH_CATEGORY_VOTE_category_id
ON dbo.MATCH_CATEGORY_VOTE(category_id);
GO

CREATE TABLE dbo.MATCH_GUESS (
    guess_id INT IDENTITY(1,1) NOT NULL,

    match_id INT NOT NULL,
    guessed_by_id INT NOT NULL,

    letter NVARCHAR(1) NOT NULL,
    is_correct BIT NOT NULL,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_MATCH_GUESS_created_at DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_MATCH_GUESS PRIMARY KEY (guess_id),

    CONSTRAINT FK_MATCH_GUESS_MATCH
        FOREIGN KEY (match_id)
        REFERENCES dbo.[MATCH](match_id),

    CONSTRAINT FK_MATCH_GUESS_PLAYER
        FOREIGN KEY (guessed_by_id)
        REFERENCES dbo.PLAYER(player_id)
);
GO

CREATE UNIQUE INDEX UX_MATCH_GUESS_match_letter
ON dbo.MATCH_GUESS(match_id, letter);
GO

CREATE INDEX IX_MATCH_GUESS_match_id
ON dbo.MATCH_GUESS(match_id);
GO

CREATE TABLE dbo.SCORE_MOVEMENT (
    score_movement_id INT IDENTITY(1,1) NOT NULL,

    player_id INT NOT NULL,
    match_id INT NOT NULL,

    points INT NOT NULL,
    movement_type NVARCHAR(30) NOT NULL,

    created_at DATETIME2 NOT NULL
        CONSTRAINT DF_SCORE_MOVEMENT_created_at DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_SCORE_MOVEMENT PRIMARY KEY (score_movement_id),

    CONSTRAINT FK_SCORE_MOVEMENT_PLAYER
        FOREIGN KEY (player_id)
        REFERENCES dbo.PLAYER(player_id),

    CONSTRAINT FK_SCORE_MOVEMENT_MATCH
        FOREIGN KEY (match_id)
        REFERENCES dbo.[MATCH](match_id)
);
GO

CREATE INDEX IX_SCORE_MOVEMENT_player_id
ON dbo.SCORE_MOVEMENT(player_id);
GO

CREATE INDEX IX_SCORE_MOVEMENT_match_id
ON dbo.SCORE_MOVEMENT(match_id);
GO

INSERT INTO dbo.LANGUAGE (language_code, name)
VALUES 
(N'es', N'Español'),
(N'en', N'English');
GO

INSERT INTO dbo.CATEGORY (category_key)
VALUES
(N'animals'),
(N'technology');
GO

INSERT INTO dbo.CATEGORY_TRANSLATION (category_id, language_code, name)
SELECT category_id, N'es', N'Animales'
FROM dbo.CATEGORY
WHERE category_key = N'animals';

INSERT INTO dbo.CATEGORY_TRANSLATION (category_id, language_code, name)
SELECT category_id, N'en', N'Animals'
FROM dbo.CATEGORY
WHERE category_key = N'animals';

INSERT INTO dbo.CATEGORY_TRANSLATION (category_id, language_code, name)
SELECT category_id, N'es', N'Tecnología'
FROM dbo.CATEGORY
WHERE category_key = N'technology';

INSERT INTO dbo.CATEGORY_TRANSLATION (category_id, language_code, name)
SELECT category_id, N'en', N'Technology'
FROM dbo.CATEGORY
WHERE category_key = N'technology';
GO

DECLARE @AnimalsCategoryId INT = (
    SELECT category_id
    FROM dbo.CATEGORY
    WHERE category_key = N'animals'
);

DECLARE @TechnologyCategoryId INT = (
    SELECT category_id
    FROM dbo.CATEGORY
    WHERE category_key = N'technology'
);

INSERT INTO dbo.WORD (category_id)
VALUES
(@AnimalsCategoryId),
(@AnimalsCategoryId),
(@TechnologyCategoryId);

DECLARE @DogWordId INT = 1;
DECLARE @CatWordId INT = 2;
DECLARE @ComputerWordId INT = 3;

INSERT INTO dbo.WORD_TRANSLATION (word_id, language_code, word_text, description)
VALUES
(@DogWordId, N'es', N'perro', N'Animal doméstico conocido por ser fiel al ser humano.'),
(@DogWordId, N'en', N'dog', N'Domestic animal known for being loyal to humans.'),
(@CatWordId, N'es', N'gato', N'Animal doméstico pequeño que suele maullar.'),
(@CatWordId, N'en', N'cat', N'Small domestic animal that usually meows.'),
(@ComputerWordId, N'es', N'computadora', N'Dispositivo electrónico usado para procesar información.'),
(@ComputerWordId, N'en', N'computer', N'Electronic device used to process information.');
GO
