using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MknImmiSql.Api.V1.tables;

public class ListTables
{
    [Required] public string[] Tables { get; set; }
    public ListTables()
    {
        Tables = new string[GlocalServeiseContest.ContestSingleton.Instance.Tables.Count];
        int i = 0;
        foreach (var x in GlocalServeiseContest.ContestSingleton.Instance.Tables)
        {
            Tables[i] = x.Key;
            i++;
        }
    }
}

[Route("/api/v1/tables/list")]
public class ListController : Controller
{
    [HttpGet]
    public ListTables Get()
    {
        return new ListTables();
    }
}
