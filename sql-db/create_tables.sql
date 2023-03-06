-- Creation of AddDocument table
CREATE TABLE adddocument (
  correlationid uuid,
  docid uuid,
  tempbloburl TEXT NOT NULL,
  permanenturl TEXT,
  filename varchar(500),
  CONSTRAINT adddocument_pkey PRIMARY KEY (correlationid )
);


-- INSERT INTO AddDocument (CorrelationId, DocId, TempBlobURL,PermanentURL,FileName) VALUES 
-- (99302414-6000-42c1-b9f5-2597091b26b4,'','http://test.com','',''), 
