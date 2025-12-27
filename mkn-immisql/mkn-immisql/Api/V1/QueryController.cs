using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MknImmiSql.Api.V1.tables;
using MknImmiSql.Parser;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MknImmiSql.Api.V1;




public class QueryInput
{
    [Required] public String query { get; set; }
}


public class QueryShemaOutput
{
    [Required] public TableSchemaInfo Schema { get; set; }
    [Required] public object[][] result { get; set; }

    public QueryShemaOutput(TableQueryInfo x)
    {
        Schema = new TableSchemaInfo(x.TableInfo);

        List<object[]> info = new List<object[]>();
        for (int i = 0; i < x.TableInfo.TableRowsInfo.Count; ++i)
        {
            List<object> tmp = new List<object>();
            for (int j = 0; j < x.TableInfo.TableColumnInfo.Count; ++j)
                tmp.Add(x.TableInfo.TableRowsInfo[i].CurrentRowObjects[j]?.ToString());
            info.Add(tmp.ToArray());
        }
        result = info.ToArray();
    }
}


[Route("/api/v1/query")]
public class QueryController : Controller
{
    private readonly IHostApplicationLifetime _lifetime;

    public QueryController(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }
    [HttpPost]
    public IActionResult Post([FromBody] QueryInput input)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        if (input.query[input.query.Length - 1] == ';')
            input.query = input.query.Substring(0, input.query.Length - 1);
        var x = CentralParser.ParseQuery(input.query);
        if (x.Code == 400)
            return BadRequest();
        return StatusCode(x.Code, new QueryShemaOutput(x));
    }

}
