using Microsoft.Extensions.Logging;

namespace PowerOrchestrator.MAUI.Services;

#if !NET8_0

/// <summary>
/// Responsive design service interface for cross-platform UI optimization
/// </summary>
public interface IResponsiveDesignService
{
    /// <summary>
    /// Gets the current breakpoint based on screen size
    /// </summary>
    BreakpointType CurrentBreakpoint { get; }

    /// <summary>
    /// Gets responsive spacing for the current device
    /// </summary>
    /// <param name="baseSpacing">Base spacing value</param>
    /// <returns>Responsive spacing value</returns>
    double GetResponsiveSpacing(double baseSpacing);

    /// <summary>
    /// Gets responsive font size for the current device
    /// </summary>
    /// <param name="baseFontSize">Base font size</param>
    /// <returns>Responsive font size</returns>
    double GetResponsiveFontSize(double baseFontSize);

    /// <summary>
    /// Gets responsive grid columns for the current device
    /// </summary>
    /// <param name="maxColumns">Maximum columns for desktop</param>
    /// <returns>Number of columns for current device</returns>
    int GetResponsiveColumns(int maxColumns);

    /// <summary>
    /// Gets responsive margin for the current device
    /// </summary>
    /// <param name="baseMargin">Base margin value</param>
    /// <returns>Responsive margin</returns>
    Thickness GetResponsiveMargin(double baseMargin);

    /// <summary>
    /// Gets responsive padding for the current device
    /// </summary>
    /// <param name="basePadding">Base padding value</param>
    /// <returns>Responsive padding</returns>
    Thickness GetResponsivePadding(double basePadding);

    /// <summary>
    /// Determines if compact mode should be used
    /// </summary>
    /// <returns>True if compact mode should be used</returns>
    bool ShouldUseCompactMode();

    /// <summary>
    /// Event fired when screen size changes
    /// </summary>
    event EventHandler<BreakpointType>? BreakpointChanged;
}

/// <summary>
/// Responsive design breakpoint types
/// </summary>
public enum BreakpointType
{
    /// <summary>
    /// Mobile phones (up to 600dp)
    /// </summary>
    Mobile,

    /// <summary>
    /// Tablets in portrait (600-900dp)
    /// </summary>
    TabletPortrait,

    /// <summary>
    /// Tablets in landscape (900-1200dp)
    /// </summary>
    TabletLandscape,

    /// <summary>
    /// Desktop and large tablets (1200dp+)
    /// </summary>
    Desktop
}

/// <summary>
/// Responsive design service implementation
/// </summary>
public class ResponsiveDesignService : IResponsiveDesignService
{
    private readonly ILogger<ResponsiveDesignService> _logger;
    private readonly IPlatformService _platformService;
    private BreakpointType _currentBreakpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponsiveDesignService"/> class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="platformService">The platform service</param>
    public ResponsiveDesignService(
        ILogger<ResponsiveDesignService> logger,
        IPlatformService platformService)
    {
        _logger = logger;
        _platformService = platformService;
        _currentBreakpoint = CalculateCurrentBreakpoint();

        _logger.LogInformation("ResponsiveDesignService initialized with breakpoint: {Breakpoint}", _currentBreakpoint);
    }

    /// <inheritdoc/>
    public BreakpointType CurrentBreakpoint => _currentBreakpoint;

    /// <inheritdoc/>
    public event EventHandler<BreakpointType>? BreakpointChanged;

    /// <inheritdoc/>
    public double GetResponsiveSpacing(double baseSpacing)
    {
        try
        {
            var scale = GetScaleFactor();
            var responsiveSpacing = baseSpacing * scale;

            _logger.LogDebug("Responsive spacing: {Base} -> {Responsive} (scale: {Scale})", 
                baseSpacing, responsiveSpacing, scale);

            return responsiveSpacing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating responsive spacing");
            return baseSpacing;
        }
    }

    /// <inheritdoc/>
    public double GetResponsiveFontSize(double baseFontSize)
    {
        try
        {
            var scale = GetFontScaleFactor();
            var responsiveFontSize = baseFontSize * scale;

            _logger.LogDebug("Responsive font size: {Base} -> {Responsive} (scale: {Scale})", 
                baseFontSize, responsiveFontSize, scale);

            return responsiveFontSize;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating responsive font size");
            return baseFontSize;
        }
    }

    /// <inheritdoc/>
    public int GetResponsiveColumns(int maxColumns)
    {
        try
        {
            var columns = _currentBreakpoint switch
            {
                BreakpointType.Mobile => Math.Min(maxColumns, 1),
                BreakpointType.TabletPortrait => Math.Min(maxColumns, 2),
                BreakpointType.TabletLandscape => Math.Min(maxColumns, 3),
                BreakpointType.Desktop => maxColumns,
                _ => 1
            };

            _logger.LogDebug("Responsive columns: {Max} -> {Responsive} for {Breakpoint}", 
                maxColumns, columns, _currentBreakpoint);

            return columns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating responsive columns");
            return 1;
        }
    }

