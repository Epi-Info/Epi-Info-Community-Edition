
CREATE TABLE metaDataTypes (
       HasPattern           bit NOT NULL,
       HasSize              bit NOT NULL,
       HasRange             bit NOT NULL,
       Name                 nvarchar(30) NOT NULL,
       DataTypeId           int NOT NULL
)
go


ALTER TABLE metaDataTypes
       ADD PRIMARY KEY CLUSTERED (DataTypeId ASC)
go


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


CREATE TABLE metaFieldGroups (
       FieldId              int NOT NULL,
       GroupId              int NOT NULL
)
go

CREATE INDEX XIF2metaFieldGroups ON metaFieldGroups
(
       FieldId                        ASC
)
go

CREATE INDEX XIF1metaFieldGroups ON metaFieldGroups
(
       GroupId                        ASC
)
go


ALTER TABLE metaFieldGroups
       ADD PRIMARY KEY CLUSTERED (FieldId ASC, GroupId ASC)
go


CREATE TABLE metaFields (
       Name                 nvarchar(64) NOT NULL,
       PromptText           nvarchar(255) NULL,
       FieldTypeId          int NOT NULL,
       ControlFontFamily    nvarchar(20) NULL,
       ControlFontStyle     nvarchar(50) NULL,
       ControlFontSize      decimal(2) NULL,
       ControlTopPositionPercentage float NULL,
       ControlLeftPositionPercentage float NULL,
       ControlHeightPercentage float NULL,
       ControlWidthPercentage float NULL,
       TabIndex             int NULL,
       HasTabStop           bit NULL,
       PromptFontFamily     nvarchar(20) NULL,
       PromptFontStyle      nvarchar(50) NULL,
       PromptFontSize       decimal(2) NULL,
       PromptScriptName     nvarchar(20) NULL,
       PromptTopPositionPercentage float NULL,
       PromptLeftPositionPercentage float NULL,
       ControlScriptName    nvarchar(20) NULL,
       ShouldRepeatLast     bit NULL,
       IsRequired           bit NULL,
       IsReadOnly           bit NULL,
       ShouldRetainImageSize bit NULL,
       MaxLength            smallint NULL,
       Lower                nvarchar(20) NULL,
       Upper                nvarchar(20) NULL,
       Pattern              nvarchar(50) NULL,
       ShowTextOnRight      bit NULL,
       CheckCodeBefore      ntext NULL,
       CheckCodeAfter       ntext NULL,
       RelateCondition      nvarchar(255) NULL,
       ShouldReturnToParent bit NULL,
       SourceTableName      nvarchar(50) NULL,
       CodeColumnName       nvarchar(50) NULL,
       TextColumnName       nvarchar(50) NULL,
       Sort                 bit NULL,
       IsExclusiveTable     bit NULL,
       FieldId              int IDENTITY,
       PageId               int NULL,
       SourceFieldId        int NULL,
       RelatedViewId        int NULL,
       ViewId               int NOT NULL,
       DataTableName        nvarchar(64) NOT NULL
)
go

CREATE INDEX XIF2metaFields ON metaFields
(
       PageId                         ASC
)
go

CREATE INDEX XIF1metaFields ON metaFields
(
       FieldTypeId                    ASC
)
go

CREATE INDEX XIF4metaFields ON metaFields
(
       RelatedViewId                  ASC
)
go

CREATE INDEX XIF5metaFields ON metaFields
(
       ViewId                         ASC
)
go


ALTER TABLE metaFields
       ADD PRIMARY KEY CLUSTERED (FieldId ASC)
go


ALTER TABLE metaFields
       ADD  UNIQUE (ViewId ASC,Name ASC)
go


CREATE TABLE metaFieldTypes (
       FieldTypeId          int NOT NULL,
       DataTypeId           int NOT NULL,
       Name                 nvarchar(50) NOT NULL,
       HasFont              bit NOT NULL,
       HasRepeatLast        bit NOT NULL,
       HasRequired          bit NOT NULL,
       HasReadOnly          bit NOT NULL,
       HasRetainImageSize   bit NOT NULL,
       IsDropDown           bit NOT NULL,
       IsGridColumn         bit NOT NULL,
       IsSystem             bit NOT NULL,
       DefaultPatternId		int NOT NULL
)
go

CREATE INDEX XIF1metaFieldTypes ON metaFieldTypes
(
       DataTypeId                     ASC
)
go


ALTER TABLE metaFieldTypes
       ADD PRIMARY KEY CLUSTERED (FieldTypeId ASC)
go


CREATE TABLE metaGridColumns (
       GridColumnId         int IDENTITY,
       Name                 nvarchar(64) NOT NULL,
       Size                 smallint NULL,
       Position             smallint NOT NULL,
       FieldTypeId          int NOT NULL,
       Text                 nvarchar(255) NOT NULL,
       ShouldRepeatLast     bit NOT NULL,
       IsRequired           bit NULL,
       IsReadOnly           bit NULL,
       Pattern              nvarchar(50) NULL,
       Upper                nvarchar(20) NULL,
       Lower                nvarchar(20) NULL,
       FieldId              int NOT NULL,
       Width                int NOT NULL
)
go

CREATE INDEX XIF2metaGridColumns ON metaGridColumns
(
       FieldId                        ASC
)
go

CREATE INDEX XIF1metaGridColumns ON metaGridColumns
(
       FieldTypeId                    ASC
)
go


ALTER TABLE metaGridColumns
       ADD PRIMARY KEY CLUSTERED (GridColumnId ASC)
go


ALTER TABLE metaGridColumns
       ADD  UNIQUE (Name ASC)
go


