using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Interfaces;

namespace RVM.LogStream.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RetentionController(
    IRetentionPolicyRepository policyRepo,
    ILogger<RetentionController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RetentionPolicyResponse>>> GetAll(CancellationToken ct)
    {
        var policies = await policyRepo.GetAllAsync(ct);
        return policies.Select(MapToResponse).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RetentionPolicyResponse>> GetById(Guid id, CancellationToken ct)
    {
        var policy = await policyRepo.GetByIdAsync(id, ct);
        if (policy is null) return NotFound();
        return MapToResponse(policy);
    }

    [HttpPost]
    public async Task<ActionResult<RetentionPolicyResponse>> Create(CreateRetentionPolicyRequest request, CancellationToken ct)
    {
        var policy = new RetentionPolicy
        {
            SourcePattern = request.SourcePattern,
            RetentionDays = request.RetentionDays,
            IsEnabled = request.IsEnabled,
        };

        await policyRepo.AddAsync(policy, ct);
        logger.LogInformation("Created retention policy for '{Pattern}' ({Days}d)", policy.SourcePattern, policy.RetentionDays);

        return CreatedAtAction(nameof(GetById), new { id = policy.Id }, MapToResponse(policy));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RetentionPolicyResponse>> Update(Guid id, UpdateRetentionPolicyRequest request, CancellationToken ct)
    {
        var policy = await policyRepo.GetByIdAsync(id, ct);
        if (policy is null) return NotFound();

        if (request.SourcePattern is not null) policy.SourcePattern = request.SourcePattern;
        if (request.RetentionDays.HasValue) policy.RetentionDays = request.RetentionDays.Value;
        if (request.IsEnabled.HasValue) policy.IsEnabled = request.IsEnabled.Value;

        await policyRepo.UpdateAsync(policy, ct);
        return MapToResponse(policy);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var policy = await policyRepo.GetByIdAsync(id, ct);
        if (policy is null) return NotFound();

        await policyRepo.DeleteAsync(id, ct);
        return NoContent();
    }

    private static RetentionPolicyResponse MapToResponse(RetentionPolicy p) => new(
        p.Id, p.SourcePattern, p.RetentionDays, p.IsEnabled, p.CreatedAt, p.UpdatedAt);
}
