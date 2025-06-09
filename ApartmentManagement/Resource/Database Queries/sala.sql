-- 1. Create schema
CREATE SCHEMA IF NOT EXISTS sala;

CREATE SEQUENCE IF NOT EXISTS sala.resident_id_seq START 1;
CREATE SEQUENCE IF NOT EXISTS sala.bill_id_seq START 1;
CREATE SEQUENCE IF NOT EXISTS sala.payment_id_seq START 1;

-- 2. Create tables
CREATE TABLE sala.users (
    user_id  VARCHAR(20) PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    role VARCHAR(50) NOT NULL,
    status VARCHAR(50) DEFAULT 'Active',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP
);

CREATE TABLE sala.buildings (
    building_id  VARCHAR(20) PRIMARY KEY,
    building_name VARCHAR(255) NOT NULL,
    address VARCHAR(255) NOT NULL,
    manager_id VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (manager_id) REFERENCES sala.users(user_id) ON DELETE SET NULL
);

CREATE TABLE sala.apartments (
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
    FOREIGN KEY (building_id) REFERENCES sala.buildings(building_id) ON DELETE CASCADE
);

CREATE TABLE sala.residents (
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
    FOREIGN KEY (apartment_id) REFERENCES sala.apartments(apartment_id) ON DELETE CASCADE,
    FOREIGN KEY (owner_id) REFERENCES sala.residents(resident_id) ON DELETE CASCADE
);

CREATE TABLE sala.bills (
    bill_id  VARCHAR(20) PRIMARY KEY,
    apartment_id  VARCHAR(20) NOT NULL,
    bill_type  VARCHAR(50) NOT NULL,
    bill_amount  DECIMAL(12, 2) NOT NULL,
    due_date  TIMESTAMP NOT NULL,
    bill_date  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    payment_status VARCHAR(50) DEFAULT 'Pending',
    FOREIGN KEY (apartment_id) REFERENCES sala.apartments(apartment_id) ON DELETE CASCADE
);

CREATE TABLE sala.payments (
    payment_id VARCHAR(20) PRIMARY KEY,
    apartment_id VARCHAR(20) NOT NULL,
    total_amount DECIMAL(12, 2) NOT NULL,
    payment_date TIMESTAMP DEFAULT '9999-12-31 23:59:59',
    payment_method VARCHAR(50) DEFAULT '',
    payment_status VARCHAR(50) DEFAULT 'Pending',
    payment_created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (apartment_id) REFERENCES sala.apartments(apartment_id) ON DELETE CASCADE
);

CREATE TABLE sala.paymentsdetail (
    bill_id VARCHAR(20),
    payment_id VARCHAR(20),
    PRIMARY KEY (bill_id, payment_id),
    FOREIGN KEY (bill_id) REFERENCES sala.bills(bill_id) ON DELETE CASCADE,
    FOREIGN KEY (payment_id) REFERENCES sala.payments(payment_id) ON DELETE CASCADE
);

CREATE TABLE sala.service_requests (
    request_id  VARCHAR(20) PRIMARY KEY,
    apartment_id  VARCHAR(20)  NOT NULL,
    resident_id  VARCHAR(20) NOT NULL,
    category VARCHAR(50) NOT NULL,
    description TEXT NOT NULL,
    amount  DECIMAL(12, 2) NOT null DEFAULT 0,
    status VARCHAR(50) DEFAULT 'Pending',
    request_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_date TIMESTAMP,
    FOREIGN KEY (apartment_id) REFERENCES sala.apartments(apartment_id) ON DELETE CASCADE,
    FOREIGN KEY (resident_id) REFERENCES sala.residents(resident_id) ON DELETE CASCADE
);

