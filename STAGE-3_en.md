**Language:** [Русский](STAGE-3.md) | **English**

---

# Working with Data in Tables

In the third stage we'll implement:

1. Adding data to a table
2. Reading data from a table
3. Updating data in a table
4. Deleting data from a table

All resulting tables formed as a result of executing a command must be
returned from the endpoint, in the same way as it's done in commands from the previous
part of the assignment. I.e., there should be a schema and data.

## Adding Data to a Table

```sql
INSERT INTO table_name (column1_name, column2_name, ..., columnN_name)
VALUES
    (1, true, ..., 'str_value1'),
    (2, true, ..., 'str_value2'),
    ...,
    (100, false, ..., 'str_value100')
RETURNING column1_name, column5_name;
```

The INSERT INTO command adds data to table "table_name", after the table name,
in parentheses, the names of columns are specified for which we'll specify values later.
If when creating the table default values were specified for some columns
using the DEFAULT keyword, then such a column can be omitted when adding
data to the table, in which case you need to use this default value.
Also, you cannot specify a column that has SERIAL type, because the value of this column
should be assigned automatically when adding.

The part of the query with the RETURNING keyword is optional and indicates which
columns we'll return in the query execution result, consider an example:

There's a table "tab1", which has a field "id" with SERIAL type, field "name" with STRING type and field
"is_ready" with BOOLEAN type, let's say for the "is_ready" field the default value is FALSE,
and the counter for the id field was, let's say 100 at the time of query execution start, then as a
result of executing the query:

```sql
INSERT INTO tab1 (name)
VALUES ('n1'), ('n2'), ('n3')
RETURNING id, is_ready
```

We should get a table like:

```
------------------
| id  | is_ready |
------------------
| 100 | false    |
| 101 | false    |
| 102 | false    |
------------------
```

If RETURNING is absent, then an empty table is used as result,
without columns and rows in it.

When adding data to the table, an important case should be considered.
In situations when the table has a PRIMARY KEY field, for example
with STRING type, and we try to add a record to the table that already has another
record with the same value. Then we should return error 409 (Conflict), because the data
we passed conflicts with what's already in the table.

## Reading Data from a Table

```sql
SELECT column1_name, column2_name, ..., columnN_name FROM table_name
WHERE column1_name > 10
ORDER BY column2_name ASC
LIMIT 10
```

I'll add a few more examples of correct queries:

```sql
SELECT * FROM table_name;
SELECT column_name FROM table_name WHERE id < 10;
SELECT column_name FROM table_name LIMIT 10;
SELECT column_name FROM table_name ORDER BY column_name DESC;
```

So to get some data from a table, the SELECT FROM command is used.
After the SELECT keyword comes a list of column names we want to get from the table.
If instead of a list the \* symbol is specified, then all columns are selected. After the list necessarily comes
the FROM keyword, after it the table name. This is where the required part of the query ends.
All other keywords are optional and may be absent.

The WHERE keyword sets a condition on the column value. Only those rows are returned
from the table that satisfy the condition. In real life, a condition can contain complex
expressions, for example:

```sql
WHERE column1_name > 10 AND (column2_name = 'abc' OR column3_name = true)
```

We'll make an assumption and will support only a condition on one column and will
support only the following set of operators =, !=, >, <, >= and <=.
When comparing strings, we use Ordinal comparison. A column with SERIAL type in the comparison expression
is interpreted as INTEGER. If the condition is not specified, all rows are returned.

The ORDER BY keyword specifies sorting of the result by the value of the column whose name comes
after ORDER BY. Also, after the column name you need to specify the sorting direction
ASC - sets ascending (from smaller value to larger), DESC - sets descending.
If sorting is not specified, rows are returned in the order of their placement in the table.

The LIMIT 10 keyword limits the number of rows that will be returned in the result.
If no limit is set, all matching rows are returned.

## Updating Data in a Table

```sql
UPDATE table_name
SET
    column1_name = 'some str value',
    column2_name = DEFAULT
WHERE
    column3_name != 123
RETURNING
    column4_name
```

The UPDATE command updates all rows in table "table_name" matching the condition specified in WHERE.
If no condition is specified, then absolutely all rows are updated. After the SET keyword
a list of columns with their new values is specified. If the
DEFAULT keyword is specified as a value, then the default value that was specified when
creating the table is used. If no default value was specified for the column, then this is an error and you need to
return error code 400. As with any other errors found when parsing an SQL query.

The RETURNING keyword, as in other commands, is optional and specifies the list of columns
that need to be returned in the resulting table. If among the returned columns
there's a column we're updating (specified in the SET section), then its
new value should be returned.

## Deleting Data from a Table

```sql
DELETE FROM table_name
WHERE column1_name = 'value'
RETURNING id;
```

To delete data from a table in SQL, the DELETE FROM command is used. This command
also has an optional WHERE part that specifies the condition for deleted records, and
RETURNING which allows specifying the list of columns that the query should return.
The result of this command includes those rows that the query deletes. If the
WHERE keyword is not specified, all rows are deleted from the table. If the RETURNING keyword
is not specified, we return an empty table. Also, an important feature of this command is
that it doesn't change counter values (in our case the only possible field
with SERIAL type). I.e., if records were added to the table and let's say the SERIAL counter,
for the last row was 10, after which we deleted all rows from the table, and added a new
row, then the counter for it will be 11.
