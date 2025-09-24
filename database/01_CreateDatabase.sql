-- =============================================
-- xBazar Database Creation Script
-- Multi-vendor Marketplace Database
-- =============================================

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'xBazarDb')
BEGIN
    CREATE DATABASE [xBazarDb]
    COLLATE Turkish_CI_AS;
END
GO

USE [xBazarDb];
GO

-- =============================================
-- Enable Full-Text Search
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'xBazarFullTextCatalog')
BEGIN
    CREATE FULLTEXT CATALOG [xBazarFullTextCatalog];
END
GO
