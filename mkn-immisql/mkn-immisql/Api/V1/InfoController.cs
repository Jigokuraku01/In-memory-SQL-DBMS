using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace MknImmiSql.Api.V1;

public class ServiceInfo
{
    [Required] public String Timestamp { get; }
    [Required] public Int32 ProcessId { get; }
    [Required] public String TerminationToken { get; }

    public ServiceInfo()
    {
        Timestamp = DateTime.Now.ToString("O");
        ProcessId = Environment.ProcessId;
        TerminationToken = GlocalServeiseContest.ContestSingleton.Instance.token;
    }
}

[Route("/api/v1/info")]
public class InfoController : Controller
{
    [HttpGet]
    public ServiceInfo Get()
    {
        return new ServiceInfo();
    }
}
