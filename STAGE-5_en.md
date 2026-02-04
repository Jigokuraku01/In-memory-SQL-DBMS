**Language:** [Русский](STAGE-5.md) | **English**

---

# Supporting Nested Queries and Table Joins

This part of the work will be dedicated to adding support for nested queries and
table joins. To implement the plan, you'll need to complete
three steps:

1. Support the AS operator for renaming a field in the output table
2. Support nested SELECT query
3. Support [INNER] JOIN, LEFT JOIN and RIGHT JOIN commands

## AS Operator

```sql
SELECT
    field1 AS id,
    field2,
    field3 AS description
FROM
    table1
```

The AS operator can be specified after the column name that's selected from the table.
If specified, in the resulting table the corresponding column should be returned
with the new name.

## Nested SELECT

In our SQL dialect, there will be a single place where you can use
a nested query, consider a couple examples:

```sql
-- 1
SELECT name FROM (SELECT id, name FROM tab1 WHERE id < 5);

-- 2
SELECT name FROM (SELECT id, name FROM tab1) WHERE id < 5;

-- 3
SELECT name FROM SELECT id, name FROM tab1 WHERE id < 5;
```

All 3 queries are correct, while the first is equivalent to the third query. In other words,
in the place where we can have a nested query, we can either put parentheses or not.
If there are parentheses, then only what's written in parentheses relates to the nested query.
If not, then everything written after the second SELECT relates to the nested query.

We'll support only one nested query.

## Supporting [INNER] JOIN, LEFT JOIN and RIGHT JOIN

Now about the main stage of this part of the work. If before this you organized the code
the way we discussed in class. I.e., made a parser that outputs
a command hidden behind an interface, then adding JOINs will be much
easier. If not, then you should stop and refactor the code, introducing into the program
the necessary interfaces.

The JOIN command in relation to SELECT will be nested, so its addition
should only affect the parser and implementation of an additional command that will
join two tables.

First, let's understand how INNER, LEFT and RIGHT JOINs differ. Let's say we have
two tables:

```
table: tab_names
----------------
| id | name    |
----------------
| 1  | 'name1' |
| 2  | 'name2' |
| 5  | 'name5' |
----------------

table: tab_last_names
----------------------------
| id | name_id | last_name |
----------------------------
| 10 | 1       | 'ln1'     |
| 20 | 2       | 'ln2'     |
| 30 | 3       | 'ln3'     |
----------------------------
```

Let's look at the result of executing the following queries, I specifically put the nested query
for joining tables in (), although here they can be omitted, to show the boundaries of the
JOIN command itself:

```sql
-- INNER JOIN (the INNER keyword can be omitted)
SELECT
    tab_names.name AS first_name,
    tab_last_names.last_name AS second_name
FROM
(
    tab_names JOIN tab_last_names
    ON tab_names.id = tab_last_names.name_id
)

-- LEFT JOIN
SELECT
    tab_names.name AS first_name,
    tab_last_names.last_name AS second_name
FROM
(
    tab_names LEFT JOIN tab_last_names
    ON tab_names.id = tab_last_names.name_id
)

-- RIGHT JOIN
SELECT
    tab_names.name AS first_name,
    tab_last_names.last_name AS second_name
FROM
(
    tab_names RIGHT JOIN tab_last_names
    ON tab_names.id = tab_last_names.name_id
)
```

All JOINs are binary operators. They take two tables as input and return
a joined table, according to the specified equivalence class, using
the ON operator. In our dialect, we'll support only comparison by one
specific field. Fields must have the same type. Since in general the value in a column
can repeat (since we didn't make indexes and data constraints), matching
is performed with the first encountered (any) row. In tests, I guarantee uniqueness
of values in columns by which joining will be performed. Also in tests there will be
only 1 to 1 relationships. So below are two sets of tables, the first set -
the join result. The second set - the result of the entire query.

```
Result of [INNER] JOIN
---------------------------------------------------------------------------------------------------------
| tab_names.id | tab_names.name | tab_last_names.id | tab_last_names.name_id | tab_last_names.last_name |
---------------------------------------------------------------------------------------------------------
| 1            | 'name1'        | 10                | 1                      | 'ln1'                    |
| 2            | 'name2'        | 20                | 2                      | 'ln2'                    |
---------------------------------------------------------------------------------------------------------

Result of LEFT JOIN
---------------------------------------------------------------------------------------------------------
| tab_names.id | tab_names.name | tab_last_names.id | tab_last_names.name_id | tab_last_names.last_name |
---------------------------------------------------------------------------------------------------------
| 1            | 'name1'        | 10                | 1                      | 'ln1'                    |
| 2            | 'name2'        | 20                | 2                      | 'ln2'                    |
| 5            | 'name5'        | NULL              | NULL                   | NULL                     |
---------------------------------------------------------------------------------------------------------

Result of RIGHT JOIN
---------------------------------------------------------------------------------------------------------
| tab_names.id | tab_names.name | tab_last_names.id | tab_last_names.name_id | tab_last_names.last_name |
---------------------------------------------------------------------------------------------------------
| 1            | 'name1'        | 10                | 1                      | 'ln1'                    |
| 2            | 'name2'        | 20                | 2                      | 'ln2'                    |
| NULL         | NULL           | 30                | 3                      | 'ln3'                    |
---------------------------------------------------------------------------------------------------------
```

```
Query result using [INNER] JOIN
--------------------------
| first_name | last_name |
--------------------------
| 'name1'    | 'ln1'     |
| 'name2'    | 'ln2'     |
--------------------------

Query result using LEFT JOIN
--------------------------
| first_name | last_name |
--------------------------
| 'name1'    | 'ln1'     |
| 'name2'    | 'ln2'     |
| 'name5'    | NULL      |
--------------------------

Query result using RIGHT JOIN
--------------------------
| first_name | last_name |
--------------------------
| 'name1'    | 'ln1'     |
| 'name2'    | 'ln2'     |
| NULL       | 'ln3'     |
--------------------------
```

Also worth noting two important points. First, the result schema after joining
will no longer correspond to the original tables.

1. When executing any JOINs, all fields having PRIMARY KEY must lose this attribute.
2. When executing LEFT and RIGHT JOINs, all fields must become nullable.

To simplify the implementation logic, columns of the temporary table formed after merging
two tables should be named in the form 'table_name.column_name'. And the dot in the name should be interpreted
as part of the name. I guarantee this aspect in tests. Also if in SELECT
the AS operator is not specified, then in the resulting schema the column should be named exactly as
it's specified, for example "tab_names.name".