CREATE TABLE metaGroups (
       GroupId              int IDENTITY,
       PageId               int NOT NULL,
       Name                 nvarchar(50) NOT NULL,
       Title                nvarchar(50) NOT NULL,
       TopPositionPercentage float NOT NULL,
       LeftPositionPercentage float NOT NULL,
       HeightPercentage     float NOT NULL,
       WidthPercentage      float NOT NULL,
       BackgroundColor      int NULL,
       ViewId               int NOT NULL
)
go

CREATE INDEX XIF1metaGroups ON metaGroups
(
       PageId                         ASC
)
go

CREATE INDEX XIF2metaGroups ON metaGroups
(
       ViewId                         ASC
)
go

ALTER TABLE metaGroups
       ADD PRIMARY KEY CLUSTERED (GroupId ASC)
go

ALTER TABLE metaGroups
       ADD  UNIQUE (ViewId ASC,Name ASC)
go

CREATE TABLE metaOptions (
       OptionId             int IDENTITY,
       Text                 nvarchar(50) NOT NULL,
       FieldId              int NOT NULL
)
go

CREATE INDEX XIF1metaOptions ON metaOptions
(
       FieldId                        ASC
)
go


ALTER TABLE metaOptions
       ADD PRIMARY KEY CLUSTERED (OptionId ASC)
go

CREATE TABLE metaPages (
       PageId               int IDENTITY,
       Name                 nvarchar(50) NOT NULL,
       Position             smallint NOT NULL,
       CheckCodeBefore      ntext NULL,
       CheckCodeAfter       ntext NULL,
       ViewId               int NOT NULL
)
go

CREATE INDEX XIF1metaPages ON metaPages
(
       ViewId                         ASC
)
go


ALTER TABLE metaPages
       ADD PRIMARY KEY CLUSTERED (PageId ASC)
go


CREATE TABLE metaPatterns (
       PatternId            int NOT NULL,
       Expression           nvarchar(30) NOT NULL,
       DataTypeId           int NOT NULL,
       Mask					nvarchar(30) NOT NULL,
       FormattedExpression	nvarchar(30) NOT NULL
)
go

CREATE INDEX XIF1metaPatterns ON metaPatterns
(
       DataTypeId                     ASC
)
go


ALTER TABLE metaPatterns
       ADD PRIMARY KEY CLUSTERED (PatternId ASC)
go


CREATE TABLE metaPrograms (
       ProgramId            int IDENTITY,
       Name                 nvarchar(64) NOT NULL,
       Content              ntext NOT NULL,
       Comment              ntext NULL,
       DateCreated          datetime NOT NULL,
       DateModified         datetime NOT NULL,
       Author               nvarchar(64) NULL
)
go


ALTER TABLE metaPrograms
       ADD PRIMARY KEY CLUSTERED (ProgramId ASC)
go


ALTER TABLE metaPrograms
       ADD  UNIQUE (Name ASC)
go


CREATE TABLE metaViews (
       ViewId               int IDENTITY,
       Name                 nvarchar(64) NOT NULL,
       IsRelatedView        bit NOT NULL,
       CheckCodeBefore      ntext NULL,
       CheckCodeAfter       ntext NULL,
       RecordCheckCodeBefore ntext NULL,
       RecordCheckCodeAfter ntext NULL,
       CheckCodeVariableDefinitions ntext NULL
)
go


ALTER TABLE metaViews
       ADD PRIMARY KEY CLUSTERED (ViewId ASC)
go


ALTER TABLE metaViews
       ADD  UNIQUE (Name ASC)
go


ALTER TABLE metaFieldGroups
       ADD FOREIGN KEY (GroupId)
                             REFERENCES metaGroups  (GroupId)
go


ALTER TABLE metaFieldGroups
       ADD FOREIGN KEY (FieldId)
                             REFERENCES metaFields  (FieldId)
go


ALTER TABLE metaFields
       ADD FOREIGN KEY (ViewId)
                             REFERENCES metaViews  (ViewId)
go


ALTER TABLE metaFields
       ADD FOREIGN KEY (RelatedViewId)
                             REFERENCES metaViews  (ViewId)
go


ALTER TABLE metaFields
       ADD FOREIGN KEY (FieldTypeId)
                             REFERENCES metaFieldTypes  (FieldTypeId)
go


ALTER TABLE metaFields
       ADD FOREIGN KEY (PageId)
                             REFERENCES metaPages  (PageId)
go


ALTER TABLE metaFields
       ADD FOREIGN KEY (SourceFieldId)
                             REFERENCES metaFields  (FieldId)
go


ALTER TABLE metaFieldTypes
       ADD FOREIGN KEY (DataTypeId)
                             REFERENCES metaDataTypes  (DataTypeId)
go


ALTER TABLE metaGridColumns
       ADD FOREIGN KEY (FieldTypeId)
                             REFERENCES metaFieldTypes  (FieldTypeId)
go


ALTER TABLE metaGridColumns
       ADD FOREIGN KEY (FieldId)
                             REFERENCES metaFields  (FieldId)
go


ALTER TABLE metaGroups
       ADD FOREIGN KEY (ViewId)
                             REFERENCES metaViews  (ViewId)
go


ALTER TABLE metaGroups
       ADD FOREIGN KEY (PageId)
                             REFERENCES metaPages  (PageId)
go


ALTER TABLE metaOptions
       ADD FOREIGN KEY (FieldId)
                             REFERENCES metaFields  (FieldId)
go


ALTER TABLE metaPages
       ADD FOREIGN KEY (ViewId)
                             REFERENCES metaViews  (ViewId)
go


ALTER TABLE metaPatterns
       ADD FOREIGN KEY (DataTypeId)
                             REFERENCES metaDataTypes  (DataTypeId)
go



