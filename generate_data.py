
import psycopg2
import random
from faker import Faker
from datetime import date, timedelta
import sys
from psycopg2.extras import execute_values

# --- Connection Details ---
# IMPORTANT: The connection string is hardcoded here for simplicity.
# For production environments, it is strongly recommended to use environment
# variables or a secret management tool to handle sensitive data like database credentials.
conn_string = "postgresql://postgres:postgresa@localhost:5432/nquisDB"

# --- Data Generation ---
fake = Faker()
departments = ["Engineering", "Sales", "Marketing", "HR", "Finance", "IT", "Operations", "Legal", "R&D", "Support"]

def generate_employee():
    """Generates a single employee record as a tuple."""
    first_name = fake.first_name()
    last_name = fake.last_name()
    department = random.choice(departments)
    # Generate a random hire date within the last 10 years
    start_date = date.today() - timedelta(days=10 * 365)
    end_date = date.today()
    hire_date = start_date + timedelta(days=random.randint(0, (end_date - start_date).days))
    return (first_name, last_name, department, hire_date)

# --- Database Insertion ---
def insert_data():
    """Connects to the database and inserts 1 million employee records."""
    conn = None  # Initialize conn to None
    try:
        conn = psycopg2.connect(conn_string)
        cursor = conn.cursor()
        print("Successfully connected to the database.")

        batch_size = 10000
        total_records = 1000000

        print(f"Generating and inserting {total_records} records in batches of {batch_size}...")

        for i in range(0, total_records, batch_size):
            # Generate a batch of records
            records_to_insert = [generate_employee() for _ in range(batch_size)]
            
            # Use execute_values for efficient batch insertion
            execute_values(
                cursor,
                "INSERT INTO Employees (first_name, last_name, department, hire_date) VALUES %s",
                records_to_insert
            )
            
            conn.commit()
            
            # Display progress
            progress = (i + batch_size) / total_records * 100
            sys.stdout.write(f"\rInserted {i + batch_size}/{total_records} records ({progress:.2f}%)")
            sys.stdout.flush()

        print("\nData insertion complete.")

    except psycopg2.OperationalError as e:
        print(f"\nConnection Error: Could not connect to the database.")
        print(f"Please check your connection string and ensure the database server is running.")
        print(f"Details: {e}")
    except psycopg2.Error as e:
        print(f"\nDatabase error: {e}")
        if conn:
            conn.rollback()  # Rollback any partial changes on error
    finally:
        if conn:
            cursor.close()
            conn.close()
            print("\nDatabase connection closed.")

if __name__ == "__main__":
    # Before running, ensure you have the required libraries installed:
    # pip install psycopg2-binary Faker
    insert_data()
