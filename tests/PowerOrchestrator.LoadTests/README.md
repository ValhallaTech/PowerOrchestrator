# PowerOrchestrator Performance Tests

This directory contains comprehensive performance tests for the PowerOrchestrator Phase 1 implementation, validating database, caching, and query optimization requirements.

## Test Categories

### 1. Database Performance Tests (`DatabasePerformanceTests.cs`)
- **Script Pagination Performance**: Validates < 100ms response time for 50-item pagination
- **Concurrent Operations**: Tests 10+ simultaneous database operations
- **Large Dataset Performance**: Validates performance with 10k+ scripts and 50k+ executions
- **Memory Usage**: Ensures < 1GB memory usage during normal operations

### 2. Redis Cache Performance Tests (`RedisCachePerformanceTests.cs`)
- **Cache Operations**: Validates < 5ms response time for cache operations
- **Cache Hit Rate**: Tests and validates 95%+ cache hit rate
- **Memory Management**: Tests LRU eviction and memory efficiency
- **Concurrent Cache Operations**: Tests 1000+ concurrent cache operations

### 3. Dapper Query Optimization Tests (`DapperOptimizationTests.cs`)
- **Bulk Operations**: Tests bulk vs individual query performance
- **Query Optimization**: Validates proper indexing and query efficiency
- **Connection Management**: Tests connection pooling and reuse efficiency
- **Parameter Binding**: Validates security and performance of parameter binding

### 4. Materialized Views Performance Tests (`MaterializedViewPerformanceTests.cs`)
- **Performance Improvement**: Validates 50%+ improvement over direct queries
- **Refresh Efficiency**: Tests materialized view refresh performance
- **Automated Refresh**: Tests scheduled refresh procedures

## Performance Requirements (Phase 1)

| Component | Requirement | Test Validation |
|-----------|-------------|-----------------|
| Script Pagination | < 100ms for 50 items | ✅ Database Performance Tests |
| Concurrent Users | 10+ simultaneous operations | ✅ Database Performance Tests |
| Cache Hit Rate | > 95% for frequent data | ✅ Redis Performance Tests |
| Cache Operations | < 5ms response time | ✅ Redis Performance Tests |
| Memory Usage | < 1GB for normal operations | ✅ Database Performance Tests |
| Materialized Views | 50% improvement vs direct queries | ✅ Materialized View Tests |
| Concurrent Cache Ops | 1000+ operations supported | ✅ Redis Performance Tests |

## Running the Tests

### Prerequisites
1. PostgreSQL 17.5 running (docker-compose.dev.yml)
2. Redis 8.0.3 running (docker-compose.dev.yml)

### Start Services
```bash
docker-compose -f docker-compose.dev.yml up -d postgres redis
```

### Run All Performance Tests
```bash
dotnet test tests/PowerOrchestrator.LoadTests/PowerOrchestrator.LoadTests.csproj
```

### Run Specific Test Category
```bash
# Database performance tests
dotnet test --filter "DatabasePerformanceTests"

# Redis cache performance tests  
dotnet test --filter "RedisCachePerformanceTests"

# Dapper optimization tests
dotnet test --filter "DapperOptimizationTests"

# Materialized view tests
dotnet test --filter "MaterializedViewPerformanceTests"

# Phase 1 validation
dotnet test --filter "Phase1_Performance_Requirements_Should_Be_Met"
```

## Test Infrastructure

### Database Seeder (`Infrastructure/DatabaseSeeder.cs`)
- Generates 10,000+ test scripts with realistic data
- Creates 50,000+ test executions with various scenarios
- Optimized bulk insert operations for fast test data setup

### Performance Test Base (`Infrastructure/PerformanceTestBase.cs`)
- Common utilities for performance measurement
- Database and Redis connection management
- Service availability checking
- Accurate timing measurement with Stopwatch

## Performance Metrics Collected

- **Response Times**: All major operations timed with microsecond precision
- **Throughput**: Operations per second under various loads
- **Memory Usage**: .NET memory consumption during operations
- **Concurrency**: Performance under simultaneous user loads
- **Cache Efficiency**: Hit rates, eviction patterns, memory usage
- **Query Performance**: Execution plans, index usage, optimization effectiveness

## Test Data Generation

The tests use deterministic random generation with fixed seeds for reproducible results:
- **Script Data**: Realistic PowerShell scripts with metadata
- **Execution Data**: Various status types, durations, and outcomes
- **Cache Data**: JSON serialized objects mimicking real usage patterns
- **Parameter Data**: Complex query scenarios with proper parameter binding

## Integration with CI/CD

### Automated Performance Validation
The performance tests are fully integrated into the CI/CD pipeline as a separate job that runs after the main build and test phase. This provides several benefits:

- **Performance Regression Detection**: Every code change is validated against Phase 1 performance requirements
- **Enterprise Standards Enforcement**: Ensures deployment readiness by validating all enterprise performance criteria
- **Non-Blocking Architecture**: Performance tests run in parallel/after main tests, maintaining fast feedback loops
- **Service Availability Handling**: Tests gracefully skip when PostgreSQL/Redis services are unavailable

### CI/CD Pipeline Structure
```yaml
build-and-test -> performance-tests -> [build-docker, foundation-validation]
```

### Performance Requirements Validated in CI
| Requirement | Threshold | Test Method |
|-------------|-----------|-------------|
| Pagination Response | < 100ms for 50 items | Automated timing validation |
| Concurrent Operations | 10+ simultaneous users | Load testing with 15+ concurrent operations |
| Cache Response Time | < 5ms operations | Redis performance measurement |
| Cache Hit Rate | > 95% for frequent data | Cache efficiency validation |
| Memory Usage | < 1GB normal operations | .NET memory monitoring |
| Materialized Views | 50% performance improvement | Query comparison testing |

### Running in CI Environment
The tests are designed to:
- Run in containerized environments (PostgreSQL 17.5 + Redis 8.0.3)
- Provide clear pass/fail criteria based on performance requirements
- Generate detailed performance reports with microsecond precision
- Validate performance regression prevention through baseline comparisons
- Handle resource constraints typical in CI environments

### Artifacts Generated
- Performance test results (TRX format)
- Detailed timing measurements
- Memory usage reports
- Cache efficiency metrics