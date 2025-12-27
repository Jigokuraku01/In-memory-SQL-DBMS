using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.ComponentModel.DataAnnotations;

namespace MknImmiSql.Api.V1;

public class TerminateInput
{
    [Required] public String token { get; set; }
}

[Route("/api/v1/terminate")]
public class TerminateController : Controller
{

    private readonly IHostApplicationLifetime _lifetime;

    public TerminateController(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }
    [HttpPost]
    public IActionResult Post([FromBody] TerminateInput input)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        String curToken = GlocalServeiseContest.ContestSingleton.Instance.token;
        if (input.token != curToken)
            return StatusCode(403);
        _lifetime.StopApplication();
        return Ok(200);
    }
}