    /// <inheritdoc/>
    public Thickness GetResponsiveMargin(double baseMargin)
    {
        try
        {
            var scale = GetScaleFactor();
            var margin = baseMargin * scale;

            return new Thickness(margin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating responsive margin");
            return new Thickness(baseMargin);
        }
    }

    /// <inheritdoc/>
    public Thickness GetResponsivePadding(double basePadding)
    {
        try
        {
            var scale = GetScaleFactor();
            var padding = basePadding * scale;

            return new Thickness(padding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating responsive padding");
            return new Thickness(basePadding);
        }
    }

    /// <inheritdoc/>
    public bool ShouldUseCompactMode()
    {
        try
        {
            var compactMode = _currentBreakpoint == BreakpointType.Mobile ||
                             (_currentBreakpoint == BreakpointType.TabletPortrait && _platformService.DeviceIdiom == DeviceIdiomType.Phone);

            _logger.LogDebug("Should use compact mode: {CompactMode} for {Breakpoint}", compactMode, _currentBreakpoint);

            return compactMode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining compact mode");
            return true; // Default to compact mode on error
        }
    }

    /// <summary>
    /// Updates the current breakpoint and fires change event if needed
    /// </summary>
    /// <param name="width">The current screen width</param>
    /// <param name="height">The current screen height</param>
    public void UpdateBreakpoint(double width, double height)
    {
        try
        {
            var newBreakpoint = CalculateBreakpoint(width, height);
            
            if (newBreakpoint != _currentBreakpoint)
            {
                var oldBreakpoint = _currentBreakpoint;
                _currentBreakpoint = newBreakpoint;

                _logger.LogInformation("Breakpoint changed from {Old} to {New} (size: {Width}x{Height})", 
                    oldBreakpoint, newBreakpoint, width, height);

                BreakpointChanged?.Invoke(this, newBreakpoint);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating breakpoint");
        }
    }

    /// <summary>
    /// Calculates the current breakpoint based on device characteristics
    /// </summary>
    /// <returns>The current breakpoint</returns>
    private BreakpointType CalculateCurrentBreakpoint()
    {
        try
        {
#if !NET8_0
            var mainDisplay = DeviceDisplay.Current.MainDisplayInfo;
            var widthDp = mainDisplay.Width / mainDisplay.Density;
            var heightDp = mainDisplay.Height / mainDisplay.Density;

            return CalculateBreakpoint(widthDp, heightDp);
#else
            // Console mode - assume desktop
            return BreakpointType.Desktop;
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating current breakpoint");
            return BreakpointType.Desktop; // Safe default
        }
    }

    /// <summary>
    /// Calculates breakpoint based on screen dimensions
    /// </summary>
    /// <param name="width">Screen width in density-independent pixels</param>
    /// <param name="height">Screen height in density-independent pixels</param>
    /// <returns>The appropriate breakpoint</returns>
    private BreakpointType CalculateBreakpoint(double width, double height)
    {
        // Use the smaller dimension to handle orientation changes
        var smallerDimension = Math.Min(width, height);

        return smallerDimension switch
        {
            < 600 => BreakpointType.Mobile,
            < 900 => BreakpointType.TabletPortrait,
            < 1200 => BreakpointType.TabletLandscape,
            _ => BreakpointType.Desktop
        };
    }

    /// <summary>
    /// Gets the scale factor for spacing and sizing
    /// </summary>
    /// <returns>Scale factor</returns>
    private double GetScaleFactor()
    {
        return _currentBreakpoint switch
        {
            BreakpointType.Mobile => 0.8,
            BreakpointType.TabletPortrait => 1.0,
            BreakpointType.TabletLandscape => 1.1,
            BreakpointType.Desktop => 1.2,
            _ => 1.0
        };
    }

    /// <summary>
    /// Gets the scale factor for fonts
    /// </summary>
    /// <returns>Font scale factor</returns>
    private double GetFontScaleFactor()
    {
        var baseScale = GetScaleFactor();
        var displayScale = _platformService.GetDisplayScaling();

        // Adjust font scaling based on device density
        return baseScale * Math.Min(displayScale, 1.5); // Cap at 1.5x to prevent overly large fonts
    }
}
#else
/// <summary>
/// Console mode stubs for responsive design service
/// </summary>
public interface IResponsiveDesignService
{
    BreakpointType CurrentBreakpoint { get; }
    double GetResponsiveSpacing(double baseSpacing);
    double GetResponsiveFontSize(double baseFontSize);
    int GetResponsiveColumns(int maxColumns);
    bool ShouldUseCompactMode();
}

public enum BreakpointType { Desktop }

public class ResponsiveDesignService : IResponsiveDesignService
{
    public BreakpointType CurrentBreakpoint => BreakpointType.Desktop;
    public double GetResponsiveSpacing(double baseSpacing) => baseSpacing;
    public double GetResponsiveFontSize(double baseFontSize) => baseFontSize;
    public int GetResponsiveColumns(int maxColumns) => maxColumns;
    public bool ShouldUseCompactMode() => false;
}
#endif