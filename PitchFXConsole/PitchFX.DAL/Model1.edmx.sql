
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 04/28/2012 10:25:03
-- Generated from EDMX file: C:\Users\Mike\documents\visual studio 2010\Projects\PitchFXConsole\PitchFX.DAL\Model1.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [PitchFX];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'AtBats'
CREATE TABLE [dbo].[AtBats] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Strikes] smallint  NOT NULL,
    [Balls] smallint  NOT NULL,
    [Outs] smallint  NOT NULL,
    [start_tfs] nvarchar(max)  NOT NULL,
    [start_tfs_zulu] nvarchar(max)  NOT NULL,
    [batter] int  NOT NULL,
    [stand] nvarchar(max)  NOT NULL,
    [b_height] nvarchar(max)  NOT NULL,
    [pitcher] int  NOT NULL,
    [p_throws] nvarchar(max)  NOT NULL,
    [atbat_des] nvarchar(max)  NOT NULL,
    [atbat_event] nvarchar(max)  NOT NULL,
    [Inning] smallint  NOT NULL,
    [atbat_num] smallint  NOT NULL,
    [GameId] int  NOT NULL
);
GO

-- Creating table 'Games'
CREATE TABLE [dbo].[Games] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [GameDate] datetime  NOT NULL,
    [HomeTeam] nvarchar(max)  NOT NULL,
    [AwayTeam] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Pitches'
CREATE TABLE [dbo].[Pitches] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [des] nvarchar(max)  NOT NULL,
    [pitch_id] int  NOT NULL,
    [type] nvarchar(max)  NOT NULL,
    [tfs] nvarchar(max)  NOT NULL,
    [tfs_zulu] nvarchar(max)  NOT NULL,
    [x] decimal(18,0)  NOT NULL,
    [y] decimal(18,0)  NOT NULL,
    [sv_id] nvarchar(max)  NOT NULL,
    [start_speed] decimal(18,0)  NOT NULL,
    [end_speed] decimal(18,0)  NOT NULL,
    [sz_top] decimal(18,0)  NOT NULL,
    [sz_bot] decimal(18,0)  NOT NULL,
    [pfx_x] decimal(18,0)  NOT NULL,
    [pfx_z] decimal(18,0)  NOT NULL,
    [px] decimal(18,0)  NOT NULL,
    [pz] decimal(18,0)  NOT NULL,
    [x0] decimal(18,0)  NOT NULL,
    [y0] decimal(18,0)  NOT NULL,
    [z0] decimal(18,0)  NOT NULL,
    [vx0] decimal(18,0)  NOT NULL,
    [vy0] decimal(18,0)  NOT NULL,
    [vz0] decimal(18,0)  NOT NULL,
    [ax] decimal(18,0)  NOT NULL,
    [ay] decimal(18,0)  NOT NULL,
    [az] decimal(18,0)  NOT NULL,
    [break_y] decimal(18,0)  NOT NULL,
    [break_angle] decimal(18,0)  NOT NULL,
    [break_length] decimal(18,0)  NOT NULL,
    [pitch_type] nvarchar(max)  NOT NULL,
    [type_confidence] decimal(18,0)  NOT NULL,
    [zone] int  NOT NULL,
    [nasty] int  NOT NULL,
    [spin_dir] decimal(18,0)  NOT NULL,
    [spin_rate] decimal(18,0)  NOT NULL,
    [cc] nvarchar(max)  NULL,
    [mt] nvarchar(max)  NULL,
    [AtBatId] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'AtBats'
ALTER TABLE [dbo].[AtBats]
ADD CONSTRAINT [PK_AtBats]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Games'
ALTER TABLE [dbo].[Games]
ADD CONSTRAINT [PK_Games]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Pitches'
ALTER TABLE [dbo].[Pitches]
ADD CONSTRAINT [PK_Pitches]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [GameId] in table 'AtBats'
ALTER TABLE [dbo].[AtBats]
ADD CONSTRAINT [FK_GameAtBat]
    FOREIGN KEY ([GameId])
    REFERENCES [dbo].[Games]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GameAtBat'
CREATE INDEX [IX_FK_GameAtBat]
ON [dbo].[AtBats]
    ([GameId]);
GO

-- Creating foreign key on [AtBatId] in table 'Pitches'
ALTER TABLE [dbo].[Pitches]
ADD CONSTRAINT [FK_AtBatPitch]
    FOREIGN KEY ([AtBatId])
    REFERENCES [dbo].[AtBats]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_AtBatPitch'
CREATE INDEX [IX_FK_AtBatPitch]
ON [dbo].[Pitches]
    ([AtBatId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------