-- 3. Create functions
-- Auto-generate resident_id in format RXXX using a sequence
CREATE OR REPLACE FUNCTION sala.set_resident_id()
RETURNS TRIGGER AS $$
BEGIN
    NEW.resident_id := 'R' || LPAD(nextval('sala.resident_id_seq')::text, 3, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Auto-generate bill_id in format BI0001, BI0002, etc. using a sequence
CREATE OR REPLACE FUNCTION sala.set_bill_id()
RETURNS TRIGGER AS $$
BEGIN
    NEW.bill_id := 'BI' || LPAD(nextval('sala.bill_id_seq')::text, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Auto-generate payment_id in format P0001, P0002, etc. using a sequence
CREATE OR REPLACE FUNCTION sala.set_payment_id()
RETURNS TRIGGER AS $$
BEGIN
    NEW.payment_id := 'P' || LPAD(nextval('sala.payment_id_seq')::text, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create function to automatically update current_population when there is a change in the residents table
CREATE OR REPLACE FUNCTION sala.update_current_population()
RETURNS TRIGGER AS $$
BEGIN
    -- If operation is INSERT (adding a new resident to an apartment)
    IF (TG_OP = 'INSERT') THEN
        UPDATE sala.apartments
        SET current_population = current_population + 1
        WHERE apartment_id = NEW.apartment_id;
        RETURN NEW;
    -- If operation is DELETE (removing a resident from an apartment)
    ELSIF (TG_OP = 'DELETE') THEN
        UPDATE sala.apartments
        SET current_population = current_population - 1
        WHERE apartment_id = OLD.apartment_id;
        RETURN OLD;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create function to check and ensure current_population does not exceed max_population
CREATE OR REPLACE FUNCTION sala.check_population_limit()
RETURNS TRIGGER AS $$
BEGIN
    -- Check the number of residents after adding (INSERT) or updating (UPDATE)
    IF (TG_OP = 'INSERT') THEN
        -- If adding a resident, check if current_population exceeds max_population
        IF (SELECT current_population + 1 > max_population FROM sala.apartments WHERE apartment_id = NEW.apartment_id) THEN
            RAISE EXCEPTION 'Cannot insert more residents: current_population exceeds max_population';
        END IF;
    ELSIF (TG_OP = 'UPDATE') THEN
        -- If updating a resident, check if current_population exceeds max_population
        IF (SELECT current_population + 1 > max_population FROM sala.apartments WHERE apartment_id = NEW.apartment_id) THEN
            RAISE EXCEPTION 'Cannot update resident: current_population exceeds max_population';
        END IF;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create function to update vacancy_status based on current_population
CREATE OR REPLACE FUNCTION sala.update_vacancy_status()
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

-- Create function to ensure owner_id is a resident_id of the same apartment
CREATE OR REPLACE FUNCTION sala.check_owner_id()
RETURNS TRIGGER AS $$
BEGIN
    -- Check if owner_id is the resident_id of the same apartment
    IF NOT EXISTS (
        SELECT 1
        FROM sala.residents
        WHERE resident_id = NEW.owner_id
        AND apartment_id = NEW.apartment_id
    ) THEN
        RAISE EXCEPTION 'owner_id must be a resident_id of the same apartment';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create a function to generate or update apartment payment for a specific month
CREATE OR REPLACE FUNCTION sala.complete_apartment_payment(
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

    -- Find the pending payment for this apartment for the specified month
    SELECT payment_id INTO v_payment_id
    FROM sala.payments
    WHERE apartment_id = p_apartment_id
    AND EXTRACT(YEAR FROM payment_created_date) = v_current_year
    AND EXTRACT(MONTH FROM payment_created_date) = v_current_month
    AND payment_status = 'Pending'
    LIMIT 1;

    -- If no pending payment exists, return false
    IF v_payment_id IS NULL THEN
        RETURN FALSE;
    END IF;

    -- Update payment method and status to 'Completed'
    UPDATE sala.payments
    SET payment_method = p_payment_method,
        payment_status = 'Completed'
    WHERE payment_id = v_payment_id;

    -- Note: Updating associated bills to 'Paid' will be handled by the 'after_payment_status_update' trigger.

    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

-- REINTRODUCED FUNCTION: Manages apartment payments, creating one if it doesn't exist for the month.
-- This function ensures a payment record exists for a given apartment for a specific month/year.
-- If no payment exists, it creates a new 'Pending' payment.
-- It then links the provided bill_id to this payment.
CREATE OR REPLACE FUNCTION sala.manage_apartment_payment(
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
    FROM sala.payments
    WHERE apartment_id = p_apartment_id
    AND EXTRACT(YEAR FROM payment_created_date) = v_year
    AND EXTRACT(MONTH FROM payment_created_date) = v_month
    AND payment_status = 'Pending' -- Only consider pending payments for linking
    LIMIT 1;

    -- If no pending payment exists for this apartment for this month, create one
    IF v_payment_id IS NULL THEN
        INSERT INTO sala.payments (apartment_id, payment_created_date, total_amount, payment_status)
        VALUES (p_apartment_id, DATE_TRUNC('month', p_bill_date), 0, 'Pending') -- Set to the first day of the month
        RETURNING payment_id INTO v_payment_id;
    END IF;

    -- Link the bill to this payment
    -- Check if the bill is already linked to prevent duplicates in paymentsdetail
    IF NOT EXISTS (SELECT 1 FROM sala.paymentsdetail WHERE bill_id = p_bill_id AND payment_id = v_payment_id) THEN
        INSERT INTO sala.paymentsdetail (bill_id, payment_id)
        VALUES (p_bill_id, v_payment_id);
    END IF;

    RETURN v_payment_id;
END;
$$ LANGUAGE plpgsql;

-- Trigger to update bill payment status when a payment is completed
-- This trigger ensures that all bills associated with a payment are marked 'Paid'
-- when the payment's status changes from 'Pending' to 'Completed'.
CREATE OR REPLACE FUNCTION sala.update_bill_payment_status()
RETURNS TRIGGER AS $$
BEGIN
    -- When a payment status changes to 'Completed' from any other status
    IF NEW.payment_status = 'Completed' AND OLD.payment_status IS DISTINCT FROM NEW.payment_status THEN
        -- Update all associated bills to 'Paid'
        UPDATE sala.bills
        SET payment_status = 'Paid'
        WHERE bill_id IN (
            SELECT bill_id
            FROM sala.paymentsdetail
            WHERE payment_id = NEW.payment_id
        );
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- REINTRODUCED FUNCTION: Automatically links newly inserted bills to apartment payments.
-- This function is called by a trigger after a new bill is inserted.
-- It ensures that the bill is linked to the appropriate monthly payment, creating one if necessary.
CREATE OR REPLACE FUNCTION sala.link_bill_to_payment()
RETURNS TRIGGER AS $$
DECLARE
    v_payment_id VARCHAR(20);
BEGIN
    -- Only process pending bills
    IF NEW.payment_status = 'Pending' THEN
        -- Link the bill to the appropriate payment
        v_payment_id := sala.manage_apartment_payment(NEW.apartment_id, NEW.bill_id, NEW.bill_date);
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Update the function to calculate payment total amount
-- This trigger function maintains the total_amount on the payments table
-- by adding or subtracting the bill_amount of linked bills.
CREATE OR REPLACE FUNCTION sala.update_payment_total_amount()
RETURNS TRIGGER AS $$
DECLARE
    bill_amount DECIMAL(12, 2);
BEGIN
    -- For INSERT operations into paymentsdetail
    IF (TG_OP = 'INSERT') THEN
        -- Get the bill amount from the bills table
        SELECT b.bill_amount INTO bill_amount
        FROM sala.bills b
        WHERE b.bill_id = NEW.bill_id;

        -- Update the payment total_amount by adding the bill amount
        UPDATE sala.payments
        SET total_amount = total_amount + bill_amount
        WHERE payment_id = NEW.payment_id;
    END IF;

    -- For DELETE operations from paymentsdetail
    IF (TG_OP = 'DELETE') THEN
        -- Get the bill amount from the bills table (using OLD.bill_id)
        SELECT b.bill_amount INTO bill_amount
        FROM sala.bills b
        WHERE b.bill_id = OLD.bill_id;

        -- Update the payment total_amount by subtracting the bill amount
        UPDATE sala.payments
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
-- This function sets the completed_date for service requests
-- when their status changes from 'In Progress' to 'Completed'.
CREATE OR REPLACE FUNCTION sala.set_completed_date()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.status = 'In Progress' AND NEW.status = 'Completed' THEN
        NEW.completed_date := LOCALTIMESTAMP;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create function to set payment_date when payment_status changes
-- This function sets the payment_date for payments
-- when their status changes from 'Pending' to 'Completed'.
CREATE OR REPLACE FUNCTION sala.set_payment_date()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.payment_status = 'Pending' AND NEW.payment_status = 'Completed' THEN
        NEW.payment_date := CURRENT_TIMESTAMP;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- REVISED FUNCTION: Create a service bill when a service request is completed
-- This function now creates or updates a 'Service' bill
-- when a service_request's status changes to 'Completed'.
CREATE OR REPLACE FUNCTION sala.create_service_bill()
RETURNS TRIGGER AS $$
DECLARE
    existing_bill_id VARCHAR(20);
BEGIN
    -- This function is now triggered AFTER UPDATE on service_requests
    -- when status becomes 'Completed' and amount > 0.
    -- The logic below assumes these conditions are met by the trigger WHEN clause.

    -- Find an existing pending 'Service' bill for the same apartment
    SELECT bill_id INTO existing_bill_id
    FROM sala.bills
    WHERE apartment_id = NEW.apartment_id
      AND bill_type = 'Service'
      AND payment_status = 'Pending' -- Only consider pending service bills for aggregation
    LIMIT 1;

    IF existing_bill_id IS NOT NULL THEN
        -- If a bill exists, add the new service amount to it
        UPDATE sala.bills
        SET bill_amount = bill_amount + NEW.amount
        WHERE bill_id = existing_bill_id;
    ELSE
        -- If no pending service bill exists, create a new one
        INSERT INTO sala.bills (
            apartment_id,
            bill_type,
            bill_amount,
            due_date,
            bill_date,
            payment_status
        )
        VALUES (
            NEW.apartment_id,
            'Service',
            NEW.amount,
            NEW.completed_date + INTERVAL '15 days', -- Due date 15 days from completion
            NEW.completed_date,                      -- Bill date is the completion date
            'Pending'
        );
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 4. Create all triggers
-- Trigger to call the function before INSERT on residents to auto-generate resident_id
CREATE OR REPLACE TRIGGER before_resident_insert
BEFORE INSERT ON sala.residents
FOR EACH ROW
EXECUTE FUNCTION sala.set_resident_id();

-- Trigger to call the function before INSERT on bills to auto-generate bill_id
CREATE OR REPLACE TRIGGER before_bill_insert
BEFORE INSERT ON sala.bills
FOR EACH ROW
EXECUTE FUNCTION sala.set_bill_id();

-- Trigger to call the function before INSERT on payments to auto-generate payment_id
CREATE OR REPLACE TRIGGER before_payment_insert
BEFORE INSERT ON sala.payments
FOR EACH ROW
WHEN (NEW.payment_id IS NULL)
EXECUTE FUNCTION sala.set_payment_id();

-- Trigger to call the function after INSERT on residents to update current_population
CREATE OR REPLACE TRIGGER after_resident_insert
AFTER INSERT ON sala.residents
FOR EACH ROW
EXECUTE FUNCTION sala.update_current_population();

-- Trigger to call the function after DELETE on residents to update current_population
CREATE OR REPLACE TRIGGER after_resident_delete
AFTER DELETE ON sala.residents
FOR EACH ROW
EXECUTE FUNCTION sala.update_current_population();

-- Trigger before INSERT on apartments to update vacancy_status
CREATE OR REPLACE TRIGGER update_vacancy_status_insert
BEFORE INSERT ON sala.apartments
FOR EACH ROW
EXECUTE FUNCTION sala.update_vacancy_status();

-- Trigger before UPDATE on apartments to update vacancy_status
CREATE OR REPLACE TRIGGER update_vacancy_status_update
BEFORE UPDATE ON sala.apartments
FOR EACH ROW
EXECUTE FUNCTION sala.update_vacancy_status();

-- Check when owner_id is provided
CREATE OR REPLACE TRIGGER check_owner_id_insert
BEFORE INSERT ON sala.apartments
FOR EACH ROW
WHEN (NEW.owner_id IS NOT NULL)
EXECUTE FUNCTION sala.check_owner_id();

CREATE OR REPLACE TRIGGER check_owner_id_update
BEFORE UPDATE ON sala.apartments
FOR EACH ROW
WHEN (NEW.owner_id IS NOT NULL)
EXECUTE FUNCTION sala.check_owner_id();

-- Triggers for population limit checking
CREATE OR REPLACE TRIGGER before_resident_insert_check_limit
BEFORE INSERT ON sala.residents
FOR EACH ROW
EXECUTE FUNCTION sala.check_population_limit();

CREATE OR REPLACE TRIGGER before_resident_update_check_limit
BEFORE UPDATE ON sala.residents
FOR EACH ROW
WHEN (OLD.apartment_id IS DISTINCT FROM NEW.apartment_id)
EXECUTE FUNCTION sala.check_population_limit();

-- Create triggers for automatically updating status linking bills to payments
CREATE OR REPLACE TRIGGER after_bill_insert_link_payment
AFTER INSERT ON sala.bills
FOR EACH ROW
EXECUTE FUNCTION sala.link_bill_to_payment();

CREATE OR REPLACE TRIGGER after_payment_status_update
AFTER UPDATE OF payment_status ON sala.payments
FOR EACH ROW
EXECUTE FUNCTION sala.update_bill_payment_status();

-- Create triggers for payment total amount calculation
CREATE OR REPLACE TRIGGER after_paymentdetail_insert_update_total
AFTER INSERT ON sala.paymentsdetail
FOR EACH ROW
EXECUTE FUNCTION sala.update_payment_total_amount();

CREATE OR REPLACE TRIGGER after_paymentdetail_delete_update_total
AFTER DELETE ON sala.paymentsdetail
FOR EACH ROW
EXECUTE FUNCTION sala.update_payment_total_amount();

-- Create trigger on service_requests table before updating status
CREATE OR REPLACE TRIGGER trg_set_completed_date
BEFORE UPDATE OF status ON sala.service_requests
FOR EACH ROW
WHEN (OLD.status = 'In Progress' AND NEW.status = 'Completed')
EXECUTE FUNCTION sala.set_completed_date();

-- Create trigger on payments table before updating payment_status
CREATE OR REPLACE TRIGGER trg_update_payment_date
BEFORE UPDATE OF payment_status ON sala.payments
FOR EACH ROW
WHEN (OLD.payment_status = 'Pending' AND NEW.payment_status = 'Completed')
EXECUTE FUNCTION sala.set_payment_date();

-- REVISED TRIGGER: Trigger for service_requests to create service bills
-- This trigger now fires AFTER an UPDATE on 'service_requests',
-- specifically when the 'status' changes to 'Completed' and the 'amount' is greater than 0.
CREATE OR REPLACE TRIGGER trg_create_service_bill
AFTER UPDATE OF status ON sala.service_requests
FOR EACH ROW
WHEN (NEW.status = 'Completed' AND OLD.status IS DISTINCT FROM NEW.status AND NEW.amount > 0)
EXECUTE FUNCTION sala.create_service_bill();

-- 5. Insert data

INSERT INTO sala.buildings (building_id, building_name, address)
VALUES ('B001', 'Sala', '789 Sala Boulevard');

INSERT INTO sala.apartments (apartment_id, building_id, max_population, transfer_status)
VALUES
    ('S01.01', 'B001', 6, 'Available'),
    ('S01.02', 'B001', 6, 'Available'),
    ('S02.03', 'B001', 6, 'Not Available'),
    ('S02.04', 'B001', 6, 'Available'),
    ('S03.05', 'B001', 6, 'Available'),
    ('S03.06', 'B001', 6, 'Available'),
    ('S04.07', 'B001', 6, 'Available'),
    ('S04.08', 'B001', 6, 'Available'),
    ('S05.09', 'B001', 6, 'Not Available'),
    ('S05.10', 'B001', 6, 'Available'),
    ('S06.11', 'B001', 6, 'Available'),
    ('S06.12', 'B001', 6, 'Available'),
    ('S07.13', 'B001', 6, 'Not Available'),
    ('S07.14', 'B001', 6, 'Available'),
    ('S08.15', 'B001', 6, 'Available'),
    ('S08.16', 'B001', 6, 'Available'),
    ('S09.17', 'B001', 6, 'Available'),
    ('S09.18', 'B001', 6, 'Available'),
    ('S10.19', 'B001', 6, 'Not Available'),
    ('S10.20', 'B001', 6, 'Available');

INSERT INTO sala.residents
    (name, phone_number, email, sex, identification_number, apartment_id, created_at, updated_at, date_registered)
VALUES
    -- First 20 residents with a created_at, updated_at and date_registered set to one month ago
    ('Carlos Silva', '0901112235', 'carlos.silva@example.com', 'Male', '076000870001', 'S01.01', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Diana Martinez', '0912233447', 'diana.martinez@example.com', 'Female', '076120870002', 'S01.02', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Eduardo Lopez', '0923344558', 'eduardo.lopez@example.com', 'Male', '076220880003', 'S02.03', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Fatima Rodriguez', '0934455669', 'fatima.rodriguez@example.com', 'Female', '076320890004', 'S02.04', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Gabriel Hernandez', '0945566780', 'gabriel.hernandez@example.com', 'Male', '076420900005', 'S03.05', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Helena Gonzalez', '0956677881', 'helena.gonzalez@example.com', 'Female', '076520910006', 'S03.06', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Ivan Perez', '0967788992', 'ivan.perez@example.com', 'Male', '076620920007', 'S04.07', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Julia Sanchez', '0978899003', 'julia.sanchez@example.com', 'Female', '076720930008', 'S04.08', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Kevin Ramirez', '0989900114', 'kevin.ramirez@example.com', 'Male', '076820940009', 'S05.09', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Lucia Torres', '0912345680', 'lucia.torres@example.com', 'Female', '076920950010', 'S05.10', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Miguel Flores', '0923456781', 'miguel.flores@example.com', 'Male', '077020960011', 'S06.11', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Natalia Rivera', '0934567892', 'natalia.rivera@example.com', 'Female', '077120970012', 'S06.12', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Omar Morales', '0945678903', 'omar.morales@example.com', 'Male', '077220980013', 'S07.13', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Paloma Ortega', '0956789014', 'paloma.ortega@example.com', 'Female', '077320990014', 'S07.14', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Quinton Vargas', '0967890125', 'quinton.vargas@example.com', 'Male', '077420000015', 'S08.15', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Rosa Castro', '0978901236', 'rosa.castro@example.com', 'Female', '077520010016', 'S08.16', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Sebastian Ruiz', '0989012347', 'sebastian.ruiz@example.com', 'Male', '077620020017', 'S09.17', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Teresa Diaz', '0912345682', 'teresa.diaz@example.com', 'Female', '077720030018', 'S09.18', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Ulises Jimenez', '0923456784', 'ulises.jimenez@example.com', 'Male', '077820040019', 'S10.19', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Valentina Mendez', '0934567895', 'valentina.mendez@example.com', 'Female', '077920050020', 'S10.20', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    -- Next 20 residents with the current timestamp for created_at, updated_at and date_registered
    ('Walter Cruz', '0911122336', 'walter.cruz@example.com', 'Male', '078220180041', 'S01.01', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Ximena Reyes', '0922233447', 'ximena.reyes@example.com', 'Female', '078320190042', 'S01.02', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Yago Gutierrez', '0933344558', 'yago.gutierrez@example.com', 'Male', '078420200043', 'S02.03', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Zara Moreno', '0944455669', 'zara.moreno@example.com', 'Female', '078520210044', 'S02.04', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Andres Muñoz', '0955566780', 'andres.munoz@example.com', 'Male', '078620220045', 'S03.05', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Beatriz Romero', '0966677881', 'beatriz.romero@example.com', 'Female', '078720230046', 'S03.06', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Cristian Navarro', '0977788992', 'cristian.navarro@example.com', 'Male', '078820240047', 'S04.07', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Daniela Medina', '0988899003', 'daniela.medina@example.com', 'Female', '078920250048', 'S04.08', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Emilio Aguilar', '0910000114', 'emilio.aguilar@example.com', 'Male', '079020260049', 'S05.09', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Fernanda Delgado', '0921112235', 'fernanda.delgado@example.com', 'Female', '079120270050', 'S05.10', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Gonzalo Vega', '0932223346', 'gonzalo.vega@example.com', 'Male', '079220280051', 'S06.11', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Hilda Carrillo', '0943334457', 'hilda.carrillo@example.com', 'Female', '079320290052', 'S06.12', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Ignacio Soto', '0954445568', 'ignacio.soto@example.com', 'Male', '079420300053', 'S07.13', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Juana Contreras', '0965556679', 'juana.contreras@example.com', 'Female', '079520310054', 'S07.14', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Karina Espinoza', '0976667780', 'karina.espinoza@example.com', 'Female', '079620320055', 'S08.15', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Lorenzo Valdez', '0987778891', 'lorenzo.valdez@example.com', 'Male', '079720330056', 'S08.16', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Marisol Ponce', '0918889902', 'marisol.ponce@example.com', 'Female', '079820340057', 'S09.17', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Nicolas Salinas', '0929990003', 'nicolas.salinas@example.com', 'Male', '079920350058', 'S09.18', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Ofelia Fuentes', '0930001114', 'ofelia.fuentes@example.com', 'Female', '080020360059', 'S10.19', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Pablo Ramos', '0941112235', 'pablo.ramos@example.com', 'Male', '080120370060', 'S10.20', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

-- Insert mock bills for various apartments
INSERT INTO sala.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status)
VALUES
    -- Apartment S01.01 bills
    ('Electricity', 'S01.01', 870.00, '2025-05-25', '2025-06-01', 'Pending'),
    ('Water', 'S01.01', 330.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'S01.01', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Apartment S01.02 bills
    ('Electricity', 'S01.02', 800.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'S01.02', 300.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'S01.02', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Apartment S02.03 bills
    ('Electricity', 'S02.03', 920.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'S02.03', 360.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'S02.03', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Apartment S02.04 bills
    ('Electricity', 'S02.04', 640.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'S02.04', 220.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'S02.04', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Previous month bills for S01.01 (already paid)
    ('Electricity', 'S01.01', 810.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Water', 'S01.01', 310.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Maintenance', 'S01.01', 200.00, '2025-04-25', '2025-04-01', 'Pending'),

    -- Previous month bills for S01.02 (already paid)
    ('Electricity', 'S01.02', 740.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Water', 'S01.02', 230.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Maintenance', 'S01.02', 200.00, '2025-04-25', '2025-04-01', 'Pending');

-- Now let's complete some payments for the previous month
-- Note: We don't need to manually insert into payments or paymentsdetail tables
-- as our triggers will handle that automatically

-- Complete payments for April for apartments S01.01 and S01.02
SELECT sala.complete_apartment_payment('S01.01', 'Bank Transfer', 2025, 4);
SELECT sala.complete_apartment_payment('S01.02', 'Cash', 2025, 4);

-- Insert bills for some apartments for June (future month)
INSERT INTO sala.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status)
VALUES
    ('Electricity', 'S02.03', 880.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Water', 'S02.03', 340.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Maintenance', 'S02.03', 200.00, '2025-06-25', '2025-06-01', 'Pending'),

    ('Electricity', 'S02.04', 790.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Water', 'S02.04', 310.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Maintenance', 'S02.04', 200.00, '2025-06-25', '2025-06-01', 'Pending');

-- Complete some current month payments
SELECT sala.complete_apartment_payment('S02.03', 'Bank Transfer', 2025, 5);
SELECT sala.complete_apartment_payment('S02.04', 'Mobile Payment', 2025, 5);

-- Insert some special bills
INSERT INTO sala.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status)
VALUES
    ('Special Maintenance', 'S01.01', 1600.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Special Maintenance', 'S01.02', 1300.00, '2025-05-25', '2025-05-01', 'Pending');

-- Insert service requests with an initial status (e.g., 'In Progress')
INSERT INTO sala.service_requests (
    request_id, apartment_id, resident_id, category, description, status, amount, request_date, completed_date
)
VALUES
    ('SSR001', 'S01.01', 'R001', 'Plumbing', 'Water pressure issue', 'In Progress', 160, CURRENT_TIMESTAMP - INTERVAL '14 days', NULL),
    ('SSR002', 'S01.02', 'R002', 'Electrical', 'Switch replacement', 'In Progress', 130, CURRENT_TIMESTAMP - INTERVAL '27 days', NULL),
    ('SSR003', 'S02.03', 'R003', 'HVAC', 'Ventilation cleaning', 'In Progress', 440, CURRENT_TIMESTAMP - INTERVAL '16 days', NULL),
    ('SSR004', 'S02.04', 'R004', 'Cleaning', 'Window cleaning service', 'In Progress', 110, CURRENT_TIMESTAMP - INTERVAL '32 days', NULL),
    ('SSR005', 'S03.05', 'R005', 'Security', 'Lock replacement', 'In Progress', 210, CURRENT_TIMESTAMP - INTERVAL '11 days', NULL),
    ('SSR006', 'S03.06', 'R006', 'Maintenance', 'Cabinet repair', 'In Progress', 390, CURRENT_TIMESTAMP - INTERVAL '23 days', NULL),
    ('SSR007', 'S04.07', 'R007', 'Plumbing', 'Pipe leak repair', 'In Progress', 260, CURRENT_TIMESTAMP - INTERVAL '42 days', NULL),
    ('SSR008', 'S04.08', 'R008', 'Electrical', 'Circuit repair', 'In Progress', 320, CURRENT_TIMESTAMP - INTERVAL '19 days', NULL),
    ('SSR009', 'S05.09', 'R009', 'HVAC', 'Filter replacement', 'In Progress', 480, CURRENT_TIMESTAMP - INTERVAL '58 days', NULL),
    ('SSR010', 'S05.10', 'R010', 'Cleaning', 'Bathroom deep clean', 'In Progress', 140, CURRENT_TIMESTAMP - INTERVAL '13 days', NULL);

-- Now, update some service requests to 'Completed' to trigger bill creation
UPDATE sala.service_requests
SET status = 'Completed'
WHERE request_id IN ('SSR001', 'SSR002', 'SSR003', 'SSR004', 'SSR005', 'SSR006');

-- Test the accumulation for an existing pending service bill
-- Let's say SSR001 gets another related service completed
INSERT INTO sala.service_requests (
    request_id, apartment_id, resident_id, category, description, status, amount, request_date, completed_date
)
VALUES
    ('SSR011', 'S01.01', 'R001', 'Plumbing', 'Follow-up water pressure adjustment', 'In Progress', 85, CURRENT_TIMESTAMP - INTERVAL '1 day', NULL);

UPDATE sala.service_requests
SET status = 'Completed'
WHERE request_id = 'SSR011';

-- Count all occupied apartments for 2 cases: until now and until 1 month ago
SELECT COUNT(*) AS occupied_apartments_now
FROM sala.apartments
WHERE current_population > 0;

SELECT COUNT(DISTINCT apartment_id) AS occupied_apartments_1_month_ago
FROM sala.residents
WHERE created_at <= CURRENT_DATE - INTERVAL '1 month';

-- 6. Show data from all of the TABLES
SELECT * FROM sala.users;

SELECT * FROM sala.buildings;
SELECT * FROM sala.apartments;
SELECT * FROM sala.residents;

SELECT * FROM sala.bills;
SELECT * FROM sala.payments;
SELECT * FROM sala.paymentsdetail;

SELECT * FROM sala.service_requests;

ALTER TABLE sala.service_requests 
ALTER COLUMN completed_date TYPE TIMESTAMPTZ,
ALTER COLUMN request_date TYPE TIMESTAMPTZ;