-- =============================================
-- Full-Text Search Indexes
-- =============================================

-- Products Full-Text Search
CREATE FULLTEXT INDEX ON [dbo].[Products] 
(
    [Name] LANGUAGE 1055,
    [Description] LANGUAGE 1055,
    [ShortDescription] LANGUAGE 1055
)
KEY INDEX [PK_Products]
ON [xBazarFullTextCatalog];

-- Categories Full-Text Search
CREATE FULLTEXT INDEX ON [dbo].[Categories] 
(
    [Name] LANGUAGE 1055,
    [Description] LANGUAGE 1055
)
KEY INDEX [PK_Categories]
ON [xBazarFullTextCatalog];

-- Stores Full-Text Search
CREATE FULLTEXT INDEX ON [dbo].[Stores] 
(
    [Name] LANGUAGE 1055,
    [Description] LANGUAGE 1055
)
KEY INDEX [PK_Stores]
ON [xBazarFullTextCatalog];

-- Reviews Full-Text Search
CREATE FULLTEXT INDEX ON [dbo].[Reviews] 
(
    [Title] LANGUAGE 1055,
    [Comment] LANGUAGE 1055
)
KEY INDEX [PK_Reviews]
ON [xBazarFullTextCatalog];
