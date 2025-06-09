-- 1. Create schema
CREATE SCHEMA IF NOT EXISTS plaza;

CREATE SEQUENCE IF NOT EXISTS plaza.resident_id_seq START 1;
CREATE SEQUENCE IF NOT EXISTS plaza.bill_id_seq START 1;
CREATE SEQUENCE IF NOT EXISTS plaza.payment_id_seq START 1;

-- 2. Create tables
CREATE TABLE plaza.users (
    user_id  VARCHAR(20) PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    role VARCHAR(50) NOT NULL,
    status VARCHAR(50) DEFAULT 'Active',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP
);

CREATE TABLE plaza.buildings (
    building_id  VARCHAR(20) PRIMARY KEY,
    building_name VARCHAR(255) NOT NULL,
    address VARCHAR(255) NOT NULL,
    manager_id VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (manager_id) REFERENCES plaza.users(user_id) ON DELETE SET NULL
);

CREATE TABLE plaza.apartments (
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
    FOREIGN KEY (building_id) REFERENCES plaza.buildings(building_id) ON DELETE CASCADE
);

CREATE TABLE plaza.residents (
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
    FOREIGN KEY (apartment_id) REFERENCES plaza.apartments(apartment_id) ON DELETE CASCADE,
    FOREIGN KEY (owner_id) REFERENCES plaza.residents(resident_id) ON DELETE CASCADE
);

CREATE TABLE plaza.bills (
    bill_id  VARCHAR(20) PRIMARY KEY,
    apartment_id  VARCHAR(20) NOT NULL,
    bill_type  VARCHAR(50) NOT NULL,
    bill_amount  DECIMAL(12, 2) NOT NULL,
    due_date  TIMESTAMP NOT NULL,
    bill_date  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    payment_status VARCHAR(50) DEFAULT 'Pending',
    FOREIGN KEY (apartment_id) REFERENCES plaza.apartments(apartment_id) ON DELETE CASCADE
);

CREATE TABLE plaza.payments (
    payment_id VARCHAR(20) PRIMARY KEY,
    apartment_id VARCHAR(20) NOT NULL,
    total_amount DECIMAL(12, 2) NOT NULL,
    payment_date TIMESTAMP DEFAULT '9999-12-31 23:59:59',
    payment_method VARCHAR(50) DEFAULT '',
    payment_status VARCHAR(50) DEFAULT 'Pending',
    payment_created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (apartment_id) REFERENCES plaza.apartments(apartment_id) ON DELETE CASCADE
);

CREATE TABLE plaza.paymentsdetail (
    bill_id VARCHAR(20),
    payment_id VARCHAR(20),
    PRIMARY KEY (bill_id, payment_id),
    FOREIGN KEY (bill_id) REFERENCES plaza.bills(bill_id) ON DELETE CASCADE,
    FOREIGN KEY (payment_id) REFERENCES plaza.payments(payment_id) ON DELETE CASCADE
);

CREATE TABLE plaza.service_requests (
    request_id  VARCHAR(20) PRIMARY KEY,
    apartment_id  VARCHAR(20)  NOT NULL,
    resident_id  VARCHAR(20) NOT NULL,
    category VARCHAR(50) NOT NULL,
    description TEXT NOT NULL,
    amount  DECIMAL(12, 2) NOT null DEFAULT 0,
    status VARCHAR(50) DEFAULT 'Pending',
    request_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_date TIMESTAMP,
    FOREIGN KEY (apartment_id) REFERENCES plaza.apartments(apartment_id) ON DELETE CASCADE,
    FOREIGN KEY (resident_id) REFERENCES plaza.residents(resident_id) ON DELETE CASCADE
);

