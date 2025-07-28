using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PowerOrchestrator.API.DTOs;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.API.Controllers;

/// <summary>
/// Controller for managing PowerShell scripts
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ScriptsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ScriptsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ScriptsController
    /// </summary>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="logger">The logger</param>
    public ScriptsController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ScriptsController> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all scripts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of scripts</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ScriptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ScriptDto>>> GetScripts(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all scripts");
        
        var scripts = await _unitOfWork.Scripts.GetAllAsync(cancellationToken);
        var scriptDtos = _mapper.Map<IEnumerable<ScriptDto>>(scripts);
        
        return Ok(scriptDtos);
    }

    /// <summary>
    /// Gets a script by ID
    /// </summary>
    /// <param name="id">The script ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The script</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ScriptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScriptDto>> GetScript(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting script with ID: {ScriptId}", id);
        
        var script = await _unitOfWork.Scripts.GetByIdAsync(id, cancellationToken);
        if (script == null)
        {
            _logger.LogWarning("Script with ID {ScriptId} not found", id);
            return NotFound();
        }

        var scriptDto = _mapper.Map<ScriptDto>(script);
        return Ok(scriptDto);
    }

    /// <summary>
    /// Creates a new script
    /// </summary>
    /// <param name="createScriptDto">The script data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created script</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ScriptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScriptDto>> CreateScript(CreateScriptDto createScriptDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new script: {ScriptName}", createScriptDto.Name);
        
        var script = _mapper.Map<Script>(createScriptDto);
        script.Id = Guid.NewGuid();
        
        await _unitOfWork.Scripts.AddAsync(script, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        var scriptDto = _mapper.Map<ScriptDto>(script);
        
        _logger.LogInformation("Script created with ID: {ScriptId}", script.Id);
        
        return CreatedAtAction(nameof(GetScript), new { id = script.Id }, scriptDto);
    }

    /// <summary>
    /// Updates an existing script
    /// </summary>
    /// <param name="id">The script ID</param>
    /// <param name="updateScriptDto">The updated script data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated script</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ScriptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScriptDto>> UpdateScript(Guid id, UpdateScriptDto updateScriptDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating script with ID: {ScriptId}", id);
        
        var existingScript = await _unitOfWork.Scripts.GetByIdAsync(id, cancellationToken);
        if (existingScript == null)
        {
            _logger.LogWarning("Script with ID {ScriptId} not found for update", id);
            return NotFound();
        }

        _mapper.Map(updateScriptDto, existingScript);
        _unitOfWork.Scripts.Update(existingScript);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        var scriptDto = _mapper.Map<ScriptDto>(existingScript);
        
        _logger.LogInformation("Script with ID {ScriptId} updated successfully", id);
        
        return Ok(scriptDto);
    }

    /// <summary>
    /// Deletes a script
    /// </summary>
    /// <param name="id">The script ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScript(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting script with ID: {ScriptId}", id);
        
        var script = await _unitOfWork.Scripts.GetByIdAsync(id, cancellationToken);
        if (script == null)
        {
            _logger.LogWarning("Script with ID {ScriptId} not found for deletion", id);
            return NotFound();
        }

        _unitOfWork.Scripts.Remove(script);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Script with ID {ScriptId} deleted successfully", id);
        
        return NoContent();
    }
}