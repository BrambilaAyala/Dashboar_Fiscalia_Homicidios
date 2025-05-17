import os
import pdfplumber
import pandas as pd
from datetime import datetime
import re

# Ruta a la carpeta con archivos PDF
input_folder = 'C:\\Users\\leona\\Documents\\ProyectoFisca\\DataPdf'
output_folder = 'C:\\Users\\leona\\Documents\\ProyectoFisca\\Datacsv'

# Crea la carpeta de salida si no existe
os.makedirs(output_folder, exist_ok=True)

# Función para normalizar texto
def normalize_text(text):
    if isinstance(text, str):
        return re.sub(r'\([^)]*\)', '', text).strip()
    return text

# Función para verificar si un PDF contiene texto
def es_pdf_con_texto(pdf_path):
    with pdfplumber.open(pdf_path) as pdf:
        for page in pdf.pages:
            text = page.extract_text()
            if text and text.strip():
                return True
    return False

# Función para convertir tablas de un PDF a CSV
def pdf_table_to_csv_with_normalization(pdf_path, csv_path):
    if not es_pdf_con_texto(pdf_path):
        print(f"El archivo {pdf_path} no contiene texto y se omitirá.")
        return

    date_str = os.path.basename(pdf_path).split('_')[1].replace(".pdf", "")
    date_obj = datetime.strptime(date_str, "%d%m%Y")

    data = []
    with pdfplumber.open(pdf_path) as pdf:
        for page in pdf.pages:
            tables = page.extract_tables()
            for table in tables:
                for row in table:
                    if row and row[0] not in ["Entidad", "Municipio", "Muertos", "Hombre", "Mujer", "No Identificado", "Fuente"]:
<<<<<<< HEAD
                        row[0] = normalize_text(row[0].upper() if isinstance(row[0], str) else row[0])
                        row[1] = row[1].upper() if isinstance(row[1], str) else row[1]
                        row.append(date_obj)
                        data.append(row)
=======
                        if len(row) == 7:
                            row[0] = normalize_text(row[0].upper() if isinstance(row[0], str) else row[0])
                            row[1] = row[1].upper() if isinstance(row[1], str) else row[1]
                            row.append(date_obj)
                            data.append(row)
                        else:
                            print(f"Error en la cantidad de columnas ({len(row)}): {row}")
>>>>>>> 7f909d8 (Correcion de graficas por trimestre)

    df = pd.DataFrame(data, columns=["Entidad", "Municipio", "Muertos", "Hombre", "Mujer", "No Identificado", "Fuente", "Fecha"])
    
    # Limpieza de datos
    df = df[~df['Entidad'].isin(['HOMICIDIOS DOLOSOS', 'No de Muertos', 'TOTALES:', 'HOMICIDIOS CULPOSOS',
                                  'NO DE MUERTOS', 'HOMICIDIOS CULPOSOS'])].reset_index(drop=True)
    numeric_columns = ['Muertos', 'Hombre', 'Mujer', 'No Identificado']
    df[numeric_columns] = df[numeric_columns].replace('-', 0)
    df[numeric_columns] = df[numeric_columns].apply(pd.to_numeric, errors='coerce').fillna(0).astype(int)
    
    # Guarda el DataFrame limpio en un archivo CSV
    df.to_csv(csv_path, index=False, encoding='utf-8')

# Recorre todos los archivos PDF en la carpeta de entrada y convierte cada uno a CSV
for filename in os.listdir(input_folder):
    if filename.endswith(".pdf"):
        pdf_path = os.path.join(input_folder, filename)
        csv_filename = filename.replace(".pdf", ".csv")
        csv_path = os.path.join(output_folder, csv_filename)
        pdf_table_to_csv_with_normalization(pdf_path, csv_path)
        print(f"Convertido: {filename} a {csv_filename}")