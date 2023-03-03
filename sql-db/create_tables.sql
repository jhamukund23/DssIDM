-- Creation of AddDocument table
CREATE TABLE AddDocument (
  CorrelationId uuid PRIMARY KEY,
  DocId uuid,
  TempBlobURL TEXT NOT NULL,
  PermanentURL TEXT,
  FileName varchar(500),
  PRIMARY KEY (CorrelationId)
);



-- INSERT INTO AddDocument (CorrelationId, DocId, TempBlobURL,PermanentURL,FileName) VALUES 
-- (99302414-6000-42c1-b9f5-2597091b26b4,'','http://test.com','',''), 
