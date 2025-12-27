using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MknImmiSql.Inner_Logic.V1;
using System.ComponentModel.DataAnnotations;

namespace MknImmiSql.Api.V1.tables;


public class DefaultValueInfo
{
    [Required] public bool IsSpecified { get; set; }
    [Required] public bool IsNull { get; set; }
    [Required] public string Value { get; set; }

    public DefaultValueInfo(DefaultValue x)
    {
        IsSpecified = x.IsSpecified;
        Value = x.Value;
        IsNull = x.IsNull;
    }
}

public class TableSchemaColumnInfo
{
    [Required] public string Name { get; set; }
    [Required] public string Type { get; set; }
    [Required] public bool IsPKey { get; set; }
    [Required] public bool IsNullable { get; set; }
    [Required] public DefaultValueInfo DefaultValue { get; set; }

    public TableSchemaColumnInfo(ColumnInfo x)
    {
        Name = x.Name;
        Type = x.Type.ToLower();
        IsPKey = x.IsPKey;
        IsNullable = x.IsNullable;
        DefaultValue = new DefaultValueInfo(x.DefaultValue);
    }
}

public class TableSchemaInfo
{
    [Required] public TableSchemaColumnInfo[] Columns { get; set; }
    public TableSchemaInfo(TableInfo x)
    {
        Columns = new TableSchemaColumnInfo[x.TableColumnInfo.Count];
        int i = 0;
        foreach (var y in x.TableColumnInfo)
        {
            Columns[i++] = new TableSchemaColumnInfo(y);
        }
    }
}

public class PostTablesSchemaOutput
{
    [Required] public TableSchemaInfo Schema { get; set; }
    public PostTablesSchemaOutput(TableInfo name)
    {
        Schema = new TableSchemaInfo(name);
    }
}

public class ShemaInput
{
    [Required] public string name { get; set; }
}


[Route("/api/v1/tables/schema")]
public class ShemaController : Controller
{
    private readonly IHostApplicationLifetime _lifetime;

    public ShemaController(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }
    [HttpPost]
    public IActionResult Post([FromBody] ShemaInput input)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!GlocalServeiseContest.ContestSingleton.Instance.Tables.ContainsKey(input.name))
            return StatusCode(404);
        return StatusCode(200, new PostTablesSchemaOutput(GlocalServeiseContest.ContestSingleton.Instance.Tables[input.name]));
    }
}
