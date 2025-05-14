create table switchboard (
	id serial primary key,
	name varchar(100) not null,
	manufactured_for varchar(100),
	serial_number varchar(50),
	manufacture_date date
);

create table test_procedure (
	id serial primary key,
	name varchar(100) not null,
	description text,
	procedure_json jsonb,
	version varchar(20),
	created_at timestamp default current_timestamp
);

create table tester (
	id serial primary key,
	name varchar(100) not null,
	certification varchar(100),
	department varchar(50),
	created_at timestamp default current_timestamp
);

-- Create the switchboard_test table to link switchboards with test procedures
create table switchboard_test (
	id serial primary key,
	switchboard_id integer not null references switchboard(id),
	test_procedure_id integer not null references test_procedure(id),
	created_at timestamp default current_timestamp,
	unique(switchboard_id, test_procedure_id)
);

-- Create the test_execution table to track each test run
create table test_execution (
	id serial primary key,
	switchboard_test_id integer not null references switchboard_test(id),
	tester_id integer not null references tester(id),
	start_time timestamp not null default current_timestamp,
	end_time timestamp,
	status varchar(20) check (status in ('scheduled', 'in_progress', 'completed', 'failed', 'aborted')),
	created_at timestamp default current_timestamp
);

-- Create the test_result table to store detailed results for each component tested
create table test_result (
	id serial primary key,
	execution_id integer not null references test_execution(id),
	step_id integer not null,
	requirement varchar(255) not null,
	passed boolean,
	notes text,
	created_at timestamp default current_timestamp
);

-- Create indexes for performance optimization
create index idx_switchboard_test_switchboard_id on switchboard_test(switchboard_id);
create index idx_switchboard_test_test_procedure_id on switchboard_test(test_procedure_id);
create index idx_test_execution_switchboard_test_id on test_execution(switchboard_test_id);
create index idx_test_execution_tester_id on test_execution(tester_id);
create index idx_test_result_execution_id on test_result(execution_id);

