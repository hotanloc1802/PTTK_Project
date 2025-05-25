-- 1. Create schema
CREATE SCHEMA mien_dong;
CREATE SCHEMA plaza;
CREATE SCHEMA sala;
CREATE SCHEMA green_view;

CREATE SEQUENCE IF NOT EXISTS mien_dong.resident_id_seq START 1;
CREATE SEQUENCE IF NOT EXISTS mien_dong.bill_id_seq START 1;
CREATE SEQUENCE IF NOT EXISTS mien_dong.payment_id_seq START 1;

-- 2. Create tables
CREATE TABLE mien_dong.users (
    user_id  VARCHAR(20) PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    role VARCHAR(50) NOT NULL,
    status VARCHAR(50) DEFAULT 'Active',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP
);

CREATE TABLE mien_dong.buildings (
    building_id  VARCHAR(20) PRIMARY KEY,
    building_name VARCHAR(255) NOT NULL,
    address VARCHAR(255) NOT NULL,
    manager_id VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (manager_id) REFERENCES mien_dong.users(user_id) ON DELETE SET NULL
);

CREATE TABLE mien_dong.apartments (
    apartment_id  VARCHAR(20) PRIMARY KEY,
    building_id VARCHAR(20) NOT NULL,
    owner_id  VARCHAR(20),
    max_population INTEGER NOT NULL,
    current_population INTEGER DEFAULT 0,
    transfer_status VARCHAR(50) DEFAULT 'Available',
    vacancy_status VARCHAR(50) DEFAULT 'Vacant',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_registered TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (building_id) REFERENCES mien_dong.buildings(building_id) ON DELETE CASCADE
);

CREATE TABLE mien_dong.residents (
    resident_id VARCHAR(20) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    phone_number VARCHAR(15),
    email VARCHAR(255),
    sex VARCHAR(10) CHECK (sex IN ('Male', 'Female', 'Other')) NOT NULL,
    identification_number VARCHAR(50) UNIQUE NOT NULL,
    apartment_id VARCHAR(20) NOT NULL,
    resident_status VARCHAR(50) DEFAULT 'Resident',
    owner_id VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    date_registered TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (apartment_id) REFERENCES mien_dong.apartments(apartment_id) ON DELETE CASCADE,
    FOREIGN KEY (owner_id) REFERENCES mien_dong.residents(resident_id) ON DELETE CASCADE
);

CREATE TABLE mien_dong.bills (
    bill_id       VARCHAR(20) PRIMARY KEY,
    apartment_id  VARCHAR(20) NOT NULL,
    bill_type     VARCHAR(50) NOT NULL,
    bill_amount   DECIMAL(12, 2) NOT NULL,
    due_date      TIMESTAMP NOT NULL,
    bill_date     TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    payment_status VARCHAR(50) DEFAULT 'Pending',
    FOREIGN KEY (apartment_id) REFERENCES mien_dong.apartments(apartment_id) ON DELETE CASCADE
);

CREATE TABLE mien_dong.payments (
    payment_id VARCHAR(20) PRIMARY KEY,
    apartment_id VARCHAR(20) NOT NULL,
    total_amount DECIMAL(12, 2) NOT NULL,
    payment_date TIMESTAMP DEFAULT '9999-12-31 23:59:59',
    payment_method VARCHAR(50) DEFAULT '',
    payment_status VARCHAR(50) DEFAULT 'Pending',
    payment_created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (apartment_id) REFERENCES mien_dong.apartments(apartment_id) ON DELETE CASCADE

);

CREATE TABLE mien_dong.paymentsdetail (
    bill_id VARCHAR(20),
    payment_id VARCHAR(20),
    PRIMARY KEY (bill_id, payment_id),
    FOREIGN KEY (bill_id) REFERENCES mien_dong.bills(bill_id) ON DELETE CASCADE,
    FOREIGN KEY (payment_id) REFERENCES mien_dong.payments(payment_id) ON DELETE CASCADE
);

