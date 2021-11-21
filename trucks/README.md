# Trucks
All business logic a data processing specific behaviors should live in this project.  And dependencies on storage (except empemeral disk) should be in the `../server/Trucks.csrpoj` project.

## Panther
Responsible for downloading settlement statements.

## Excel
Responsible for converting binary Excel files into xml based xslx format and Model entities.

## Model
Represents the business model entities.

## Repository
Defines _only_ the interfaces for persistent storage.