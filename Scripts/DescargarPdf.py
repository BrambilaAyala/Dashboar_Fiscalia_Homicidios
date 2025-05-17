import requests
import os
from datetime import datetime, timedelta

# Define la URL base
base_url = "http://www.informeseguridad.cns.gob.mx/files/homicidios_{}.pdf"

# Define la fecha de inicio y la fecha de fin
<<<<<<< HEAD
start_date = datetime(2024, 2, 2)  # Fecha de inicio
=======
start_date = datetime(2022, 2, 2)  # Fecha de inicio
>>>>>>> 7f909d8 (Correcion de graficas por trimestre)
end_date = datetime(2024, 4, 20)   # Fecha de fin

# Especifica el directorio para almacenar los PDFs
pdf_directory = 'C:\\Users\\leona\Documents\\ProyectoFisca\\DataPdf'
os.makedirs(pdf_directory, exist_ok=True)  # Crea el directorio si no existe

# Itera sobre cada día en el rango de fechas
current_date = start_date
while current_date <= end_date:
    # Formatea la fecha en el formato requerido (ddmmyyyy)
    formatted_date = current_date.strftime('%d%m%Y')
    
    # Genera la URL del PDF
    pdf_url = base_url.format(formatted_date)
    
    # Verifica el enlace antes de descargar
    print(f"Descargando: {pdf_url}")


    try:
        # Realiza la solicitud para obtener el PDF
        pdf_response = requests.get(pdf_url)

        # Verifica si la solicitud fue exitosa
        if pdf_response.status_code == 200:
            pdf_name = os.path.join(pdf_directory, f'homicidios_{formatted_date}.pdf')  # Nombre del archivo
            with open(pdf_name, 'wb') as f:
                f.write(pdf_response.content)
            print(f"Descargado: {pdf_name}")
        else:
            print(f"Error al descargar {pdf_url}: {pdf_response.status_code}")
    except requests.exceptions.RequestException as e:
        print(f"Error. La pagina no responde: {e}")

    # Avanza al siguiente día
    current_date += timedelta(days=1)