# Umbraco Document Type Renamer Tool
When renaming document types in Umbraco, it is easy to miss something or make a typo when renaming. This console app aims to help prevent issues caused by this by automating the renaming process.

## Using the application

### Configure appsettings.json
- `BaseDirectory` - the path to the root of your Umbraco project,
- `OriginalDocTypeName` - the name of the document type you want to rename,
- `OriginalDocTypeAlias` - the alias of the document type you want to rename - optional, will be calculated based on the `"OriginalDocTypeName"` value if left blank,
- `NewDocTypeName` - the new name for the document type you are renaming,
- `uSyncDirectory` - the path to your uSync directory,
- `IgnoreCase` - this value is passed to `string.Replace` to determine if casing should be ignored when replacing the original document type name or aliases. Will default to `true` if left blank
- `CultureCode` - the culture code passed to `string.Replace`, defaults to `en-GB` if left blank

### Run the console app

If the values for `BaseDirectory`, `OriginalDocTypeName`, `NewDocTypeName` or `uSyncDirectory` has been left blank in `appsettings.json`, you will be prompted to enter these. No validation takes place to verify these values are well formed.

A limited description of what has been changed will be output to the console.

### Review what has changed

Use git to determine if the changes made by the tool are suitable.

### Run uSync

Umbraco will not pick up the new doctype name unless a uSync import of settings and content is run.

## Known limitations

This application does not support renaming templates. This will need to be done manually.