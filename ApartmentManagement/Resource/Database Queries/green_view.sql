-- 1. Create schema
CREATE SCHEMA IF NOT EXISTS green_view;

CREATE SEQUENCE IF NOT EXISTS green_view.resident_id_seq START 1;
CREATE SEQUENCE IF NOT EXISTS green_view.bill_id_seq START 1;
CREATE SEQUENCE IF NOT EXISTS green_view.payment_id_seq START 1;

-- 2. Create tables
CREATE TABLE green_view.users (
    user_id  VARCHAR(20) PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    role VARCHAR(50) NOT NULL,
    status VARCHAR(50) DEFAULT 'Active',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP
);

CREATE TABLE green_view.buildings (
    building_id  VARCHAR(20) PRIMARY KEY,
    building_name VARCHAR(255) NOT NULL,
    address VARCHAR(255) NOT NULL,
    manager_id VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (manager_id) REFERENCES green_view.users(user_id) ON DELETE SET NULL
);

CREATE TABLE green_view.apartments (
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
    FOREIGN KEY (building_id) REFERENCES green_view.buildings(building_id) ON DELETE CASCADE
);

CREATE TABLE green_view.residents (
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
    FOREIGN KEY (apartment_id) REFERENCES green_view.apartments(apartment_id) ON DELETE CASCADE,
    FOREIGN KEY (owner_id) REFERENCES green_view.residents(resident_id) ON DELETE CASCADE
);

CREATE TABLE green_view.bills (
    bill_id  VARCHAR(20) PRIMARY KEY,
    apartment_id  VARCHAR(20) NOT NULL,
    bill_type  VARCHAR(50) NOT NULL,
    bill_amount  DECIMAL(12, 2) NOT NULL,
    due_date  TIMESTAMP NOT NULL,
    bill_date  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    payment_status VARCHAR(50) DEFAULT 'Pending',
    FOREIGN KEY (apartment_id) REFERENCES green_view.apartments(apartment_id) ON DELETE CASCADE
);

CREATE TABLE green_view.payments (
    payment_id VARCHAR(20) PRIMARY KEY,
    apartment_id VARCHAR(20) NOT NULL,
    total_amount DECIMAL(12, 2) NOT NULL,
    payment_date TIMESTAMP DEFAULT '9999-12-31 23:59:59',
    payment_method VARCHAR(50) DEFAULT '',
    payment_status VARCHAR(50) DEFAULT 'Pending',
    payment_created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (apartment_id) REFERENCES green_view.apartments(apartment_id) ON DELETE CASCADE
);

CREATE TABLE green_view.paymentsdetail (
    bill_id VARCHAR(20),
    payment_id VARCHAR(20),
    PRIMARY KEY (bill_id, payment_id),
    FOREIGN KEY (bill_id) REFERENCES green_view.bills(bill_id) ON DELETE CASCADE,
    FOREIGN KEY (payment_id) REFERENCES green_view.payments(payment_id) ON DELETE CASCADE
);

CREATE TABLE green_view.service_requests (
    request_id  VARCHAR(20) PRIMARY KEY,
    apartment_id  VARCHAR(20)  NOT NULL,
    resident_id  VARCHAR(20) NOT NULL,
    category VARCHAR(50) NOT NULL,
    description TEXT NOT NULL,
    amount  DECIMAL(12, 2) NOT null DEFAULT 0,
    status VARCHAR(50) DEFAULT 'Pending',
    request_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_date TIMESTAMP,
    FOREIGN KEY (apartment_id) REFERENCES green_view.apartments(apartment_id) ON DELETE CASCADE,
    FOREIGN KEY (resident_id) REFERENCES green_view.residents(resident_id) ON DELETE CASCADE
);

