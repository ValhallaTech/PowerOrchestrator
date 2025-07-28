using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PowerOrchestrator.API.DTOs;
using PowerOrchestrator.Application.Interfaces;

namespace PowerOrchestrator.API.Controllers;

/// <summary>
/// Controller for managing script executions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExecutionsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ExecutionsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ExecutionsController
    /// </summary>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="logger">The logger</param>
    public ExecutionsController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ExecutionsController> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all executions
    /// </summary>
    /// <param name="scriptId">Optional script ID to filter executions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of executions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExecutionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ExecutionDto>>> GetExecutions(
        [FromQuery] Guid? scriptId = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting executions {Filter}", scriptId.HasValue ? $"for script {scriptId}" : "for all scripts");
        
        var executions = scriptId.HasValue 
            ? await _unitOfWork.Executions.GetByScriptIdAsync(scriptId.Value, cancellationToken)
            : await _unitOfWork.Executions.GetAllAsync(cancellationToken);
            
        var executionDtos = _mapper.Map<IEnumerable<ExecutionDto>>(executions);
        
        return Ok(executionDtos);
    }

    /// <summary>
    /// Gets an execution by ID
    /// </summary>
    /// <param name="id">The execution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The execution</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExecutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutionDto>> GetExecution(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting execution with ID: {ExecutionId}", id);
        
        var execution = await _unitOfWork.Executions.GetByIdAsync(id, cancellationToken);
        if (execution == null)
        {
            _logger.LogWarning("Execution with ID {ExecutionId} not found", id);
            return NotFound();
        }

        var executionDto = _mapper.Map<ExecutionDto>(execution);
        return Ok(executionDto);
    }

    /// <summary>
    /// Gets the most recent executions for each script
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recent executions</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IEnumerable<ExecutionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ExecutionDto>>> GetRecentExecutions(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting recent executions");
        
        var recentExecutions = await _unitOfWork.Executions.GetRecentAsync(cancellationToken: cancellationToken);
        var executionDtos = _mapper.Map<IEnumerable<ExecutionDto>>(recentExecutions);
        
        return Ok(executionDtos);
    }
}