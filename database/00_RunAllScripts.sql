-- =============================================
-- xBazar Database Setup - Run All Scripts
-- =============================================
-- Bu script tüm database scriptlerini sırayla çalıştırır
-- =============================================

PRINT 'Starting xBazar Database Setup...'

-- 1. Create Database
PRINT '1. Creating Database...'
:r 01_CreateDatabase.sql

-- 2. Create Tables
PRINT '2. Creating Tables...'
:r 02_CreateTables.sql
:r 03_CreateProductTables.sql
:r 04_CreateOrderTables.sql
:r 05_CreateOtherTables.sql

-- 3. Create Foreign Keys
PRINT '3. Creating Foreign Keys...'
:r 06_CreateForeignKeys.sql

-- 4. Create Indexes
PRINT '4. Creating Indexes...'
:r 07_CreateIndexes.sql

-- 5. Create Full-Text Search
PRINT '5. Creating Full-Text Search...'
:r 08_CreateFullTextIndexes.sql

-- 6. Create Role Permission Table
PRINT '6. Creating Role Permission Table...'
:r 10_CreateRolePermissionTable.sql

-- 7. Update Roles Table
PRINT '7. Updating Roles Table...'
:r 11_UpdateRolesTable.sql

-- 8. Insert Seed Data
PRINT '8. Inserting Seed Data...'
:r 09_SeedData.sql

-- 9. Insert Roles and Permissions
PRINT '9. Inserting Roles and Permissions...'
:r 12_SeedRolesAndPermissions.sql

-- 10. Create ApiKey Table
PRINT '10. Creating ApiKey Table...'
:r 13_CreateApiKeyTable.sql

-- 11. Create PaymentMethods Table
PRINT '11. Creating PaymentMethods Table...'
:r 14_CreatePaymentMethodTable.sql

-- 12. Create SecurityEvents Table
PRINT '12. Creating SecurityEvents Table...'
:r 15_CreateSecurityEventTable.sql

PRINT 'xBazar Database Setup Completed Successfully!'
PRINT 'Database: xBazarDb'
PRINT 'Tables Created: 20+'
PRINT 'Indexes Created: 50+'
PRINT 'Seed Data Inserted: Yes'
