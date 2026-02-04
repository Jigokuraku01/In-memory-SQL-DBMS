**Language:** [Русский](STAGE-2.md) | **English**

---

# Creating and Deleting Tables

In the second stage we'll implement:

1. Logic for creating and deleting tables
2. Start writing an SQL query parser
3. Add endpoints (handles) to the project for executing SQL queries and several additional ones for getting the list of tables and table schema

## SQL Queries We'll Learn to Parse and Execute in This Part

In this part, we'll support two commands: creating and deleting tables. In SQL form, they look like this:

```sql
-- Creating a table
CREATE TABLE [IF NOT EXISTS] table_name (
    column1_name TYPE [PRIMARY KEY] [NOT NULL] [DEFAULT value],
    column2_name TYPE [NOT NULL] [DEFAULT value],
    ...
    columnN_name TYPE [NOT NULL] [DEFAULT value],
);

-- Deleting a table
DROP TABLE [IF EXISTS] table_name;
```

Let me decode what's written:

1. ';' at the end of the query is usually only placed in scripts that may have many sequential SQL commands, however, since we're making an endpoint that will execute only one command at a time, ';' at the end should simply be ignored by your parser
2. SQL keywords are written in uppercase, names/values in lowercase, however SQL keywords are case-insensitive, so if you rewrite everything in lowercase, everything should parse correctly, while keeping in mind that data, for example value for the DEFAULT keyword, is case-sensitive - consider this when implementing the parser
3. keywords enclosed in [] are optional parts of the SQL expression, but as always there are lots of special cases
4. IF NOT EXISTS - for CREATE TABLE, if present, the endpoint should always return OK(200) response and the result of whether the table was created or not, if absent, then in case of an attempt to create a table with a name that already exists, you need to return Conflict(409) from the endpoint
5. PRIMARY KEY - a sign that this field is a key, this means it must have a unique value for each record in the table, cannot be NULL, i.e. NOT NULL - is automatically applied to this column and doesn't need to be written
6. NOT NULL - a sign that values in this column cannot be NULL, i.e. when adding data (we'll implement this command in the future) you always need to specify a specific value that is not NULL
7. DEFAULT - this is a keyword that sets the default value for this field, used when adding new records (rows) to the table, if it's not specified when adding, then this default value should be written to the table
8. IF EXISTS - for DROP TABLE, if present, the endpoint should always return OK(200) and result true/false depending on table existence, if not specified, then the endpoint should return NotFound(404) error

A couple words about which types we'll support in our database, below is a table with explanation:

```
SQL type    C# type     Comment
-----------------------------------
BOOLEAN     Boolean     In SQL there are two keywords TRUE and FALSE for setting values
INTEGER     Int64       Signed 64-bit number
FLOAT       Double      64-bit floating point
STRING      String      Unicode string (encoding: utf16-le)
SERIAL      Int64       Counter used as
                        PRIMARY KEY for identifying records in the table.
                        Each time a new record is added to the table, it's incremented.
                        To store the current counter value, usually DBMS creates a separate table
                        with one column of INTEGER type and one record with the current
                        counter value. Let's agree that in our SQL dialect it will be possible
                        to use this type only together with PRIMARY KEY. For storing the value,
                        I think we can proceed like other databases.
```

Now I'll give several examples of correct table creation queries:

```sql
CREATE TABLE tab1 (id SERIAL PRIMARY KEY);
create table tab2 (name string not null, description string);
CREATE TABLE tab3 (
    "id" String Primary Key,
    column BOOLEAN DEFAULT TRUE
);
```

As you can see, an SQL query can also be split into lines, or can be written in one line, roughly speaking, line breaks can be interpreted as spaces.
And now several examples of incorrect queries (the parser should correctly handle all such cases, those listed here and not only):

```sql
-- According to our agreement, id - must be marked as PRIMARY KEY
CREATE TABLE table (id SERIAL);

-- There can be only one PRIMARY KEY in a table
CREATE TABLE table (
    id1 STRING PRIMARY KEY,
    id2 INTEGER PRIMARY KEY
);

-- Unknown type
CREATE TABLE table (id BIGINT);

-- Incorrect default value type (already marked as cannot be NULL)
CREATE TABLE table (name STRING NOT NULL DEFAULT NULL);

-- Incorrect default value type (types don't match)
-- (note that string values are enclosed in single quotes, not double)
CREATE TABLE table (name INTEGER NOT NULL DEFAULT 'Hello world!');
```

## New Endpoints (Handles)

### GET /api/v1/tables/list

```
GET /api/v1/tables/list

input: no input arguments

output:
{
  tables: ["table1", "table2", ..., "tableN"]
}
```

This endpoint should return a list of table names that are currently created in
our database. To get such JSON as output, you can use as result
a class like:

```csharp
public class GetTablesOutput
{
    [Required] public String[] Tables { get; set; }
}
```

### POST /api/v1/tables/schema

```
POST /api/v1/tables/schema

input:
{
  "name": "my_table_name"
}

output:
{
  "schema": {
    "columns": [                // list of column information (in order of their addition when creating the table)
      {
        "name": "id",           // column name
        "type": "serial",       // data type in column
        "isPKey": true,         // sign that this is PRIMARY KEY
        "isNullable": false,    // sign that the field is nullable
        "defaultValue": {       // information about default value
          "isSpecified": false, // sign that DEFAULT was specified for the column
          "isNull": false,      // sign that the default value = NULL
          "value": ""           // default value in string form
        }
      },
      {
        "name": "description",
        "type": "string",
        "isPKey": false,
        "isNullable": true,
        "defaultValue": {
          "isSpecified": true,
          "isNull": true,
          "value": ""
        }
      },
      ...                       // and so on
    ]
  }
}
```

In terms of classes on the service side, this representation will work:

```csharp
public class DefaultValueInfo
{
    [Required] public Boolean IsSpecified { get; set; }
    [Required] public Boolean IsNull { get; set; }
    [Required] public String Value { get; set; }
}

public class TableSchemaColumnInfo
{
    [Required] public String Name { get; set; }
    [Required] public String Type { get; set; }
    [Required] public Boolean IsPKey { get; set; }
    [Required] public Boolean IsNullable { get; set; }
    [Required] public DefaultValueInfo DefaultValue { get; set; }
}

public class TableSchemaInfo
{
    [Required] public TableSchemaColumnInfo[] Columns { get; set; }
}

public class PostTablesSchemaOutput
{
    [Required] public TableSchemaInfo Schema { get; set; }
}
```

The endpoint should return the table schema by table name, which represents a set of
information about the table and columns. Let's look at an example query that creates a table
and the result of getting the schema:

```sql
CREATE TABLE goods (
    "id" SERIAL PRIMARY KEY,
    "name" STRING NOT NULL,
    "description" STRING DEFAULT NULL,
    "price" INTEGER NOT NULL DEFAULT 10,
    "stock" INTEGER,
    "is_foreign" BOOLEAN NOT NULL
)
```

Note that in SQL, something enclosed in double quotes is a name (for example, of a column or table). If you
need to set a value for a string, you should use single quotes. Double quotes are usually used for
resolving name conflicts with keywords. For example, the word "user" might be reserved by the DBMS for a variable storing the current username from
which the query is executed. However, if enclosed in double quotes "user" - the query interpreter will unambiguously determine
it as a column or table name, or some other object, but not as a language keyword. And overall, double quotes can be omitted.
However, supporting this syntax feature when parsing queries is necessary.
For a table created this way, we should get this result when querying its schema:

```json
{
  "schema": {
    "columns": [
      {
        "name": "id",
        "type": "serial",
        "isPKey": true,
        "isNullable": false,
        "defaultValue": {
          "isSpecified": false,
          "isNull": false,
          "value": ""
        }
      },
      {
        "name": "name",
        "type": "string",
        "isPKey": false,
        "isNullable": false,
        "defaultValue": {
          "isSpecified": false,
          "isNull": false,
          "value": ""
        }
      },
      {
        "name": "description",
        "type": "string",
        "isPKey": false,
        "isNullable": true,
        "defaultValue": {
          "isSpecified": true,
          "isNull": true,
          "value": ""
        }
      },
      {
        "name": "price",
        "type": "integer",
        "isPKey": false,
        "isNullable": false,
        "defaultValue": {
          "isSpecified": true,
          "isNull": false,
          "value": "10"
        }
      },
      {
        "name": "stock",
        "type": "integer",
        "isPKey": false,
        "isNullable": true,
        "defaultValue": {
          "isSpecified": false,
          "isNull": false,
          "value": ""
        }
      },
      {
        "name": "is_foreign",
        "type": "boolean",
        "isPKey": false,
        "isNullable": false,
        "defaultValue": {
          "isSpecified": false,
          "isNull": false,
          "value": ""
        }
      }
    ]
  }
}
```

### POST /api/v1/query

```
POST /api/v1/query

input:
{
  "query": "SELECT id, name FROM table"
}

output:
{
  "schema": {
    "columns": [
      {
        "name": "id",
        "type": "serial",
        "isPKey": true,
        "isNullable": false,
        "defaultValue": {
          "isSpecified": false,
          "isNull": false,
          "value": ""
        }
      },
      {
        "name": "name",
        "type": "string",
        "isPKey": false,
        "isNullable": false,
        "defaultValue": {
          "isSpecified": false,
          "isNull": false,
          "value": ""
        }
      }
    ]
  },
  "result": [
    ["1", "record1"],
    ["2", null],
    ["3", "some name"]
  ]
}
```

Here's the general view of the response to arbitrary SQL queries. In fact, in the response you need
to provide the schema of the table we got when executing the query and rows with data.
The schema is formed by the same structures as in the previous endpoint. And the "result" array
contains rows, each value of which corresponds to the schema. Note
in the result all values are returned as strings - this is needed to simplify forming the response
on the service side. Also nullable types can contain null - this will also work out
of the box, just assign null to the corresponding string when returning the result.

In the current part of the work, we won't have such "large" and "complex" tables in the result, since
we're only implementing creating and deleting tables. CREATE TABLE and DROP TABLE operations
should return a table consisting of a single "result" column and a single row
which will contain the execution result true - in case of successful table creation/deletion
false - otherwise.

This is how the response for these queries should look. Immediately write generalized code for returning
values for an arbitrary table.

```json
{
  "schema": {
    "columns": [
      {
        "name": "result",
        "type": "boolean",
        "isPKey": false,
        "isNullable": false,
        "defaultValue": {
          "isSpecified": false,
          "isNull": false,
          "value": ""
        }
      }
    ]
  },
  "result": [["true"]]
}
```

## Assignment

So let's draw a line on what needs to be done:

1. Develop Table and TableSchema classes that will store table data and table data schema respectively.
2. Implement an SQL command parser for CREATE TABLE and DROP TABLE, which converts queries into executable commands. A possible variant of organizing the command interface and example we discussed in practice.
3. Implement 3 new endpoints with logic as described in the assignment.
