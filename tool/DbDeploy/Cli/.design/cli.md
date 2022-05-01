# Deploy database
Deploys a database from scripts.

```sh
datask-dbdeploy deploy <connection string>
    [--database <database name>]
    [--mode Migrate|Full]
    [--directory <scripts directory>]
```

Arg | Description
----|------------
`<connection string>` | The server connection string (without database name) or the full connection string (without database name).
`<database name>` | The name of the database, if a server connection string is specified. Optional if the full connection string is specified.
`<mode>` | How to deploy the database. `Migrate` only runs the pending migration scripts. `Full` will drop the database if it exists and run all the deployment scripts. If the database doesn't exist, then specifying `Migrate` will be the same as `Full`.
`<scripts directory>` | The directory or base directory that contains the deployment scripts. Default: the current directory.

# Drop database
Drops the specified database.

```sh
datash-dbdeploy drop <connection string>
    [--database <database name>]
```

Arg | Description
----|------------
`<connection string>` | The server connection string (without database name) or the full connection string (without database name).
`<database name>` | The name of the database, if a server connection string is specified. Optional if the full connection string is specified.
