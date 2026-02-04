**Language:** [Русский](README.md) | **English**

---

Assignment 1: [STAGE-1](STAGE-1_en.md)

Assignment 2: [STAGE-2](STAGE-2_en.md)

Assignment 3: [STAGE-3](STAGE-3_en.md)

Assignment 4: [STAGE-4](STAGE-4_en.md)

Assignment 5: [STAGE-5](STAGE-5_en.md)

---

## Project Overview

This project is an educational in-memory SQL DBMS implemented as a minimalist ASP.NET Core Web API. The service parses and executes a subset of SQL, maintains tables and data in memory, and exposes HTTP endpoints for query execution and metadata access. The goal is to practice designing a simple DBMS core, writing a SQL parser, and building a clean API surface with predictable JSON responses.

## Technology Stack

- **Language:** C# (LangVersion 10)
- **Platform:** .NET 6 (ASP.NET Core Minimal Web API)
- **Project type:** Web API service
- **Runtime storage:** In-memory data structures (no external DB)
- **API:** REST-like endpoints, JSON input/output
- **Tooling:** Swagger for development-time exploration

## Supported Features and Operations

### Metadata and Service Control

- **Service info endpoint** (with termination token in development/testing)
- **Graceful shutdown** via a secured termination endpoint

### Table Management (DDL)

- `CREATE TABLE` with support for:
  - Optional `IF NOT EXISTS`
  - Column types: `BOOLEAN`, `INTEGER`, `FLOAT`, `STRING`, `SERIAL`
  - Constraints: `PRIMARY KEY`, `NOT NULL`
  - Default values via `DEFAULT`
- `DROP TABLE` with optional `IF EXISTS`
- **List tables** and **get table schema** endpoints

### Data Manipulation (DML)

- `INSERT INTO ... VALUES ...` with optional `RETURNING`
- `SELECT ... FROM ...` with:
  - `*` or explicit column list
  - Optional `WHERE` with comparison operators `=`, `!=`, `>`, `<`, `>=`, `<=`
  - Optional `ORDER BY` with `ASC`/`DESC`
  - Optional `LIMIT`
- `UPDATE ... SET ...` with optional `WHERE` and `RETURNING`
- `DELETE FROM ...` with optional `WHERE` and `RETURNING`

### Advanced Query Support

- `AS` for column aliasing
- **Nested SELECT** in the `FROM` clause

## Output Format

All query results are returned as a **schema + rows** payload. The schema mirrors column metadata, and all data values are serialized as strings (with `null` for missing values), simplifying JSON responses and keeping output consistent across different SQL types.
