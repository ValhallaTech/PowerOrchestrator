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
    script_id UUID NOT NULL REFERENCES powerorchestrator.scripts(id),
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
CREATE INDEX IF NOT EXISTS idx_scripts_created_at ON powerorchestrator.scripts(created_at);
CREATE INDEX IF NOT EXISTS idx_scripts_tags ON powerorchestrator.scripts USING gin(tags);

CREATE INDEX IF NOT EXISTS idx_executions_script_id ON powerorchestrator.executions(script_id);
CREATE INDEX IF NOT EXISTS idx_executions_status ON powerorchestrator.executions(status);
CREATE INDEX IF NOT EXISTS idx_executions_created_at ON powerorchestrator.executions(created_at);

CREATE INDEX IF NOT EXISTS idx_audit_logs_entity ON powerorchestrator.audit_logs(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_timestamp ON powerorchestrator.audit_logs(timestamp);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON powerorchestrator.audit_logs(user_id);

-- Insert initial health check data
INSERT INTO powerorchestrator.health_checks (service_name, status, message) 
VALUES 
    ('database', 'healthy', 'PostgreSQL 17.5 initialized successfully'),
    ('application', 'pending', 'Waiting for application startup')
ON CONFLICT DO NOTHING;

-- Insert sample development data
INSERT INTO powerorchestrator.scripts (name, description, content, created_by, updated_by)
VALUES 
    ('hello-world', 'Basic PowerShell Hello World script', 'Write-Host "Hello, PowerOrchestrator!"', uuid_generate_v4(), uuid_generate_v4()),
    ('system-info', 'Get system information', 'Get-ComputerInfo | Select-Object WindowsProductName, TotalPhysicalMemory, CsProcessors', uuid_generate_v4(), uuid_generate_v4())
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