-- Function to get all switchboards
CREATE OR REPLACE FUNCTION get_switchboards_for_combobox()
RETURNS TABLE (
    id INTEGER,
    display_name VARCHAR(200)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        s.id,
        -- Format the display name to include the ID, name, and if available, the serial number
        CASE
            WHEN s.serial_number IS NOT NULL THEN 
                s.name || ' (' || s.serial_number || ')'
            ELSE 
                s.name
        END AS display_name
    FROM 
        switchboard s
    ORDER BY 
        s.name ASC;
END;
$$ LANGUAGE plpgsql;

	-- Function to add a new tester
CREATE OR REPLACE FUNCTION add_tester(
    p_name VARCHAR(100),
    p_certification VARCHAR(100) DEFAULT NULL,
    p_department VARCHAR(50) DEFAULT NULL
)
RETURNS INTEGER AS $$
DECLARE
    new_tester_id INTEGER;
BEGIN
    -- Input validation
    IF p_name IS NULL OR p_name = '' THEN
        RAISE EXCEPTION 'Tester name cannot be null or empty';
    END IF;
    
    -- Insert the new tester
    INSERT INTO tester (
        name,
        certification,
        department,
        created_at
    ) VALUES (
        p_name,
        p_certification,
        p_department,
        CURRENT_TIMESTAMP
    ) RETURNING id INTO new_tester_id;
    
    -- Return the ID of the newly created tester
    RETURN new_tester_id;
EXCEPTION
    WHEN OTHERS THEN
        -- Log error and re-raise
        RAISE NOTICE 'Error adding tester: %', SQLERRM;
        RAISE;
END;
$$ LANGUAGE plpgsql;

-- Function to get only the newest test procedure ID for a switchboard based on version
CREATE OR REPLACE FUNCTION get_latest_switchboard_test_id(p_switchboard_id INTEGER)
RETURNS INTEGER AS $$
DECLARE
    v_test_id INTEGER;
BEGIN
    -- Get the ID of the latest test procedure for the given switchboard
    SELECT 
        st.test_procedure_id INTO v_test_id
    FROM 
        switchboard_test st
    JOIN 
        test_procedure tp ON st.test_procedure_id = tp.id
    WHERE 
        st.switchboard_id = p_switchboard_id
    ORDER BY 
        -- Order by version (treating it as a semantic version)
        string_to_array(tp.version, '.')::int[] DESC,
        -- If versions are identical, get the most recently updated test
        tp.updated_at DESC NULLS LAST
    LIMIT 1;
    
    -- Return the test ID (will be NULL if no tests found)
    RETURN v_test_id;
END;
$$ LANGUAGE plpgsql;

-- Function to save a single test result based on switchboard ID and test procedure ID
CREATE OR REPLACE FUNCTION save_test_result(
    p_switchboard_id INTEGER,
    p_test_procedure_id INTEGER,
    p_tester_id INTEGER,
	p_step_id INTEGER,
    p_requirement VARCHAR(255),
    p_passed BOOLEAN,
    p_notes TEXT DEFAULT NULL,
	p_execution_id INTEGER DEFAULT NULL
)
RETURNS INTEGER AS $$
DECLARE
    v_switchboard_test_id INTEGER;
    v_result_id INTEGER;
BEGIN
    -- Input validation
    IF p_switchboard_id IS NULL THEN
        RAISE EXCEPTION 'Switchboard ID cannot be null';
    END IF;

    IF p_test_procedure_id IS NULL THEN
        RAISE EXCEPTION 'Test procedure ID cannot be null';
    END IF;

    IF p_tester_id IS NULL THEN
        RAISE EXCEPTION 'Tester ID cannot be null';
    END IF;
	
    IF p_step_id IS NULL THEN
        RAISE EXCEPTION 'Step ID cannot be null';
    END IF;
	
    IF p_requirement IS NULL OR p_requirement = '' THEN
        RAISE EXCEPTION 'Requirement cannot be null or empty';
    END IF;
    
    -- Get the switchboard_test ID
    SELECT id INTO v_switchboard_test_id
    FROM switchboard_test
    WHERE switchboard_id = p_switchboard_id AND test_procedure_id = p_test_procedure_id
    LIMIT 1;
    
    IF v_switchboard_test_id IS NULL THEN
        RAISE EXCEPTION 'No switchboard_test found for switchboard ID % and test procedure ID %', 
            p_switchboard_id, p_test_procedure_id;
    END IF;
    if p_execution_id IS NULL THEN
	    -- Create a test execution
	    INSERT INTO test_execution (
	        switchboard_test_id,
	        tester_id,
	        start_time,
	        status,
	        notes
	    ) VALUES (
	        v_switchboard_test_id,
	        p_tester_id,
	        CURRENT_TIMESTAMP,
	        'completed',
	        'Test execution created by save_test_result function'
	    ) RETURNING id INTO p_execution_id;
	END IF;
    
    -- Insert the test result
    INSERT INTO test_result (
        execution_id,
		step_id,
        requirement,
        passed,
        notes,
        created_at
    ) VALUES (
        p_execution_id,
		p_step_id,
        p_requirement,
        p_passed,
        p_notes,
        CURRENT_TIMESTAMP
    ) RETURNING id INTO v_result_id;
    
    -- Return the execution ID
    RETURN p_execution_id;
EXCEPTION
    WHEN OTHERS THEN
        -- Log error and re-raise
        RAISE NOTICE 'Error saving test result: %', SQLERRM;
        RAISE;
END;
$$ LANGUAGE plpgsql;

-- Insert sample data into the switchboard table
INSERT INTO switchboard (name, manufactured_for, serial_number, manufacture_date) VALUES
('Main Distribution Panel A', 'Acme Industries', '123456', '2025-01-10'),
('Solar Inverter System', 'GreenEnergy Solutions', '223456', '2025-02-28');

-- Insert two example testers
INSERT INTO tester (name, certification, department) VALUES
('tester1', 'Senior Electrical Engineer', 'Engineering'),
('tester2', 'UL Certified Inspector', 'Quality Assurance');

-- Insert a sample test procedure with simplified JSON
INSERT INTO test_procedure (name, description, procedure_json, version) VALUES
(
    'Standard Switchboard Electrical Safety Inspection',
    'A comprehensive inspection procedure to verify electrical safety compliance and operational integrity of switchboards.',
    '{"steps": [{"step_id": 1, "name": "Visual Inspection"}, {"step_id": 2, "name": "Insulation Resistance Test"}, {"step_id": 3, "name": "Continuity Testing"}, {"step_id": 4, "name": "Functional Testing"}, {"step_id": 5, "name": "Final Documentation"}], "safety_requirements": ["Use appropriate PPE", "De-energize equipment before testing"]}',
    '1.2'
);

-- Must be run in psql shell because of unknown cursor issue
INSERT INTO test_procedure (name, description, procedure_json, version) VALUES
(
    'Standard Switchboard Electrical Safety Inspection',
    'A comprehensive inspection procedure to verify electrical safety compliance and operational integrity of switchboards. Includes insulation resistance testing, continuity checks, and functional operation verification.',
    '{"steps": [
        {"step_id": 1, "name": "Visual Inspection", "description": "Inspect for physical damage, proper labeling, and component verification.", "requirements": ["No visible damage", "All labels present", "Components match specifications"], "equipment_needed": ["Inspection checklist", "Camera"]},
        {"step_id": 2, "name": "Insulation Resistance Test", "description": "Test insulation resistance between phases and ground.", "requirements": ["Minimum 1 MΩ for circuits ≤ 1000V"], "equipment_needed": ["Insulation tester", "Test leads", "PPE"]},
        {"step_id": 3, "name": "Continuity Testing", "description": "Verify continuity of protective conductors and ground connections.", "requirements": ["Resistance < 0.1 Ω for protective conductors"], "equipment_needed": ["Multimeter", "Test leads"]},
        {"step_id": 4, "name": "Functional Testing", "description": "Test operation of circuit breakers, switches, and indicators.", "requirements": ["Smooth mechanical operation", "Correct indicator function"], "equipment_needed": ["Operation tools"]},
        {"step_id": 5, "name": "Documentation", "description": "Complete test records and verification documents.", "requirements": ["All results recorded", "Non-conformances documented"], "equipment_needed": ["Documentation forms"]}
    ], "safety_requirements": ["De-energize circuits during testing", "Wear appropriate PPE", "Only certified personnel for high-voltage testing"]}',
    '0.1'
);
(
    'Test Test',
    'No description',
    '{"steps": [
        {"step_id": 1, "name": "Visual Inspection", "description": "Inspect for physical damage, proper labeling, and component verification.", "requirements": ["No visible damage", "All labels present", "Components match specifications"], "equipment_needed": ["Inspection checklist", "Camera"]},
        {"step_id": 2, "name": "Insulation Resistance Test", "description": "Test insulation resistance between phases and ground.", "requirements": ["Minimum 1 MΩ for circuits ≤ 1000V"], "equipment_needed": ["Insulation tester", "Test leads", "PPE"]},
        {"step_id": 3, "name": "Continuity Testing", "description": "Verify continuity of protective conductors and ground connections.", "requirements": ["Resistance < 0.1 Ω for protective conductors"], "equipment_needed": ["Multimeter", "Test leads"]},
        {"step_id": 4, "name": "Functional Testing", "description": "Test operation of circuit breakers, switches, and indicators.", "requirements": ["Smooth mechanical operation", "Correct indicator function"], "equipment_needed": ["Operation tools"]},
        {"step_id": 5, "name": "Documentation", "description": "Complete test records and verification documents.", "requirements": ["All results recorded", "Non-conformances documented"], "equipment_needed": ["Documentation forms"]}
    ], "safety_requirements": ["De-energize circuits during testing", "Wear appropriate PPE", "Only certified personnel for high-voltage testing"]}',
    '0.2'
);

INSERT INTO switchboard_test (switchboard_id, test_procedure_id) VALUES
(1,	1),
(2, 2);
