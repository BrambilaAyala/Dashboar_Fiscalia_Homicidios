import pandas as pd
import os

# Cargar el archivo de referencia
ruta_referencia = 'C:\\Users\\leona\\Documents\\ProyectoFisca\\ENTIDAD_FEDERATIVA_201602.csv'
df_referencia = pd.read_csv(ruta_referencia)

# Crear un diccionario de mapeo
mapeo_entidades = {
    "BAJA\nCALIFORNIA": "BAJA CALIFORNIA",
    "BAJA CALIFORNIA\nSUR": "BAJA CALIFORNIA SUR",
    "CIUDAD DE\nMÉXICO": "CIUDAD DE MÉXICO",
    "MICHOACÁN\nDE OCAMPO": "MICHOACÁN DE OCAMPO",
    "VERACRUZ DE\nIGNACIO DE LA\nLLAVE": "VERACRUZ DE IGNACIO DE LA LLAVE",
    "QUINTANA\nROO": "QUINTANA ROO",
    "SAN LUIS\nPOTOSÍ": "SAN LUIS POTOSÍ",
    "COAHUILA DE\nZARAGOZA": "COAHUILA DE ZARAGOZA",
    "MICHOACAN DE OCAMPO": "MICHOACÁN DE OCAMPO",
    "michoacán de ocampo": "MICHOACÁN DE OCAMPO",
    "veracruz de ignacio de la llave": "VERACRUZ DE IGNACIO DE LA LLAVE",
}

# Ruta a la carpeta con archivos CSV generados
input_folder = 'C:\\Users\\leona\\Documents\\ProyectoFisca\\Datacsv'
output_folder = 'C:\\Users\\leona\\Documents\\ProyectoFisca\\Datacsv_normalizados'

# Crear la carpeta de salida si no existe
os.makedirs(output_folder, exist_ok=True)

# Función para normalizar los nombres de las entidades
def normalizar_entidades(df, mapeo):
    df['Entidad'] = df['Entidad'].str.upper()
    df['Entidad'] = df['Entidad'].replace(mapeo)
    return df

# Función para rellenar los valores vacíos en la columna "Entidad"
def rellenar_entidades_vacias(df):
    #df['Entidad'] = df['Entidad'].fillna(method='ffill')  #llena los valores vacios con la ultima entidad encontrada
    df['Entidad'] = df['Entidad'].ffill()
    return df

def rellenar_municipios_vacias(df):
    #df['Entidad'] = df['Entidad'].fillna(method='ffill')  #llena los valores vacios con la ultima entidad encontrada
    df['Municipio'] = df['Municipio'].ffill()
    return df

# Función para eliminar filas vacías en 'Entidad' y 'Municipio' al inicio del archivo
def eliminar_filas_vacias(df):
    # Se eliminan las filas donde ambas columnas 'Entidad' y 'Municipio' sean NaN o vacías
    df = df.dropna(subset=['Entidad', 'Municipio'], how='all')
    return df


# Recorre todos los archivos CSV en la carpeta de entrada
for filename in os.listdir(input_folder):
    if filename.endswith(".csv"):
        # Cargar el archivo CSV
        csv_path = os.path.join(input_folder, filename)
        df = pd.read_csv(csv_path)

        # Normalizar los nombres de las entidades
        df = normalizar_entidades(df, mapeo_entidades)
        # Rellenar valores vacíos en la columna "Entidad"
        df = rellenar_entidades_vacias(df)
        df = rellenar_municipios_vacias(df)
        df = eliminar_filas_vacias(df)

        # Guardar el archivo normalizado en la carpeta de salida
        output_path = os.path.join(output_folder, filename)
        df.to_csv(output_path, index=False, encoding='utf-8')

        print(f"Normalizado: {filename} guardado en {output_path}")