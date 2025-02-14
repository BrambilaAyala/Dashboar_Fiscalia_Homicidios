import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Bar } from 'react-chartjs-2'; // Para usar gráficos de barras

const Estadisticas = () => {
    const [datos, setDatos] = useState([]);

    useEffect(() => {
        axios.get('/homicidios') // Cambia la URL según corresponda
            .then(response => {
                setDatos(response.data);
            })
            .catch(error => {
                console.error("Error al obtener los datos", error);
            });
    }, []);

    const procesarDatos = () => {
        const labels = [...new Set(datos.map(d => d.Entidad))]; // Entidades únicas
        const values = labels.map(label => 
            datos.filter(d => d.Entidad === label)
                .reduce((acc, curr) => acc + curr.Num_Muertos, 0) // Suma de muertos por entidad
        );

        return {
            labels,
            datasets: [
                {
                    label: 'Número de Muertos por Entidad',
                    data: values,
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 1,
                },
            ],
        };
    };

    return (
        <div>
            <h2>Gráfica de Homicidios</h2>
            {datos.length > 0 ? (
                <Bar data={procesarDatos()} />
            ) : (
                <p>Cargando datos...</p>
            )}
        </div>
    );
};

export default Estadisticas;

