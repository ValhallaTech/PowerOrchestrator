-- PowerOrchestrator Database Initialization Script
-- PostgreSQL 17.5 Development Environment

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements";

-- Create application schema
CREATE SCHEMA IF NOT EXISTS powerorchestrator;

-- Set default search path for both dev and test databases
-- This will apply to the database we're currently connected to
SET search_path TO powerorchestrator, public;

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

CREATE TYPE powerorchestrator.repository_status AS ENUM (
    'active',
    'inactive',
    'error',
    'syncing'
);

-- Basic application tables for Phase 0 validation

-- Users table (extends ASP.NET Core Identity)
CREATE TABLE IF NOT EXISTS powerorchestrator.users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_name VARCHAR(256),
    normalized_user_name VARCHAR(256),
    email VARCHAR(256),
    normalized_email VARCHAR(256),
    email_confirmed BOOLEAN NOT NULL DEFAULT false,
    password_hash TEXT,
    security_stamp TEXT,
    concurrency_stamp TEXT,
    phone_number TEXT,
    phone_number_confirmed BOOLEAN NOT NULL DEFAULT false,
    two_factor_enabled BOOLEAN NOT NULL DEFAULT false,
    lockout_end TIMESTAMP WITH TIME ZONE,
    lockout_enabled BOOLEAN NOT NULL DEFAULT false,
    access_failed_count INTEGER NOT NULL DEFAULT 0,
    first_name VARCHAR(100) NOT NULL DEFAULT '',
    last_name VARCHAR(100) NOT NULL DEFAULT '',
    is_mfa_enabled BOOLEAN NOT NULL DEFAULT false,
    mfa_secret VARCHAR(255),
    last_login_at TIMESTAMP WITH TIME ZONE,
    last_login_ip VARCHAR(45),
    failed_login_attempts INTEGER NOT NULL DEFAULT 0,
    locked_until TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(255) NOT NULL DEFAULT '',
    updated_by VARCHAR(255) NOT NULL DEFAULT '',
    row_version BYTEA
);

-- Roles table (extends ASP.NET Core Identity)
CREATE TABLE IF NOT EXISTS powerorchestrator.roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(256),
    normalized_name VARCHAR(256),
    concurrency_stamp TEXT,
    description VARCHAR(500) NOT NULL DEFAULT '',
    is_system_role BOOLEAN NOT NULL DEFAULT false,
    permissions TEXT NOT NULL DEFAULT '[]',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(255) NOT NULL DEFAULT '',
    updated_by VARCHAR(255) NOT NULL DEFAULT '',
    row_version BYTEA
);

-- User roles junction table
CREATE TABLE IF NOT EXISTS powerorchestrator.user_roles (
    user_id UUID NOT NULL REFERENCES powerorchestrator.users(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES powerorchestrator.roles(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);

-- GitHub repositories table
CREATE TABLE IF NOT EXISTS powerorchestrator.github_repositories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    owner VARCHAR(100) NOT NULL,
    name VARCHAR(100) NOT NULL,
    full_name VARCHAR(200) NOT NULL,
    description TEXT,
    is_private BOOLEAN NOT NULL DEFAULT false,
    default_branch VARCHAR(50) NOT NULL DEFAULT 'main',
    last_sync_at TIMESTAMP WITH TIME ZONE,
    status powerorchestrator.repository_status NOT NULL DEFAULT 'active',
    configuration TEXT NOT NULL DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    row_version BYTEA,
    CONSTRAINT github_repositories_full_name_unique UNIQUE (full_name)
);

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
    parameters_schema JSONB,
    tags JSONB,
    metadata JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_by UUID NOT NULL,
    CONSTRAINT scripts_name_version_unique UNIQUE (name, version)
);

