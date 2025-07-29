-- PowerOrchestrator Database Initialization Script
-- PostgreSQL 17.5 Development Environment

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements";

-- Create application schema
CREATE SCHEMA IF NOT EXISTS powerorchestrator;

-- Set default search path
ALTER DATABASE powerorchestrator_dev SET search_path TO powerorchestrator, public;

-- Create enum types
CREATE TYPE powerorchestrator.script_status AS ENUM (
    'draft',
    'active',
    'inactive',
    'archived'
);

CREATE TYPE powerorchestrator.execution_status AS ENUM (
    'pending',
    'running',
    'completed',
    'failed',
    'cancelled'
);

-- Basic application tables for Phase 0 validation

-- Scripts table
CREATE TABLE IF NOT EXISTS powerorchestrator.scripts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    content TEXT NOT NULL,
    version VARCHAR(50) NOT NULL DEFAULT '1.0.0',
    status powerorchestrator.script_status NOT NULL DEFAULT 'draft',
    is_active BOOLEAN NOT NULL DEFAULT true,
    timeout_seconds INTEGER NOT NULL DEFAULT 300,
    tags JSONB,
    metadata JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_by UUID NOT NULL,
    CONSTRAINT scripts_name_version_unique UNIQUE (name, version)
);

-- Executions table
CREATE TABLE IF NOT EXISTS powerorchestrator.executions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    script_id UUID NOT NULL REFERENCES powerorchestrator.scripts(id) ON DELETE CASCADE,
    status powerorchestrator.execution_status NOT NULL DEFAULT 'pending',
    parameters JSONB,
    result JSONB,
    output TEXT,
    error_output TEXT,
    started_at TIMESTAMP WITH TIME ZONE,
    completed_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID NOT NULL
);

-- Audit log table
CREATE TABLE IF NOT EXISTS powerorchestrator.audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    entity_type VARCHAR(100) NOT NULL,
    entity_id UUID NOT NULL,
    action VARCHAR(50) NOT NULL,
    old_values JSONB,
    new_values JSONB,
    user_id UUID NOT NULL,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    ip_address INET,
    user_agent TEXT
);

