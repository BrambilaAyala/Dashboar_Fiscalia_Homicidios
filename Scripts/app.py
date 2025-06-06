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
)
cursor = connection.cursor()

database_name = "fiscalia"

cursor.execute(f"CREATE DATABASE IF NOT EXISTS {database_name}")
cursor.execute(f"USE {database_name}")

cursor.execute("""
    CREATE TABLE IF NOT EXISTS Entidad (
        id INT PRIMARY KEY AUTO_INCREMENT,
        nombre VARCHAR(100) NOT NULL UNIQUE
    )
""")

cursor.execute("""
    CREATE TABLE IF NOT EXISTS Municipio (
        id INT PRIMARY KEY AUTO_INCREMENT,
        nombre VARCHAR(100) NOT NULL,
        entidad_id INT,
        UNIQUE (nombre, entidad_id),
        FOREIGN KEY (entidad_id) REFERENCES Entidad(id)
    )
""")

cursor.execute("""
    CREATE TABLE IF NOT EXISTS Homicidios (
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

# Cargar el archivo de municipios
municipios_path = 'C:\\Users\\leona\\Documents\\ProyectoFisca\\MUNICIPIOS_202408.csv'
df_municipios = pd.read_csv(municipios_path)

# Función para normalizar nombres de municipios
def normalize_municipio_name(name):
    if isinstance(name, str):
        # Eliminar saltos de línea y espacios innecesarios
        name = re.sub(r'\n', ' ', name).strip()
        name = re.sub(r'\s+', ' ', name)
        return name.upper()
    return name

# Normalizar los nombres de los municipios en el archivo de referencia
df_municipios['MUNICIPIO'] = df_municipios['MUNICIPIO'].apply(normalize_municipio_name)

# Crear un conjunto de municipios válidos
municipios_validos = set(df_municipios['MUNICIPIO'])

# Carpeta donde están los CSVs normalizados
csv_folder = "C:\\Users\\leona\\Documents\\ProyectoFisca\\Datacsv_normalizados"

# Procesar cada archivo CSV en la carpeta
for csv_file in os.listdir(csv_folder):
    if csv_file.endswith(".csv"):
        file_path = os.path.join(csv_folder, csv_file)

        # Leer el archivo CSV actual
        df = pd.read_csv(file_path)

        # Asegurar que las columnas necesarias estén presentes
        expected_columns = ['Entidad', 'Municipio', 'Fecha', 
                            'No de Muertos', 'Hombre', 'Mujer', 
                            'No Identificado', 'Fuente']
        for col in expected_columns:
            if col not in df.columns:
                if col == 'Fecha':
                    df[col] = pd.NaT
                elif col in ['No de Muertos', 'Hombre', 'Mujer', 'No Identificado']:
                    df[col] = 0
                else:
                    df[col] = ''

        # Convertir 'Fecha' a datetime y luego a cadena en formato 'YYYY-MM-DD'
        df['Fecha'] = pd.to_datetime(df['Fecha'], errors='coerce').dt.strftime('%Y-%m-%d')

        # Limpiar y normalizar datos
        df['Entidad'] = df['Entidad'].astype(str).fillna('')
        df['Municipio'] = df['Municipio'].apply(normalize_municipio_name).fillna('')
        df['No Identificado'] = pd.to_numeric(df['No Identificado'], errors='coerce').fillna(0).astype(int)
        df['Hombre'] = pd.to_numeric(df['Hombre'], errors='coerce').fillna(0).astype(int)
        df['Mujer'] = pd.to_numeric(df['Mujer'], errors='coerce').fillna(0).astype(int)
        df['Fuente'] = df['Fuente'].astype(str).fillna('')

        # Filtrar municipios válidos
        df = df[df['Municipio'].isin(municipios_validos)]

        # Insertar entidades
        entidad_unica = df['Entidad'].unique()
        for entidad in entidad_unica:
            #cursor.execute("INSERT IGNORE INTO Entidad (nombre) VALUES (%s)", (entidad,))
            cursor.execute("SELECT id FROM Entidad WHERE nombre = %s", (entidad,))
            result = cursor.fetchone()

            if not result:  # Solo insertamos si no existe
                cursor.execute("INSERT INTO Entidad (nombre) VALUES (%s)", (entidad,))
        #connection.commit()
                connection.commit()

        # Obtener IDs de entidades
        cursor.execute("SELECT id, nombre FROM Entidad")
        entidad_ids = {nombre: id for id, nombre in cursor.fetchall()}

        # Insertar municipios
        municipios_unicos = df[['Municipio', 'Entidad']].drop_duplicates()
        for _, row in municipios_unicos.iterrows():
            entidad_id = entidad_ids[row['Entidad']]
            cursor.execute("SELECT id FROM Municipio WHERE nombre = %s AND entidad_id = %s", (row['Municipio'], entidad_id))
            #cursor.execute("INSERT IGNORE INTO Municipio (nombre, entidad_id) VALUES (%s, %s)", (row['Municipio'], entidad_id))
            result = cursor.fetchone()
    
            if not result:  # Solo insertamos si no existe
                cursor.execute("INSERT INTO Municipio (nombre, entidad_id) VALUES (%s, %s)", (row['Municipio'], entidad_id))
                connection.commit()

        # Obtener IDs de municipios
        cursor.execute("SELECT id, nombre FROM Municipio")
        municipio_ids = {nombre: id for id, nombre in cursor.fetchall()}

        # Insertar registros en la tabla Homicidios
        for _, row in df.iterrows():
            municipio_id = municipio_ids.get(row['Municipio'])
            cursor.execute("""
                INSERT INTO Homicidios (municipio_id, fecha, num_muertos,
                 hombres, mujeres, no_identificado, fuente)
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