CREATE TABLE mien_dong.service_requests (
    request_id  VARCHAR(20) PRIMARY KEY,
    apartment_id  VARCHAR(20)  NOT NULL,
    resident_id  VARCHAR(20) NOT NULL,
    category VARCHAR(50) NOT NULL,
    description TEXT NOT NULL,
    status VARCHAR(50) DEFAULT 'Pending',
    request_date TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    completed_date TIMESTAMP WITHOUT TIME ZONE,
    FOREIGN KEY (apartment_id) REFERENCES mien_dong.apartments(apartment_id) ON DELETE CASCADE,
    FOREIGN KEY (resident_id) REFERENCES mien_dong.residents(resident_id) ON DELETE CASCADE
);

-- 3. Create functions
-- Auto-generate resident_id in format RXXX using a sequence
CREATE OR REPLACE FUNCTION set_resident_id() 
RETURNS TRIGGER AS $$
BEGIN
    NEW.resident_id := 'R' || LPAD(nextval('mien_dong.resident_id_seq')::text, 3, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Auto-generate bill_id in format BI0001, BI0002, etc. using a sequence
CREATE OR REPLACE FUNCTION set_bill_id() 
RETURNS TRIGGER AS $$
BEGIN
    NEW.bill_id := 'BI' || LPAD(nextval('mien_dong.bill_id_seq')::text, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Auto-generate payment_id in format P0001, P0002, etc. using a sequence
CREATE OR REPLACE FUNCTION set_payment_id() 
RETURNS TRIGGER AS $$
BEGIN
    NEW.payment_id := 'P' || LPAD(nextval('mien_dong.payment_id_seq')::text, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create function to automatically update current_population when there is a change in the residents table
CREATE OR REPLACE FUNCTION update_current_population() 
RETURNS TRIGGER AS $$
BEGIN
    -- If operation is INSERT (adding a new resident to an apartment)
    IF (TG_OP = 'INSERT') THEN
        UPDATE mien_dong.apartments
        SET current_population = current_population + 1
        WHERE apartment_id = NEW.apartment_id;
        RETURN NEW;
    -- If operation is DELETE (removing a resident from an apartment)
    ELSIF (TG_OP = 'DELETE') THEN
        UPDATE mien_dong.apartments
        SET current_population = current_population - 1
        WHERE apartment_id = OLD.apartment_id;
        RETURN OLD;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create function to check and ensure current_population does not exceed max_population
CREATE OR REPLACE FUNCTION check_population_limit() 
RETURNS TRIGGER AS $$
BEGIN
    -- Check the number of residents after adding (INSERT) or updating (UPDATE)
    IF (TG_OP = 'INSERT') THEN
        -- If adding a resident, check if current_population exceeds max_population
        IF (SELECT current_population + 1 > max_population FROM mien_dong.apartments WHERE apartment_id = NEW.apartment_id) THEN
            RAISE EXCEPTION 'Cannot insert more residents: current_population exceeds max_population';
        END IF;
    ELSIF (TG_OP = 'UPDATE') THEN
        -- If updating a resident, check if current_population exceeds max_population
        IF (SELECT current_population + 1 > max_population FROM mien_dong.apartments WHERE apartment_id = NEW.apartment_id) THEN
            RAISE EXCEPTION 'Cannot update resident: current_population exceeds max_population';
        END IF;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create function to update vacancy_status based on current_population
CREATE OR REPLACE FUNCTION update_vacancy_status() 
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.current_population = 0 THEN
        NEW.vacancy_status := 'Vacant';
    ELSE
        -- When transitioning from 0 residents to at least one, update date_registered
        IF (TG_OP = 'UPDATE' AND OLD.current_population = 0) THEN
            NEW.date_registered := CURRENT_TIMESTAMP;
        END IF;
        NEW.vacancy_status := 'Occupied';
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create function to manage apartment ownership when resident status changes
CREATE OR REPLACE FUNCTION manage_apartment_ownership()
RETURNS TRIGGER AS $$
BEGIN
    -- Only process when resident_status changes to 'Owner' or during INSERT with 'Owner' status
    IF (TG_OP = 'INSERT' AND NEW.resident_status = 'Owner') OR 
       (TG_OP = 'UPDATE' AND OLD.resident_status != 'Owner' AND NEW.resident_status = 'Owner') THEN
        
        -- Step 1: Set all other residents in the same apartment to 'Resident' status
        UPDATE mien_dong.residents
        SET resident_status = 'Resident'
        WHERE apartment_id = NEW.apartment_id 
        AND resident_id != NEW.resident_id;
        
        -- Step 2: Set all residents' owner_id in the same apartment to the new owner's resident_id
        -- (including the new owner themselves)
        UPDATE mien_dong.residents
        SET owner_id = NEW.resident_id
        WHERE apartment_id = NEW.apartment_id;
        
        -- Step 3: Update the apartment's owner_id to the new owner
        UPDATE mien_dong.apartments
        SET owner_id = NEW.resident_id
        WHERE apartment_id = NEW.apartment_id;
        
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Also create a trigger to handle when a resident (who is an owner) is deleted
CREATE OR REPLACE FUNCTION handle_owner_deletion()
RETURNS TRIGGER AS $$
BEGIN
    -- If the deleted resident was an owner, clear ownership for the apartment
    IF OLD.resident_status = 'Owner' THEN
        -- Clear owner_id for all remaining residents in the apartment
        UPDATE mien_dong.residents
        SET owner_id = NULL
        WHERE apartment_id = OLD.apartment_id;
        
        -- Clear owner_id for the apartment itself
        UPDATE mien_dong.apartments
        SET owner_id = NULL
        WHERE apartment_id = OLD.apartment_id;
    END IF;
    
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

-- Create function to ensure owner_id is a resident_id of the same apartment
CREATE OR REPLACE FUNCTION check_owner_id()
RETURNS TRIGGER AS $$
BEGIN
    -- Check if owner_id is the resident_id of the same apartment
    IF NOT EXISTS (
        SELECT 1
        FROM mien_dong.residents
        WHERE resident_id = NEW.owner_id
        AND apartment_id = NEW.apartment_id
    ) THEN
        RAISE EXCEPTION 'owner_id must be a resident_id of the same apartment';
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create a function to generate or update apartment payment for a specific month
CREATE OR REPLACE FUNCTION manage_apartment_payment(
    p_apartment_id VARCHAR(20),
    p_bill_id VARCHAR(20),
    p_bill_date TIMESTAMP
) RETURNS VARCHAR(20) AS $$
DECLARE
    v_payment_id VARCHAR(20);
    v_year INT;
    v_month INT;
BEGIN
    -- Extract year and month from bill date
    v_year := EXTRACT(YEAR FROM p_bill_date);
    v_month := EXTRACT(MONTH FROM p_bill_date);
    
    -- Check if a payment already exists for this apartment for this month/year
    SELECT payment_id INTO v_payment_id
    FROM mien_dong.payments
    WHERE apartment_id = p_apartment_id
    AND EXTRACT(YEAR FROM payment_created_date) = v_year
    AND EXTRACT(MONTH FROM payment_created_date) = v_month
    LIMIT 1;
    
    -- If no payment exists for this apartment for this month, create one
    IF v_payment_id IS NULL THEN
        INSERT INTO mien_dong.payments (apartment_id, payment_created_date, total_amount, payment_status)
        VALUES (p_apartment_id, p_bill_date, 0, 'Pending')
        RETURNING payment_id INTO v_payment_id;
    END IF;
    
    -- Link the bill to this payment
    INSERT INTO mien_dong.paymentsdetail (bill_id, payment_id)
    VALUES (p_bill_id, v_payment_id);
    
    -- Update the payment total_amount (will be handled by trigger)
    
    RETURN v_payment_id;
END;
$$ LANGUAGE plpgsql;

-- Create a function to complete a payment with a specific payment method
CREATE OR REPLACE FUNCTION complete_apartment_payment(
    p_apartment_id VARCHAR(20),
    p_payment_method VARCHAR(50),
    p_year INT DEFAULT NULL,
    p_month INT DEFAULT NULL
) RETURNS BOOLEAN AS $$
DECLARE
    v_payment_id VARCHAR(20);
    v_current_year INT;
    v_current_month INT;
BEGIN
    -- If year/month not provided, use current month
    IF p_year IS NULL THEN
        v_current_year := EXTRACT(YEAR FROM CURRENT_DATE);
    ELSE
        v_current_year := p_year;
    END IF;
    
    IF p_month IS NULL THEN
        v_current_month := EXTRACT(MONTH FROM CURRENT_DATE);
    ELSE
        v_current_month := p_month;
    END IF;
    
    -- Find the payment for this apartment for the specified month
    SELECT payment_id INTO v_payment_id
    FROM mien_dong.payments
    WHERE apartment_id = p_apartment_id
    AND EXTRACT(YEAR FROM payment_created_date) = v_current_year
    AND EXTRACT(MONTH FROM payment_created_date) = v_current_month
    AND payment_status = 'Pending'
    LIMIT 1;
    
    -- If no pending payment exists, return false
    IF v_payment_id IS NULL THEN
        RETURN FALSE;
    END IF;
    
    -- Update payment method and status
    UPDATE mien_dong.payments
    SET payment_method = p_payment_method,
        payment_status = 'Completed'
    WHERE payment_id = v_payment_id;
    
    -- Update all associated bills to 'Paid' (will be handled by trigger)
    
    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

-- Create a function to update bill payment status when a payment is completed
CREATE OR REPLACE FUNCTION update_bill_payment_status()
RETURNS TRIGGER AS $$
BEGIN
    -- When a payment status changes to 'Completed'
    IF NEW.payment_status = 'Completed' AND OLD.payment_status != 'Completed' THEN
        -- Update all associated bills to 'Paid'
        UPDATE mien_dong.bills
        SET payment_status = 'Paid'
        WHERE bill_id IN (
            SELECT bill_id 
            FROM mien_dong.paymentsdetail
            WHERE payment_id = NEW.payment_id
        );
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create a function to automatically link bills to apartment payments
CREATE OR REPLACE FUNCTION link_bill_to_payment()
RETURNS TRIGGER AS $$
DECLARE
    v_payment_id VARCHAR(20);
BEGIN
    -- Only process pending bills
    IF NEW.payment_status = 'Pending' THEN
        -- Link the bill to the appropriate payment
        v_payment_id := manage_apartment_payment(NEW.apartment_id, NEW.bill_id, NEW.bill_date);
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Update the function to calculate payment total amount
CREATE OR REPLACE FUNCTION update_payment_total_amount()
RETURNS TRIGGER AS $$
DECLARE
    affected_payment_id VARCHAR(20);
    bill_amount DECIMAL(12, 2);
BEGIN
    -- For INSERT operations
    IF (TG_OP = 'INSERT') THEN
        -- Get the bill amount
        SELECT b.bill_amount INTO bill_amount
        FROM mien_dong.bills b
        WHERE b.bill_id = NEW.bill_id;
        
        -- Update the payment total_amount
        UPDATE mien_dong.payments
        SET total_amount = total_amount + bill_amount
        WHERE payment_id = NEW.payment_id;
    END IF;

    -- For DELETE operations
    IF (TG_OP = 'DELETE') THEN
        -- Get the bill amount
        SELECT b.bill_amount INTO bill_amount
        FROM mien_dong.bills b
        WHERE b.bill_id = OLD.bill_id;
        
        -- Update the payment total_amount
        UPDATE mien_dong.payments
        SET total_amount = total_amount - bill_amount
        WHERE payment_id = OLD.payment_id;
    END IF;

    -- For INSERT, return NEW; for DELETE, return OLD
    IF (TG_OP = 'INSERT') THEN
        RETURN NEW;
    ELSE
        RETURN OLD;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create function to update completed_date when status changes
CREATE OR REPLACE FUNCTION set_completed_date()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.status = 'In Progress' AND NEW.status = 'Completed' THEN
        NEW.completed_date := LOCALTIMESTAMP;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create function to set payment_date when payment_status changes
CREATE OR REPLACE FUNCTION set_payment_date()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.payment_status = 'Pending' AND NEW.payment_status = 'Completed' THEN
        NEW.payment_date := CURRENT_TIMESTAMP;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 4. Create all triggers

-- Trigger to call the function before INSERT on residents to auto-generate resident_id
CREATE TRIGGER before_resident_insert
BEFORE INSERT ON mien_dong.residents
FOR EACH ROW
EXECUTE FUNCTION set_resident_id();

-- Trigger to call the function before INSERT on bills to auto-generate bill_id
CREATE TRIGGER before_bill_insert
BEFORE INSERT ON mien_dong.bills
FOR EACH ROW
EXECUTE FUNCTION set_bill_id();

-- Trigger to call the function before INSERT on payments to auto-generate payment_id
CREATE TRIGGER before_payment_insert
BEFORE INSERT ON mien_dong.payments
FOR EACH ROW
WHEN (NEW.payment_id IS NULL)
EXECUTE FUNCTION set_payment_id();

-- Trigger to call the function after INSERT on residents to update current_population
CREATE TRIGGER after_resident_insert
AFTER INSERT ON mien_dong.residents
FOR EACH ROW
EXECUTE FUNCTION update_current_population();

-- Trigger to call the function after DELETE on residents to update current_population
CREATE TRIGGER after_resident_delete
AFTER DELETE ON mien_dong.residents
FOR EACH ROW
EXECUTE FUNCTION update_current_population();

-- Trigger before INSERT on apartments to update vacancy_status
CREATE TRIGGER update_vacancy_status_insert
BEFORE INSERT ON mien_dong.apartments
FOR EACH ROW
EXECUTE FUNCTION update_vacancy_status();

-- Trigger before UPDATE on apartments to update vacancy_status
CREATE TRIGGER update_vacancy_status_update
BEFORE UPDATE ON mien_dong.apartments
FOR EACH ROW
EXECUTE FUNCTION update_vacancy_status();

-- Create trigger that fires after INSERT or UPDATE on residents table
CREATE TRIGGER after_resident_owner_change
AFTER INSERT OR UPDATE OF resident_status ON mien_dong.residents
FOR EACH ROW
EXECUTE FUNCTION manage_apartment_ownership();

-- Create trigger for owner deletion
CREATE TRIGGER after_owner_deletion
AFTER DELETE ON mien_dong.residents
FOR EACH ROW
WHEN (OLD.resident_status = 'Owner')
EXECUTE FUNCTION handle_owner_deletion();

-- Check when owner_id is provided
CREATE TRIGGER check_owner_id_insert
BEFORE INSERT ON mien_dong.apartments
FOR EACH ROW
WHEN (NEW.owner_id IS NOT NULL)
EXECUTE FUNCTION check_owner_id();

CREATE TRIGGER check_owner_id_update
BEFORE UPDATE ON mien_dong.apartments
FOR EACH ROW
WHEN (NEW.owner_id IS NOT NULL)
EXECUTE FUNCTION check_owner_id();

-- Triggers for population limit checking
CREATE TRIGGER before_resident_insert_check_limit
BEFORE INSERT ON mien_dong.residents
FOR EACH ROW
EXECUTE FUNCTION check_population_limit();

CREATE TRIGGER before_resident_update_check_limit
BEFORE UPDATE ON mien_dong.residents
FOR EACH ROW
WHEN (OLD.apartment_id IS DISTINCT FROM NEW.apartment_id)
EXECUTE FUNCTION check_population_limit();

-- Create triggers for automatically updating status linking bills to payments
CREATE TRIGGER after_bill_insert_link_payment
AFTER INSERT ON mien_dong.bills
FOR EACH ROW
EXECUTE FUNCTION link_bill_to_payment();

CREATE TRIGGER after_payment_status_update
AFTER UPDATE OF payment_status ON mien_dong.payments
FOR EACH ROW
EXECUTE FUNCTION update_bill_payment_status();

-- Create triggers for payment total amount calculation
CREATE TRIGGER after_paymentdetail_insert_update_total
AFTER INSERT ON mien_dong.paymentsdetail
FOR EACH ROW
EXECUTE FUNCTION update_payment_total_amount();

CREATE TRIGGER after_paymentdetail_delete_update_total
AFTER DELETE ON mien_dong.paymentsdetail
FOR EACH ROW
EXECUTE FUNCTION update_payment_total_amount();

-- Create trigger on service_requests table before updating status
CREATE TRIGGER trg_set_completed_date
BEFORE UPDATE OF status ON mien_dong.service_requests
FOR EACH ROW
WHEN (OLD.status = 'In Progress' AND NEW.status = 'Completed')
EXECUTE FUNCTION set_completed_date();

-- Create trigger on payments table before updating payment_status
CREATE TRIGGER trg_update_payment_date
BEFORE UPDATE OF payment_status ON mien_dong.payments
FOR EACH ROW
WHEN (OLD.payment_status = 'Pending' AND NEW.payment_status = 'Completed')
EXECUTE FUNCTION set_payment_date();

-- 5. Insert data

INSERT INTO mien_dong.buildings (building_id, building_name, address) 
VALUES ('B001', 'Mien Dong', '123 Mien Dong Street');

INSERT INTO mien_dong.apartments (apartment_id, building_id, max_population, transfer_status) 
VALUES 
    ('A01.01', 'B001', 6, 'Available'),
    ('A01.02', 'B001', 6, 'Available'),
    ('A02.03', 'B001', 6, 'Not Available'),
    ('A02.04', 'B001', 6, 'Available'),
    ('A03.05', 'B001', 6, 'Available'),
    ('A03.06', 'B001', 6, 'Available'),
    ('A04.07', 'B001', 6, 'Available'),
    ('A04.08', 'B001', 6, 'Available'),
    ('A05.09', 'B001', 6, 'Not Available'),
    ('A05.10', 'B001', 6, 'Available'),
    ('A06.11', 'B001', 6, 'Available'),
    ('A06.12', 'B001', 6, 'Available'),
    ('A07.13', 'B001', 6, 'Not Available'),
    ('A07.14', 'B001', 6, 'Available'),
    ('A08.15', 'B001', 6, 'Available'),
    ('A08.16', 'B001', 6, 'Available'),
    ('A09.17', 'B001', 6, 'Available'),
    ('A09.18', 'B001', 6, 'Available'),
    ('A10.19', 'B001', 6, 'Not Available'),
    ('A10.20', 'B001', 6, 'Available');

INSERT INTO mien_dong.residents 
    (name, phone_number, email, sex, identification_number, apartment_id, created_at, updated_at, date_registered)
VALUES
    -- First 20 residents with a created_at, updated_at and date_registered set to one month ago
    ('John Smith', '0901112233', 'john.smith@example.com', 'Male', '074000870001', 'A01.01', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Emily Johnson', '0912233445', 'emily.johnson@example.com', 'Female', '074120870002', 'A01.02', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Robert Brown', '0923344556', 'robert.brown@example.com', 'Male', '074220880003', 'A02.03', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Jessica Davis', '0934455667', 'jessica.davis@example.com', 'Female', '074320890004', 'A02.04', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Michael Miller', '0945566778', 'michael.miller@example.com', 'Male', '074420900005', 'A03.05', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Sarah Wilson', '0956677889', 'sarah.wilson@example.com', 'Female', '074520910006', 'A03.06', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('David Moore', '0967788990', 'david.moore@example.com', 'Male', '074620920007', 'A04.07', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('James Taylor', '0978899001', 'james.taylor@example.com', 'Male', '074720930008', 'A04.08', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Linda Anderson', '0989900112', 'linda.anderson@example.com', 'Female', '074820940009', 'A05.09', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Barbara Thomas', '0912345678', 'barbara.thomas@example.com', 'Female', '074920950010', 'A05.10', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('William Jackson', '0923456789', 'william.jackson@example.com', 'Male', '074020960011', 'A06.11', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Richard White', '0934567890', 'richard.white@example.com', 'Male', '074120970012', 'A06.12', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Susan Harris', '0945678901', 'susan.harris@example.com', 'Female', '074220980013', 'A07.13', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Joseph Martin', '0956789012', 'joseph.martin@example.com', 'Male', '074320990014', 'A07.14', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Charles Thompson', '0967890123', 'charles.thompson@example.com', 'Male', '074420000015', 'A08.15', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Patricia Garcia', '0978901234', 'patricia.garcia@example.com', 'Female', '074520010016', 'A08.16', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Jennifer Martinez', '0989012345', 'jennifer.martinez@example.com', 'Female', '074620020017', 'A09.17', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Thomas Robinson', '0912345680', 'thomas.robinson@example.com', 'Male', '074720030018', 'A09.18', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Christopher Clark', '0923456782', 'christopher.clark@example.com', 'Male', '074820040019', 'A10.19', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Margaret Rodriguez', '0934567893', 'margaret.rodriguez@example.com', 'Female', '074920050020', 'A10.20', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    -- Next 20 residents with the current timestamp for created_at, updated_at and date_registered
    ('Elizabeth Lewis', '0911122334', 'elizabeth.lewis@example.com', 'Female', '075220180041', 'A01.01', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Daniel Lee', '0922233445', 'daniel.lee@example.com', 'Male', '075320190042', 'A01.02', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Matthew Walker', '0933344556', 'matthew.walker@example.com', 'Male', '075420200043', 'A02.03', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Laura Hall', '0944455667', 'laura.hall@example.com', 'Female', '075520210044', 'A02.04', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Anthony Allen', '0955566778', 'anthony.allen@example.com', 'Male', '075620220045', 'A03.05', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Nancy Young', '0966677889', 'nancy.young@example.com', 'Female', '075720230046', 'A03.06', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Mark Hernandez', '0977788990', 'mark.hernandez@example.com', 'Male', '075820240047', 'A04.07', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Betty King', '0988899001', 'betty.king@example.com', 'Female', '075920250048', 'A04.08', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Steven Wright', '0910000112', 'steven.wright@example.com', 'Male', '076020260049', 'A05.09', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Paul Lopez', '0921112233', 'paul.lopez@example.com', 'Male', '076120270050', 'A05.10', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Mary Hill', '0932223344', 'mary.hill@example.com', 'Female', '076220280051', 'A06.11', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('George Scott', '0943334455', 'george.scott@example.com', 'Male', '076320290052', 'A06.12', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Karen Green', '0954445566', 'karen.green@example.com', 'Female', '076420300053', 'A07.13', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Edward Adams', '0965556677', 'edward.adams@example.com', 'Male', '076520310054', 'A07.14', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Dorothy Baker', '0976667788', 'dorothy.baker@example.com', 'Female', '076620320055', 'A08.15', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Brian Gonzalez', '0987778899', 'brian.gonzalez@example.com', 'Male', '076720330056', 'A08.16', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Lisa Nelson', '0918889900', 'lisa.nelson@example.com', 'Female', '076820340057', 'A09.17', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Kevin Carter', '0929990001', 'kevin.carter@example.com', 'Male', '076920350058', 'A09.18', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Karen Mitchell', '0930001112', 'karen.mitchell@example.com', 'Female', '077020360059', 'A10.19', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Steven Perez', '0941112233', 'steven.perez@example.com', 'Male', '077120370060', 'A10.20', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

-- INSERT INTO mien_dong.residents (name, phone_number, email, sex, identification_number, apartment_id)
-- VALUES
--     ('Bobby Brown', '0911124117', 'alice.brown@example', 'Male', '089220320760', 'A01.01');

-- SELECT * FROM mien_dong.apartments;

-- Insert mock bills for various apartments
INSERT INTO mien_dong.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status) 
VALUES
    -- Apartment A01.01 bills
    ('Electricity', 'A01.01', 850.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'A01.01', 320.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'A01.01', 200.00, '2025-05-25', '2025-05-01', 'Pending'),
    
    -- Apartment A01.02 bills
    ('Electricity', 'A01.02', 780.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'A01.02', 290.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'A01.02', 200.00, '2025-05-25', '2025-05-01', 'Pending'),
    
    -- Apartment A02.03 bills
    ('Electricity', 'A02.03', 900.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'A02.03', 350.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'A02.03', 200.00, '2025-05-25', '2025-05-01', 'Pending'),
    
    -- Apartment A02.04 bills
    ('Electricity', 'A02.04', 600.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'A02.04', 200.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'A02.04', 200.00, '2025-05-25', '2025-05-01', 'Pending'),
    
    -- Previous month bills for A01.01 (already paid)
    ('Electricity', 'A01.01', 800.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Water', 'A01.01', 300.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Maintenance', 'A01.01', 200.00, '2025-04-25', '2025-04-01', 'Pending'),
    
    -- Previous month bills for A01.02 (already paid)
    ('Electricity', 'A01.02', 700.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Water', 'A01.02', 200.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Maintenance', 'A01.02', 200.00, '2025-04-25', '2025-04-01', 'Pending');

-- Now let's complete some payments for the previous month
-- Note: We don't need to manually insert into payments or paymentsdetail tables
-- as our triggers will handle that automatically

-- Complete payments for April for apartments A01.01 and A01.02
SELECT complete_apartment_payment('A01.01', 'Bank Transfer', 2025, 4);
SELECT complete_apartment_payment('A01.02', 'Cash', 2025, 4);

-- Insert bills for some apartments for June (future month)
INSERT INTO mien_dong.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status) 
VALUES
    ('Electricity', 'A02.03', 800.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Water', 'A02.03', 330.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Maintenance', 'A02.03', 200.00, '2025-06-25', '2025-06-01', 'Pending'),
    
    ('Electricity', 'A02.04', 760.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Water', 'A02.04', 300.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Maintenance', 'A02.04', 200.00, '2025-06-25', '2025-06-01', 'Pending');

-- Complete some current month payments
SELECT complete_apartment_payment('A02.03', 'Bank Transfer', 2025, 5);
SELECT complete_apartment_payment('A02.04', 'Mobile Payment', 2025, 5);

-- Insert some special bills
INSERT INTO mien_dong.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status)
VALUES
    ('Special Maintenance', 'A01.01', 1500.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Special Maintenance', 'A01.02', 1200.00, '2025-05-25', '2025-05-01', 'Pending');

-- Insert service requests
INSERT INTO mien_dong.service_requests (request_id, apartment_id, resident_id, category, description, status, request_date, completed_date)
VALUES
    ('SR001', 'A01.01', 'R001', 'Plumbing', 'Leaky faucet in kitchen', 'Completed', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, CURRENT_TIMESTAMP),
    ('SR002', 'A01.02', 'R002', 'Electrical', 'Light bulb replacement in living room', 'Completed', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, CURRENT_TIMESTAMP),
    ('SR003', 'A02.03', 'R003', 'HVAC', 'Air conditioning not working', 'Completed', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, CURRENT_TIMESTAMP),
    ('SR004', 'A02.04', 'R004', 'Cleaning', 'Common area cleaning request', 'Completed', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, CURRENT_TIMESTAMP),
    ('SR005', 'A03.05', 'R005', 'Security', 'Lost key request', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR006', 'A03.06', 'R006', 'Maintenance', 'Broken window in bedroom', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR007', 'A04.07', 'R007', 'Plumbing', 'Clogged toilet issue', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR008', 'A04.08', 'R008', 'Electrical', 'Power outage in apartment', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR009', 'A05.09', 'R009', 'HVAC', 'Heating system not working', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR010', 'A05.10', 'R010', 'Cleaning', 'Balcony cleaning request', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR011', 'A06.11', 'R011', 'Security', 'Access card request', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR012', 'A06.12', 'R012', 'Maintenance', 'Pest control request', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR013', 'A07.13', 'R013', 'Plumbing', 'Water heater issue', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR014', 'A07.14', 'R014', 'Electrical', 'Wiring issue in living room', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR015', 'A08.15', 'R015', 'HVAC', 'Ventilation issue in kitchen', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR016', 'A08.16', 'R016', 'Cleaning', 'Garage cleaning request', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR017', 'A09.17', 'R017', 'Security', 'Visitor access request', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR018', 'A09.18', 'R018', 'Maintenance', 'Roof leak issue', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR019', 'A10.19', 'R019', 'Plumbing', 'Sewer backup issue in bathroom', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL),
    ('SR020', 'A10.20', 'R020', 'Electrical', 'Circuit breaker issue in kitchen', 'In Progress', CURRENT_TIMESTAMP - ((floor(random()*90)) || ' days')::interval, NULL);

-- Count all occupied apartments for 2 cases: until now and until 1 month ago
SELECT COUNT(*) AS occupied_apartments_now
FROM mien_dong.apartments
WHERE current_population > 0;

SELECT COUNT(DISTINCT apartment_id) AS occupied_apartments_1_month_ago
FROM mien_dong.residents
WHERE created_at <= CURRENT_DATE - INTERVAL '1 month';

-- 6. Show data from all of the TABLES
SELECT * FROM mien_dong.users;

SELECT * FROM mien_dong.buildings;
SELECT * FROM mien_dong.apartments;
SELECT * FROM mien_dong.residents;

SELECT * FROM mien_dong.bills;
SELECT * FROM mien_dong.payments;
SELECT * FROM mien_dong.paymentsdetail;

SELECT * FROM mien_dong.service_requests;