-- Health check table
CREATE TABLE IF NOT EXISTS powerorchestrator.health_checks (
    id SERIAL PRIMARY KEY,
    service_name VARCHAR(100) NOT NULL,
    status VARCHAR(20) NOT NULL,
    message TEXT,
    checked_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_scripts_status ON powerorchestrator.scripts(status);
CREATE INDEX IF NOT EXISTS idx_scripts_is_active ON powerorchestrator.scripts(is_active);
CREATE INDEX IF NOT EXISTS idx_scripts_created_at ON powerorchestrator.scripts(created_at);
CREATE INDEX IF NOT EXISTS idx_scripts_tags ON powerorchestrator.scripts USING gin(tags);

CREATE INDEX IF NOT EXISTS idx_executions_script_id ON powerorchestrator.executions(script_id);
CREATE INDEX IF NOT EXISTS idx_executions_status ON powerorchestrator.executions(status);
CREATE INDEX IF NOT EXISTS idx_executions_created_at ON powerorchestrator.executions(created_at);
CREATE INDEX IF NOT EXISTS idx_executions_completed_at ON powerorchestrator.executions(completed_at);

CREATE INDEX IF NOT EXISTS idx_audit_logs_entity ON powerorchestrator.audit_logs(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_timestamp ON powerorchestrator.audit_logs(timestamp);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON powerorchestrator.audit_logs(user_id);

-- Create materialized views for performance testing
CREATE MATERIALIZED VIEW IF NOT EXISTS powerorchestrator.mv_execution_statistics AS
SELECT 
    DATE_TRUNC('day', e.created_at) as execution_date,
    COUNT(*) as total_executions,
    COUNT(CASE WHEN e.status = 'completed' THEN 1 END) as successful_executions,
    COUNT(CASE WHEN e.status = 'failed' THEN 1 END) as failed_executions,
    COUNT(CASE WHEN e.status = 'cancelled' THEN 1 END) as cancelled_executions,
    ROUND(AVG(EXTRACT(EPOCH FROM (e.completed_at - e.started_at)) * 1000), 2) as avg_duration_ms,
    MAX(EXTRACT(EPOCH FROM (e.completed_at - e.started_at)) * 1000) as max_duration_ms,
    MIN(EXTRACT(EPOCH FROM (e.completed_at - e.started_at)) * 1000) as min_duration_ms,
    ROUND(STDDEV(EXTRACT(EPOCH FROM (e.completed_at - e.started_at)) * 1000), 2) as duration_stddev,
    ROUND(COUNT(CASE WHEN e.status = 'completed' THEN 1 END) * 100.0 / NULLIF(COUNT(*), 0), 2) as success_rate
FROM powerorchestrator.executions e
JOIN powerorchestrator.scripts s ON e.script_id = s.id
WHERE e.created_at >= NOW() - INTERVAL '90 days'
  AND e.completed_at IS NOT NULL AND e.started_at IS NOT NULL
GROUP BY DATE_TRUNC('day', e.created_at)
ORDER BY execution_date DESC;

CREATE MATERIALIZED VIEW IF NOT EXISTS powerorchestrator.mv_script_performance AS
SELECT 
    s.id as script_id,
    s.name as script_name,
    s.description as script_description,
    s.tags,
    s.is_active,
    COUNT(e.id) as total_executions,
    COUNT(CASE WHEN e.status = 'completed' THEN 1 END) as successful_executions,
    COUNT(CASE WHEN e.status = 'failed' THEN 1 END) as failed_executions,
    ROUND(AVG(EXTRACT(EPOCH FROM (e.completed_at - e.started_at)) * 1000), 2) as avg_duration_ms,
    ROUND(STDDEV(EXTRACT(EPOCH FROM (e.completed_at - e.started_at)) * 1000), 2) as duration_stddev,
    MAX(EXTRACT(EPOCH FROM (e.completed_at - e.started_at)) * 1000) as max_duration_ms,
    MIN(EXTRACT(EPOCH FROM (e.completed_at - e.started_at)) * 1000) as min_duration_ms,
    MAX(e.completed_at) as last_execution_time,
    ROUND(COUNT(CASE WHEN e.status = 'completed' THEN 1 END) * 100.0 / NULLIF(COUNT(e.id), 0), 2) as success_rate,
    ROUND(AVG(CASE WHEN e.status = 'completed' THEN EXTRACT(EPOCH FROM (e.completed_at - e.started_at)) * 1000 END), 2) as avg_success_duration_ms
FROM powerorchestrator.scripts s
LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id
WHERE s.is_active = true
  AND (e.created_at >= NOW() - INTERVAL '90 days' OR e.created_at IS NULL)
  AND (e.completed_at IS NOT NULL AND e.started_at IS NOT NULL OR e.id IS NULL)
GROUP BY s.id, s.name, s.description, s.tags, s.is_active
ORDER BY total_executions DESC NULLS LAST;

-- Create indexes on materialized views
CREATE INDEX IF NOT EXISTS idx_mv_execution_statistics_date ON powerorchestrator.mv_execution_statistics(execution_date);
CREATE INDEX IF NOT EXISTS idx_mv_script_performance_executions ON powerorchestrator.mv_script_performance(total_executions);
CREATE INDEX IF NOT EXISTS idx_mv_script_performance_last_exec ON powerorchestrator.mv_script_performance(last_execution_time);

-- Insert initial health check data
INSERT INTO powerorchestrator.health_checks (service_name, status, message) 
VALUES 
    ('database', 'healthy', 'PostgreSQL 17.5 initialized successfully'),
    ('application', 'pending', 'Waiting for application startup')
ON CONFLICT DO NOTHING;

-- Insert sample development data
INSERT INTO powerorchestrator.scripts (name, description, content, is_active, timeout_seconds, created_by, updated_by)
VALUES 
    ('hello-world', 'Basic PowerShell Hello World script', 'Write-Host "Hello, PowerOrchestrator!"', true, 30, uuid_generate_v4(), uuid_generate_v4()),
    ('system-info', 'Get system information', 'Get-ComputerInfo | Select-Object WindowsProductName, TotalPhysicalMemory, CsProcessors', true, 60, uuid_generate_v4(), uuid_generate_v4())
ON CONFLICT (name, version) DO NOTHING;

-- Grant permissions
GRANT USAGE ON SCHEMA powerorchestrator TO powerorch;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA powerorchestrator TO powerorch;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA powerorchestrator TO powerorch;

-- Log successful initialization
DO $$
BEGIN
    RAISE NOTICE 'PowerOrchestrator database initialized successfully at %', CURRENT_TIMESTAMP;
END $$;