-- 3. Create functions
-- Auto-generate resident_id in format RXXX using a sequence
CREATE OR REPLACE FUNCTION plaza.set_resident_id()
RETURNS TRIGGER AS $$
BEGIN
    NEW.resident_id := 'R' || LPAD(nextval('plaza.resident_id_seq')::text, 3, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Auto-generate bill_id in format BI0001, BI0002, etc. using a sequence
CREATE OR REPLACE FUNCTION plaza.set_bill_id()
RETURNS TRIGGER AS $$
BEGIN
    NEW.bill_id := 'BI' || LPAD(nextval('plaza.bill_id_seq')::text, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Auto-generate payment_id in format P0001, P0002, etc. using a sequence
CREATE OR REPLACE FUNCTION plaza.set_payment_id()
RETURNS TRIGGER AS $$
BEGIN
    NEW.payment_id := 'P' || LPAD(nextval('plaza.payment_id_seq')::text, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Auto-generate service_request_id in format SRXXX using a sequence
CREATE OR REPLACE FUNCTION plaza.set_service_request_id()
RETURNS TRIGGER AS $$
BEGIN
    NEW.request_id := 'SR' || LPAD(nextval('plaza.payment_id_seq')::text, 4, '0');
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create function to automatically update current_population when there is a change in the residents table
CREATE OR REPLACE FUNCTION plaza.update_current_population()
RETURNS TRIGGER AS $$
BEGIN
    -- If operation is INSERT (adding a new resident to an apartment)
    IF (TG_OP = 'INSERT') THEN
        UPDATE plaza.apartments
        SET current_population = current_population + 1
        WHERE apartment_id = NEW.apartment_id;
        RETURN NEW;
    -- If operation is DELETE (removing a resident from an apartment)
    ELSIF (TG_OP = 'DELETE') THEN
        UPDATE plaza.apartments
        SET current_population = current_population - 1
        WHERE apartment_id = OLD.apartment_id;
        RETURN OLD;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create function to check and ensure current_population does not exceed max_population
CREATE OR REPLACE FUNCTION plaza.check_population_limit()
RETURNS TRIGGER AS $$
BEGIN
    -- Check the number of residents after adding (INSERT) or updating (UPDATE)
    IF (TG_OP = 'INSERT') THEN
        -- If adding a resident, check if current_population exceeds max_population
        IF (SELECT current_population + 1 > max_population FROM plaza.apartments WHERE apartment_id = NEW.apartment_id) THEN
            RAISE EXCEPTION 'Cannot insert more residents: current_population exceeds max_population';
        END IF;
    ELSIF (TG_OP = 'UPDATE') THEN
        -- If updating a resident, check if current_population exceeds max_population
        IF (SELECT current_population + 1 > max_population FROM plaza.apartments WHERE apartment_id = NEW.apartment_id) THEN
            RAISE EXCEPTION 'Cannot update resident: current_population exceeds max_population';
        END IF;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create function to update vacancy_status based on current_population
CREATE OR REPLACE FUNCTION plaza.update_vacancy_status()
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
CREATE OR REPLACE FUNCTION plaza.check_owner_id()
RETURNS TRIGGER AS $$
BEGIN
    -- Check if owner_id is the resident_id of the same apartment
    IF NOT EXISTS (
        SELECT 1
        FROM plaza.residents
        WHERE resident_id = NEW.owner_id
        AND apartment_id = NEW.apartment_id
    ) THEN
        RAISE EXCEPTION 'owner_id must be a resident_id of the same apartment';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create a function to generate or update apartment payment for a specific month
CREATE OR REPLACE FUNCTION plaza.complete_apartment_payment(
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
    FROM plaza.payments
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
    UPDATE plaza.payments
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
CREATE OR REPLACE FUNCTION plaza.manage_apartment_payment(
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
    FROM plaza.payments
    WHERE apartment_id = p_apartment_id
    AND DATE_TRUNC('month', payment_created_date) = v_payment_date
    AND payment_status = 'Pending'
    LIMIT 1;

    -- If no pending payment exists for bills from this month, create one
    IF v_payment_id IS NULL THEN
        INSERT INTO plaza.payments (apartment_id, payment_created_date, total_amount, payment_status)
        VALUES (p_apartment_id, v_payment_date, 0, 'Pending')
        RETURNING payment_id INTO v_payment_id;
    END IF;

    -- Link the bill to this payment if not already linked
    IF NOT EXISTS (SELECT 1 FROM plaza.paymentsdetail WHERE bill_id = p_bill_id AND payment_id = v_payment_id) THEN
        INSERT INTO plaza.paymentsdetail (bill_id, payment_id)
        VALUES (p_bill_id, v_payment_id);
    END IF;

    RETURN v_payment_id;
END;
$$ LANGUAGE plpgsql;

-- Trigger to update bill payment status when a payment is completed
-- This trigger ensures that all bills associated with a payment are marked 'Paid'
-- when the payment's status changes from 'Pending' to 'Completed'.
CREATE OR REPLACE FUNCTION plaza.update_bill_payment_status()
RETURNS TRIGGER AS $$
BEGIN
    -- When a payment status changes to 'Completed' from any other status
    IF NEW.payment_status = 'Completed' AND OLD.payment_status IS DISTINCT FROM NEW.payment_status THEN
        -- Update all associated bills to 'Paid'
        UPDATE plaza.bills
        SET payment_status = 'Paid'
        WHERE bill_id IN (
            SELECT bill_id
            FROM plaza.paymentsdetail
            WHERE payment_id = NEW.payment_id
        );
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- REINTRODUCED FUNCTION: Automatically links newly inserted bills to apartment payments.
-- This function is called by a trigger after a new bill is inserted.
-- It ensures that the bill is linked to the appropriate monthly payment, creating one if necessary.
CREATE OR REPLACE FUNCTION plaza.link_bill_to_payment()
RETURNS TRIGGER AS $$
DECLARE
    v_payment_id VARCHAR(20);
BEGIN
    -- Only process pending bills, but EXCLUDE penalty bills from auto-linking
    IF NEW.payment_status = 'Pending' AND NEW.bill_type != 'Late Payment Penalty' THEN
        -- Link the bill to the appropriate payment
        v_payment_id := plaza.manage_apartment_payment(NEW.apartment_id, NEW.bill_id, NEW.bill_date);
    END IF;
    -- Note: Penalty bills are manually linked in the create_penalty_bill function

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Update the function to calculate payment total amount
-- This trigger function maintains the total_amount on the payments table
-- by adding or subtracting the bill_amount of linked bills.
CREATE OR REPLACE FUNCTION plaza.update_payment_total_amount()
RETURNS TRIGGER AS $$
DECLARE
    bill_amount DECIMAL(12, 2);
BEGIN
    -- For INSERT operations into paymentsdetail
    IF (TG_OP = 'INSERT') THEN
        -- Get the bill amount from the bills table
        SELECT b.bill_amount INTO bill_amount
        FROM plaza.bills b
        WHERE b.bill_id = NEW.bill_id;

        -- Update the payment total_amount by adding the bill amount
        UPDATE plaza.payments
        SET total_amount = total_amount + bill_amount
        WHERE payment_id = NEW.payment_id;
    END IF;

    -- For DELETE operations from paymentsdetail
    IF (TG_OP = 'DELETE') THEN
        -- Get the bill amount from the bills table (using OLD.bill_id)
        SELECT b.bill_amount INTO bill_amount
        FROM plaza.bills b
        WHERE b.bill_id = OLD.bill_id;

        -- Update the payment total_amount by subtracting the bill amount
        UPDATE plaza.payments
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
CREATE OR REPLACE FUNCTION plaza.set_completed_date()
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
CREATE OR REPLACE FUNCTION plaza.set_payment_date()
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
CREATE OR REPLACE FUNCTION plaza.create_service_bill()
RETURNS TRIGGER AS $$
DECLARE
    existing_bill_id VARCHAR(20);
BEGIN
    -- This function is now triggered AFTER UPDATE on service_requests
    -- when status becomes 'Completed' and amount > 0.
    -- The logic below assumes these conditions are met by the trigger WHEN clause.

    -- Find an existing pending 'Service' bill for the same apartment
    SELECT bill_id INTO existing_bill_id
    FROM plaza.bills
    WHERE apartment_id = NEW.apartment_id
      AND bill_type = 'Service'
      AND payment_status = 'Pending' -- Only consider pending service bills for aggregation
    LIMIT 1;

    IF existing_bill_id IS NOT NULL THEN
        -- If a bill exists, add the new service amount to it
        UPDATE plaza.bills
        SET bill_amount = bill_amount + NEW.amount
        WHERE bill_id = existing_bill_id;
    ELSE
        -- If no pending service bill exists, create a new one
        INSERT INTO plaza.bills (
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
CREATE OR REPLACE FUNCTION plaza.create_penalty_bill()
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
            FROM plaza.bills b 
            INNER JOIN plaza.paymentsdetail pd ON b.bill_id = pd.bill_id
            WHERE pd.payment_id = NEW.payment_id 
            AND b.bill_type = 'Late Payment Penalty'
        ) THEN
            -- Calculate 5% penalty of the total amount
            v_penalty_amount := NEW.total_amount * 0.05;
            
            -- Insert new penalty bill
            INSERT INTO plaza.bills (
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
            INSERT INTO plaza.paymentsdetail (bill_id, payment_id)
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
AFTER UPDATE OF status ON plaza.service_requests
FOR EACH ROW
WHEN (NEW.status = 'Completed' AND OLD.status IS DISTINCT FROM NEW.status AND NEW.amount > 0)
EXECUTE FUNCTION plaza.create_service_bill();

-- Trigger to call the function before INSERT on residents to auto-generate resident_id
CREATE OR REPLACE TRIGGER before_resident_insert
BEFORE INSERT ON plaza.residents
FOR EACH ROW
EXECUTE FUNCTION plaza.set_resident_id();

-- Trigger to call the function before INSERT on bills to auto-generate bill_id
CREATE OR REPLACE TRIGGER before_bill_insert
BEFORE INSERT ON plaza.bills
FOR EACH ROW
EXECUTE FUNCTION plaza.set_bill_id();

-- Trigger to call the function before INSERT on payments to auto-generate payment_id
CREATE OR REPLACE TRIGGER before_payment_insert
BEFORE INSERT ON plaza.payments
FOR EACH ROW
WHEN (NEW.payment_id IS NULL)
EXECUTE FUNCTION plaza.set_payment_id();

-- Trigger to call the function before INSERT on service_requests to auto-generate request_id
CREATE OR REPLACE TRIGGER before_service_request_insert
BEFORE INSERT ON plaza.service_requests
FOR EACH ROW
EXECUTE FUNCTION plaza.set_service_request_id();

-- Trigger to call the function after INSERT on residents to update current_population
CREATE OR REPLACE TRIGGER after_resident_insert
AFTER INSERT ON plaza.residents
FOR EACH ROW
EXECUTE FUNCTION plaza.update_current_population();

-- Trigger to call the function after DELETE on residents to update current_population
CREATE OR REPLACE TRIGGER after_resident_delete
AFTER DELETE ON plaza.residents
FOR EACH ROW
EXECUTE FUNCTION plaza.update_current_population();

-- Trigger before INSERT on apartments to update vacancy_status
CREATE OR REPLACE TRIGGER update_vacancy_status_insert
BEFORE INSERT ON plaza.apartments
FOR EACH ROW
EXECUTE FUNCTION plaza.update_vacancy_status();

-- Trigger before UPDATE on apartments to update vacancy_status
CREATE OR REPLACE TRIGGER update_vacancy_status_update
BEFORE UPDATE ON plaza.apartments
FOR EACH ROW
EXECUTE FUNCTION plaza.update_vacancy_status();

-- Check when owner_id is provided
CREATE OR REPLACE TRIGGER check_owner_id_insert
BEFORE INSERT ON plaza.apartments
FOR EACH ROW
WHEN (NEW.owner_id IS NOT NULL)
EXECUTE FUNCTION plaza.check_owner_id();

CREATE OR REPLACE TRIGGER check_owner_id_update
BEFORE UPDATE ON plaza.apartments
FOR EACH ROW
WHEN (NEW.owner_id IS NOT NULL)
EXECUTE FUNCTION plaza.check_owner_id();

-- Triggers for population limit checking
CREATE OR REPLACE TRIGGER before_resident_insert_check_limit
BEFORE INSERT ON plaza.residents
FOR EACH ROW
EXECUTE FUNCTION plaza.check_population_limit();

CREATE OR REPLACE TRIGGER before_resident_update_check_limit
BEFORE UPDATE ON plaza.residents
FOR EACH ROW
WHEN (OLD.apartment_id IS DISTINCT FROM NEW.apartment_id)
EXECUTE FUNCTION plaza.check_population_limit();

-- Create triggers for automatically updating status linking bills to payments
CREATE OR REPLACE TRIGGER after_bill_insert_link_payment
AFTER INSERT ON plaza.bills
FOR EACH ROW
EXECUTE FUNCTION plaza.link_bill_to_payment();

CREATE OR REPLACE TRIGGER after_payment_status_update
AFTER UPDATE OF payment_status ON plaza.payments
FOR EACH ROW
EXECUTE FUNCTION plaza.update_bill_payment_status();

-- Create triggers for payment total amount calculation
CREATE OR REPLACE TRIGGER after_paymentdetail_insert_update_total
AFTER INSERT ON plaza.paymentsdetail
FOR EACH ROW
EXECUTE FUNCTION plaza.update_payment_total_amount();

CREATE OR REPLACE TRIGGER after_paymentdetail_delete_update_total
AFTER DELETE ON plaza.paymentsdetail
FOR EACH ROW
EXECUTE FUNCTION plaza.update_payment_total_amount();

-- Create trigger on service_requests table before updating status
CREATE OR REPLACE TRIGGER trg_set_completed_date
BEFORE UPDATE OF status ON plaza.service_requests
FOR EACH ROW
WHEN (OLD.status = 'In Progress' AND NEW.status = 'Completed')
EXECUTE FUNCTION plaza.set_completed_date();

-- Create trigger on payments table before updating payment_status
CREATE OR REPLACE TRIGGER trg_update_payment_date
BEFORE UPDATE OF payment_status ON plaza.payments
FOR EACH ROW
WHEN (OLD.payment_status = 'Pending' AND NEW.payment_status = 'Completed')
EXECUTE FUNCTION plaza.set_payment_date();

-- Create trigger to execute penalty bill creation
CREATE OR REPLACE TRIGGER trg_create_penalty_bill
AFTER UPDATE OF payment_status ON plaza.payments
FOR EACH ROW
WHEN (NEW.payment_status = 'Overdue' AND OLD.payment_status IS DISTINCT FROM 'Overdue')
EXECUTE FUNCTION plaza.create_penalty_bill();

-- 5. Insert data

INSERT INTO plaza.buildings (building_id, building_name, address)
VALUES ('B001', 'Plaza', '456 Plaza Avenue');

INSERT INTO plaza.apartments (apartment_id, building_id, max_population, transfer_status)
VALUES
    ('P01.01', 'B001', 6, 'Available'),
    ('P01.02', 'B001', 6, 'Available'),
    ('P02.03', 'B001', 6, 'Not Available'),
    ('P02.04', 'B001', 6, 'Available'),
    ('P03.05', 'B001', 6, 'Available'),
    ('P03.06', 'B001', 6, 'Available'),
    ('P04.07', 'B001', 6, 'Available'),
    ('P04.08', 'B001', 6, 'Available'),
    ('P05.09', 'B001', 6, 'Not Available'),
    ('P05.10', 'B001', 6, 'Available'),
    ('P06.11', 'B001', 6, 'Available'),
    ('P06.12', 'B001', 6, 'Available'),
    ('P07.13', 'B001', 6, 'Not Available'),
    ('P07.14', 'B001', 6, 'Available'),
    ('P08.15', 'B001', 6, 'Available'),
    ('P08.16', 'B001', 6, 'Available'),
    ('P09.17', 'B001', 6, 'Available'),
    ('P09.18', 'B001', 6, 'Available'),
    ('P10.19', 'B001', 6, 'Not Available'),
    ('P10.20', 'B001', 6, 'Available');

INSERT INTO plaza.residents
    (name, phone_number, email, sex, identification_number, apartment_id, created_at, updated_at, date_registered)
VALUES
    -- First 20 residents with a created_at, updated_at and date_registered set to one month ago
    ('Alice Cooper', '0901112234', 'alice.cooper@example.com', 'Female', '075000870001', 'P01.01', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Bob Wilson', '0912233446', 'bob.wilson@example.com', 'Male', '075120870002', 'P01.02', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Carol Davis', '0923344557', 'carol.davis@example.com', 'Female', '075220880003', 'P02.03', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('David Martinez', '0934455668', 'david.martinez@example.com', 'Male', '075320890004', 'P02.04', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Eva Thompson', '0945566779', 'eva.thompson@example.com', 'Female', '075420900005', 'P03.05', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Frank Garcia', '0956677880', 'frank.garcia@example.com', 'Male', '075520910006', 'P03.06', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Grace Lee', '0967788991', 'grace.lee@example.com', 'Female', '075620920007', 'P04.07', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Henry Rodriguez', '0978899002', 'henry.rodriguez@example.com', 'Male', '075720930008', 'P04.08', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Iris Walker', '0989900113', 'iris.walker@example.com', 'Female', '075820940009', 'P05.09', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Jack Hall', '0912345679', 'jack.hall@example.com', 'Male', '075920950010', 'P05.10', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Kelly Allen', '0923456780', 'kelly.allen@example.com', 'Female', '076020960011', 'P06.11', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Larry Young', '0934567891', 'larry.young@example.com', 'Male', '076120970012', 'P06.12', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Monica Hernandez', '0945678902', 'monica.hernandez@example.com', 'Female', '076220980013', 'P07.13', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Nathan King', '0956789013', 'nathan.king@example.com', 'Male', '076320990014', 'P07.14', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Olivia Wright', '0967890124', 'olivia.wright@example.com', 'Female', '076420000015', 'P08.15', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Peter Lopez', '0978901235', 'peter.lopez@example.com', 'Male', '076520010016', 'P08.16', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Quinn Hill', '0989012346', 'quinn.hill@example.com', 'Female', '076620020017', 'P09.17', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Ryan Scott', '0912345681', 'ryan.scott@example.com', 'Male', '076720030018', 'P09.18', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Samantha Green', '0923456783', 'samantha.green@example.com', 'Female', '076820040019', 'P10.19', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    ('Tom Adams', '0934567894', 'tom.adams@example.com', 'Male', '076920050020', 'P10.20', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month', CURRENT_TIMESTAMP - INTERVAL '1 month'),
    -- Next 20 residents with the current timestamp for created_at, updated_at and date_registered
    ('Ursula Baker', '0911122335', 'ursula.baker@example.com', 'Female', '077220180041', 'P01.01', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Victor Gonzalez', '0922233446', 'victor.gonzalez@example.com', 'Male', '077320190042', 'P01.02', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Wendy Nelson', '0933344557', 'wendy.nelson@example.com', 'Female', '077420200043', 'P02.03', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Xavier Carter', '0944455668', 'xavier.carter@example.com', 'Male', '077520210044', 'P02.04', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Yolanda Mitchell', '0955566779', 'yolanda.mitchell@example.com', 'Female', '077620220045', 'P03.05', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Zach Perez', '0966677880', 'zach.perez@example.com', 'Male', '077720230046', 'P03.06', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Amy Roberts', '0977788991', 'amy.roberts@example.com', 'Female', '077820240047', 'P04.07', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Blake Turner', '0988899002', 'blake.turner@example.com', 'Male', '077920250048', 'P04.08', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Chloe Phillips', '0910000113', 'chloe.phillips@example.com', 'Female', '078020260049', 'P05.09', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Derek Campbell', '0921112234', 'derek.campbell@example.com', 'Male', '078120270050', 'P05.10', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Emma Parker', '0932223345', 'emma.parker@example.com', 'Female', '078220280051', 'P06.11', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Felix Evans', '0943334456', 'felix.evans@example.com', 'Male', '078320290052', 'P06.12', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Gina Edwards', '0954445567', 'gina.edwards@example.com', 'Female', '078420300053', 'P07.13', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Hugo Collins', '0965556678', 'hugo.collins@example.com', 'Male', '078520310054', 'P07.14', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Ivy Stewart', '0976667789', 'ivy.stewart@example.com', 'Female', '078620320055', 'P08.15', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Jake Sanchez', '0987778890', 'jake.sanchez@example.com', 'Male', '078720330056', 'P08.16', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Kara Morris', '0918889901', 'kara.morris@example.com', 'Female', '078820340057', 'P09.17', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Leo Rogers', '0929990002', 'leo.rogers@example.com', 'Male', '078920350058', 'P09.18', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Mia Reed', '0930001113', 'mia.reed@example.com', 'Female', '079020360059', 'P10.19', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
    ('Noah Cook', '0941112234', 'noah.cook@example.com', 'Male', '079120370060', 'P10.20', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

-- Insert mock bills for various apartments
INSERT INTO plaza.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status)
VALUES
    -- Apartment P01.01 bills
    ('Electricity', 'P01.01', 820.00, '2025-05-25', '2025-06-01', 'Pending'),
    ('Water', 'P01.01', 310.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'P01.01', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Apartment P01.02 bills
    ('Electricity', 'P01.02', 750.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'P01.02', 280.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'P01.02', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Apartment P02.03 bills
    ('Electricity', 'P02.03', 880.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'P02.03', 340.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'P02.03', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Apartment P02.04 bills
    ('Electricity', 'P02.04', 620.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Water', 'P02.04', 210.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Maintenance', 'P02.04', 200.00, '2025-05-25', '2025-05-01', 'Pending'),

    -- Previous month bills for P01.01 (already paid)
    ('Electricity', 'P01.01', 790.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Water', 'P01.01', 290.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Maintenance', 'P01.01', 200.00, '2025-04-25', '2025-04-01', 'Pending'),

    -- Previous month bills for P01.02 (already paid)
    ('Electricity', 'P01.02', 720.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Water', 'P01.02', 220.00, '2025-04-25', '2025-04-01', 'Pending'),
    ('Maintenance', 'P01.02', 200.00, '2025-04-25', '2025-04-01', 'Pending');

-- Now let's complete some payments for the previous month
-- Note: We don't need to manually insert into payments or paymentsdetail tables
-- as our triggers will handle that automatically

-- Complete payments for April for apartments P01.01 and P01.02
SELECT plaza.complete_apartment_payment('P01.01', 'Bank Transfer', 2025, 4);
SELECT plaza.complete_apartment_payment('P01.02', 'Cash', 2025, 4);

-- Insert bills for some apartments for June (future month)
INSERT INTO plaza.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status)
VALUES
    ('Electricity', 'P02.03', 850.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Water', 'P02.03', 320.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Maintenance', 'P02.03', 200.00, '2025-06-25', '2025-06-01', 'Pending'),

    ('Electricity', 'P02.04', 780.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Water', 'P02.04', 290.00, '2025-06-25', '2025-06-01', 'Pending'),
    ('Maintenance', 'P02.04', 200.00, '2025-06-25', '2025-06-01', 'Pending');

-- Complete some current month payments
SELECT plaza.complete_apartment_payment('P02.03', 'Bank Transfer', 2025, 5);
SELECT plaza.complete_apartment_payment('P02.04', 'Mobile Payment', 2025, 5);

-- Insert some special bills
INSERT INTO plaza.bills (bill_type, apartment_id, bill_amount, due_date, bill_date, payment_status)
VALUES
    ('Special Maintenance', 'P01.01', 1400.00, '2025-05-25', '2025-05-01', 'Pending'),
    ('Special Maintenance', 'P01.02', 1100.00, '2025-05-25', '2025-05-01', 'Pending');

-- Insert service requests with an initial status (e.g., 'In Progress')
INSERT INTO plaza.service_requests (
    apartment_id, resident_id, category, description, status, amount, request_date, completed_date
)
VALUES
    ('P01.01', 'R001', 'Plumbing', 'Drain clog in bathroom', 'In Progress', 140, CURRENT_TIMESTAMP - INTERVAL '10 days', NULL),
    ('P01.02', 'R002', 'Electrical', 'Outlet repair in bedroom', 'In Progress', 110, CURRENT_TIMESTAMP - INTERVAL '25 days', NULL),
    ('P02.03', 'R003', 'HVAC', 'AC unit maintenance', 'In Progress', 400, CURRENT_TIMESTAMP - INTERVAL '12 days', NULL),
    ('P02.04', 'R004', 'Cleaning', 'Deep cleaning service', 'In Progress', 90, CURRENT_TIMESTAMP - INTERVAL '30 days', NULL),
    ('P03.05', 'R005', 'Security', 'Key replacement', 'In Progress', 180, CURRENT_TIMESTAMP - INTERVAL '8 days', NULL),
    ('P03.06', 'R006', 'Maintenance', 'Door handle repair', 'In Progress', 360, CURRENT_TIMESTAMP - INTERVAL '20 days', NULL),
    ('P04.07', 'R007', 'Plumbing', 'Shower head replacement', 'In Progress', 220, CURRENT_TIMESTAMP - INTERVAL '38 days', NULL),
    ('P04.08', 'R008', 'Electrical', 'Ceiling fan installation', 'In Progress', 290, CURRENT_TIMESTAMP - INTERVAL '16 days', NULL),
    ('P05.09', 'R009', 'HVAC', 'Thermostat calibration', 'In Progress', 450, CURRENT_TIMESTAMP - INTERVAL '55 days', NULL),
    ('P05.10', 'R010', 'Cleaning', 'Carpet cleaning', 'In Progress', 120, CURRENT_TIMESTAMP - INTERVAL '9 days', NULL);

-- Now, update some service requests to 'Completed' to trigger bill creation
UPDATE plaza.service_requests
SET status = 'Completed'
WHERE request_id IN ('SR001', 'SR002', 'SR003', 'SR004', 'SR005', 'SR006');

-- Test the accumulation for an existing pending service bill
-- Let's say SR001 gets another related service completed
INSERT INTO plaza.service_requests (
    apartment_id, resident_id, category, description, status, amount, request_date, completed_date
)
VALUES
    ('P01.01', 'R001', 'Plumbing', 'Follow-up drain maintenance', 'In Progress', 65, CURRENT_TIMESTAMP - INTERVAL '1 day', NULL);

UPDATE plaza.service_requests
SET status = 'Completed'
WHERE request_id = 'SR011';

-- Count all occupied apartments for 2 cases: until now and until 1 month ago
SELECT COUNT(*) AS occupied_apartments_now
FROM plaza.apartments
WHERE current_population > 0;

SELECT COUNT(DISTINCT apartment_id) AS occupied_apartments_1_month_ago
FROM plaza.residents
WHERE created_at <= CURRENT_DATE - INTERVAL '1 month';

-- 6. Show data from all of the TABLES
SELECT * FROM plaza.users;

SELECT * FROM plaza.buildings;
SELECT * FROM plaza.apartments;
SELECT * FROM plaza.residents;

SELECT * FROM plaza.bills;
SELECT * FROM plaza.payments;
SELECT * FROM plaza.paymentsdetail;

SELECT * FROM plaza.service_requests;

ALTER TABLE plaza.service_requests 
ALTER COLUMN completed_date TYPE TIMESTAMPTZ,
ALTER COLUMN request_date TYPE TIMESTAMPTZ;