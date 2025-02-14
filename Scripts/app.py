import pandas as pd
import mysql.connector
import os
from datetime import datetime
import re

# Conecta a la base de datos MySQL
connection = mysql.connector.connect(
    host="localhost",
    user="root",
    password="HolaMundo22"
    #database="fiscalia"
)
cursor = connection.cursor()

database_name = "fiscalia"

cursor.execute(f"CREATE DATABASE IF NOT EXISTS {database_name}")
cursor.execute(f"USE {database_name}")

cursor.execute("""
    CREATE TABLE IF NOT EXISTS Entidad (
        id INT PRIMARY KEY AUTO_INCREMENT,
        nombre VARCHAR(100) NOT NULL
    )
""")

cursor.execute("""
    CREATE TABLE IF NOT EXISTS Municipio (
        id INT PRIMARY KEY AUTO_INCREMENT,
        nombre VARCHAR(100) NOT NULL,
        entidad_id INT,
        FOREIGN KEY (entidad_id) REFERENCES Entidad(id)
    )
""")

cursor.execute("""
    CREATE TABLE IF NOT EXISTS Homicidios3 (
        id INT PRIMARY KEY AUTO_INCREMENT,
        municipio_id INT,
        fecha DATE,
        num_muertos INT,
        hombres INT,
        mujeres INT,
        no_identificado INT,
        fuente VARCHAR(255),
        FOREIGN KEY (municipio_id) REFERENCES Municipio(id)
    )
""")
connection.commit()


# Carpeta donde están los CSVs convertidos
csv_folder = "C:\\Users\\leona\\vscode\\ProyectoFisca\\Datacsv"

# Procesar cada archivo CSV en la carpeta
for csv_file in os.listdir(csv_folder):
    if csv_file.endswith(".csv"):
        file_path = os.path.join(csv_folder, csv_file)

        date_str = csv_file.split('_')[1].replace(".csv", "")  # Obtiene '21092024' del nombre
        date_obj = datetime.strptime(date_str, "%d%m%Y")       # Convierte a datetime con formato '21-09-2024'
        
        
        # Leer el archivo CSV actual
        df = pd.read_csv(file_path)
        
        # Añadir la fecha extraída a la columna 'Fecha'
        df['Fecha'] = date_obj
        
        # Leer el archivo CSV actual
        df = pd.read_csv(file_path)
        df = df[~df['Entidad'].str.contains("Totales:", na=False)]


        df['Entidad'] = df['Entidad'].str.replace(r'\s\(\d+\)', '', regex=True)
        
        df['Entidad'] = df['Entidad'].ffill()
        df['Municipio'] = df['Municipio'].ffill()

        # Verificar y crear las columnas faltantes
        expected_columns = ['Entidad', 'Municipio', 'Fecha', 'No de Muertos', 'Hombre', 'Mujer', 'No Identificado', 'Fuente']
        for col in expected_columns:
            if col not in df.columns:
                # Asigna valores por defecto según el tipo de datos esperado
                if col == 'Fecha':
                    df[col] = pd.NaT  # Para fechas
                elif col in ['No de Muertos', 'Hombre', 'Mujer', 'No Identificado']:
                    df[col] = 0  # Para columnas numéricas
                else:
                    df[col] = ''  # Para texto
        df['Fecha'] = pd.to_datetime(df['Fecha'], errors='coerce')
        df = df.dropna(subset=['Entidad', 'Municipio'])

        df['Entidad'] = df['Entidad'].astype(str).fillna('')  # Convertir 'Entidad' a texto y reemplazar NaN con cadena vacía
        df['Municipio'] = df['Municipio'].astype(str).fillna('')  # Convertir 'Municipio' a texto y reemplazar NaN
        
        df = df.dropna(subset=['Entidad', 'Municipio'])
        # Convertir 'Fecha' a datetime y reemplazar NaT con None
        #df['Fecha'] = df['Fecha'].apply(lambda x: x if pd.notnull(x) else None)
        df['Fecha'] = df['Fecha'].apply(lambda x: x.strftime('%Y-%m-%d') if pd.notnull(x) else None)

        #la columna  contenga solo valores numéricos
        df['No Identificado'] = pd.to_numeric(df['No Identificado'], errors='coerce').fillna(0).astype(int)
        #columna Hombre contenga solo valores numéricos
        df['Hombre'] = pd.to_numeric(df['Hombre'], errors='coerce').fillna(0).astype(int)
        df['Mujer'] = pd.to_numeric(df['Mujer'], errors='coerce').fillna(0).astype(int)
        df['No Identificado'] = df['No Identificado'].fillna(0).astype(int)
        df['Fuente'] = df['Fuente'].astype(str).fillna('')  # Convertir 'Fuente' a texto
        
        df['Entidad'] = df['Entidad'].ffill()
        df['Municipio'] = df['Municipio'].ffill()

        # Inserta entidades
        for entidad in df['Entidad'].unique():
            cursor.execute("INSERT IGNORE INTO Entidad (nombre) VALUES (%s)", (entidad,))
        connection.commit()
        
        # Diccionario de entidades para relacionar nombres con sus IDs
        cursor.execute("SELECT id, nombre FROM Entidad")
        entidad_ids = {nombre: id for id, nombre in cursor.fetchall()}
        
        # Inserta municipios
        for municipio, entidad in zip(df['Municipio'], df['Entidad']):
            entidad_id = entidad_ids[entidad]
            cursor.execute("INSERT IGNORE INTO Municipio (nombre, entidad_id) VALUES (%s, %s)", (municipio, entidad_id))
        connection.commit()
        
        # Diccionario de municipios para relacionar nombres con sus IDs
        cursor.execute("SELECT id, nombre FROM Municipio")
        municipio_ids = {nombre: id for id, nombre in cursor.fetchall()}
        
        # Inserta registros en la tabla Homicidios3
        for _, row in df.iterrows():
            municipio_id = municipio_ids.get(row['Municipio'])
            cursor.execute("""
                INSERT INTO Homicidios3 (municipio_id, fecha, num_muertos, hombres, mujeres, no_identificado, fuente)
                VALUES (%s, %s, %s, %s, %s, %s, %s)
            """, (
                municipio_id,
                row['Fecha'],
                row['No de Muertos'],
                row['Hombre'],
                row['Mujer'],
                row['No Identificado'],
                row['Fuente']
            ))
        connection.commit()

# Cerrar la conexión a la base de datos
cursor.close()
connection.close()


