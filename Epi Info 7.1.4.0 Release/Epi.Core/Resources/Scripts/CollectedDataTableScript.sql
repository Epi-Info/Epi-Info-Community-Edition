
CREATE TABLE metaDBInfo (
       ProjectId            uniqueidentifier NOT NULL,
       ProjectLocation      nvarchar(255) NOT NULL,
       ProjectName          nvarchar(64) NOT NULL,
       EpiVersion           nvarchar(20) NOT NULL,
       Purpose              int NOT NULL
)
go

ALTER TABLE metaDBInfo
       ADD PRIMARY KEY CLUSTERED (ProjectId ASC)
go

CREATE TABLE sysDataTables (
       TableName            char(64) NOT NULL,
       ViewId               int NOT NULL
)
go

ALTER TABLE sysDataTables
       ADD PRIMARY KEY CLUSTERED (TableName ASC)
go