-- Repository scripts junction table
CREATE TABLE IF NOT EXISTS powerorchestrator.repository_scripts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    repository_id UUID NOT NULL REFERENCES powerorchestrator.github_repositories(id) ON DELETE CASCADE,
    script_id UUID NOT NULL REFERENCES powerorchestrator.scripts(id) ON DELETE CASCADE,
    relative_path VARCHAR(500) NOT NULL,
    discovered_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT repository_scripts_unique UNIQUE (repository_id, script_id, relative_path)
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
-- Add unique constraints required for ON CONFLICT clauses using proper PostgreSQL syntax
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_name = 'uq_users_normalized_email'
        AND table_name = 'users'
        AND table_schema = 'powerorchestrator'
    ) THEN
        ALTER TABLE powerorchestrator.users ADD CONSTRAINT uq_users_normalized_email UNIQUE (normalized_email);
    END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_name = 'uq_users_normalized_user_name'
        AND table_name = 'users'
        AND table_schema = 'powerorchestrator'
    ) THEN
        ALTER TABLE powerorchestrator.users ADD CONSTRAINT uq_users_normalized_user_name UNIQUE (normalized_user_name);
    END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_name = 'uq_roles_normalized_name'
        AND table_name = 'roles'
        AND table_schema = 'powerorchestrator'
    ) THEN
        ALTER TABLE powerorchestrator.roles ADD CONSTRAINT uq_roles_normalized_name UNIQUE (normalized_name);
    END IF;
END
$$;

CREATE INDEX IF NOT EXISTS idx_users_email ON powerorchestrator.users(email);
CREATE INDEX IF NOT EXISTS idx_users_normalized_email ON powerorchestrator.users(normalized_email);
CREATE INDEX IF NOT EXISTS idx_users_normalized_user_name ON powerorchestrator.users(normalized_user_name);

CREATE INDEX IF NOT EXISTS idx_roles_normalized_name ON powerorchestrator.roles(normalized_name);

CREATE INDEX IF NOT EXISTS idx_github_repositories_owner ON powerorchestrator.github_repositories(owner);
CREATE INDEX IF NOT EXISTS idx_github_repositories_status ON powerorchestrator.github_repositories(status);
CREATE INDEX IF NOT EXISTS idx_github_repositories_last_sync ON powerorchestrator.github_repositories(last_sync_at);

CREATE INDEX IF NOT EXISTS idx_scripts_status ON powerorchestrator.scripts(status);
CREATE INDEX IF NOT EXISTS idx_scripts_is_active ON powerorchestrator.scripts(is_active);
CREATE INDEX IF NOT EXISTS idx_scripts_created_at ON powerorchestrator.scripts(created_at);
CREATE INDEX IF NOT EXISTS idx_scripts_tags ON powerorchestrator.scripts USING gin(tags);

CREATE INDEX IF NOT EXISTS idx_executions_script_id ON powerorchestrator.executions(script_id);
CREATE INDEX IF NOT EXISTS idx_executions_status ON powerorchestrator.executions(status);
CREATE INDEX IF NOT EXISTS idx_executions_created_at ON powerorchestrator.executions(created_at);
CREATE INDEX IF NOT EXISTS idx_executions_completed_at ON powerorchestrator.executions(completed_at);

CREATE INDEX IF NOT EXISTS idx_repository_scripts_repository_id ON powerorchestrator.repository_scripts(repository_id);
CREATE INDEX IF NOT EXISTS idx_repository_scripts_script_id ON powerorchestrator.repository_scripts(script_id);
CREATE INDEX IF NOT EXISTS idx_repository_scripts_path ON powerorchestrator.repository_scripts(relative_path);

