### API 404 Issues Encountered in Integration Tests

Certain endpoints are returning 404 Not Found during automated testing. The affected endpoints include:

- /api/scripts
- /api/repositories
- /api/executions
- /api/users
- /api/roles

This may require investigation of routing, controller registration, or test environment configuration.