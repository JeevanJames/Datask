## Create Excel file from database schema

Creates an Excel file from a database schema. This creates one worksheet per table. Each worksheet contains one Excel table named after the the DB table (`<schema name>.<table name>`).

```
Usage:
  exceldata create <connection string> <file name> [options]

Arguments:
  <connection string>   The connection string to the database to create the Excel
                        file from.
  <file name>           The name of the Excel file to create.

Options:
  -i, --include   [*] One or more regular expressions specifying the tables to
                  include. This should match the <schema>.<table> format.
  -e, --exclude   [*] One or more regular expressions specifying the tables to
                  exclude. This should match the <schema>.<table> format. Tables
                  are excluded after considering the tables to include.
      --force     If the Excel file already exists, overwrite it.
```

## Validate Excel file is up-to-date with the database

Validates whether an Excel file is in-sync with the database. This includes verifying:
* Changes to database schema, such as changing data types, added columns, deleted columns, etc.
* Table name changes.
* New tables added, existing tables deleted.

```
Usage:
  exceldata validate <connection string> <file name>

Arguments:
  <connection string>   The connection string to the database to validate the Excel
                        file from.
  <file name>           The name of the Excel file to validate.
```

## Update Excel file to sync up with database

Updates an Excel file with changes in the database, while maintaining existing data.

```
Usage:
  exceldata validate <connection string> <file name>

Arguments:
  <connection string>   The connection string to the database to update the Excel
                        file from.
  <file name>           The name of the Excel file to update.
```
