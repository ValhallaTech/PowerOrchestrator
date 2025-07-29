-- Migration script to add ON DELETE CASCADE to executions.script_id foreign key
-- This fixes test failures caused by constraint violations during cleanup

BEGIN;

-- Drop existing foreign key constraint
ALTER TABLE powerorchestrator.executions 
DROP CONSTRAINT IF EXISTS executions_script_id_fkey;

-- Add new foreign key constraint with ON DELETE CASCADE
ALTER TABLE powerorchestrator.executions 
ADD CONSTRAINT executions_script_id_fkey 
FOREIGN KEY (script_id) REFERENCES powerorchestrator.scripts(id) ON DELETE CASCADE;

-- Log the migration
INSERT INTO powerorchestrator.audit_logs (id, entity_type, entity_id, action, old_values, new_values, user_id, timestamp)
VALUES (
    uuid_generate_v4(), 
    'Schema', 
    uuid_generate_v4(), 
    'Migration', 
    '{"constraint": "executions_script_id_fkey", "delete_action": "NO ACTION"}'::jsonb,
    '{"constraint": "executions_script_id_fkey", "delete_action": "CASCADE"}'::jsonb,
    uuid_generate_v4(), 
    CURRENT_TIMESTAMP
);

COMMIT;

-- Verify the constraint was updated
\echo 'Verifying foreign key constraint update...'
SELECT 
    conname,
    CASE confdeltype 
        WHEN 'a' THEN 'NO ACTION'
        WHEN 'r' THEN 'RESTRICT' 
        WHEN 'c' THEN 'CASCADE'
        WHEN 'n' THEN 'SET NULL'
        WHEN 'd' THEN 'SET DEFAULT'
    END as delete_action
FROM pg_constraint 
WHERE conname = 'executions_script_id_fkey';