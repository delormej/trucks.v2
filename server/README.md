# Trucks.Server
Responsible for all web server implementation specific details as well as cloud persistance, message queuing / processing.  Any business logic independant of cloud service details should be handled in the `../trucks/Trucks.csproj` project.

## 1. ConvertXlsSettlementsAsync
### PantherClient::DownloadXls
* Downloads binary Excel settlement statements

### ExcelConverter::UploadXls
* Uploads settlement statements to online conversion service
* Enqueues a message to check back in _x_ minutes to see if conversion is complete

## 2. ParseSettlementsAsync
### ExcelConverter::DownloadXlsx
* Downloads available xlsx files

### Parser::ParseXlsx
* Parse downloaded xlsx files into Model objects

### Model::SettlementHistory
* POCO model representation

## 3. SaveSettlementsAsync
### ISettlementRepository::SaveSettlement
* Persists POCO model to backing store.