-- 3. Create functions
-- Auto-generate resident_id in format RXXX using a sequence
CREATE OR REPLACE FUNCTION green_view.set_resident_id()
RETURNS TRIGGER AS $$
BEGIN
    NEW.resident_id := 'R' || LPAD(nextval('green_view.resident_id_seq')::text, 3, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Auto-generate bill_id in format BI0001, BI0002, etc. using a sequence
CREATE OR REPLACE FUNCTION green_view.set_bill_id()
RETURNS TRIGGER AS $$
BEGIN
    NEW.bill_id := 'BI' || LPAD(nextval('green_view.bill_id_seq')::text, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Auto-generate payment_id in format P0001, P0002, etc. using a sequence
CREATE OR REPLACE FUNCTION green_view.set_payment_id()
RETURNS TRIGGER AS $$
BEGIN
    NEW.payment_id := 'P' || LPAD(nextval('green_view.payment_id_seq')::text, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Auto-generate service_request_id in format SRXXX using a sequence
CREATE OR REPLACE FUNCTION green_view.set_service_request_id()
RETURNS TRIGGER AS $$
BEGIN
    NEW.request_id := 'SR' || LPAD(nextval('green_view.payment_id_seq')::text, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create function to automatically update current_population when there is a change in the residents table
CREATE OR REPLACE FUNCTION green_view.update_current_population()
RETURNS TRIGGER AS $$
BEGIN
    -- If operation is INSERT (adding a new resident to an apartment)
    IF (TG_OP = 'INSERT') THEN
        UPDATE green_view.apartments
        SET current_population = current_population + 1
        WHERE apartment_id = NEW.apartment_id;
        RETURN NEW;
    -- If operation is DELETE (removing a resident from an apartment)
    ELSIF (TG_OP = 'DELETE') THEN
        UPDATE green_view.apartments
        SET current_population = current_population - 1
        WHERE apartment_id = OLD.apartment_id;
        RETURN OLD;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create function to check and ensure current_population does not exceed max_population
CREATE OR REPLACE FUNCTION green_view.check_population_limit()
RETURNS TRIGGER AS $$
BEGIN
    -- Check the number of residents after adding (INSERT) or updating (UPDATE)
    IF (TG_OP = 'INSERT') THEN
        -- If adding a resident, check if current_population exceeds max_population
        IF (SELECT current_population + 1 > max_population FROM green_view.apartments WHERE apartment_id = NEW.apartment_id) THEN
            RAISE EXCEPTION 'Cannot insert more residents: current_population exceeds max_population';
        END IF;
    ELSIF (TG_OP = 'UPDATE') THEN
        -- If updating a resident, check if current_population exceeds max_population
        IF (SELECT current_population + 1 > max_population FROM green_view.apartments WHERE apartment_id = NEW.apartment_id) THEN
            RAISE EXCEPTION 'Cannot update resident: current_population exceeds max_population';
        END IF;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create function to update vacancy_status based on current_population
CREATE OR REPLACE FUNCTION green_view.update_vacancy_status()
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
CREATE OR REPLACE FUNCTION green_view.check_owner_id()
RETURNS TRIGGER AS $$
BEGIN
    -- Check if owner_id is the resident_id of the same apartment
    IF NOT EXISTS (
        SELECT 1
        FROM green_view.residents
        WHERE resident_id = NEW.owner_id
        AND apartment_id = NEW.apartment_id
    ) THEN
        RAISE EXCEPTION 'owner_id must be a resident_id of the same apartment';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create a function to generate or update apartment payment for a specific month
CREATE OR REPLACE FUNCTION green_view.complete_apartment_payment(
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
    FROM green_view.payments
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
    UPDATE green_view.payments
    SET payment_method = p_payment_method,
        payment_status = 'Completed'
    WHERE payment_id = v_payment_id;

    -- Note: Updating associated bills to 'Paid' will be handled by the 'after_payment_status_update' trigger.

    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

-- This function ensures a payment record exists for a given apartment for a specific month/year.
-- If no payment exists, it creates a new 'Pending' payment.
-- It then links the provided bill_id to this payment.
CREATE OR REPLACE FUNCTION green_view.manage_apartment_payment(
    p_apartment_id VARCHAR(20),
    p_bill_id VARCHAR(20),
    p_bill_date TIMESTAMP
) RETURNS VARCHAR(20) AS $$
DECLARE
    v_payment_id VARCHAR(20);
    v_bill_month DATE;
    v_payment_date DATE;
BEGIN
    -- Get the first day of the bill's month
    v_bill_month := DATE_TRUNC('month', p_bill_date);
    -- Set payment date to first day of next month
    v_payment_date := v_bill_month + INTERVAL '1 month';

    -- Check if a payment already exists for bills from this month
    SELECT payment_id INTO v_payment_id
    FROM green_view.payments
    WHERE apartment_id = p_apartment_id
    AND DATE_TRUNC('month', payment_created_date) = v_payment_date
    AND payment_status = 'Pending'
    LIMIT 1;

    -- If no pending payment exists for bills from this month, create one
    IF v_payment_id IS NULL THEN
        INSERT INTO green_view.payments (apartment_id, payment_created_date, total_amount, payment_status)
        VALUES (p_apartment_id, v_payment_date, 0, 'Pending')
        RETURNING payment_id INTO v_payment_id;
    END IF;

    -- Link the bill to this payment if not already linked
    IF NOT EXISTS (SELECT 1 FROM green_view.paymentsdetail WHERE bill_id = p_bill_id AND payment_id = v_payment_id) THEN
        INSERT INTO green_view.paymentsdetail (bill_id, payment_id)
        VALUES (p_bill_id, v_payment_id);
    END IF;

    RETURN v_payment_id;
END;
$$ LANGUAGE plpgsql;

-- Trigger to update bill payment status when a payment is completed
-- This trigger ensures that all bills associated with a payment are marked 'Paid'
-- when the payment's status changes from 'Pending' to 'Completed'.
CREATE OR REPLACE FUNCTION green_view.update_bill_payment_status()
RETURNS TRIGGER AS $$
BEGIN
    -- When a payment status changes to 'Completed' from any other status
    IF NEW.payment_status = 'Completed' AND OLD.payment_status IS DISTINCT FROM NEW.payment_status THEN
        -- Update all associated bills to 'Paid'
        UPDATE green_view.bills
        SET payment_status = 'Paid'
        WHERE bill_id IN (
            SELECT bill_id
            FROM green_view.paymentsdetail
            WHERE payment_id = NEW.payment_id
        );
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- REINTRODUCED FUNCTION: Automatically links newly inserted bills to apartment payments.
-- This function is called by a trigger after a new bill is inserted.
-- It ensures that the bill is linked to the appropriate monthly payment, creating one if necessary.
CREATE OR REPLACE FUNCTION green_view.link_bill_to_payment()
RETURNS TRIGGER AS $$
DECLARE
    v_payment_id VARCHAR(20);
BEGIN
    -- Only process pending bills, but EXCLUDE penalty bills from auto-linking
    IF NEW.payment_status = 'Pending' AND NEW.bill_type != 'Late Payment Penalty' THEN
        -- Link the bill to the appropriate payment
        v_payment_id := green_view.manage_apartment_payment(NEW.apartment_id, NEW.bill_id, NEW.bill_date);
    END IF;
    -- Note: Penalty bills are manually linked in the create_penalty_bill function

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Update the function to calculate payment total amount
-- This trigger function maintains the total_amount on the payments table
-- by adding or subtracting the bill_amount of linked bills.
CREATE OR REPLACE FUNCTION green_view.update_payment_total_amount()
RETURNS TRIGGER AS $$
DECLARE
    bill_amount DECIMAL(12, 2);
BEGIN
    -- For INSERT operations into paymentsdetail
    IF (TG_OP = 'INSERT') THEN
        -- Get the bill amount from the bills table
        SELECT b.bill_amount INTO bill_amount
        FROM green_view.bills b
        WHERE b.bill_id = NEW.bill_id;

        -- Update the payment total_amount by adding the bill amount
        UPDATE green_view.payments
        SET total_amount = total_amount + bill_amount
        WHERE payment_id = NEW.payment_id;
    END IF;

    -- For DELETE operations from paymentsdetail
    IF (TG_OP = 'DELETE') THEN
        -- Get the bill amount from the bills table (using OLD.bill_id)
        SELECT b.bill_amount INTO bill_amount
        FROM green_view.bills b
        WHERE b.bill_id = OLD.bill_id;

        -- Update the payment total_amount by subtracting the bill amount
        UPDATE green_view.payments
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
CREATE OR REPLACE FUNCTION green_view.set_completed_date()
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
CREATE OR REPLACE FUNCTION green_view.set_payment_date()
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
CREATE OR REPLACE FUNCTION green_view.create_service_bill()
RETURNS TRIGGER AS $$
DECLARE
    existing_bill_id VARCHAR(20);
BEGIN
    -- This function is now triggered AFTER UPDATE on service_requests
    -- when status becomes 'Completed' and amount > 0.
    -- The logic below assumes these conditions are met by the trigger WHEN clause.

    -- Find an existing pending 'Service' bill for the same apartment
    SELECT bill_id INTO existing_bill_id
    FROM green_view.bills
    WHERE apartment_id = NEW.apartment_id
      AND bill_type = 'Service'
      AND payment_status = 'Pending' -- Only consider pending service bills for aggregation
    LIMIT 1;

    IF existing_bill_id IS NOT NULL THEN
        -- If a bill exists, add the new service amount to it
        UPDATE green_view.bills
        SET bill_amount = bill_amount + NEW.amount
        WHERE bill_id = existing_bill_id;
    ELSE
        -- If no pending service bill exists, create a new one
        INSERT INTO green_view.bills (
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

-- Create function to handle penalty bill creation for overdue payments
CREATE OR REPLACE FUNCTION green_view.create_penalty_bill()
RETURNS TRIGGER AS $$
DECLARE
    v_penalty_amount DECIMAL(12, 2);
    v_bill_id VARCHAR(20);
BEGIN
    -- Only proceed if status is changing to 'Overdue' from a non-overdue status
    IF NEW.payment_status = 'Overdue' AND (OLD.payment_status IS NULL OR OLD.payment_status != 'Overdue') THEN
        
        -- Check if a penalty bill already exists for THIS SPECIFIC PAYMENT
        IF NOT EXISTS (
            SELECT 1 
            FROM green_view.bills b 
            INNER JOIN green_view.paymentsdetail pd ON b.bill_id = pd.bill_id
            WHERE pd.payment_id = NEW.payment_id 
            AND b.bill_type = 'Late Payment Penalty'
        ) THEN
            -- Calculate 5% penalty of the total amount
            v_penalty_amount := NEW.total_amount * 0.05;
            
            -- Insert new penalty bill
            INSERT INTO green_view.bills (
                apartment_id,
                bill_type,
                bill_amount,
                due_date,
                bill_date,
                payment_status
            )
            VALUES (
                NEW.apartment_id,
                'Late Payment Penalty',
                v_penalty_amount,
                CURRENT_TIMESTAMP + INTERVAL '1 month', -- Due 1 month from now
                CURRENT_TIMESTAMP,
                'Pending'
            )
            RETURNING bill_id INTO v_bill_id;

            -- Link the penalty bill ONLY to the overdue payment
            INSERT INTO green_view.paymentsdetail (bill_id, payment_id)
            VALUES (v_bill_id, NEW.payment_id);
            
            -- Important: Do NOT let this penalty bill get auto-linked to other payments
            -- The penalty should stay with the original overdue payment
        END IF;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- 4. Create all triggers
-- Trigger to create a service bill when a service request is completed
CREATE OR REPLACE TRIGGER trg_create_service_bill
AFTER UPDATE OF status ON green_view.service_requests
FOR EACH ROW
WHEN (NEW.status = 'Completed' AND OLD.status IS DISTINCT FROM NEW.status AND NEW.amount > 0)
EXECUTE FUNCTION green_view.create_service_bill();

-- Trigger to call the function before INSERT on residents to auto-generate resident_id
CREATE OR REPLACE TRIGGER before_resident_insert
BEFORE INSERT ON green_view.residents
FOR EACH ROW
EXECUTE FUNCTION green_view.set_resident_id();

-- Trigger to call the function before INSERT on bills to auto-generate bill_id
CREATE OR REPLACE TRIGGER before_bill_insert
BEFORE INSERT ON green_view.bills
FOR EACH ROW
EXECUTE FUNCTION green_view.set_bill_id();

-- Trigger to call the function before INSERT on payments to auto-generate payment_id
CREATE OR REPLACE TRIGGER before_payment_insert
BEFORE INSERT ON green_view.payments
FOR EACH ROW
WHEN (NEW.payment_id IS NULL)
EXECUTE FUNCTION green_view.set_payment_id();

-- Trigger to call the function before INSERT on service_requests to auto-generate request_id
CREATE OR REPLACE TRIGGER before_service_request_insert
BEFORE INSERT ON green_view.service_requests
FOR EACH ROW
EXECUTE FUNCTION green_view.set_service_request_id();

-- Trigger to call the function after INSERT on residents to update current_population
CREATE OR REPLACE TRIGGER after_resident_insert
AFTER INSERT ON green_view.residents
FOR EACH ROW
EXECUTE FUNCTION green_view.update_current_population();

-- Trigger to call the function after DELETE on residents to update current_population
CREATE OR REPLACE TRIGGER after_resident_delete
AFTER DELETE ON green_view.residents
FOR EACH ROW
EXECUTE FUNCTION green_view.update_current_population();

-- Trigger before INSERT on apartments to update vacancy_status
CREATE OR REPLACE TRIGGER update_vacancy_status_insert
BEFORE INSERT ON green_view.apartments
FOR EACH ROW
EXECUTE FUNCTION green_view.update_vacancy_status();

-- Trigger before UPDATE on apartments to update vacancy_status
CREATE OR REPLACE TRIGGER update_vacancy_status_update
BEFORE UPDATE ON green_view.apartments
FOR EACH ROW
EXECUTE FUNCTION green_view.update_vacancy_status();

-- Check when owner_id is provided
CREATE OR REPLACE TRIGGER check_owner_id_insert
BEFORE INSERT ON green_view.apartments
FOR EACH ROW
WHEN (NEW.owner_id IS NOT NULL)
EXECUTE FUNCTION green_view.check_owner_id();

CREATE OR REPLACE TRIGGER check_owner_id_update
BEFORE UPDATE ON green_view.apartments
FOR EACH ROW
WHEN (NEW.owner_id IS NOT NULL)
EXECUTE FUNCTION green_view.check_owner_id();

-- Triggers for population limit checking
CREATE OR REPLACE TRIGGER before_resident_insert_check_limit
BEFORE INSERT ON green_view.residents
FOR EACH ROW
EXECUTE FUNCTION green_view.check_population_limit();

CREATE OR REPLACE TRIGGER before_resident_update_check_limit
BEFORE UPDATE ON green_view.residents
FOR EACH ROW
WHEN (OLD.apartment_id IS DISTINCT FROM NEW.apartment_id)
EXECUTE FUNCTION green_view.check_population_limit();

-- Create triggers for automatically updating status linking bills to payments
CREATE OR REPLACE TRIGGER after_bill_insert_link_payment
AFTER INSERT ON green_view.bills
FOR EACH ROW
EXECUTE FUNCTION green_view.link_bill_to_payment();

CREATE OR REPLACE TRIGGER after_payment_status_update
AFTER UPDATE OF payment_status ON green_view.payments
FOR EACH ROW
EXECUTE FUNCTION green_view.update_bill_payment_status();

-- Create triggers for payment total amount calculation
CREATE OR REPLACE TRIGGER after_paymentdetail_insert_update_total
AFTER INSERT ON green_view.paymentsdetail
FOR EACH ROW
EXECUTE FUNCTION green_view.update_payment_total_amount();

CREATE OR REPLACE TRIGGER after_paymentdetail_delete_update_total
AFTER DELETE ON green_view.paymentsdetail
FOR EACH ROW
EXECUTE FUNCTION green_view.update_payment_total_amount();

-- Create trigger on service_requests table before updating status
CREATE OR REPLACE TRIGGER trg_set_completed_date
BEFORE UPDATE OF status ON green_view.service_requests
FOR EACH ROW
WHEN (OLD.status = 'In Progress' AND NEW.status = 'Completed')
EXECUTE FUNCTION green_view.set_completed_date();

-- Create trigger on payments table before updating payment_status
CREATE OR REPLACE TRIGGER trg_update_payment_date
BEFORE UPDATE OF payment_status ON green_view.payments
FOR EACH ROW
WHEN (OLD.payment_status = 'Pending' AND NEW.payment_status = 'Completed')
EXECUTE FUNCTION green_view.set_payment_date();

-- Create trigger to execute penalty bill creation
CREATE OR REPLACE TRIGGER trg_create_penalty_bill
AFTER UPDATE OF payment_status ON green_view.payments
FOR EACH ROW
WHEN (NEW.payment_status = 'Overdue' AND OLD.payment_status IS DISTINCT FROM 'Overdue')
EXECUTE FUNCTION green_view.create_penalty_bill();

-- 5. Insert data

INSERT INTO green_view.buildings (building_id, building_name, address)
VALUES ('B001', 'Green View', '321 Green View Gardens');

INSERT INTO green_view.apartments (apartment_id, building_id, max_population, transfer_status)
VALUES
    ('G01.01', 'B001', 6, 'Available'),
    ('G01.02', 'B001', 6, 'Available'),
    ('G02.03', 'B001', 6, 'Not Available'),
    ('G02.04', 'B001', 6, 'Available'),
    ('G03.05', 'B001', 6, 'Available'),
    ('G03.06', 'B001', 6, 'Available'),
    ('G04.07', 'B001', 6, 'Available'),
    ('G04.08', 'B001', 6, 'Available'),
    ('G05.09', 'B001', 6, 'Not Available'),
    ('G05.10', 'B001', 6, 'Available'),
    ('G06.11', 'B001', 6, 'Available'),
    ('G06.12', 'B001', 6, 'Available'),
    ('G07.13', 'B001', 6, 'Not Available'),
    ('G07.14', 'B001', 6, 'Available'),
    ('G08.15', 'B001', 6, 'Available'),
    ('G08.16', 'B001', 6, 'Available'),
    ('G09.17', 'B001', 6, 'Available'),
    ('G09.18', 'B001', 6, 'Available'),
    ('G10.19', 'B001', 6, 'Not Available'),
    ('G10.20', 'B001', 6, 'Available');

INSERT INTO green_view.residents
    (name, phone_number, email, sex, identification_number, apartment_id, created_at, updated_at, date_registered)
VALUES
    -- First 20 residents with a created_at, updated_at and date_registered set to one month ago
    ('Adrian Green', '0901112236', 'adrian.green@example.com', 'Male', '077000870001', 'G01.01', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Bianca Woods', '0912233448', 'bianca.woods@example.com', 'Female', '077120870002', 'G01.02', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Cameron Forest', '0923344559', 'cameron.forest@example.com', 'Male', '077220880003', 'G02.03', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Daphne Leaf', '0934455660', 'daphne.leaf@example.com', 'Female', '077320890004', 'G02.04', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Ethan Olive', '0945566781', 'ethan.olive@example.com', 'Male', '077420900005', 'G03.05', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Fiona Sage', '0956677882', 'fiona.sage@example.com', 'Female', '077520910006', 'G03.06', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Graham Pine', '0967788993', 'graham.pine@example.com', 'Male', '077620920007', 'G04.07', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Hazel Mint', '0978899004', 'hazel.mint@example.com', 'Female', '077720930008', 'G04.08', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Ian Fern', '0989900115', 'ian.fern@example.com', 'Male', '077820940009', 'G05.09', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Jade Willow', '0912345681', 'jade.willow@example.com', 'Female', '077920950010', 'G05.10', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Kyle Moss', '0923456782', 'kyle.moss@example.com', 'Male', '078020960011', 'G06.11', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Luna Ivy', '0934567893', 'luna.ivy@example.com', 'Female', '078120970012', 'G06.12', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Mason Elm', '0945678904', 'mason.elm@example.com', 'Male', '078220980013', 'G07.13', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Nova Cedar', '0956789015', 'nova.cedar@example.com', 'Female', '078320990014', 'G07.14', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Owen Birch', '0967890126', 'owen.birch@example.com', 'Male', '078420000015', 'G08.15', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Poppy Ash', '0978901237', 'poppy.ash@example.com', 'Female', '078520010016', 'G08.16', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Quinn Jade', '0989012348', 'quinn.jade@example.com', 'Male', '078620020017', 'G09.17', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('River Oak', '0912345683', 'river.oak@example.com', 'Female', '078720030018', 'G09.18', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Storm Thyme', '0923456785', 'storm.thyme@example.com', 'Male', '078820040019', 'G10.19', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Terra Basil', '0934567896', 'terra.basil@example.com', 'Female', '078920050020', 'G10.20', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    -- Next 20 residents with the current timestamp for created_at, updated_at and date_registered
    ('Urban Rose', '0911122337', 'urban.rose@example.com', 'Male', '079220180041', 'G01.01', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Violet Clover', '0922233448', 'violet.clover@example.com', 'Female', '079320190042', 'G01.02', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Woody Laurel', '0933344559', 'woody.laurel@example.com', 'Male', '079420200043', 'G02.03', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Xara Bloom', '0944455660', 'xara.bloom@example.com', 'Female', '079520210044', 'G02.04', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Yarrow Stone', '0955566781', 'yarrow.stone@example.com', 'Male', '079620220045', 'G03.05', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Zinnia Vale', '0966677882', 'zinnia.vale@example.com', 'Female', '079720230046', 'G03.06', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Atlas Grove', '0977788993', 'atlas.grove@example.com', 'Male', '079820240047', 'G04.07', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Brook Meadow', '0988899004', 'brook.meadow@example.com', 'Female', '079920250048', 'G04.08', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Clay Ridge', '0910000115', 'clay.ridge@example.com', 'Male', '080020260049', 'G05.09', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Dawn Fields', '0921112236', 'dawn.fields@example.com', 'Female', '080120270050', 'G05.10', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Echo Valley', '0932223347', 'echo.valley@example.com', 'Female', '080220280051', 'G06.11', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Flint Meadows', '0943334458', 'flint.meadows@example.com', 'Male', '080320290052', 'G06.12', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Glen Rivers', '0954445569', 'glen.rivers@example.com', 'Male', '080420300053', 'G07.13', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Haven Brooks', '0965556670', 'haven.brooks@example.com', 'Female', '080520310054', 'G07.14', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Iris Gardens', '0976667781', 'iris.gardens@example.com', 'Female', '080620320055', 'G08.15', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Jasper Hills', '0987778892', 'jasper.hills@example.com', 'Male', '080720330056', 'G08.16', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Kai Shores', '0918889903', 'kai.shores@example.com', 'Male', '080820340057', 'G09.17', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Lake Winters', '0929990004', 'lake.winters@example.com', 'Female', '080920350058', 'G09.18', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Moss Springs', '0930001115', 'moss.springs@example.com', 'Male', '081020360059', 'G10.19', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Nell Peaks', '0941112236', 'nell.peaks@example.com', 'Female', '081120370060', 'G10.20', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

-- Insert mock bills for various apartments
INSERT INTO green_view.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status)
VALUES
    -- Apartment G01.01 bills
    ('Electricity', 'G01.01', 890.00, '2025-05-25', '2025-06-01', 'Pending'),
    ('Water', 'G01.01', 340.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'G01.01', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Apartment G01.02 bills
    ('Electricity', 'G01.02', 820.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'G01.02', 310.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'G01.02', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Apartment G02.03 bills
    ('Electricity', 'G02.03', 940.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'G02.03', 370.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'G02.03', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Apartment G02.04 bills
    ('Electricity', 'G02.04', 660.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'G02.04', 230.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'G02.04', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Previous month bills for G01.01 (already paid)
    ('Electricity', 'G01.01', 830.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Water', 'G01.01', 320.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Maintenance', 'G01.01', 200.00, '2025-04-25', '2025-04-01', 'Pending'),

    -- Previous month bills for G01.02 (already paid)
    ('Electricity', 'G01.02', 760.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Water', 'G01.02', 240.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Maintenance', 'G01.02', 200.00, '2025-04-25', '2025-04-01', 'Pending');

-- Now let's complete some payments for the previous month
-- Note: We don't need to manually insert into payments or paymentsdetail tables
-- as our triggers will handle that automatically

-- Complete payments for April for apartments G01.01 and G01.02
SELECT green_view.complete_apartment_payment('G01.01', 'Bank Transfer', 2025, 4);
SELECT green_view.complete_apartment_payment('G01.02', 'Cash', 2025, 4);

-- Insert bills for some apartments for June (future month)
INSERT INTO green_view.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status)
VALUES
    ('Electricity', 'G02.03', 900.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Water', 'G02.03', 350.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Maintenance', 'G02.03', 200.00, '2025-06-25', '2025-06-01', 'Pending'),

    ('Electricity', 'G02.04', 810.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Water', 'G02.04', 320.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Maintenance', 'G02.04', 200.00, '2025-06-25', '2025-06-01', 'Pending');

-- Complete some current month payments
SELECT green_view.complete_apartment_payment('G02.03', 'Bank Transfer', 2025, 5);
SELECT green_view.complete_apartment_payment('G02.04', 'Mobile Payment', 2025, 5);

-- Insert some special bills
INSERT INTO green_view.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status)
VALUES
    ('Special Maintenance', 'G01.01', 1700.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Special Maintenance', 'G01.02', 1400.00, '2025-05-25', '2025-05-01', 'Pending');

-- Insert service requests with an initial status (e.g., 'In Progress')
INSERT INTO green_view.service_requests (
    apartment_id, resident_id, category, description, status, amount, request_date, completed_date
)
VALUES
    ('G01.01', 'R001', 'Plumbing', 'Garden irrigation system repair', 'In Progress', 180, CURRENT_TIMESTAMP - INTERVAL '15 days', NULL),
    ('G01.02', 'R002', 'Electrical', 'Outdoor lighting installation', 'In Progress', 150, CURRENT_TIMESTAMP - INTERVAL '29 days', NULL),
    ('G02.03', 'R003', 'HVAC', 'Green roof ventilation system', 'In Progress', 460, CURRENT_TIMESTAMP - INTERVAL '17 days', NULL),
    ('G02.04', 'R004', 'Cleaning', 'Solar panel cleaning', 'In Progress', 120, CURRENT_TIMESTAMP - INTERVAL '34 days', NULL),
    ('G03.05', 'R005', 'Security', 'Eco-friendly access system', 'In Progress', 230, CURRENT_TIMESTAMP - INTERVAL '12 days', NULL),
    ('G03.06', 'R006', 'Maintenance', 'Balcony garden setup', 'In Progress', 410, CURRENT_TIMESTAMP - INTERVAL '25 days', NULL),
    ('G04.07', 'R007', 'Plumbing', 'Rainwater collection system', 'In Progress', 280, CURRENT_TIMESTAMP - INTERVAL '44 days', NULL),
    ('G04.08', 'R008', 'Electrical', 'Energy-efficient appliance setup', 'In Progress', 340, CURRENT_TIMESTAMP - INTERVAL '21 days', NULL),
    ('G05.09', 'R009', 'HVAC', 'Geothermal system maintenance', 'In Progress', 500, CURRENT_TIMESTAMP - INTERVAL '61 days', NULL),
    ('G05.10', 'R010', 'Cleaning', 'Green space maintenance', 'In Progress', 160, CURRENT_TIMESTAMP - INTERVAL '14 days', NULL);

-- Now, update some service requests to 'Completed' to trigger bill creation
UPDATE green_view.service_requests
SET status = 'Completed'
WHERE request_id IN ('SR001', 'SR002', 'SR003', 'SR004', 'SR005', 'SR006');

-- Test the accumulation for an existing pending service bill
-- Let's say SR001 gets another related service completed
INSERT INTO green_view.service_requests (
    apartment_id, resident_id, category, description, status, amount, request_date, completed_date
)
VALUES
    ('G01.01', 'R001', 'Plumbing', 'Follow-up irrigation optimization', 'In Progress', 95, CURRENT_TIMESTAMP - INTERVAL '1 day', NULL);

UPDATE green_view.service_requests
SET status = 'Completed'
WHERE request_id = 'SR011';

-- Count all occupied apartments for 2 cases: until now and until 1 month ago
SELECT COUNT(*) AS occupied_apartments_now
FROM green_view.apartments
WHERE current_population > 0;

SELECT COUNT(DISTINCT apartment_id) AS occupied_apartments_1_month_ago
FROM green_view.residents
WHERE created_at <= CURRENT_DATE - INTERVAL '1 month';

-- 6. Show data from all of the TABLES
SELECT * FROM green_view.users;

SELECT * FROM green_view.buildings;
SELECT * FROM green_view.apartments;
SELECT * FROM green_view.residents;

SELECT * FROM green_view.bills;
SELECT * FROM green_view.payments;
SELECT * FROM green_view.paymentsdetail;

SELECT * FROM green_view.service_requests;

-- describe all table
ALTER TABLE green_view.service_requests 
ALTER COLUMN completed_date TYPE TIMESTAMPTZ,
ALTER COLUMN request_date TYPE TIMESTAMPTZ;