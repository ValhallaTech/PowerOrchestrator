using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PowerOrchestrator.API.DTOs;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Services;

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
    private readonly IPowerShellExecutionService _executionService;
    private readonly IMapper _mapper;
    private readonly ILogger<ExecutionsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ExecutionsController
    /// </summary>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="executionService">The PowerShell execution service</param>
    /// <param name="mapper">The AutoMapper instance</param>
    /// <param name="logger">The logger</param>
    public ExecutionsController(
        IUnitOfWork unitOfWork, 
        IPowerShellExecutionService executionService,
        IMapper mapper, 
        ILogger<ExecutionsController> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));
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

    /// <summary>
    /// Executes a script by ID
    /// </summary>
    /// <param name="scriptId">The script ID to execute</param>
    /// <param name="parameters">Optional execution parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The execution ID for tracking</returns>
    [HttpPost("execute/{scriptId:guid}")]
    [ProducesResponseType(typeof(ExecutionResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutionResponseDto>> ExecuteScript(
        Guid scriptId, 
        [FromBody] Dictionary<string, object>? parameters = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing script {ScriptId}", scriptId);

        try
        {
            // Validate the execution first
            var validation = await _executionService.ValidateExecutionAsync(scriptId, parameters, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(new { Errors = validation.Errors, Warnings = validation.Warnings });
            }

            var executionId = await _executionService.ExecuteScriptAsync(scriptId, parameters, cancellationToken);
            
            var response = new ExecutionResponseDto
            {
                ExecutionId = executionId,
                Status = "Accepted",
                Message = "Script execution started successfully"
            };

            return Accepted($"/api/executions/{executionId}", response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid script ID {ScriptId}", scriptId);
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting script execution for {ScriptId}", scriptId);
            return BadRequest(new { Message = "Failed to start script execution", Error = ex.Message });
        }
    }

    /// <summary>
    /// Executes script content directly
    /// </summary>
    /// <param name="request">Script execution request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The execution ID for tracking</returns>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(ExecutionResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExecutionResponseDto>> ExecuteScriptContent(
        [FromBody] ExecuteScriptDto request, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ScriptContent))
        {
            return BadRequest(new { Message = "Script content is required" });
        }

        _logger.LogInformation("Executing script content");

        try
        {
            var executionId = await _executionService.ExecuteScriptContentAsync(request.ScriptContent, request.Parameters, cancellationToken);
            
            var response = new ExecutionResponseDto
            {
                ExecutionId = executionId,
                Status = "Accepted",
                Message = "Script execution started successfully"
            };

            return Accepted($"/api/executions/{executionId}", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting script content execution");
            return BadRequest(new { Message = "Failed to start script execution", Error = ex.Message });
        }
    }

    /// <summary>
    /// Cancels a running execution
    /// </summary>
    /// <param name="id">The execution ID to cancel</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the cancellation</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ExecutionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutionResponseDto>> CancelExecution(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling execution {ExecutionId}", id);

        var success = await _executionService.CancelExecutionAsync(id, cancellationToken);
        
        if (!success)
        {
            return NotFound(new { Message = "Execution not found or could not be cancelled" });
        }

        var response = new ExecutionResponseDto
        {
            ExecutionId = id,
            Status = "Cancelled",
            Message = "Execution cancelled successfully"
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets the execution metrics for an execution
    /// </summary>
    /// <param name="id">The execution ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution metrics</returns>
    [HttpGet("{id:guid}/metrics")]
    [ProducesResponseType(typeof(ExecutionMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutionMetricsDto>> GetExecutionMetrics(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting metrics for execution {ExecutionId}", id);

        var metrics = await _executionService.GetExecutionMetricsAsync(id, cancellationToken);
        if (metrics == null)
        {
            return NotFound(new { Message = "Execution not found" });
        }

        var metricsDto = _mapper.Map<ExecutionMetricsDto>(metrics);
        return Ok(metricsDto);
    }

    /// <summary>
    /// Gets all currently running executions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of running executions</returns>
    [HttpGet("running")]
    [ProducesResponseType(typeof(IEnumerable<ExecutionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ExecutionDto>>> GetRunningExecutions(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting running executions");
        
        var runningExecutions = await _executionService.GetRunningExecutionsAsync(cancellationToken);
        var executionDtos = _mapper.Map<IEnumerable<ExecutionDto>>(runningExecutions);
        
        return Ok(executionDtos);
    }
}