CREATE INDEX IF NOT EXISTS idx_audit_logs_entity ON powerorchestrator.audit_logs(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_timestamp ON powerorchestrator.audit_logs(timestamp);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON powerorchestrator.audit_logs(user_id);

-- Create materialized views for performance testing
CREATE MATERIALIZED VIEW IF NOT EXISTS powerorchestrator.mv_execution_statistics AS
SELECT 
    e.script_id,
    s.name as script_name,
    COUNT(*) as total_executions,
    COUNT(CASE WHEN e.status = 'completed' THEN 1 END) as successful_executions,
    COUNT(CASE WHEN e.status = 'failed' THEN 1 END) as failed_executions,
    AVG(EXTRACT(EPOCH FROM (e.completed_at - e.started_at))) as avg_duration_seconds,
    MAX(e.completed_at) as last_execution,
    MIN(e.created_at) as first_execution
FROM powerorchestrator.executions e
JOIN powerorchestrator.scripts s ON e.script_id = s.id
WHERE e.completed_at IS NOT NULL AND e.started_at IS NOT NULL
GROUP BY e.script_id, s.name;

CREATE MATERIALIZED VIEW IF NOT EXISTS powerorchestrator.mv_script_performance AS
SELECT 
    s.id,
    s.name,
    s.description,
    s.tags,
    s.is_active,
    COUNT(e.id) as execution_count,
    MAX(e.completed_at) as last_execution,
    AVG(EXTRACT(EPOCH FROM (e.completed_at - e.started_at))) as avg_duration_seconds
FROM powerorchestrator.scripts s
LEFT JOIN powerorchestrator.executions e ON s.id = e.script_id 
    AND e.completed_at IS NOT NULL AND e.started_at IS NOT NULL
WHERE s.is_active = true
GROUP BY s.id, s.name, s.description, s.tags, s.is_active;

-- Create indexes on materialized views
CREATE INDEX IF NOT EXISTS idx_mv_execution_statistics_script_id ON powerorchestrator.mv_execution_statistics(script_id);
CREATE INDEX IF NOT EXISTS idx_mv_script_performance_id ON powerorchestrator.mv_script_performance(id);

-- Insert initial health check data
INSERT INTO powerorchestrator.health_checks (service_name, status, message) 
VALUES 
    ('database', 'healthy', 'PostgreSQL 17.5 initialized successfully'),
    ('application', 'pending', 'Waiting for application startup')
ON CONFLICT DO NOTHING;

-- Insert sample development data

-- Insert sample roles
INSERT INTO powerorchestrator.roles (id, name, normalized_name, description, is_system_role, permissions)
VALUES 
    (uuid_generate_v4(), 'Administrator', 'ADMINISTRATOR', 'System administrator with full access', true, '["*"]'),
    (uuid_generate_v4(), 'PowerUser', 'POWERUSER', 'Power user with script management access', false, '["scripts.read", "scripts.write", "executions.read"]'),
    (uuid_generate_v4(), 'ReadOnly', 'READONLY', 'Read-only access to scripts and executions', false, '["scripts.read", "executions.read"]')
ON CONFLICT (normalized_name) DO NOTHING;

-- Insert sample users  
INSERT INTO powerorchestrator.users (id, user_name, normalized_user_name, email, normalized_email, email_confirmed, first_name, last_name)
VALUES 
    (uuid_generate_v4(), 'admin', 'ADMIN', 'admin@powerorchestrator.com', 'ADMIN@POWERORCHESTRATOR.COM', true, 'System', 'Administrator'),
    (uuid_generate_v4(), 'poweruser', 'POWERUSER', 'poweruser@powerorchestrator.com', 'POWERUSER@POWERORCHESTRATOR.COM', true, 'Power', 'User'),
    (uuid_generate_v4(), 'readonly', 'READONLY', 'readonly@powerorchestrator.com', 'READONLY@POWERORCHESTRATOR.COM', true, 'Read', 'Only')
ON CONFLICT (normalized_email) DO NOTHING;

-- Insert sample repositories
INSERT INTO powerorchestrator.github_repositories (owner, name, full_name, description, is_private, created_by, updated_by)
VALUES 
    ('PowerShell', 'PowerShell', 'PowerShell/PowerShell', 'PowerShell for every system!', false, uuid_generate_v4(), uuid_generate_v4()),
    ('microsoft', 'PowerToys', 'microsoft/PowerToys', 'Windows system utilities to maximize productivity', false, uuid_generate_v4(), uuid_generate_v4()),
    ('PowerShell', 'PSReadLine', 'PowerShell/PSReadLine', 'A bash inspired readline implementation for PowerShell', false, uuid_generate_v4(), uuid_generate_v4())
ON CONFLICT (full_name) DO NOTHING;

-- Insert sample scripts
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